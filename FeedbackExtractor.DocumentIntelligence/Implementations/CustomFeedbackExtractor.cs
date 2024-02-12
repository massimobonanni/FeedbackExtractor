using Azure.AI.DocumentIntelligence;
using Azure;
using FeedbackExtractor.Core.Entities;
using FeedbackExtractor.Core.Interfaces;
using FeedbackExtractor.DocumentIntelligence.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackExtractor.DocumentIntelligence.Implementations
{
    public class CustomFeedbackExtractor : IFeedbackExtractor
    {

        private readonly CustomFeedbackExtractorConfiguration config;
        private readonly ILogger<CustomFeedbackExtractor> logger;

        public CustomFeedbackExtractor(IConfiguration configuration, ILogger<CustomFeedbackExtractor> logger)
        {
            this.config = CustomFeedbackExtractorConfiguration.Load(configuration);
            this.logger = logger;
        }

        public async Task<SessionFeedback> ExtractSessionFeedbackAsync(Stream sourceDocument, CancellationToken cancellationToken = default)
        {
            var session = new SessionFeedback();

            AzureKeyCredential credential = new AzureKeyCredential(this.config.Key);
            DocumentIntelligenceClient client = new DocumentIntelligenceClient(new Uri(this.config.Endpoint), credential);

            var options = new List<DocumentAnalysisFeature>() {
                DocumentAnalysisFeature.OcrHighResolution,
                DocumentAnalysisFeature.Languages
            };

            var request = new AnalyzeDocumentContent();
            request.Base64Source = BinaryData.FromStream(sourceDocument);

            try
            {
                var operation = await client.AnalyzeDocumentAsync(WaitUntil.Started,
                       this.config.ModelName, request, features: options, cancellationToken: cancellationToken);
                do
                {
                    await Task.Delay(125);
                    await operation.UpdateStatusAsync(cancellationToken: cancellationToken);
                } while (!operation.HasCompleted);

                if (operation.HasValue)
                {
                    session=ToSessionFeedback(operation.Value);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error extracting feedback from document");
                session = null;
            }

            return session;
        }

        private SessionFeedback ToSessionFeedback(AnalyzeResult source)
        {
            var session = new SessionFeedback();

            var document = source.Documents.FirstOrDefault();
            if (document == null)
                return session;

            if (document.Fields.ContainsKey("EventName"))
                session.EventName = document.Fields["EventName"]?.Content;
            if (document.Fields.ContainsKey("SessionCode"))
                session.SessionCode = document.Fields["SessionCode"]?.Content;

            session.EventQuality= ExtractSelectionMarkValue("EventQuality", document.Fields);
            session.SessionQuality= ExtractSelectionMarkValue("SessionQuality", document.Fields);
            session.SpeakerQuality= ExtractSelectionMarkValue("SpeakerQuality", document.Fields);

            if (document.Fields.ContainsKey("Comment"))
                session.Comment = document.Fields["Comment"]?.Content;

            return session;
        }

        private int? ExtractSelectionMarkValue(string fieldPrefix, IReadOnlyDictionary<string,DocumentField> fields)
        {
            for (int i = 1; i <= 5; i++)
            {
                var fieldName = $"{fieldPrefix}{i}";
                if (fields.ContainsKey(fieldName) && fields[fieldName]?.ValueSelectionMark == "selected")
                {
                    return i;
                }
            }
            return null;
        }
    }
}
