using FeedbackExtractor.Core.Entities;
using FeedbackExtractor.Core.Interfaces;
using FeedbackExtractor.DocumentIntelligence.Implementations;
using FeedbackExtractor.OpenAI.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackExtractor.Orchestration.Implementations
{
    public class OrchestratorFeedbackExtractor : IFeedbackExtractor
    {
        private readonly IReadOnlyCollection<IFeedbackExtractor> _feedbackExtractors;
        private readonly ILogger<OrchestratorFeedbackExtractor> _logger;
        private readonly HttpClient _httpClient;

        public OrchestratorFeedbackExtractor(IConfiguration configuration, ILoggerFactory loggerFactory,
            HttpClient httpClient)
        {
            _logger = loggerFactory.CreateLogger<OrchestratorFeedbackExtractor>();
            _httpClient = httpClient;

            _feedbackExtractors = new List<IFeedbackExtractor>
                {
                    new CustomFeedbackExtractor(configuration, loggerFactory.CreateLogger<CustomFeedbackExtractor>()),
                    new OpenAIFeedbackExtractor(configuration, loggerFactory.CreateLogger<OpenAIFeedbackExtractor>(), httpClient)
                };
        }

        public async Task<SessionFeedback> ExtractSessionFeedbackAsync(Stream sourceDocument, CancellationToken cancellationToken = default)
        {
            var response = new SessionFeedback();
            foreach (var extractor in _feedbackExtractors)
            {
                _logger.LogInformation($"Extracting session feedback using {extractor.GetType().Name}");
                var stepResponse = await extractor.ExtractSessionFeedbackAsync(sourceDocument, cancellationToken);
                response = MergeFeedbacks(response, stepResponse);
                if (response.IsValid())
                {
                    break;
                }
                sourceDocument.Position = 0;
            }
            return response;
        }

        private SessionFeedback MergeFeedbacks(SessionFeedback firstResponse, SessionFeedback secondResponse)
        {
            var mergedResponse = new SessionFeedback
            {
                EventName = firstResponse?.EventName ?? secondResponse?.EventName,
                SessionCode = firstResponse?.SessionCode ?? secondResponse?.SessionCode,
                Comment = firstResponse?.Comment ?? secondResponse?.Comment,
                EventQuality = firstResponse?.EventQuality ?? secondResponse?.EventQuality,
                SessionQuality = firstResponse?.SessionQuality ?? secondResponse?.SessionQuality,
                SpeakerQuality = firstResponse?.SpeakerQuality ?? secondResponse?.SpeakerQuality
            };

            return mergedResponse;
        }
    }
}
