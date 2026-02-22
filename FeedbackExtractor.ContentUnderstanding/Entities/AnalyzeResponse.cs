using System.Text.Json.Serialization;

namespace FeedbackExtractor.ContentUnderstanding.Entities
{
    /// <summary>
    /// Represents the response from the Content Understanding analyze operation.
    /// </summary>
    internal class AnalyzeResponse
    {
        /// <summary>
        /// Gets or sets the operation ID.
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the status of the operation.
        /// </summary>
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        /// <summary>
        /// Gets or sets the result of the analysis.
        /// </summary>
        [JsonPropertyName("result")]
        public AnalyzeResultContent? Result { get; set; }
    }

    /// <summary>
    /// Represents the result content from the Content Understanding analyze operation.
    /// </summary>
    internal class AnalyzeResultContent
    {
        /// <summary>
        /// Gets or sets the analyzer ID used.
        /// </summary>
        [JsonPropertyName("analyzerId")]
        public string? AnalyzerId { get; set; }

        /// <summary>
        /// Gets or sets the API version used.
        /// </summary>
        [JsonPropertyName("apiVersion")]
        public string? ApiVersion { get; set; }

        /// <summary>
        /// Gets or sets the contents of the analysis.
        /// </summary>
        [JsonPropertyName("contents")]
        public List<AnalyzeContent>? Contents { get; set; }
    }

    /// <summary>
    /// Represents a single content item in the analysis result.
    /// </summary>
    internal class AnalyzeContent
    {
        /// <summary>
        /// Gets or sets the fields extracted from the content.
        /// </summary>
        [JsonPropertyName("fields")]
        public Dictionary<string, AnalyzeField>? Fields { get; set; }

        /// <summary>
        /// Gets or sets the markdown representation of the content.
        /// </summary>
        [JsonPropertyName("markdown")]
        public string? Markdown { get; set; }
    }

    /// <summary>
    /// Represents a field extracted from the analyzed content.
    /// </summary>
    internal class AnalyzeField
    {
        /// <summary>
        /// Gets or sets the type of the field.
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the value of the field as a string.
        /// </summary>
        [JsonPropertyName("valueString")]
        public string? ValueString { get; set; }

        /// <summary>
        /// Gets or sets the value of the field as an integer.
        /// </summary>
        [JsonPropertyName("valueInteger")]
        public int? ValueInteger { get; set; }
    }
}
