using FeedbackExtractor.ContentUnderstanding.Entities;
using FeedbackExtractor.ContentUnderstanding.Interfaces;
using FeedbackExtractor.Core.Entities;
using FeedbackExtractor.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FeedbackExtractor.ContentUnderstanding.Implementations
{
    /// <summary>
    /// Extracts session feedback from a document using Azure Content Understanding.
    /// </summary>
    public class ContentUnderstandingFeedbackExtractor : IFeedbackExtractor
    {
        private readonly IContentUnderstandingClient contentUnderstandingClient;
        private readonly ILogger<ContentUnderstandingFeedbackExtractor> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentUnderstandingFeedbackExtractor"/> class.
        /// </summary>
        /// <param name="contentUnderstandingClient">The Content Understanding client.</param>
        /// <param name="logger">The logger.</param>
        public ContentUnderstandingFeedbackExtractor(IContentUnderstandingClient contentUnderstandingClient,
            ILogger<ContentUnderstandingFeedbackExtractor> logger)
        {
            this.contentUnderstandingClient = contentUnderstandingClient;
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

            try
            {
                var analyzeResponse = await contentUnderstandingClient.AnalyzeDocumentAsync(sourceDocument, cancellationToken);
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
