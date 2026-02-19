using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.Core;
using Azure.Identity;
using FeedbackExtractor.Core.Entities;
using FeedbackExtractor.Core.Interfaces;
using FeedbackExtractor.DocumentIntelligence.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Cryptography;

namespace FeedbackExtractor.DocumentIntelligence.Implementations
{

    /// <summary>
    /// Custom implementation of the IFeedbackExtractor interface.
    /// </summary>
    public class CustomFeedbackExtractor : IFeedbackExtractor
    {

        private readonly CustomFeedbackExtractorConfiguration config;
        private readonly ILogger<CustomFeedbackExtractor> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomFeedbackExtractor"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        public CustomFeedbackExtractor(IConfiguration configuration, ILogger<CustomFeedbackExtractor> logger)
        {
            this.config = CustomFeedbackExtractorConfiguration.Load(configuration);
            this.logger = logger;
        }

        /// <summary>
        /// Extracts session feedback asynchronously from the source document.
        /// </summary>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The extracted session feedback.</returns>
        public async Task<SessionFeedback> ExtractSessionFeedbackAsync(Stream sourceDocument, CancellationToken cancellationToken = default)
        {
            var session = new SessionFeedback();

            DocumentIntelligenceClient client = null;
            if (this.config.UseIdentity())
            {
                var credential = new ClientSecretCredential(this.config.TenantId, this.config.ClientId, this.config.ClientSecret);
                client = new DocumentIntelligenceClient(new Uri(this.config.Endpoint),
                   credential);
            }
            else
            {
                var credential = new AzureKeyCredential(this.config.Key);
                client = new DocumentIntelligenceClient(new Uri(this.config.Endpoint), credential);
            }

            try
            {
                var operation = await client.AnalyzeDocumentAsync(WaitUntil.Started, this.config.ModelName, BinaryData.FromStream(sourceDocument), cancellationToken);
                do
                {
                    await Task.Delay(125);
                    await operation.UpdateStatusAsync(cancellationToken);
                } while (!operation.HasCompleted);

                if (operation.HasValue)
                {
                    var firstDocument = operation.Value.Documents.FirstOrDefault();
                    if (firstDocument != null)
                        session = firstDocument.ToSessionFeedback(this.config.MinimumConfidence);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error extracting feedback from document");
                session = null;
            }

            return session;
        }

    }
}
