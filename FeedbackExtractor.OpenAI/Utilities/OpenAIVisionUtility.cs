using FeedbackExtractor.OpenAI.Entities;

namespace FeedbackExtractor.OpenAI.Utilities
{

    ///<summary>
    /// Utility class for generating payload for feedback form in OpenAI Vision.
    /// </summary>
    internal static class OpenAIVisionUtility
    {
        /// <summary>
        /// Generates the payload for the feedback form.
        /// </summary>
        /// <param name="encodedImage">The encoded image to be included in the payload.</param>
        /// <returns>The generated payload for the feedback form.</returns>
        public static object GeneratedPayloadForFeedbackForm(string? encodedImage,
            ImageDetailParameter imageDetail = ImageDetailParameter.Auto)
        {
            var detailString = imageDetail switch
            {
                ImageDetailParameter.Auto => "auto",
                ImageDetailParameter.Low => "low",
                ImageDetailParameter.High => "high",
                _ => throw new ArgumentException("Invalid image detail parameter", nameof(imageDetail))
            };

            return new
            {
                messages = new object[]
                {
                          new {
                              role = "system",
                              content = new object[] {
                                  new {
                                      type = "text",
                                      text = Prompts.System
                                  }
                              }
                          },
                          new {
                              role = "user",
                              content = new object[] {
                                  new {
                                      type = "image_url",
                                      image_url = new {
                                          url = $"data:image/jpeg;base64,{encodedImage}",
                                          detail = detailString
                                      }
                                  },
                                  new {
                                      type = "text",
                                      text = Prompts.User
                                  }
                              }
                          }
                },
                temperature = 0.0,
                max_tokens = 800,
                stream = false
            };
        }
    }
}
