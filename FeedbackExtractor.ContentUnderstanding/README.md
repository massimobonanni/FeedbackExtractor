# FeedbackExtractor.ContentUnderstanding

## Description

`FeedbackExtractor.ContentUnderstanding` is a .NET 8 class library that integrates with **Azure AI Content Understanding** to extract session feedback data from document images or files.

The extraction workflow is:

1. The source document stream is uploaded to **Azure Blob Storage** as a temporary blob.
2. A time-limited SAS URL is generated for the uploaded blob.
3. The SAS URL is submitted to the **Azure AI Content Understanding** analyzer via its REST API.
4. The library polls the API until the analysis completes.
5. The analysis result is mapped to a `SessionFeedback` entity (defined in `FeedbackExtractor.Core`).
6. The temporary blob is deleted from storage regardless of the outcome.

The library supports two authentication modes for both the Azure AI Content Understanding service and the Azure Blob Storage account:

- **API key / Storage shared key** – the simpler option, suitable for development and testing.
- **Azure AD (client credentials)** – the recommended option for production environments, using a service principal with tenant ID, client ID, and client secret.

---

## Configuration

All configuration values are read from the `ContentUnderstanding` section of the application configuration (e.g., `appsettings.json` or `appsettings.local.json`).

```json
{
  "ContentUnderstanding": {
    "Endpoint": "<azure-ai-content-understanding-endpoint>",
    "Key": "<api-key-or-storage-account-key>",
    "AnalyzerName": "<analyzer-name>",
    "ApiVersion": "2025-05-01-preview",
    "StorageEndpoint": "<https://youraccount.blob.core.windows.net>",
    "ContainerName": "<blob-container-name>",
    "SasExpiryMinutes": 15,
    "TenantId": "<azure-ad-tenant-id>",
    "ClientId": "<azure-ad-client-id>",
    "ClientSecret": "<azure-ad-client-secret>"
  }
}
```

### Azure AI Content Understanding settings

| Key | Required | Default | Description |
|-----|----------|---------|-------------|
| `Endpoint` | Yes | – | The endpoint URL of the Azure AI Content Understanding resource. |
| `Key` | Conditional | – | The API subscription key. Required when **not** using Azure AD identity. |
| `AnalyzerName` | Yes | – | The name of the Content Understanding analyzer to use for document analysis. |
| `ApiVersion` | No | `2025-05-01-preview` | The REST API version to call. |
| `TenantId` | Conditional | – | Azure AD tenant ID. Required when using identity-based authentication. |
| `ClientId` | Conditional | – | Azure AD client (application) ID. Required when using identity-based authentication. |
| `ClientSecret` | Conditional | – | Azure AD client secret. Required when using identity-based authentication. |

### Azure Blob Storage settings

| Key | Required | Default | Description |
|-----|----------|---------|-------------|
| `StorageEndpoint` | Yes | – | The blob service endpoint URI of the Azure Storage account (e.g., `https://youraccount.blob.core.windows.net`). |
| `ContainerName` | Yes | – | The name of the blob container used to temporarily store documents during analysis. |
| `Key` | Conditional | – | The storage account access key. Required when **not** using Azure AD identity. |
| `SasExpiryMinutes` | No | `15` | Duration in minutes for which the generated SAS URL remains valid. |
| `TenantId` | Conditional | – | Azure AD tenant ID. Required when using identity-based authentication. |
| `ClientId` | Conditional | – | Azure AD client (application) ID. Required when using identity-based authentication. |
| `ClientSecret` | Conditional | – | Azure AD client secret. Required when using identity-based authentication. |

> **Authentication mode:** When all three of `TenantId`, `ClientId`, and `ClientSecret` are provided, the library automatically switches to Azure AD (client credentials) authentication for both the Content Understanding service and the Blob Storage account. Otherwise, it falls back to key-based authentication.

---

## Main Classes

### `ContentUnderstandingFeedbackExtractor`

**Namespace:** `FeedbackExtractor.ContentUnderstanding.Implementations`  
**Visibility:** `public`  
**Implements:** `IFeedbackExtractor` (from `FeedbackExtractor.Core`)

The main entry point of the library. Implements the `ExtractSessionFeedbackAsync` method, which orchestrates the full extraction pipeline by delegating to `IContentUnderstandingClient` and mapping the raw analysis result to a `SessionFeedback` instance.

Extracted fields:

| API field name | `SessionFeedback` property |
|----------------|---------------------------|
| `EventName` | `EventName` |
| `SessionCode` | `SessionCode` |
| `EventQuality` | `EventQuality` |
| `SessionQuality` | `SessionQuality` |
| `SpeakerQuality` | `SpeakerQuality` |
| `Comment` | `Comment` |

---

### `ContentUnderstandingClient`

**Namespace:** `FeedbackExtractor.ContentUnderstanding.Implementations`  
**Visibility:** `internal`  
**Implements:** `IContentUnderstandingClient`

Handles all communication with the Azure AI Content Understanding REST API. Its responsibilities are:

- Configuring the `HttpClient` with the correct authentication headers (bearer token or `Ocp-Apim-Subscription-Key`).
- Uploading the source document to Azure Blob Storage via `IBlobStorageClient` and obtaining a SAS URL.
- Submitting the SAS URL to the Content Understanding analyzer endpoint (`POST .../analyzers/{analyzerName}:analyze`).
- Polling the `Operation-Location` URL every 500 ms until the operation status is `Succeeded`, `Failed`, or `Cancelled`.
- Ensuring the temporary blob is deleted after the operation completes (success or failure).

---

### `BlobStorageClient`

**Namespace:** `FeedbackExtractor.ContentUnderstanding.Implementations`  
**Visibility:** `internal`  
**Implements:** `IBlobStorageClient`

Manages the lifecycle of temporary blobs used during document analysis. Its responsibilities are:

- **Uploading** a document stream as a new blob with a GUID-based name.
- **Generating a read-only SAS URL** for the uploaded blob, valid for the configured duration. When using Azure AD identity, it creates a user-delegation SAS; otherwise, it creates a service-level SAS with the shared storage key.
- **Deleting** the blob after it is no longer needed.

---

### `ServiceCollectionExtensions`

**Namespace:** `FeedbackExtractor.ContentUnderstanding.Extensions`  
**Visibility:** `public` (static)

Provides the `AddContentUnderstandingClient` extension method for `IServiceCollection`. Call this method in your DI registration code to register all required services:

```csharp
services.AddContentUnderstandingClient();
```

This registers:

- `IBlobStorageClient` → `BlobStorageClient` (singleton)
- `IContentUnderstandingClient` → `ContentUnderstandingClient` (singleton)

> **Note:** `ContentUnderstandingFeedbackExtractor` implements `IFeedbackExtractor` from `FeedbackExtractor.Core` and must be registered separately depending on the application's extractor selection strategy.

---

## Interfaces

| Interface | Visibility | Description |
|-----------|------------|-------------|
| `IContentUnderstandingClient` | `public` | Contract for submitting a document stream for analysis and retrieving the `AnalyzeResponse`. |
| `IBlobStorageClient` | `internal` | Contract for uploading a document, generating a SAS URL, and deleting a blob from Azure Blob Storage. |
