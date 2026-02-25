namespace FeedbackExtractor.ContentUnderstanding.Interfaces
{
    /// <summary>
    /// Represents a client for interacting with Azure Blob Storage.
    /// </summary>
    internal interface IBlobStorageClient
    {
        /// <summary>
        /// Uploads a document to blob storage, generates a SAS URL for it, and returns the blob name and URL.
        /// </summary>
        /// <param name="sourceDocument">The source document stream to upload.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The blob name and SAS URL for the uploaded document.</returns>
        Task<(string BlobName, string SasUrl)> UploadAndGetSasUrlAsync(Stream sourceDocument, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a blob from storage.
        /// </summary>
        /// <param name="blobName">The name of the blob to delete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task DeleteAsync(string blobName, CancellationToken cancellationToken = default);
    }
}
