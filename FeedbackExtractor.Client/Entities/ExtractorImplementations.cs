using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackExtractor.Client.Entities
{
    public enum ExtractorImplementations
    {
        Mock,
        DocumentIntelligence_Base,
        DocumentIntelligence_Custom,
        OpenAI
    }
}
