using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace FeedbackExtractor.DocumentIntelligence.Configurations
{

    /// <summary>
    /// Represents the configuration for the custom feedback extractor.
    /// </summary>
    internal class CustomFeedbackExtractorConfiguration
    {
        const string ConfigRootName = "DocumentIntelligenceCustom";

        /// <summary>
        /// Gets or sets the API key for the custom feedback extractor.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the endpoint URL for the custom feedback extractor.
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the model name for the custom feedback extractor.
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// Gets or sets the minimum confidence level for the custom feedback extractor.
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
        /// Loads the custom feedback extractor configuration from the specified configuration.
        /// </summary>
        /// <param name="config">The configuration object.</param>
        /// <returns>The loaded custom feedback extractor configuration.</returns>
        public static CustomFeedbackExtractorConfiguration Load(IConfiguration config)
        {
            var retVal = new CustomFeedbackExtractorConfiguration
            {
                Key = config[$"{ConfigRootName}:Key"],
                Endpoint = config[$"{ConfigRootName}:Endpoint"],
                ModelName = config[$"{ConfigRootName}:ModelName"],
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
