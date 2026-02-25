using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using FeedbackExtractor.ContentUnderstanding.Configurations;
using FeedbackExtractor.ContentUnderstanding.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FeedbackExtractor.ContentUnderstanding.Implementations
{
    /// <summary>
    /// Client for interacting with Azure Blob Storage.
    /// </summary>
    internal class BlobStorageClient : IBlobStorageClient
    {
        private readonly BlobStorageClientConfiguration config;
        private readonly ILogger<BlobStorageClient> logger;
        private BlobContainerClient containerClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobStorageClient"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        public BlobStorageClient(IConfiguration configuration,
            ILogger<BlobStorageClient> logger)
        {
            this.config = BlobStorageClientConfiguration.Load(configuration);
            this.logger = logger;
            CreateBlobContainerClient();
        }

        private void CreateBlobContainerClient()
        {
            var containerUri = new Uri($"{this.config.Endpoint.ToString().TrimEnd('/')}/{this.config.ContainerName}");

            if (this.config.UseIdentity())
            {
                var credential = new Azure.Identity.ClientSecretCredential(this.config.TenantId, this.config.ClientId, this.config.ClientSecret);
                this.containerClient = new BlobContainerClient(containerUri, credential);
            }
            else
            {
                var credential = new StorageSharedKeyCredential(new BlobUriBuilder(this.config.Endpoint).AccountName, this.config.Key);
                this.containerClient = new BlobContainerClient(containerUri, credential);
            }
        }

        /// <inheritdoc />
        public async Task<(string BlobName, string SasUrl)> UploadAndGetSasUrlAsync(Stream sourceDocument, CancellationToken cancellationToken = default)
        {
            var blobName = $"{Guid.NewGuid()}.bin";
            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(sourceDocument, overwrite: true, cancellationToken);

            this.logger.LogInformation("Uploaded blob {BlobName} to container {ContainerName}", blobName, this.config.ContainerName);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = this.config.ContainerName,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(this.config.SasExpiryMinutes)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasUrl = blobClient.GenerateSasUri(sasBuilder).ToString();

            return (blobName, sasUrl);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(string blobName, CancellationToken cancellationToken = default)
        {
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);

            this.logger.LogInformation("Deleted blob {BlobName} from container {ContainerName}", blobName, this.config.ContainerName);
        }
    }
}
