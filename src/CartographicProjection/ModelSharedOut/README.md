# ModelSharedOut

ModelSharedOut generates the shared client model for consumers of the CartographicProjection microservice. It merges OpenAPI JSON documents and produces:

- `Service/wwwroot/json-schema/CartographicProjectionMergedModel.json` (merged OpenAPI served by SwaggerUI)
- `ModelSharedOut/CartographicProjectionMergedModel.cs` (C# DTOs + API client used by external clients like WebApp and ServiceTest)

Namespace: `NORCE.Drilling.CartographicProjection.ModelShared`.

## Purpose

- Publish a stable OpenAPI bundle for this microservice and its relevant dependencies.
- Generate strongly‑typed C# client + DTOs for downstream consumers (WebApp, ServiceTest, other services).
- Follow the distributed shared model approach so clients do not take runtime dependencies on the service implementation.

## How It Fits In

- Service: Hosts the merged OpenAPI document under `wwwroot/json-schema` so it’s visible in SwaggerUI.
- WebApp / ServiceTest: Reference the generated C# file to call the service using a typed client.
- Model: Does not use ModelSharedOut directly; it depends on `ModelSharedIn` for inbound dependencies.

## Dependencies

Defined in `ModelSharedOut/ModelSharedOut.csproj:1`:
- `Microsoft.OpenApi.Readers` — Parses OpenAPI documents.
- `NSwag.CodeGeneration.CSharp` — Generates C# client and DTOs (System.Text.Json).

## Inputs & Outputs

- Inputs: `ModelSharedOut/json-schemas/*.json`
  - Typically includes the service’s OpenAPI (`CartographicProjectionFullName.json`) and any extra schemas you want to expose to clients.
- Outputs:
  - `Service/wwwroot/json-schema/CartographicProjectionMergedModel.json` (merged and normalized OpenAPI 3.0.3)
  - `ModelSharedOut/CartographicProjectionMergedModel.cs` (generated C# client and DTOs)

## Generate The Shared Model

From the solution root:

```bash
# Build and run the generator
dotnet run --project ModelSharedOut/ModelSharedOut.csproj
```

What happens:
- All JSONs in `ModelSharedOut/json-schemas/` are merged.
- Schema IDs are normalized to short names; `$ref` links are updated.
- Merged OpenAPI is saved into `Service/wwwroot/json-schema/CartographicProjectionMergedModel.json`.
- A typed client and DTOs are generated to `ModelSharedOut/CartographicProjectionMergedModel.cs` (namespace `...ModelShared`).

Notes:
- The tool searches up to the solution root, then computes paths to `ModelSharedOut` and `Service/wwwroot/json-schema`.
- OpenAPI version is downgraded to 3.0.3 for Swagger tooling compatibility.

## Usage Examples

- Using the generated client in a consumer app:

```csharp
using NORCE.Drilling.CartographicProjection.ModelShared; // generated types

using var http = new HttpClient();
var client = new Client("https://dev.digiwells.no/CartographicProjection/api/", http);

// Example call (available operations depend on the service OpenAPI)
var ids = await client.GetAllGeodeticConversionSetIdAsync();
```

- Using generated DTOs (examples depend on schemas included):

```csharp
var meta = new MetaInfo { /* set properties */ };
var req = new GeodeticConversionSet { MetaInfo = meta };
await client.PostGeodeticConversionSetAsync(req);
```

## Updating Schemas

- Replace or add JSON files under `ModelSharedOut/json-schemas/` (e.g., refresh `CartographicProjectionFullName.json`).
- Re-run the generator to refresh both the merged OpenAPI and generated C# client.
- Commit updates alongside service changes for consistent client integration.

## Implementation Details

- Entry point: `ModelSharedOut/Program.cs:1`
  - Reads JSONs, merges paths and schemas, normalizes schema IDs via `OpenApiSchemaReferenceUpdater`.
  - Writes merged OpenAPI into the Service’s public folder and generates the C# client using NSwag with System.Text.Json.
- Schema utilities: `ModelSharedOut/OpenApiSchemaReferenceUpdater.cs:1`
  - Deep‑clones schemas and updates `$ref` recursively to match normalized IDs.

