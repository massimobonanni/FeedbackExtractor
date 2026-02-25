using FeedbackExtractor.ContentUnderstanding.Entities;
using FeedbackExtractor.ContentUnderstanding.Interfaces;
using FeedbackExtractor.Core.Entities;
using FeedbackExtractor.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FeedbackExtractor.ContentUnderstanding.Implementations
{
    /// <summary>
    /// Extracts session feedback from a document using Azure Content Understanding.
    /// Implements <see cref="IFeedbackExtractor"/> by delegating document analysis to
    /// an <see cref="IContentUnderstandingClient"/> and mapping the resulting
    /// <see cref="AnalyzeContent"/> fields to a <see cref="SessionFeedback"/> instance.
    /// </summary>
    public class ContentUnderstandingFeedbackExtractor : IFeedbackExtractor
    {
        private readonly IContentUnderstandingClient contentUnderstandingClient;
        private readonly ILogger<ContentUnderstandingFeedbackExtractor> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentUnderstandingFeedbackExtractor"/> class.
        /// </summary>
        /// <param name="contentUnderstandingClient">The Content Understanding client used to analyze documents.</param>
        /// <param name="logger">The logger used for diagnostic output.</param>
        public ContentUnderstandingFeedbackExtractor(IContentUnderstandingClient contentUnderstandingClient,
            ILogger<ContentUnderstandingFeedbackExtractor> logger)
        {
            this.contentUnderstandingClient = contentUnderstandingClient;
            this.logger = logger;
        }

        /// <summary>
        /// Extracts session feedback asynchronously from a source document by submitting it
        /// to the Azure Content Understanding service and mapping the first content result
        /// to a <see cref="SessionFeedback"/> instance.
        /// </summary>
        /// <param name="sourceDocument">The source document stream to analyze.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="SessionFeedback"/> populated with the extracted data,
        /// an empty <see cref="SessionFeedback"/> if no content was returned,
        /// or <c>null</c> if an error occurred during extraction.
        /// </returns>
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

        /// <summary>
        /// Maps an <see cref="AnalyzeContent"/> instance to a <see cref="SessionFeedback"/> by
        /// reading known field names (<c>EventName</c>, <c>SessionCode</c>, <c>EventQuality</c>,
        /// <c>SessionQuality</c>, <c>SpeakerQuality</c>, <c>Comment</c>) from the content's
        /// field dictionary.
        /// </summary>
        /// <param name="content">The analyzed content whose fields are mapped to session feedback properties.</param>
        /// <returns>
        /// A <see cref="SessionFeedback"/> populated with available field values,
        /// or an empty instance if <paramref name="content"/> has no fields.
        /// </returns>
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

        /// <summary>
        /// Retrieves the string value of a named field from the field dictionary.
        /// </summary>
        /// <param name="fields">The dictionary of analyzed fields keyed by field name.</param>
        /// <param name="fieldName">The name of the field to retrieve.</param>
        /// <returns>
        /// The <see cref="AnalyzeField.ValueString"/> of the field if present and non-null;
        /// otherwise, <c>null</c>.
        /// </returns>
        private static string? GetStringFieldValue(Dictionary<string, AnalyzeField> fields, string fieldName)
        {
            if (fields.TryGetValue(fieldName, out var field) && field.ValueString != null)
            {
                return field.ValueString;
            }
            return null;
        }

        /// <summary>
        /// Retrieves the integer value of a named field from the field dictionary.
        /// First attempts to read <see cref="AnalyzeField.ValueInteger"/>; if that is not set,
        /// falls back to parsing <see cref="AnalyzeField.ValueString"/> as an integer.
        /// </summary>
        /// <param name="fields">The dictionary of analyzed fields keyed by field name.</param>
        /// <param name="fieldName">The name of the field to retrieve.</param>
        /// <returns>
        /// The integer value of the field if available directly or via string parsing;
        /// otherwise, <c>null</c>.
        /// </returns>
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
