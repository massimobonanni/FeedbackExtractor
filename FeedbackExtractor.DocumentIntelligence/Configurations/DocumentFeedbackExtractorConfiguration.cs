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
        /// Loads the document feedback extractor configuration from the specified configuration.
        /// </summary>
        /// <param name="config">The configuration object.</param>
        /// <returns>The loaded document feedback extractor configuration.</returns>
        public static DocumentFeedbackExtractorConfiguration Load(IConfiguration config)
        {
            var retVal = new DocumentFeedbackExtractorConfiguration();
            retVal.Key = config[$"{ConfigRootName}:Key"];
            retVal.Endpoint = config[$"{ConfigRootName}:Endpoint"];
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
    }
}
