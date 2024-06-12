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

        public async Task<CoreEntities.SessionFeedback> ExtractSessionFeedbackAsync(Stream sourceDocument, CancellationToken cancellationToken = default)
        {
            var imageBytes = await BinaryData.FromStreamAsync(sourceDocument);

            var options = new OpenAIClientOptions()
            {
                 Endpoint = new Uri(this.config.Endpoint)
            };

            var chatClient = new ChatClient( this.config.ModelName, new ApiKeyCredential(this.config.Key), options);

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
                    ChatMessageContentPart.CreateImageMessageContentPart(imageBytes,"image/jpeg",ImageChatMessageContentPartDetail.Auto),
                    ChatMessageContentPart.CreateTextMessageContentPart(Prompts.User)
                })
            };



            try
            {
                var response = await chatClient.CompleteChatAsync(messages, chatOptions);
            }
            catch (Exception ex) 
            {

                throw;
            }

            return null;
        }

        /// <summary>
        /// Extracts session feedback asynchronously.
        /// </summary>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The extracted session feedback.</returns>
        public async Task<CoreEntities.SessionFeedback> ExtractSessionFeedback2Async(Stream sourceDocument, CancellationToken cancellationToken = default)
        {
            var encodedImage = sourceDocument.ToBase64String();

            httpClient.DefaultRequestHeaders.Add("api-key", this.config.Key);

            var payload = OpenAIVisionUtility.GeneratedPayloadForFeedbackForm(encodedImage, this.config.ImageDetail);

            try
            {
                var response = await httpClient.PostAsync(this.config.FullUrl,
                        new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    var responsePayload = await response.Content.ReadAsStringAsync();

                    var visionResponse = JsonConvert.DeserializeObject<VisionResponse>(responsePayload);

                    this.logger.LogInformation($"Usage data: totalTokens={visionResponse.usage.total_tokens}; promptTokens={visionResponse.usage.prompt_tokens}; completionTokens={visionResponse.usage.completion_tokens}");

                    if (visionResponse.choices.Any())
                    {
                        var visionContent = visionResponse.choices[0].message?.content;
                        if (!visionContent.StartsWith("{"))
                        {
                            visionContent = $"{{{visionContent}";
                        }
                        var responseData = JsonConvert.DeserializeObject<OpenAiEntities.SessionFeedback>(visionContent);
                        return responseData.ToFeedbackSession();
                    }
                    else
                    {
                        logger.LogError($"Failed to extract feedback from the document. No choices returned");
                        return null;
                    }
                }
                else
                {
                    logger.LogError($"Failed to extract feedback from the document. Status code: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Exception during call OpenAI service");
                throw;
            }
        }
    }
}
