using FeedbackExtractor.Core.Entities;

namespace FeedbackExtractor.Core.Interfaces
{
    /// <summary>
    /// Represents an interface for extracting session feedback from a source document.
    /// </summary>
    public interface IFeedbackExtractor
    {
        /// <summary>
        /// Extracts session feedback asynchronously from the specified source document.
        /// </summary>
        /// <param name="sourceDocument">The source document containing the session feedback.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the extracted session feedback.</returns>
        Task<SessionFeedback> ExtractSessionFeedbackAsync(Stream sourceDocument, CancellationToken cancellationToken = default);
    }
}
