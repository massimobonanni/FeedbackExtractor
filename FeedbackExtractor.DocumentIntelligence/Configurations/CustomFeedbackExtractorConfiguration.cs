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
        /// Loads the custom feedback extractor configuration from the specified configuration.
        /// </summary>
        /// <param name="config">The configuration object.</param>
        /// <returns>The loaded custom feedback extractor configuration.</returns>
        public static CustomFeedbackExtractorConfiguration Load(IConfiguration config)
        {
            var retVal = new CustomFeedbackExtractorConfiguration();
            retVal.Key = config[$"{ConfigRootName}:Key"];
            retVal.Endpoint = config[$"{ConfigRootName}:Endpoint"];
            retVal.ModelName = config[$"{ConfigRootName}:ModelName"];
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
