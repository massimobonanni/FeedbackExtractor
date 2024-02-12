using FeedbackExtractor.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackExtractor.OpenAI.Entities
{
    public class SessionFeedback
    {
        public string EventName { get; set; }
        public string SessionCode { get; set; }
        public int EventQuality { get; set; }
        public int SessionQuality { get; set; }
        public int SpeakerQuality { get; set; }
        public string Comment { get; set; }

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
