using Azure;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace FeedbackExtractor.DocumentIntelligence.Configurations
{
    internal class DocumentFeedbackExtractorConfiguration
    {
        const string ConfigRootName = "DocumentIntelligenceDocument";
        public string Key { get; set; }
        public string Endpoint { get; set; }

        public float MinimumConfidence { get; set; } = 0.75f;

        public static DocumentFeedbackExtractorConfiguration Load(IConfiguration config)
        {
            var retVal = new DocumentFeedbackExtractorConfiguration();
            retVal.Key = config[$"{ConfigRootName}:Key"];
            retVal.Endpoint = config[$"{ConfigRootName}:Endpoint"];
            if (config[$"{ConfigRootName}:MinConfidence"] != null)
            { 
                if (float.TryParse(config[$"{ConfigRootName}:MinConfidence"], 
                    NumberStyles.Number|NumberStyles.Float, CultureInfo.InvariantCulture, out float minConfidence))
                {
                    retVal.MinimumConfidence = minConfidence;
                }
            }
            return retVal;
        }

    }
}
