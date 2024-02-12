using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackExtractor.OpenAI.Entities
{
    public class VisionResponse
    {
        public List<Choice> choices { get; set; }
        public int created { get; set; }
        public string id { get; set; }
        public string model { get; set; }
        public string @object { get; set; }
        public List<PromptFilterResult> prompt_filter_results { get; set; }
    }

    public class Choice
    {
        public ContentFilterResults content_filter_results { get; set; }
        public string finish_reason { get; set; }
        public int index { get; set; }
        public Message message { get; set; }
    }

    public class ContentFilterResults
    {
        public Hate hate { get; set; }
        public SelfHarm self_harm { get; set; }
        public Sexual sexual { get; set; }
        public Violence violence { get; set; }
    }

    public class Hate
    {
        public bool filtered { get; set; }
        public string severity { get; set; }
    }

    public class Message
    {
        public string content { get; set; }
        public string role { get; set; }
    }

    public class PromptFilterResult
    {
        public int prompt_index { get; set; }
        public ContentFilterResults content_filter_results { get; set; }
    }

    public class SelfHarm
    {
        public bool filtered { get; set; }
        public string severity { get; set; }
    }

    public class Sexual
    {
        public bool filtered { get; set; }
        public string severity { get; set; }
    }

    public class Violence
    {
        public bool filtered { get; set; }
        public string severity { get; set; }
    }


}
