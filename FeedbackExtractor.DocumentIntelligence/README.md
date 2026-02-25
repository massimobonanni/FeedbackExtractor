# FeedbackExtractor.DocumentIntelligence

## Description

`FeedbackExtractor.DocumentIntelligence` is a .NET 8 class library that integrates with **Azure AI Document Intelligence** to extract session feedback data from document images or files.

The library provides two independent extraction strategies, each implementing `IFeedbackExtractor` from `FeedbackExtractor.Core`:

| Extractor | Model used | Best suited for |
|-----------|-----------|-----------------|
| `CustomFeedbackExtractor` | A **custom-trained** Document Intelligence model | Structured feedback forms with labelled fields (trained in Document Intelligence Studio) |
| `DocumentFeedbackExtractor` | The **prebuilt-layout** model | General-purpose layout analysis using key-value pairs and table detection |

Both extractors support two authentication modes for the Azure AI Document Intelligence service:

- **API key** – the simpler option, suitable for development and testing.
- **Azure AD (client credentials)** – the recommended option for production environments, using a service principal with tenant ID, client ID, and client secret.

---

## Configuration

Each extractor reads its settings from a **dedicated section** of the application configuration (e.g., `appsettings.json` or `appsettings.local.json`).

### `CustomFeedbackExtractor` — section `DocumentIntelligenceCustom`

```json
{
  "DocumentIntelligenceCustom": {
    "Endpoint": "<azure-document-intelligence-endpoint>",
    "Key": "<api-key>",
    "ModelName": "<custom-model-name>",
    "MinConfidence": 0.75,
    "TenantId": "<azure-ad-tenant-id>",
    "ClientId": "<azure-ad-client-id>",
    "ClientSecret": "<azure-ad-client-secret>"
  }
}
```

| Key | Required | Default | Description |
|-----|----------|---------|-------------|
| `Endpoint` | Yes | – | The endpoint URL of the Azure AI Document Intelligence resource. |
| `Key` | Conditional | – | The API key. Required when **not** using Azure AD identity. |
| `ModelName` | Yes | – | The name of the custom-trained Document Intelligence model to use for analysis. |
| `MinConfidence` | No | `0.75` | Minimum confidence threshold (0.0–1.0) for accepting an extracted field value. Fields below this threshold are discarded. |
| `TenantId` | Conditional | – | Azure AD tenant ID. Required when using identity-based authentication. |
| `ClientId` | Conditional | – | Azure AD client (application) ID. Required when using identity-based authentication. |
| `ClientSecret` | Conditional | – | Azure AD client secret. Required when using identity-based authentication. |

### `DocumentFeedbackExtractor` — section `DocumentIntelligenceDocument`

```json
{
  "DocumentIntelligenceDocument": {
    "Endpoint": "<azure-document-intelligence-endpoint>",
    "Key": "<api-key>",
    "MinConfidence": 0.75,
    "TenantId": "<azure-ad-tenant-id>",
    "ClientId": "<azure-ad-client-id>",
    "ClientSecret": "<azure-ad-client-secret>"
  }
}
```

| Key | Required | Default | Description |
|-----|----------|---------|-------------|
| `Endpoint` | Yes | – | The endpoint URL of the Azure AI Document Intelligence resource. |
| `Key` | Conditional | – | The API key. Required when **not** using Azure AD identity. |
| `MinConfidence` | No | `0.75` | Minimum confidence threshold (0.0–1.0) for accepting key-value pairs extracted from the document. |
| `TenantId` | Conditional | – | Azure AD tenant ID. Required when using identity-based authentication. |
| `ClientId` | Conditional | – | Azure AD client (application) ID. Required when using identity-based authentication. |
| `ClientSecret` | Conditional | – | Azure AD client secret. Required when using identity-based authentication. |

> **Authentication mode:** When all three of `TenantId`, `ClientId`, and `ClientSecret` are provided, the library automatically switches to Azure AD (client credentials) authentication. Otherwise, it falls back to key-based authentication.

---

## Main Classes

### `CustomFeedbackExtractor`

**Namespace:** `FeedbackExtractor.DocumentIntelligence.Implementations`  
**Visibility:** `public`  
**Implements:** `IFeedbackExtractor` (from `FeedbackExtractor.Core`)  
**Configuration section:** `DocumentIntelligenceCustom`

Extracts session feedback using a **custom-trained** Document Intelligence model. The document is submitted for analysis via `DocumentIntelligenceClient.AnalyzeDocumentAsync`, and the operation is polled every 125 ms until completion. The first `AnalyzedDocument` in the result is mapped to a `SessionFeedback` instance using `AnalyzedDocumentExtensions.ToSessionFeedback`.

Expected labelled fields in the custom model:

| Field name | Type | `SessionFeedback` property |
|-----------|------|---------------------------|
| `EventName` | String | `EventName` |
| `SessionCode` | String | `SessionCode` |
| `EventQuality1`…`EventQuality5` | Selection mark | `EventQuality` (1–5) |
| `SessionQuality1`…`SessionQuality5` | Selection mark | `SessionQuality` (1–5) |
| `SpeakerQuality1`…`SpeakerQuality5` | Selection mark | `SpeakerQuality` (1–5) |
| `Comment` | String | `Comment` |

---

### `DocumentFeedbackExtractor`

**Namespace:** `FeedbackExtractor.DocumentIntelligence.Implementations`  
**Visibility:** `public`  
**Implements:** `IFeedbackExtractor` (from `FeedbackExtractor.Core`)  
**Configuration section:** `DocumentIntelligenceDocument`

Extracts session feedback using the **prebuilt-layout** model (no custom training required). The document is submitted for layout analysis, and the operation is polled every 125 ms until completion. The result is then mapped to `SessionFeedback` by:

- Reading key-value pairs for `Event Name:`, `Session Code:`, and `Comment:` using `AnalyzeResultExtensions.GetKeyValue`.
- Detecting the checked column in three 2-row × 5-column rating tables (event quality, session quality, speaker quality) using `AnalyzeResultExtensions.GetCheckedColumnFromTableRow`.

---

### `AnalyzedDocumentExtensions`

**Namespace:** `Azure.AI.DocumentIntelligence`  
**Visibility:** `internal` (static)

Extension methods for `AnalyzedDocument`, used by `CustomFeedbackExtractor`:

| Method | Description |
|--------|-------------|
| `ToSessionFeedback(float confidence)` | Converts an `AnalyzedDocument` to a `SessionFeedback` by reading all expected fields. |
| `GetFieldValue(string keyName, float confidence)` | Returns the `Content` of a named field if its confidence meets the threshold; otherwise returns `null`. |
| `GetSelectionmarkValue(string fieldPrefix, float confidence)` | Scans fields `{fieldPrefix}1` through `{fieldPrefix}5` and returns the index (1–5) of the first field whose selection mark is `selected` and whose confidence meets the threshold. |

---

### `AnalyzeResultExtensions`

**Namespace:** `Azure.AI.DocumentIntelligence`  
**Visibility:** `internal` (static)

Extension methods for `AnalyzeResult`, used by `DocumentFeedbackExtractor`:

| Method | Description |
|--------|-------------|
| `GetKeyValue(string keyname, float confidence, bool caseSensitive = false)` | Searches the key-value pairs of an `AnalyzeResult` for a matching key and returns its value when the confidence meets the threshold. |
| `GetCheckedColumnFromTableRow(int tableIndex, int rowIndex)` | Returns the column index of the cell in the specified table row that contains the `:selected:` marker (used for checkbox/radio detection in rating tables). |
