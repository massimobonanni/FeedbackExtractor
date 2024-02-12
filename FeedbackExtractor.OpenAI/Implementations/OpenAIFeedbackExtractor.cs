using FeedbackExtractor.Core.Entities;
using FeedbackExtractor.Core.Interfaces;
using FeedbackExtractor.OpenAI.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FeedbackExtractor.OpenAI.Implementations
{
    public class OpenAIFeedbackExtractor : IFeedbackExtractor
    {
        private readonly OpenAIFeedbackExtractorConfiguration config;
        private readonly ILogger<OpenAIFeedbackExtractor> logger;

        public OpenAIFeedbackExtractor(IConfiguration configuration, ILogger<OpenAIFeedbackExtractor> logger)
        {
            this.config = OpenAIFeedbackExtractorConfiguration.Load(configuration);
            this.logger = logger;
        }

        public Task<SessionFeedback> ExtractSessionFeedbackAsync(Stream sourceDocument, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
