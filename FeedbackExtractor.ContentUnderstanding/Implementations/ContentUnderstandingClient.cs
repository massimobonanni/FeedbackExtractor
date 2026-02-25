using Azure.Identity;
using FeedbackExtractor.ContentUnderstanding.Configurations;
using FeedbackExtractor.ContentUnderstanding.Entities;
using FeedbackExtractor.ContentUnderstanding.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FeedbackExtractor.ContentUnderstanding.Implementations
{
    /// <summary>
    /// Client for interacting with the Azure Content Understanding service.
    /// <para>
    /// This client handles document analysis by uploading documents to blob storage,
    /// submitting them to the Azure Content Understanding API, and polling for results.
    /// It supports both API key and Azure AD (client credentials) authentication.
    /// </para>
    /// </summary>
    /// <remarks>
    /// This class implements <see cref="IContentUnderstandingClient"/> and is registered as an internal service.
    /// It requires an <see cref="IHttpClientFactory"/> for HTTP communication, an <see cref="IBlobStorageClient"/>
    /// for temporary blob storage of documents, and configuration via <see cref="ContentUnderstandingClientConfiguration"/>.
    /// Uploaded blobs are automatically cleaned up after the analysis completes or fails.
    /// </remarks>
    internal class ContentUnderstandingClient : IContentUnderstandingClientandingClient"/> and is registered as an internal service.
    /// It requires an <see cref="IHttpClientFactory"/> for HTTP communication, an <see cref="IBlobStorageClient"/>
    /// for temporary blob storage of documents, and configuration via <see cref="ContentUnderstandingClientConfiguration"/>.
    /// Uploaded blobs are automatically cleaned up after the analysis completes or fails.
    /// </remarks>
    internal class ContentUnderstandingClient : IContentUnderstandingClient
    {
        private readonly ContentUnderstandingClientConfiguration config;
        private readonly ILogger<ContentUnderstandingClient> logger;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IBlobStorageClient blobStorageClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentUnderstandingClient"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        /// <param name="blobStorageClient">The blob storage client.</param>
        public ContentUnderstandingClient(IConfiguration configuration,
            ILogger<ContentUnderstandingClient> logger,
            IHttpClientFactory httpClientFactory,
            IBlobStorageClient blobStorageClient)
        {
            this.config = ContentUnderstandingClientConfiguration.Load(configuration);
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
            this.blobStorageClient = blobStorageClient;
        }

        /// <inheritdoc />
        public async Task<AnalyzeResponse?> AnalyzeDocumentAsync(Stream sourceDocument, CancellationToken cancellationToken = default)
        {
            var client = httpClientFactory.CreateClient();
            await ConfigureHttpClientAsync(client, cancellationToken);

            var (blobName, sasUrl) = await blobStorageClient.UploadAndGetSasUrlAsync(sourceDocument, cancellationToken);

            try
            {
                var operationLocation = await SubmitDocumentForAnalysisAsync(client, sasUrl, cancellationToken);
                if (string.IsNullOrEmpty(operationLocation))
                {
                    this.logger.LogError("Failed to submit document for analysis: no Operation-Location returned");
                    return null;
                }

                return await PollForResultAsync(client, operationLocation, cancellationToken);
            }
            finally
            {
                await blobStorageClient.DeleteAsync(blobName, cancellationToken);
            }
        }

        /// <summary>
        /// Configures the <see cref="HttpClient"/> with the appropriate authentication headers.
        /// </summary>
        /// <remarks>
        /// If Azure AD identity is configured (tenant ID, client ID, and client secret), a bearer token
        /// is obtained via <see cref="ClientSecretCredential"/> for the Cognitive Services scope.
        /// Otherwise, the API subscription key is added as the <c>Ocp-Apim-Subscription-Key</c> header.
        /// </remarks>
        /// <param name="client">The <see cref="HttpClient"/> instance to configure.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous configuration operation.</returns>
        private async Task ConfigureHttpClientAsync(HttpClient client, CancellationToken cancellationToken)
        {
            if (this.config.UseIdentity())
            {
                var credential = new ClientSecretCredential(this.config.TenantId, this.config.ClientId, this.config.ClientSecret);
                var tokenRequestContext = new Azure.Core.TokenRequestContext(new[] { "https://cognitiveservices.azure.com/.default" });
                var token = await credential.GetTokenAsync(tokenRequestContext, cancellationToken);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
            }
            else
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", this.config.Key);
            }
        }

        /// <summary>
        /// Submits a document URL to the Azure Content Understanding analyzer for analysis.
        /// </summary>
        /// <param name="client">The configured <see cref="HttpClient"/> to use for the request.</param>
        /// <param name="documentUrl">The SAS URL of the document to analyze.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// The <c>Operation-Location</c> header value from the response, which is used to poll
        /// for the analysis result; or <c>null</c> if the header is not present.
        /// </returns>
        /// <exception cref="HttpRequestException">Thrown when the API request returns a non-success status code.</exception>
        private async Task<string?> SubmitDocumentForAnalysisAsync(HttpClient client, string documentUrl, CancellationToken cancellationToken)
        {
            var endpoint = this.config.Endpoint.TrimEnd('/');
            var analyzerName = this.config.AnalyzerName;
            var apiVersion = this.config.ApiVersion;

            var requestUrl = $"{endpoint}/contentunderstanding/analyzers/{analyzerName}:analyze?api-version={apiVersion}";

            var requestBody = new
            {
                url = documentUrl
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(requestUrl, jsonContent, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                this.logger.LogError("API request failed with status {StatusCode}: {ErrorContent}",
                    response.StatusCode, errorContent);
            }

            response.EnsureSuccessStatusCode();

            if (response.Headers.TryGetValues("Operation-Location", out var values))
            {
                return values.FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// Polls the specified operation location until the analysis completes, fails, or is cancelled.
        /// </summary>
        /// <remarks>
        /// The method polls every 500 milliseconds. It returns the <see cref="AnalyzeResponse"/>
        /// when the status is <c>Succeeded</c>, or <c>null</c> when the status is <c>Failed</c>,
        /// <c>Cancelled</c>, or the <paramref name="cancellationToken"/> is triggered.
        /// </remarks>
        /// <param name="client">The configured <see cref="HttpClient"/> to use for polling requests.</param>
        /// <param name="operationLocation">The operation location URL to poll for the result.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// The <see cref="AnalyzeResponse"/> if the analysis succeeded; otherwise, <c>null</c>.
        /// </returns>
        /// <exception cref="HttpRequestException">Thrown when a polling request returns a non-success status code.</exception>
        private async Task<AnalyzeResponse?> PollForResultAsync(HttpClient client, string operationLocation, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(500, cancellationToken);

                var response = await client.GetAsync(operationLocation, cancellationToken);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var analyzeResponse = JsonSerializer.Deserialize<AnalyzeResponse>(responseContent);

                if (analyzeResponse == null)
                    return null;

                switch (analyzeResponse.Status)
                {
                    case "Succeeded":
                        return analyzeResponse;
                    case "Failed":
                    case "Cancelled":
                        this.logger.LogError("Content Understanding analysis failed with status: {Status}", analyzeResponse.Status);
                        return null;
                    default:
                        // Still running, continue polling
                        break;
                }
            }

            return null;
        }
    }
}
