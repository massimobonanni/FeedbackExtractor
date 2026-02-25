# FeedbackExtractor.OpenAI

## Description

`FeedbackExtractor.OpenAI` is a .NET 8 class library that integrates with **Azure OpenAI** to extract session feedback data from document images using a **vision-capable chat model** (GPT-4o or similar).

The extraction workflow is:

1. The source document stream is read into a `BinaryData` byte array.
2. A chat completion request is built with a **system prompt** (extraction instructions) and a **user message** containing both the image and a user prompt, both loaded from the embedded `Prompts.resx` resource file.
3. The request is sent to the Azure OpenAI Chat Completions API via the `AzureOpenAIClient` SDK.
4. The model response (a JSON string) is deserialized into an internal `SessionFeedback` entity.
5. The internal entity is mapped to a `FeedbackExtractor.Core.Entities.SessionFeedback` and returned to the caller.

The library uses **API key authentication** against an Azure OpenAI resource. The chat completion is configured with a temperature of `0.0` and a maximum of 100 output tokens to produce deterministic, concise structured responses.

---

## Configuration

All configuration values are read from the `OpenAI` section of the application configuration (e.g., `appsettings.json` or `appsettings.local.json`).

```json
{
  "OpenAI": {
    "Endpoint": "<https://your-resource.openai.azure.com>",
    "Key": "<azure-openai-api-key>",
    "ModelName": "<deployment-name>",
    "ImageDetail": "Auto"
  }
}
```

| Key | Required | Default | Description |
|-----|----------|---------|-------------|
| `Endpoint` | Yes | – | The endpoint URL of the Azure OpenAI resource (e.g., `https://your-resource.openai.azure.com`). |
| `Key` | Yes | – | The Azure OpenAI API key used for authentication. |
| `ModelName` | Yes | – | The name of the Azure OpenAI deployment (model) to use for chat completions. Must be a vision-capable model (e.g., `gpt-4o`). |
| `ImageDetail` | No | `Auto` | The level of detail used when sending the image to the model. Accepted values: `Auto`, `Low`, `High`. |

> **`ImageDetail` values:**
> - `Auto` — the model decides the appropriate detail level based on the image size.
> - `Low` — uses a fixed low-resolution representation (faster, cheaper, fewer tokens).
> - `High` — enables high-resolution analysis, tiling the image for finer detail (slower, more tokens).

---

## Main Classes

### `OpenAIFeedbackExtractor`

**Namespace:** `FeedbackExtractor.OpenAI.Implementations`  
**Visibility:** `public`  
**Implements:** `IFeedbackExtractor` (from `FeedbackExtractor.Core`)

The main entry point of the library. Implements `ExtractSessionFeedbackAsync`, which:

- Converts the input stream to `BinaryData`.
- Builds a `ChatMessage` list with a system message (from `Prompts.System`) and a user message containing the image content part (with the configured `ImageDetail`) and a text content part (from `Prompts.User`).
- Sends the request to Azure OpenAI using `AzureOpenAIClient` and `ChatClient`.
- Logs token usage (`TotalTokens`, `InputTokens`, `OutputTokens`) at the `Information` level.
- Deserializes the model's JSON response into an internal `SessionFeedback` entity and converts it to `FeedbackExtractor.Core.Entities.SessionFeedback`.

---

### `OpenAIFeedbackExtractorConfiguration`

**Namespace:** `FeedbackExtractor.OpenAI.Configurations`  
**Visibility:** `internal`  
**Configuration section:** `OpenAI`

Holds the runtime settings for `OpenAIFeedbackExtractor`. In addition to the raw configuration values, it exposes a computed `FullUrl` property that assembles the full Azure OpenAI Chat Completions URL:

```
{Endpoint}/openai/deployments/{ModelName}/chat/completions?api-version=2024-02-15-preview
```

Loaded via `OpenAIFeedbackExtractorConfiguration.Load(IConfiguration)`.

---

### `SessionFeedback` (OpenAI-specific entity)

**Namespace:** `FeedbackExtractor.OpenAI.Entities`  
**Visibility:** `public`

An internal data transfer object that mirrors the JSON structure returned by the model. Contains the same fields as the core entity (`EventName`, `SessionCode`, `EventQuality`, `SessionQuality`, `SpeakerQuality`, `Comment`) and provides a `ToFeedbackSession()` method to convert to `FeedbackExtractor.Core.Entities.SessionFeedback`.

---

### `ImageDetailParameter`

**Namespace:** `FeedbackExtractor.OpenAI.Entities`  
**Visibility:** `internal` (enum)

Defines the image detail level passed to the vision model:

| Value | Description |
|-------|-------------|
| `Auto` | The model selects the resolution automatically. |
| `Low` | Forces low-resolution image analysis (512×512 tiles, 85 tokens). |
| `High` | Forces high-resolution analysis with detailed tiling. |

---

### `VisionResponse`

**Namespace:** `FeedbackExtractor.OpenAI.Entities`  
**Visibility:** `public`

A legacy response model representing the raw JSON structure of an Azure OpenAI Chat Completions response (choices, messages, usage, content filter results). It is retained for reference but the current implementation uses the strongly-typed `AzureOpenAIClient` SDK instead of raw HTTP deserialization.

---

### `OpenAIVisionUtility`

**Namespace:** `FeedbackExtractor.OpenAI.Utilities`  
**Visibility:** `internal` (static)

A legacy utility class that builds the raw HTTP request payload (anonymous object) for the OpenAI vision API using a base64-encoded image string. It is retained for reference but the current implementation uses the `AzureOpenAIClient` SDK directly.

---

### `ImageDetailParameterExtensions`

**Namespace:** `FeedbackExtractor.OpenAI.Extensions`  
**Visibility:** `internal` (static)

Extension methods for `ImageDetailParameter`:

| Method | Description |
|--------|-------------|
| `ToImageChatMessageDetail()` | Converts an `ImageDetailParameter` enum value to the SDK's `ImageChatMessageContentPartDetail` value required by `ChatMessageContentPart.CreateImageMessageContentPart`. |

---

### `StreamExtensions`

**Namespace:** `System.IO`  
**Visibility:** `internal` (static)

Extension methods for `Stream`:

| Method | Description |
|--------|-------------|
| `ToBase64String()` | Reads the entire stream into a `MemoryStream` and returns its content as a Base64-encoded string. Used to encode image data for raw HTTP payloads. |

---

### `Prompts` (resource file)

**File:** `Prompts.resx` / `Prompts.Designer.cs`

An embedded .NET resource file that stores the prompts sent to the model:

| Resource key | Usage |
|-------------|-------|
| `System` | The system message that instructs the model on how to interpret the feedback form and the expected JSON output format. |
| `User` | The user message text that accompanies the image, requesting extraction of the feedback fields. |
