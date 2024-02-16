namespace FeedbackExtractor.OpenAI.Entities
{
    /// <summary>
    /// Represents the response from the vision API.
    /// </summary>
    public class VisionResponse
    {
        /// <summary>
        /// Gets or sets the list of choices.
        /// </summary>
        public List<Choice> choices { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the response was created.
        /// </summary>
        public int created { get; set; }

        /// <summary>
        /// Gets or sets the ID of the response.
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// Gets or sets the model used for the response.
        /// </summary>
        public string model { get; set; }

        /// <summary>
        /// Gets or sets the object associated with the response.
        /// </summary>
        public string @object { get; set; }

        /// <summary>
        /// Gets or sets the list of prompt filter results.
        /// </summary>
        public List<PromptFilterResult> prompt_filter_results { get; set; }
    }

    /// <summary>
    /// Represents a choice in the vision response.
    /// </summary>
    public class Choice
    {
        /// <summary>
        /// Gets or sets the content filter results for the choice.
        /// </summary>
        public ContentFilterResults content_filter_results { get; set; }

        /// <summary>
        /// Gets or sets the reason for finishing the choice.
        /// </summary>
        public string finish_reason { get; set; }

        /// <summary>
        /// Gets or sets the index of the choice.
        /// </summary>
        public int index { get; set; }

        /// <summary>
        /// Gets or sets the message associated with the choice.
        /// </summary>
        public Message message { get; set; }
    }

    /// <summary>
    /// Represents the content filter results.
    /// </summary>
    public class ContentFilterResults
    {
        /// <summary>
        /// Gets or sets the hate filter result.
        /// </summary>
        public Hate hate { get; set; }

        /// <summary>
        /// Gets or sets the self-harm filter result.
        /// </summary>
        public SelfHarm self_harm { get; set; }

        /// <summary>
        /// Gets or sets the sexual content filter result.
        /// </summary>
        public Sexual sexual { get; set; }

        /// <summary>
        /// Gets or sets the violence filter result.
        /// </summary>
        public Violence violence { get; set; }
    }

    /// <summary>
    /// Represents the hate filter result.
    /// </summary>
    public class Hate
    {
        /// <summary>
        /// Gets or sets a value indicating whether the content is filtered.
        /// </summary>
        public bool filtered { get; set; }

        /// <summary>
        /// Gets or sets the severity of the hate content.
        /// </summary>
        public string severity { get; set; }
    }

    /// <summary>
    /// Represents a message in the vision response.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Gets or sets the content of the message.
        /// </summary>
        public string content { get; set; }

        /// <summary>
        /// Gets or sets the role of the message.
        /// </summary>
        public string role { get; set; }
    }

    /// <summary>
    /// Represents the prompt filter result.
    /// </summary>
    public class PromptFilterResult
    {
        /// <summary>
        /// Gets or sets the index of the prompt.
        /// </summary>
        public int prompt_index { get; set; }

        /// <summary>
        /// Gets or sets the content filter results for the prompt.
        /// </summary>
        public ContentFilterResults content_filter_results { get; set; }
    }

    /// <summary>
    /// Represents the self-harm filter result.
    /// </summary>
    public class SelfHarm
    {
        /// <summary>
        /// Gets or sets a value indicating whether the content is filtered.
        /// </summary>
        public bool filtered { get; set; }

        /// <summary>
        /// Gets or sets the severity of the self-harm content.
        /// </summary>
        public string severity { get; set; }
    }

    /// <summary>
    /// Represents the sexual content filter result.
    /// </summary>
    public class Sexual
    {
        /// <summary>
        /// Gets or sets a value indicating whether the content is filtered.
        /// </summary>
        public bool filtered { get; set; }

        /// <summary>
        /// Gets or sets the severity of the sexual content.
        /// </summary>
        public string severity { get; set; }
    }

    /// <summary>
    /// Represents the violence filter result.
    /// </summary>
    public class Violence
    {
        /// <summary>
        /// Gets or sets a value indicating whether the content is filtered.
        /// </summary>
        public bool filtered { get; set; }

        /// <summary>
        /// Gets or sets the severity of the violence content.
        /// </summary>
        public string severity { get; set; }
    }


}
