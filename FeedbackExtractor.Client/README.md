# FeedbackExtractor.Client

## Description

`FeedbackExtractor.Client` is a **WPF desktop application** targeting .NET 8 that provides a graphical user interface for extracting session feedback from document images or PDF files.

The application allows the user to:

1. Select an **extractor implementation** from a drop-down list.
2. Open a **source document** (image or PDF) from the local file system.
3. Trigger the extraction and display the resulting `SessionFeedback` fields in the UI.

The application is built following the **MVVM pattern** using [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/). Dependency Injection is configured at startup via `Microsoft.Extensions.DependencyInjection`, with all extractor implementations registered as **keyed singletons** against the `IFeedbackExtractor` interface.

### Supported extractor implementations

| `ExtractorImplementations` value | Underlying implementation | Accepted file types |
|----------------------------------|---------------------------|---------------------|
| `OpenAI` | `OpenAIFeedbackExtractor` | JPG, JPEG, PNG |
| `DocumentIntelligence_Base` | `DocumentFeedbackExtractor` (prebuilt-layout) | PDF, JPG, JPEG, PNG |
| `DocumentIntelligence_Custom` | `CustomFeedbackExtractor` | PDF, JPG, JPEG, PNG |
| `ContentUnderstanding` | `ContentUnderstandingFeedbackExtractor` | PDF, JPG, JPEG, PNG |
| `Mixed_Models` | `OrchestratorFeedbackExtractor` | JPG, JPEG, PNG |
| `Mock` | `MockFeedbackExtractor` | Any |

---

## Configuration

The application loads configuration from two JSON files at startup, in the following order:

1. `appsettings.local.json` *(required — contains environment-specific secrets)*
2. `appsettings.json` *(required — contains shared defaults)*

Values in `appsettings.local.json` override those in `appsettings.json`. The `appsettings.local.json` file is excluded from source control via `.gitignore` and must be created manually for each environment.

The full configuration template for `appsettings.local.json` is shown below. Each section maps directly to the corresponding extractor library's configuration. Only the sections for the extractors you intend to use need to be populated.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "DocumentIntelligenceDocument": {
    "Key": "<api-key>",
    "Endpoint": "<https://your-resource.cognitiveservices.azure.com/>",
    "MinConfidence": "0.5",
    "TenantId": "<azure-ad-tenant-id>",
    "ClientId": "<azure-ad-client-id>",
    "ClientSecret": "<azure-ad-client-secret>"
  },
  "DocumentIntelligenceCustom": {
    "Key": "<api-key>",
    "Endpoint": "<https://your-resource.cognitiveservices.azure.com/>",
    "ModelName": "<custom-model-name>",
    "MinConfidence": "0.5",
    "TenantId": "<azure-ad-tenant-id>",
    "ClientId": "<azure-ad-client-id>",
    "ClientSecret": "<azure-ad-client-secret>"
  },
  "OpenAI": {
    "Key": "<azure-openai-api-key>",
    "Endpoint": "<https://your-resource.openai.azure.com/>",
    "ModelName": "<deployment-name>",
    "ImageDetail": "high",
    "TenantId": "<azure-ad-tenant-id>",
    "ClientId": "<azure-ad-client-id>",
    "ClientSecret": "<azure-ad-client-secret>"
  },
  "ContentUnderstanding": {
    "Key": "<api-key>",
    "Endpoint": "<https://your-resource.services.ai.azure.com/>",
    "AnalyzerName": "<analyzer-name>",
    "ApiVersion": "2025-05-01-preview",
    "StorageEndpoint": "<https://youraccount.blob.core.windows.net/>",
    "StorageKey": "<storage-account-key>",
    "ContainerName": "<blob-container-name>",
    "SasExpiryMinutes": 15,
    "TenantId": "<azure-ad-tenant-id>",
    "ClientId": "<azure-ad-client-id>",
    "ClientSecret": "<azure-ad-client-secret>"
  }
}
```

For a detailed description of the individual configuration keys in each section, refer to the README of the corresponding library:

- [`FeedbackExtractor.DocumentIntelligence`](../FeedbackExtractor.DocumentIntelligence/README.md) — sections `DocumentIntelligenceDocument` and `DocumentIntelligenceCustom`
- [`FeedbackExtractor.OpenAI`](../FeedbackExtractor.OpenAI/README.md) — section `OpenAI`
- [`FeedbackExtractor.ContentUnderstanding`](../FeedbackExtractor.ContentUnderstanding/README.md) — section `ContentUnderstanding`

> **Security note:** Never commit `appsettings.local.json` to source control. It is listed in `.gitignore` for this reason.

---

## Main Classes

### `App`

**File:** `App.xaml.cs`  
**Namespace:** `FeedbackExtractor.Client`

Application entry point. Overrides `OnStartup` to:

1. Build the `IConfiguration` by loading `appsettings.local.json` and then `appsettings.json`.
2. Configure the IoC container (`Ioc.Default`) by registering all services:
   - All `IFeedbackExtractor` implementations as **keyed singletons**, keyed by `ExtractorImplementations` enum values.
   - `IMessenger` (using `WeakReferenceMessenger.Default`).
   - `MainWindowViewModel` as transient.
   - `HttpClient` factory and logging.

---

### `MainWindowViewModel`

**Namespace:** `FeedbackExtractor.Client.ViewModels`  
**Visibility:** `public`  
**Base class:** `ObservableRecipient` (CommunityToolkit.Mvvm)

The sole ViewModel of the application. Exposes the following observable properties:

| Property | Type | Description |
|----------|------|-------------|
| `ExtractionTypes` | `ObservableCollection<ExtractorImplementations>` | The list of available extractor implementations shown in the UI. |
| `SelectedExtractionType` | `ExtractorImplementations?` | The extractor selected by the user. Enables/disables the `SelectFileCommand`. |
| `DocumentFilePath` | `string?` | The full path of the document selected by the user. Enables/disables the `ExtractSessionFeedbackCommand`. |
| `ExtractedSessionFeedback` | `SessionFeedback?` | The feedback result returned by the extractor, bound to the result panel. |
| `IsExtractedSessionValid` | `bool` | `true` when the extracted feedback passes `SessionFeedback.IsValid()`. |
| `IsBusy` | `bool` | `true` while extraction is in progress. Disables both commands. |

And the following commands:

| Command | Description |
|---------|-------------|
| `SelectFileCommand` | Opens a file picker dialog (via `OpenFileDialogMessage`) filtered to the file types accepted by the selected extractor. Disabled while busy or when no extractor is selected. |
| `ExtractSessionFeedbackCommand` | Resolves the keyed `IFeedbackExtractor` from DI, opens the selected file, calls `ExtractSessionFeedbackAsync`, and updates `ExtractedSessionFeedback` and `IsExtractedSessionValid`. Disabled while busy or when no file is selected. |

Messaging:

- **Sends** `OpenFileDialogMessage` to request the View to open a file dialog.
- **Receives** `FileSelectedMessage` to update `DocumentFilePath` when the user selects a file.

---

### `ExtractorImplementations`

**Namespace:** `FeedbackExtractor.Client.Entities`  
**Visibility:** `public` (enum)

Enumerates all available extractor implementations. Used as the DI key when registering and resolving `IFeedbackExtractor` instances, and as the item type in the extractor selection drop-down.

| Value | Description |
|-------|-------------|
| `Mock` | Returns hardcoded data; no Azure service required. |
| `DocumentIntelligence_Base` | Uses the Azure AI Document Intelligence prebuilt-layout model. |
| `DocumentIntelligence_Custom` | Uses a custom-trained Azure AI Document Intelligence model. |
| `OpenAI` | Uses an Azure OpenAI vision model. |
| `Mixed_Models` | Uses the orchestrator that combines multiple models. |
| `ContentUnderstanding` | Uses Azure AI Content Understanding. |

---

### `OpenFileDialogMessage`

**Namespace:** `FeedbackExtractor.Client.Messages`  
**Visibility:** `public`

A plain message object sent from the ViewModel to the View to request that a file-open dialog be displayed. Carries a `Filter` string (e.g., `"Images (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"`) that controls which file types are visible in the dialog.

---

### `FileSelectedMessage`

**Namespace:** `FeedbackExtractor.Client.Messages`  
**Visibility:** `public`  
**Base class:** `ValueChangedMessage<string>` (CommunityToolkit.Mvvm)

A message sent from the View back to the ViewModel after the user selects a file in the dialog. Carries the full file path as its value.

---

### Value Converters

All converters are located in the `FeedbackExtractor.Client.Converters` namespace and implement `IValueConverter` for use in XAML bindings.

| Converter | Conversion | Typical use |
|-----------|------------|-------------|
| `BoolToOppositeBoolConverter` | `bool` → `!bool` | Inverting `IsBusy` to enable/disable controls. |
| `NotNullToBoolConverter` | `object?` → `bool` (`null` = `false`) | Enabling a button only when a result is present. |
| `NotNullToVisibilityConverter` | `object?` → `Visibility` (`null` = `Hidden`) | Showing result panels only after extraction. |
| `VisibilityToOppositeBoolConverter` | `bool` → `Visibility` (`true` = `Hidden`) | Hiding elements when a condition is true. |
