using FeedbackExtractor.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackExtractor.OpenAI.Entities
{
    /// <summary>
    /// Represents a session feedback entity.
    /// </summary>
    public class SessionFeedback
    {
        /// <summary>
        /// Gets or sets the event name.
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// Gets or sets the session code.
        /// </summary>
        public string SessionCode { get; set; }

        /// <summary>
        /// Gets or sets the event quality.
        /// </summary>
        public int EventQuality { get; set; }

        /// <summary>
        /// Gets or sets the session quality.
        /// </summary>
        public int SessionQuality { get; set; }

        /// <summary>
        /// Gets or sets the speaker quality.
        /// </summary>
        public int SpeakerQuality { get; set; }

        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Converts the session feedback to a FeedbackExtractor.Core.Entities.SessionFeedback object.
        /// </summary>
        /// <returns>The converted FeedbackExtractor.Core.Entities.SessionFeedback object.</returns>
        public FeedbackExtractor.Core.Entities.SessionFeedback ToFeedbackSession()
        {
            return new FeedbackExtractor.Core.Entities.SessionFeedback
            {
                EventName = this.EventName,
                SessionCode = this.SessionCode,
                EventQuality = this.EventQuality,
                SessionQuality = this.SessionQuality,
                SpeakerQuality = this.SpeakerQuality,
                Comment = this.Comment
            };
        }
    }
}
