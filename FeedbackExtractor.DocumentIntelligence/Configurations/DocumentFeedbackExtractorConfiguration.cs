using Azure;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace FeedbackExtractor.DocumentIntelligence.Configurations
{
    /// <summary>
    /// Represents the configuration for the document feedback extractor.
    /// </summary>
    internal class DocumentFeedbackExtractorConfiguration
    {
        const string ConfigRootName = "DocumentIntelligenceDocument";

        /// <summary>
        /// Gets or sets the API key for the document feedback extractor.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the endpoint for the document feedback extractor.
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the minimum confidence level for extracting feedback from the document.
        /// </summary>
        public float MinimumConfidence { get; set; } = 0.75f;

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
        /// Loads the document feedback extractor configuration from the specified configuration.
        /// </summary>
        /// <param name="config">The configuration object.</param>
        /// <returns>The loaded document feedback extractor configuration.</returns>
        public static DocumentFeedbackExtractorConfiguration Load(IConfiguration config)
        {
            var retVal = new DocumentFeedbackExtractorConfiguration()
            {
                Key = config[$"{ConfigRootName}:Key"],
                Endpoint = config[$"{ConfigRootName}:Endpoint"],
                TenantId = config[$"{ConfigRootName}:TenantId"],
                ClientId = config[$"{ConfigRootName}:ClientId"],
                ClientSecret = config[$"{ConfigRootName}:ClientSecret"]
            };

            if (config[$"{ConfigRootName}:MinConfidence"] != null)
            {
                if (float.TryParse(config[$"{ConfigRootName}:MinConfidence"],
                    NumberStyles.Number | NumberStyles.Float, CultureInfo.InvariantCulture, out float minConfidence))
                {
                    retVal.MinimumConfidence = minConfidence;
                }
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
