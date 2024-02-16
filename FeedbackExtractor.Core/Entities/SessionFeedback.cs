namespace FeedbackExtractor.Core.Entities
{
    /// <summary>
    /// Represents the feedback for a session.
    /// </summary>
    public class SessionFeedback
    {
        /// <summary>
        /// Gets or sets the name of the event.
        /// </summary>
        public string? EventName { get; set; }

        /// <summary>
        /// Gets or sets the code of the session.
        /// </summary>
        public string? SessionCode { get; set; }

        /// <summary>
        /// Gets or sets the quality rating of the event.
        /// </summary>
        public int? EventQuality { get; set; }

        /// <summary>
        /// Gets or sets the quality rating of the session.
        /// </summary>
        public int? SessionQuality { get; set; }

        /// <summary>
        /// Gets or sets the quality rating of the speaker.
        /// </summary>
        public int? SpeakerQuality { get; set; }

        /// <summary>
        /// Gets or sets the comment for the session.
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// Checks if the session feedback is valid.
        /// </summary>
        /// <returns>True if the session feedback is valid, otherwise false.</returns>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(EventName) &&
                   !string.IsNullOrWhiteSpace(SessionCode) &&
                   EventQuality.HasValue && EventQuality > 0 && EventQuality <= 5 &&
                   SessionQuality.HasValue && SessionQuality > 0 && SessionQuality <= 5 &&
                   SpeakerQuality.HasValue && SpeakerQuality > 0 && SpeakerQuality <= 5;
        }
    }
}
