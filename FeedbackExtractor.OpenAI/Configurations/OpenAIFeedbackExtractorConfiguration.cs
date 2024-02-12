﻿using Azure;
using Microsoft.Extensions.Configuration;

namespace FeedbackExtractor.OpenAI.Configurations
{
    internal class OpenAIFeedbackExtractorConfiguration
    {
        const string ConfigRootName = "OpenAI";
        public string Key { get; set; }
        public string Endpoint { get; set; }
        public string ModelName { get; set; }

        public static OpenAIFeedbackExtractorConfiguration Load(IConfiguration config)
        {
            var retVal = new OpenAIFeedbackExtractorConfiguration();
            retVal.Key = config[$"{ConfigRootName}:Key"];
            retVal.Endpoint = config[$"{ConfigRootName}:Endpoint"];
            retVal.ModelName = config[$"{ConfigRootName}:ModelName"];
            return retVal;
        }

    }
}