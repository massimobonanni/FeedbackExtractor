using Microsoft.Extensions.Configuration;

namespace FeedbackExtractor.OpenAI.Configurations
{
    /// <summary>
    /// Represents the configuration settings for the OpenAI feedback extractor.
    /// </summary>
    internal class OpenAIFeedbackExtractorConfiguration
    {
        const string ConfigRootName = "OpenAI";

        /// <summary>
        /// Gets or sets the API key for accessing the OpenAI service.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the endpoint URL for the OpenAI service.
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the name of the model to be used for chat completions.
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// Gets the full URL for making chat completion requests to the OpenAI service.
        /// </summary>
        public string FullUrl
        {
            get => $"{this.Endpoint}/openai/deployments/{this.ModelName}/chat/completions?api-version=2023-07-01-preview";
        }

        /// <summary>
        /// Loads the OpenAIFeedbackExtractorConfiguration from the provided configuration.
        /// </summary>
        /// <param name="config">The configuration object.</param>
        /// <returns>An instance of OpenAIFeedbackExtractorConfiguration.</returns>
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
