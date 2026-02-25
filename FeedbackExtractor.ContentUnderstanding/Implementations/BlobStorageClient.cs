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
    /// <remarks>
    /// <para>
    /// This client provides operations to upload documents as blobs, generate time-limited
    /// SAS URLs for reading, and delete blobs after use. It is designed to support the
    /// <see cref="ContentUnderstandingClient"/> workflow, where documents are temporarily
    /// stored in blob storage before being submitted for analysis.
    /// </para>
    /// <para>
    /// Authentication is determined by the <see cref="BlobStorageClientConfiguration"/>:
    /// when Azure AD identity settings (tenant ID, client ID, client secret) are provided,
    /// a <see cref="Azure.Identity.ClientSecretCredential"/> is used and SAS tokens are
    /// generated via user-delegation keys; otherwise, a <see cref="StorageSharedKeyCredential"/>
    /// is used with service-level SAS tokens.
    /// </para>
    /// </remarks>
    internal class BlobStorageClient : IBlobStorageClient
    {
        private readonly BlobStorageClientConfiguration config;
        private readonly ILogger<BlobStorageClient> logger;
        private BlobContainerClient containerClient;
        private BlobServiceClient serviceClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobStorageClient"/> class.
        /// </summary>
        /// <param name="configuration">
        /// The application configuration from which <see cref="BlobStorageClientConfiguration"/>
        /// settings are loaded.
        /// </param>
        /// <param name="logger">The logger used to record upload, delete, and diagnostic events.</param>
        public BlobStorageClient(IConfiguration configuration,
            ILogger<BlobStorageClient> logger)
        {
            this.config = BlobStorageClientConfiguration.Load(configuration);
            this.logger = logger;
            CreateBlobClients();
        }

        /// <summary>
        /// Creates and configures the <see cref="BlobServiceClient"/> and
        /// <see cref="BlobContainerClient"/> used for all blob operations.
        /// </summary>
        /// <remarks>
        /// When <see cref="BlobStorageClientConfiguration.UseIdentity"/> returns <see langword="true"/>,

        /// a <see cref="Azure.Identity.ClientSecretCredential"/> is used to authenticate.
        /// Otherwise, a <see cref="StorageSharedKeyCredential"/> derived from the account name
        /// and access key is used.
        /// </remarks>
        private void CreateBlobClients()
        {
            var containerUri = new Uri($"{this.config.Endpoint.ToString().TrimEnd('/')}/{this.config.ContainerName}");

            if (this.config.UseIdentity())
            {
                var credential = new Azure.Identity.ClientSecretCredential(this.config.TenantId, this.config.ClientId, this.config.ClientSecret);
                this.serviceClient = new BlobServiceClient(this.config.Endpoint, credential);
            }
            else
            {
                var credential = new StorageSharedKeyCredential(new BlobUriBuilder(this.config.Endpoint).AccountName, this.config.Key);
                this.serviceClient = new BlobServiceClient(this.config.Endpoint, credential);
            }
            this.containerClient = this.serviceClient.GetBlobContainerClient(this.config.ContainerName);
        }

        /// <inheritdoc />
        public async Task<(string BlobName, string SasUrl)> UploadAndGetSasUrlAsync(Stream sourceDocument, CancellationToken cancellationToken = default)
        {
            var blobName = $"{Guid.NewGuid()}.bin";
            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(sourceDocument, overwrite: true, cancellationToken);

            this.logger.LogInformation("Uploaded blob {BlobName} to container {ContainerName}", blobName, this.config.ContainerName);

            var sasUrl = await GenerateSasUrlAsync(blobClient, blobName, cancellationToken);

            return (blobName, sasUrl);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(string blobName, CancellationToken cancellationToken = default)
        {
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);

            this.logger.LogInformation("Deleted blob {BlobName} from container {ContainerName}", blobName, this.config.ContainerName);
        }

        /// <summary>
        /// Generates a read-only SAS URL for the specified blob.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The generated SAS token expires after the number of minutes specified by
        /// <see cref="BlobStorageClientConfiguration.SasExpiryMinutes"/>.
        /// </para>
        /// <para>
        /// When Azure AD identity is configured, a user-delegation SAS is created by obtaining a
        /// delegation key from the <see cref="BlobServiceClient"/>. Otherwise, a service SAS is
        /// generated directly from the <see cref="BlobClient"/> using the shared storage key.
        /// </para>
        /// </remarks>
        /// <param name="blobClient">The <see cref="BlobClient"/> referencing the target blob.</param>
        /// <param name="blobName">The name of the blob for which to generate the SAS URL.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A read-only SAS URL string that grants temporary access to the blob.</returns>
        private async Task<string> GenerateSasUrlAsync(BlobClient blobClient, string blobName, CancellationToken cancellationToken)
        {
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = this.config.ContainerName,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(this.config.SasExpiryMinutes)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            if (this.config.UseIdentity())
            {
                // User delegation SAS: required when using RBAC / Microsoft Entra credentials
                var delegationKey = await serviceClient.GetUserDelegationKeyAsync(
                    DateTimeOffset.UtcNow,
                    DateTimeOffset.UtcNow.AddMinutes(this.config.SasExpiryMinutes),
                    cancellationToken);

                var sasToken = sasBuilder
                    .ToSasQueryParameters(delegationKey.Value, serviceClient.AccountName)
                    .ToString();

                return $"{blobClient.Uri}?{sasToken}";
            }
            else
            {
                // Service SAS: signed with the shared key
                return blobClient.GenerateSasUri(sasBuilder).ToString();
            }
        }
    }
}
