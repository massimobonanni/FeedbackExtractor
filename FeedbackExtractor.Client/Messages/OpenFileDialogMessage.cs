using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackExtractor.Client.Messages
{
    public sealed class OpenFileDialogMessage
    {
        public OpenFileDialogMessage(string filter)
        {
            this.Filter = filter;
        }
        public string Filter { get; set; }     
    }
}
