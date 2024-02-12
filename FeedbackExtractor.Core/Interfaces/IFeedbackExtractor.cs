using FeedbackExtractor.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackExtractor.Core.Interfaces
{
    public interface IFeedbackExtractor
    {
        Task<SessionFeedback> ExtractSessionFeedbackAsync(Stream sourceDocument, CancellationToken cancellationToken = default);
    }
}
