using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.Identity;
using FeedbackExtractor.Core.Entities;
using FeedbackExtractor.Core.Interfaces;
using FeedbackExtractor.DocumentIntelligence.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FeedbackExtractor.DocumentIntelligence.Implementations
{

    /// <summary>
    /// Extracts session feedback from a document using Azure Document Intelligence.
    /// </summary>
    public class DocumentFeedbackExtractor : IFeedbackExtractor
    {
        private readonly DocumentFeedbackExtractorConfiguration config;
        private readonly ILogger<DocumentFeedbackExtractor> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFeedbackExtractor"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        public DocumentFeedbackExtractor(IConfiguration configuration, ILogger<DocumentFeedbackExtractor> logger)
        {
            this.config = DocumentFeedbackExtractorConfiguration.Load(configuration);
            this.logger = logger;
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
                var operation = await client.AnalyzeDocumentAsync(WaitUntil.Started,
                       "prebuilt-layout", BinaryData.FromStream(sourceDocument), cancellationToken);
                do
                {
                    await Task.Delay(125);
                    await operation.UpdateStatusAsync(cancellationToken);
                } while (!operation.HasCompleted);

                if (operation.HasValue)
                {
                    session = ToSessionFeedback(operation.Value);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error extracting feedback from document");
                session = null;
            }

            return session;
        }

        private SessionFeedback ToSessionFeedback(AnalyzeResult source)
        {
            var session = new SessionFeedback();
            session.EventName = source.GetKeyValue("Event Name:", this.config.MinimumConfidence);
            session.SessionCode = source.GetKeyValue("Session Code:", this.config.MinimumConfidence);

            var qualityTables = source.Tables
                .Select((t, i) => new { Table = t, Index = i })
                .Where(a => a.Table.RowCount == 2 && a.Table.ColumnCount == 5)
                .ToArray();

            var eventQualityIndex = qualityTables[0].Index;
            var eventQuality = source.GetCheckedColumnFromTableRow(eventQualityIndex, 1);
            if (eventQuality.HasValue)
                session.EventQuality = eventQuality + 1;

            var sessionQualityIndex = qualityTables[1].Index;
            var sessionQuality = source.GetCheckedColumnFromTableRow(sessionQualityIndex, 1);
            if (sessionQuality.HasValue)
                session.SessionQuality = sessionQuality + 1;

            var speakerQualityIndex = qualityTables[2].Index;
            var speakerQuality = source.GetCheckedColumnFromTableRow(speakerQualityIndex, 1);
            if (speakerQuality.HasValue)
                session.SpeakerQuality = speakerQuality + 1;

            session.Comment = source.GetKeyValue("Comment:", this.config.MinimumConfidence);
            return session;
        }
    }
}
