using Azure;
using Microsoft.Extensions.Configuration;

namespace FeedbackExtractor.DocumentIntelligence.Configurations
{
    internal class DocumentFeedbackExtractorConfiguration
    {
        const string ConfigRootName = "DocumentIntelligenceDocument";
        public string Key { get; set; }
        public string Endpoint { get; set; }
   
        public static DocumentFeedbackExtractorConfiguration Load(IConfiguration config)
        {
            var retVal = new DocumentFeedbackExtractorConfiguration();
            retVal.Key = config[$"{ConfigRootName}:Key"];
            retVal.Endpoint = config[$"{ConfigRootName}:Endpoint"];
            return retVal;
        }

    }
}
