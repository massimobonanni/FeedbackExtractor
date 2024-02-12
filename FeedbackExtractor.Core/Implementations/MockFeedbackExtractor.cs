using FeedbackExtractor.Core.Entities;
using FeedbackExtractor.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackExtractor.Core.Implementations
{
    public class MockFeedbackExtractor : IFeedbackExtractor
    {
        public Task<SessionFeedback> ExtractSessionFeedbackAsync(Stream sourceDocument, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new SessionFeedback()
            {
                EventName = "Test Name",
                SessionCode = "test code",
                Comment = "test feedback",
                EventQuality = 5,
                SessionQuality = 5,
                SpeakerQuality = 5
            });
        }
    }
}
