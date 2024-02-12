using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackExtractor.Client.Messages
{
    public sealed class FileSelectedMessage : ValueChangedMessage<string>
    {
        public FileSelectedMessage(string filename):base(filename)
        {
        }
    }
}
