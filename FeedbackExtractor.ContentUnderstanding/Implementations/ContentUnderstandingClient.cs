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
    /// </summary>
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

        private async Task<string?> SubmitDocumentForAnalysisAsync(HttpClient client, string documentUrl, CancellationToken cancellationToken)
        {
            var endpoint = this.config.Endpoint.TrimEnd('/');
            var analyzerName = this.config.AnalyzerName;
            var apiVersion = this.config.ApiVersion;

            var requestUrl = $"{endpoint}/contentunderstanding/analyzers/{analyzerName}:analyze?api-version={apiVersion}";

            var requestBody = new
            {
                inputs = new[]
                {
                    new { url = documentUrl }
                }
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
