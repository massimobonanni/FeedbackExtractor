using FeedbackExtractor.Core.Interfaces;
using FeedbackExtractor.OpenAI.Configurations;
using FeedbackExtractor.OpenAI.Entities;
using FeedbackExtractor.OpenAI.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using OpenAiEntities = FeedbackExtractor.OpenAI.Entities;
using CoreEntities = FeedbackExtractor.Core.Entities;
using Azure;
using OpenAI.Chat;
using OpenAI;
using static System.Net.Mime.MediaTypeNames;
using System.ClientModel;
using Azure.Messaging;
using Azure.AI.OpenAI;
using FeedbackExtractor.OpenAI.Extensions;

namespace FeedbackExtractor.OpenAI.Implementations
{
    /// <summary>
    /// Implementation of the IFeedbackExtractor interface using OpenAI.
    /// </summary>
    public class OpenAIFeedbackExtractor : IFeedbackExtractor
    {
        private readonly OpenAIFeedbackExtractorConfiguration config;
        private readonly ILogger<OpenAIFeedbackExtractor> logger;
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAIFeedbackExtractor"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="httpClient">The HTTP client.</param>
        public OpenAIFeedbackExtractor(IConfiguration configuration, ILogger<OpenAIFeedbackExtractor> logger,
            HttpClient httpClient)
        {
            this.config = OpenAIFeedbackExtractorConfiguration.Load(configuration);
            this.logger = logger;
            this.httpClient = httpClient;
        }


        /// <summary>
        /// Extracts session feedback asynchronously.
        /// </summary>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The extracted session feedback.</returns>

        public async Task<CoreEntities.SessionFeedback> ExtractSessionFeedbackAsync(Stream sourceDocument, CancellationToken cancellationToken = default)
        {
            var imageBytes = await BinaryData.FromStreamAsync(sourceDocument);

            AzureOpenAIClient azureClient = new(
                new Uri(this.config.Endpoint),
                new ApiKeyCredential(this.config.Key));
            ChatClient chatClient = azureClient.GetChatClient(this.config.ModelName);

            var chatOptions = new ChatCompletionOptions()
            {
                MaxTokens = 100,
                Temperature = 0.0f,
            };

            var messages = new List<ChatMessage>
            {
                ChatMessage.CreateSystemMessage(Prompts.System),
                ChatMessage.CreateUserMessage(new List<ChatMessageContentPart>()
                {
                    ChatMessageContentPart.CreateImageMessageContentPart(imageBytes,
                        "image/jpeg",this.config.ImageDetail.ToImageChatMessageDetail()),
                    ChatMessageContentPart.CreateTextMessageContentPart(Prompts.User)
                })
            };

            try
            {
                var response = await chatClient.CompleteChatAsync(messages, chatOptions);

                this.logger.LogInformation($"Usage data: totalTokens={response.Value.Usage.TotalTokens}; promptTokens={response.Value.Usage.InputTokens}; completionTokens={response.Value.Usage.OutputTokens}");

                var content = response.Value.Content.FirstOrDefault();
                if (content != null)
                {
                    var responseData = JsonConvert.DeserializeObject<OpenAiEntities.SessionFeedback>(content.Text);
                    if (responseData != null)
                    {
                        return responseData.ToFeedbackSession();
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error extracting session feedback from OpenAI.");
                throw;
            }

            return null;
        }
    }
}
