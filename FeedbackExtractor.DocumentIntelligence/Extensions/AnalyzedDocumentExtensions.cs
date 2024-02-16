using FeedbackExtractor.Core.Entities;

namespace Azure.AI.DocumentIntelligence
{
    /// <summary>
    /// Provides extension methods for the AnalyzedDocument class.
    /// </summary>
    internal static class AnalyzedDocumentExtensions
    {
        /// <summary>
        /// Converts an AnalyzedDocument to a SessionFeedback.
        /// </summary>
        /// <param name="document">The AnalyzedDocument to convert.</param>
        /// <param name="confidence">The confidence level for field extraction.</param>
        /// <returns>A SessionFeedback object.</returns>
        public static SessionFeedback ToSessionFeedback(this AnalyzedDocument document, float confidence)
        {
            var session = new SessionFeedback();

            if (document == null)
                return session;

            session.EventName = document.GetFieldValue("EventName", confidence);
            session.SessionCode = document.GetFieldValue("SessionCode", confidence);
            session.EventQuality = document.GetSelectionmarkValue("EventQuality", confidence);
            session.SessionQuality = document.GetSelectionmarkValue("SessionQuality", confidence);
            session.SpeakerQuality = document.GetSelectionmarkValue("SpeakerQuality", confidence);
            session.Comment = document.GetFieldValue("Comment", confidence);

            return session;
        }

        /// <summary>
        /// Gets the value of a field from an AnalyzedDocument.
        /// </summary>
        /// <param name="document">The AnalyzedDocument to extract the field value from.</param>
        /// <param name="keyName">The name of the field to extract.</param>
        /// <param name="confidence">The confidence level for field extraction.</param>
        /// <returns>The value of the field, or null if the field is not present or the confidence is too low.</returns>
        public static string? GetFieldValue(this AnalyzedDocument document, string keyName, float confidence)
        {
            if (!document.Fields.ContainsKey(keyName))
                return null;

            if (document.Fields[keyName].Confidence < confidence)
                return null;

            return document.Fields[keyName].Content;
        }

        /// <summary>
        /// Gets the value of a selection mark from an AnalyzedDocument.
        /// </summary>
        /// <param name="document">The AnalyzedDocument to extract the selection mark value from.</param>
        /// <param name="fieldPrefix">The prefix of the field to extract.</param>
        /// <param name="confidence">The confidence level for field extraction.</param>
        /// <returns>The value of the selection mark, or null if the selection mark is not present or the confidence is too low.</returns>
        public static int? GetSelectionmarkValue(this AnalyzedDocument document, string fieldPrefix, float confidence)
        {
            for (int i = 1; i <= 5; i++)
            {
                var fieldName = $"{fieldPrefix}{i}";
                if (document.Fields.ContainsKey(fieldName) &&
                    document.Fields[fieldName].Confidence >= confidence &&
                    document.Fields[fieldName]?.ValueSelectionMark == "selected")
                {
                    return i;
                }
            }
            return null;
        }
    }
}
