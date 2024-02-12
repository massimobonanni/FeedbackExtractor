using Azure;
using Microsoft.Extensions.Configuration;

namespace FeedbackExtractor.DocumentIntelligence.Configurations
{
    internal class CustomFeedbackExtractorConfiguration
    {
        const string ConfigRootName = "DocumentIntelligenceCustom";
        public string Key { get; set; }
        public string Endpoint { get; set; }
        public string ModelName { get; set; }

        public static CustomFeedbackExtractorConfiguration Load(IConfiguration config)
        {
            var retVal = new CustomFeedbackExtractorConfiguration();
            retVal.Key = config[$"{ConfigRootName}:Key"];
            retVal.Endpoint = config[$"{ConfigRootName}:Endpoint"];
            retVal.ModelName = config[$"{ConfigRootName}:ModelName"];
            return retVal;
        }

    }
}
