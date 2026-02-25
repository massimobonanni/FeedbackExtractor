using FeedbackExtractor.ContentUnderstanding.Entities;

namespace FeedbackExtractor.ContentUnderstanding.Interfaces
{
    /// <summary>
    /// Represents a client for interacting with the Azure Content Understanding service.
    /// </summary>
    public interface IContentUnderstandingClient
    {
        /// <summary>
        /// Submits a document for analysis and polls for the result.
        /// </summary>
        /// <param name="sourceDocument">The source document stream to analyze.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The analyze response, or <c>null</c> if the analysis failed.</returns>
        Task<AnalyzeResponse?> AnalyzeDocumentAsync(Stream sourceDocument, CancellationToken cancellationToken = default);
    }
}
