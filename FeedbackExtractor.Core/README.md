# FeedbackExtractor.Core

## Description

`FeedbackExtractor.Core` is a .NET 8 class library that defines the **shared contracts and entities** used across all FeedbackExtractor projects.

It provides:

- The **`SessionFeedback`** entity, which is the canonical data model for extracted session feedback and is used as the return type by every extractor implementation in the solution.
- The **`IFeedbackExtractor`** interface, which is the common abstraction that all extractor implementations must fulfil, regardless of the underlying AI service used (Azure AI Document Intelligence, Azure AI Content Understanding, Azure OpenAI, etc.).
- The **`MockFeedbackExtractor`** class, a test-only implementation of `IFeedbackExtractor` that returns hardcoded data, useful for local development and unit testing without any Azure service dependency.

All other projects in the solution (`FeedbackExtractor.DocumentIntelligence`, `FeedbackExtractor.ContentUnderstanding`, `FeedbackExtractor.OpenAI`) depend on this library and implement `IFeedbackExtractor`.

---

## Main Classes

### `IFeedbackExtractor`

**Namespace:** `FeedbackExtractor.Core.Interfaces`  
**Visibility:** `public` (interface)

The core abstraction of the solution. Defines a single asynchronous method:

```csharp
Task<SessionFeedback> ExtractSessionFeedbackAsync(Stream sourceDocument, CancellationToken cancellationToken = default);
```

| Parameter | Description |
|-----------|-------------|
| `sourceDocument` | A `Stream` containing the document image or file to analyse. |
| `cancellationToken` | Optional cancellation token for cooperative cancellation. |

Returns a `SessionFeedback` instance populated with the data extracted from the document, or `null` if extraction fails.

---

### `SessionFeedback`

**Namespace:** `FeedbackExtractor.Core.Entities`  
**Visibility:** `public`

The canonical model for session feedback data extracted from a document. All extractor implementations produce a `SessionFeedback` as their output.

| Property | Type | Description |
|----------|------|-------------|
| `EventName` | `string?` | The name of the event. |
| `SessionCode` | `string?` | The identifier code of the session. |
| `EventQuality` | `int?` | The participant's rating of the overall event quality (1–5). |
| `SessionQuality` | `int?` | The participant's rating of the session quality (1–5). |
| `SpeakerQuality` | `int?` | The participant's rating of the speaker quality (1–5). |
| `Comment` | `string?` | A free-text comment left by the participant. |

The class also exposes an `IsValid()` method that returns `true` only when all required fields are present and the three quality ratings are in the valid range (1–5 inclusive):

```csharp
public bool IsValid()
```

---

### `MockFeedbackExtractor`

**Namespace:** `FeedbackExtractor.Core.Implementations`  
**Visibility:** `public`  
**Implements:** `IFeedbackExtractor`

A test double implementation of `IFeedbackExtractor` that immediately returns a fixed, hardcoded `SessionFeedback` without calling any external service. Useful for:

- Local development without Azure credentials.
- Unit and integration tests that need a predictable extractor response.
- UI prototyping and smoke testing.

The returned instance always has all quality ratings set to `5` and passes the `IsValid()` check.
