using FeedbackExtractor.Core.Entities;
using FeedbackExtractor.Core.Interfaces;

namespace FeedbackExtractor.Core.Implementations
{

    /// <summary>
    /// Represents a mock implementation of the feedback extractor.
    /// </summary>
    public class MockFeedbackExtractor : IFeedbackExtractor
    {
        /// <summary>
        /// Extracts session feedback asynchronously.
        /// </summary>
        /// <param name="sourceDocument">The source document stream.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The extracted session feedback.</returns>
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
