using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace FeedbackExtractor.ContentUnderstanding.Configurations
{
    /// <summary>
    /// Represents the configuration for the Content Understanding feedback extractor.
    /// </summary>
    internal class ContentUnderstandingClientConfiguration
    {
        const string ConfigRootName = "ContentUnderstanding";

        /// <summary>
        /// Gets or sets the API key for the Content Understanding service.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the endpoint URL for the Content Understanding service.
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the analyzer name to use for content analysis.
        /// </summary>
        public string AnalyzerName { get; set; }

        /// <summary>
        /// Gets or sets the API version to use. Defaults to "2025-05-01-preview".
        /// </summary>
        public string ApiVersion { get; set; }

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
        /// Loads the Content Understanding feedback extractor configuration from the specified configuration.
        /// </summary>
        /// <param name="config">The configuration object.</param>
        /// <returns>The loaded Content Understanding feedback extractor configuration.</returns>
        public static ContentUnderstandingClientConfiguration Load(IConfiguration config)
        {
            var retVal = new ContentUnderstandingClientConfiguration()
            {
                Key = config[$"{ConfigRootName}:Key"],
                Endpoint = config[$"{ConfigRootName}:Endpoint"],
                AnalyzerName = config[$"{ConfigRootName}:AnalyzerName"],
                TenantId = config[$"{ConfigRootName}:TenantId"],
                ClientId = config[$"{ConfigRootName}:ClientId"],
                ClientSecret = config[$"{ConfigRootName}:ClientSecret"],
                ApiVersion = config[$"{ConfigRootName}:ApiVersion"] ?? "2025-05-01-preview"
            };

            if (config[$"{ConfigRootName}:ApiVersion"] != null)
            {
                retVal.ApiVersion = config[$"{ConfigRootName}:ApiVersion"];
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
