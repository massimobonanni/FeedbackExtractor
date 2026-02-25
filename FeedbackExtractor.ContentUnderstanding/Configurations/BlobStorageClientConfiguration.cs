using Microsoft.Extensions.Configuration;

namespace FeedbackExtractor.ContentUnderstanding.Configurations
{
    /// <summary>
    /// Represents the configuration for the Blob Storage client.
    /// </summary>
    internal class BlobStorageClientConfiguration
    {
        const string ConfigRootName = "ContentUnderstanding";

        /// <summary>
        /// Gets or sets the storage endpoint for the Blob Storage account.
        /// </summary>
        public Uri Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the name of the blob container to use.
        /// </summary>
        public string ContainerName { get; set; }

        /// <summary>
        /// Gets or sets the key for the Blob Storage account.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the Azure AD tenant ID for authentication.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the Azure AD client ID for authentication.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the Azure AD client secret for authentication.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the SAS token expiry duration in minutes. Defaults to 15 minutes.
        /// </summary>
        public int SasExpiryMinutes { get; set; }

        /// <summary>
        /// Loads the Blob Storage client configuration from the specified configuration.
        /// </summary>
        /// <param name="config">The configuration object.</param>
        /// <returns>The loaded Blob Storage client configuration.</returns>
        public static BlobStorageClientConfiguration Load(IConfiguration config)
        {
            var retVal = new BlobStorageClientConfiguration()
            {
                Endpoint = new Uri(config[$"{ConfigRootName}:StorageEndpoint"]),
                ContainerName = config[$"{ConfigRootName}:ContainerName"],
                Key = config[$"{ConfigRootName}:Key"],
                TenantId = config[$"{ConfigRootName}:TenantId"],
                ClientId = config[$"{ConfigRootName}:ClientId"],
                ClientSecret = config[$"{ConfigRootName}:ClientSecret"],
                SasExpiryMinutes = 15
            };

            if (int.TryParse(config[$"{ConfigRootName}:SasExpiryMinutes"], out var expiryMinutes))
            {
                retVal.SasExpiryMinutes = expiryMinutes;
            }

            return retVal;
        }

        /// <summary>
        /// Determines whether Azure AD identity-based authentication should be used.
        /// </summary>
        /// <returns>
        /// <c>true</c> if all Azure AD authentication properties (TenantId, ClientId, and ClientSecret) are configured; otherwise, <c>false</c>.
        /// </returns>
        public bool UseIdentity()
        {
            return !string.IsNullOrWhiteSpace(this.TenantId) && !string.IsNullOrWhiteSpace(this.ClientId) && !string.IsNullOrWhiteSpace(this.ClientSecret);
        }
    }
}
