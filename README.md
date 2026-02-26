# AI-powered data extraction in Azure: Content Understanding vs Document Intelligence!

This repository contains the demo for the session **"AI-powered data extraction in Azure: Content Understanding vs Document Intelligence!"**. It explores two powerful Azure services for AI-driven document processing — **Azure AI Document Intelligence** and **Azure AI Content Understanding** — comparing their capabilities, customization options, and architectural trade-offs through a practical, real-world scenario to help you choose the right service for your workload.

---

## Solution Structure

The solution is organized into focused projects, each with a specific responsibility. The diagram below illustrates the dependencies:

```
FeedbackExtractor.Client (WPF UI)
├── FeedbackExtractor.Core
├── FeedbackExtractor.DocumentIntelligence
├── FeedbackExtractor.ContentUnderstanding
├── FeedbackExtractor.OpenAI
└── FeedbackExtractor.Orchestration
         ├── FeedbackExtractor.DocumentIntelligence
         └── FeedbackExtractor.OpenAI

Tests/FeedbackExtractor.Core.Test
└── FeedbackExtractor.Core
```

### [FeedbackExtractor.Core](FeedbackExtractor.Core/README.md)

The shared contracts library. Defines the `IFeedbackExtractor` interface, the `SessionFeedback` canonical entity returned by all extractors, and a `MockFeedbackExtractor` test double. Every other project in the solution depends on this library.

### [FeedbackExtractor.DocumentIntelligence](FeedbackExtractor.DocumentIntelligence/README.md)

Integration library for **Azure AI Document Intelligence**. Provides two extraction strategies:
- `CustomFeedbackExtractor` — uses a **custom-trained** Document Intelligence model for structured forms with labelled fields.
- `DocumentFeedbackExtractor` — uses the **prebuilt-layout** model for general-purpose key-value pair and table detection.

Both strategies support API key and Azure AD (client credentials) authentication.

### [FeedbackExtractor.ContentUnderstanding](FeedbackExtractor.ContentUnderstanding/README.md)

Integration library for **Azure AI Content Understanding**. Uploads the source document to Azure Blob Storage, generates a SAS URL, submits it to the Content Understanding REST API, polls for completion, and maps the result to a `SessionFeedback`.

Supports API key and Azure AD (client credentials) authentication for both the AI service and the Blob Storage account.

### [FeedbackExtractor.OpenAI](FeedbackExtractor.OpenAI/README.md)

Integration library for **Azure OpenAI**. Sends the document image to a vision-capable chat model (e.g., GPT-4o) with a structured extraction prompt, then deserializes the JSON response into a `SessionFeedback`. Temperature is fixed at `0.0` for deterministic output.

### [FeedbackExtractor.Orchestration](FeedbackExtractor.Orchestration)

Orchestration library that chains multiple extractors together. `OrchestratorFeedbackExtractor` runs extractors sequentially (Document Intelligence custom model first, then Azure OpenAI as fallback), merging partial results at each step and stopping as soon as a valid `SessionFeedback` is produced.

### [FeedbackExtractor.Client](FeedbackExtractor.Client/README.md)

A **WPF desktop application** (MVVM, .NET 8) that provides the graphical interface for the demo. The user selects an extractor implementation from a drop-down list, opens a source document, triggers the extraction, and sees the resulting `SessionFeedback` fields displayed in the UI.

Supported extractors: `OpenAI`, `DocumentIntelligence_Base`, `DocumentIntelligence_Custom`, `ContentUnderstanding`, `Mixed_Models`, `Mock`.

### [Tests/FeedbackExtractor.Core.Test](Tests/FeedbackExtractor.Core.Test)

Unit test project for the `FeedbackExtractor.Core` library. Covers entity validation and shared contract behaviour.

---