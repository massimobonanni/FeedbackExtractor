using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackExtractor.Core.Entities
{
    public class SessionFeedback
    {
        public string? EventName { get; set; }
        public string? SessionCode { get; set; }

        public int? EventQuality { get; set; }
        public int? SessionQuality { get; set; }
        public int? SpeakerQuality { get; set; }
        public string? Comment { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(EventName) &&
                   !string.IsNullOrWhiteSpace(SessionCode) &&
                   EventQuality.HasValue && EventQuality > 0 && EventQuality <=5 &&
                   SessionQuality.HasValue && SessionQuality > 0 && SessionQuality <=5 &&
                   SpeakerQuality.HasValue && SpeakerQuality > 0 && SpeakerQuality <=5;
        }
    }
}
