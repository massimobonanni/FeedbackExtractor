using Azure.Identity;
using FeedbackExtractor.ContentUnderstanding.Configurations;
using FeedbackExtractor.ContentUnderstanding.Entities;
using FeedbackExtractor.Core.Entities;
using FeedbackExtractor.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FeedbackExtractor.ContentUnderstanding.Implementations
{
    /// <summary>
    /// Extracts session feedback from a document using Azure Content Understanding.
    /// </summary>
    public class ContentUnderstandingFeedbackExtractor : IFeedbackExtractor
    {
        private readonly ContentUnderstandingFeedbackExtractorConfiguration config;
        private readonly ILogger<ContentUnderstandingFeedbackExtractor> logger;
        private readonly IHttpClientFactory httpClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentUnderstandingFeedbackExtractor"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        public ContentUnderstandingFeedbackExtractor(IConfiguration configuration,
            ILogger<ContentUnderstandingFeedbackExtractor> logger,
            IHttpClientFactory httpClientFactory)
        {
            this.config = ContentUnderstandingFeedbackExtractorConfiguration.Load(configuration);
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Extracts session feedback asynchronously from a source document.
        /// </summary>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The extracted session feedback.</returns>
        public async Task<SessionFeedback> ExtractSessionFeedbackAsync(Stream sourceDocument, CancellationToken cancellationToken = default)
        {
            var session = new SessionFeedback();

            try
            {
                var client = httpClientFactory.CreateClient();
                await ConfigureHttpClientAsync(client, cancellationToken);

                var operationLocation = await SubmitDocumentForAnalysisAsync(client, sourceDocument, cancellationToken);
                if (string.IsNullOrEmpty(operationLocation))
                {
                    this.logger.LogError("Failed to submit document for analysis: no Operation-Location returned");
                    return null;
                }

                var analyzeResponse = await PollForResultAsync(client, operationLocation, cancellationToken);
                if (analyzeResponse?.Result?.Contents != null && analyzeResponse.Result.Contents.Count > 0)
                {
                    session = ToSessionFeedback(analyzeResponse.Result.Contents[0]);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error extracting feedback from document using Content Understanding");
                session = null;
            }

            return session;
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

        private async Task<string?> SubmitDocumentForAnalysisAsync(HttpClient client, Stream sourceDocument, CancellationToken cancellationToken)
        {
            var endpoint = this.config.Endpoint.TrimEnd('/');
            var analyzerName = this.config.AnalyzerName;
            var apiVersion = this.config.ApiVersion;

            var requestUrl = $"{endpoint}/contentunderstanding/analyzers/{analyzerName}:analyze?api-version={apiVersion}";

            using var memoryStream = new MemoryStream();
            await sourceDocument.CopyToAsync(memoryStream, cancellationToken);
            var documentBytes = memoryStream.ToArray();
            var base64Content = Convert.ToBase64String(documentBytes);

            var requestBody = new
            {
                url = $"data:application/octet-stream;base64,{base64Content}"
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(requestUrl, jsonContent, cancellationToken);
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

        private static SessionFeedback ToSessionFeedback(AnalyzeContent content)
        {
            var session = new SessionFeedback();

            if (content?.Fields == null)
                return session;

            session.EventName = GetStringFieldValue(content.Fields, "EventName");
            session.SessionCode = GetStringFieldValue(content.Fields, "SessionCode");
            session.EventQuality = GetIntFieldValue(content.Fields, "EventQuality");
            session.SessionQuality = GetIntFieldValue(content.Fields, "SessionQuality");
            session.SpeakerQuality = GetIntFieldValue(content.Fields, "SpeakerQuality");
            session.Comment = GetStringFieldValue(content.Fields, "Comment");

            return session;
        }

        private static string? GetStringFieldValue(Dictionary<string, AnalyzeField> fields, string fieldName)
        {
            if (fields.TryGetValue(fieldName, out var field) && field.ValueString != null)
            {
                return field.ValueString;
            }
            return null;
        }

        private static int? GetIntFieldValue(Dictionary<string, AnalyzeField> fields, string fieldName)
        {
            if (fields.TryGetValue(fieldName, out var field))
            {
                if (field.ValueInteger.HasValue)
                {
                    return field.ValueInteger.Value;
                }
                if (field.ValueString != null && int.TryParse(field.ValueString, out var intValue))
                {
                    return intValue;
                }
            }
            return null;
        }
    }
}
