# Service

The Service project hosts the CartographicProjection HTTP API. It exposes endpoints to manage projections and conversion sets and serves an OpenAPI document and Scalar API reference. Data persist in a local SQLite database under `home/CartographicProjection.db`.

## Purpose

- Expose REST endpoints for `CartographicProjection`, `CartographicConversionSet`, and `CartographicProjectionType`.
- Serve a merged OpenAPI schema at `CartographicProjection/api/swagger` and static assets from `wwwroot`.
- Persist data in SQLite; maintain and periodically clean the database.

## Install & Run

- Prerequisites: .NET SDK 8.0, Docker (optional), internet access to dependency services if configured.

- Local run (dev):
  - Build and run: `dotnet run --project Service/Service.csproj`
  - Scalar API reference: `http://localhost:8080/CartographicProjection/api/swagger`

- Configuration:
  - `GeodeticDatumHostURL`: base URL to the GeodeticDatum service (used by `APIUtils`).
    - Dev default: `Service/appsettings.Development.json:1`
    - Prod default: `Service/appsettings.Production.json:1`
  - Override via environment variable `GeodeticDatumHostURL` or in `appsettings.*.json`.

- Docker:
  - Build: `docker build -t norcedrillingcartographicprojectionservice -f Service/Dockerfile .`
  - Run: `docker run -p 8080:8080 -e GeodeticDatumHostURL=https://dev.digiwells.no/ -v %CD%/home:/home norcedrillingcartographicprojectionservice`
    - On Linux/macOS, replace `%CD%` with `$(pwd)`.
    - Volume maps the SQLite DB, optional external configuration, and generated MCP hub instance id to `home/` in your workspace.
    - The Docker image reads optional service configuration from `/home/CartographicProjection.Service.json`. Override this path with `CARTOGRAPHICPROJECTION_EXTERNAL_CONFIG` if needed.

External configuration example:

```json
{
  "McpHub": {
    "Enabled": true,
    "HubBaseUrl": "https://mcp-hub.example.com/api",
    "RegistrationEndpoint": "McpMicroservice",
    "RetryIntervalSeconds": 60,
    "PublicBaseUrl": "https://dev.digiwells.no",
    "ServiceName": "CartographicProjection",
    "InstanceId": "",
    "UnregisterOnShutdown": true
  }
}
```

## MCP Server

The service exposes a Model Context Protocol endpoint alongside the REST API:

- Streamable HTTP transport: `/CartographicProjection/api/mcp`
- WebSocket transport: `/CartographicProjection/api/mcp/ws`

The MCP tool surface mirrors the REST API:

- `ping`
- CartographicProjection: `cartographic_projection_get_all_ids`, `cartographic_projection_get_all_meta_info`, `cartographic_projection_get_by_id`, `cartographic_projection_get_all_light`, `cartographic_projection_get_all`, `cartographic_projection_create`, `cartographic_projection_update_by_id`, `cartographic_projection_delete_by_id`
- CartographicConversionSet: `cartographic_conversion_set_get_all_ids`, `cartographic_conversion_set_get_all_meta_info`, `cartographic_conversion_set_get_by_id`, `cartographic_conversion_set_get_all_light`, `cartographic_conversion_set_get_all`, `cartographic_conversion_set_create`, `cartographic_conversion_set_update_by_id`, `cartographic_conversion_set_delete_by_id`
- CartographicProjectionType: `cartographic_projection_type_get_all_ids`, `cartographic_projection_type_get_by_id`, `cartographic_projection_type_get_all`
- Usage statistics: `cartographic_projection_usage_statistics_get`

The `create` and `update_by_id` tools expect the same JSON object body as the corresponding REST endpoints, wrapped in a `cartographicProjection` or `cartographicConversionSet` argument. Every REST-backed tool publishes an explicit JSON input schema describing identifiers, metadata, projection algorithms, applicable parameter units, coordinate representations, datums, and full-update identity matching.

Coordinate conversion uses a persistent-case workflow:

1. Generate a UUID and call `cartographic_conversion_set_create` with that UUID in `cartographicConversionSet.MetaInfo.ID`, an existing `CartographicProjectionID`, and one or more coordinate inputs.
2. After creation succeeds, call `cartographic_conversion_set_get_by_id` with the same UUID to retrieve calculated projected, datum, WGS84, octree, and grid-convergence values.
3. When the results have been consumed and the case is only temporary, call `cartographic_conversion_set_delete_by_id` with the same UUID.

The selected CartographicProjection supplies its associated GeodeticDatum. Linear coordinates and depths are meters; angular coordinates and grid convergence are radians. For each coordinate item, provide a complete northing/easting/vertical-depth triplet, a complete geodetic triplet in the projection datum or WGS84, or a valid octree representation.

When `McpHub:Enabled` is true, the service registers itself on the configured MCP hub with a fixed service type id, a configured or persisted instance id, and MCP endpoint URLs derived from `PublicBaseUrl`:

- `PublicBaseUrl + "/CartographicProjection/api/mcp"`
- `PublicBaseUrl` converted to `ws`/`wss` plus `"/CartographicProjection/api/mcp/ws"`

If `HubBaseUrl` or `PublicBaseUrl` is missing, registration is skipped. If the hub is configured but unreachable, registration is retried every `RetryIntervalSeconds` seconds. On graceful shutdown, the service attempts to unregister its instance when `UnregisterOnShutdown` is true.

## Usage Examples

- Base path: `http://localhost:8080/CartographicProjection/api`
- Scalar API reference: `/swagger`

- CartographicProjectionType (enumeration and prototypes):
  - `GET /CartographicProjectionType` → list of supported `ProjectionType` values
  - `GET /CartographicProjectionType/{id}` → prototype for a given type
  - `GET /CartographicProjectionType/HeavyData` → full list with flags

- CartographicProjection (CRUD):
  - `GET /CartographicProjection` → list of IDs
  - `GET /CartographicProjection/MetaInfo` → list of `MetaInfo`
  - `GET /CartographicProjection/{id}` → single item
  - `GET /CartographicProjection/LightData` / `HeavyData` → lightweight/heavy lists
  - `POST /CartographicProjection` → create
  - `PUT /CartographicProjection/{id}` → update
  - `DELETE /CartographicProjection/{id}` → delete

- CartographicConversionSet (CRUD + compute on add/update):
  - `GET /CartographicConversionSet` / `MetaInfo` / `{id}` / `LightData` / `HeavyData`
  - `POST /CartographicConversionSet` → add and compute
  - `PUT /CartographicConversionSet/{id}` → recompute and update
  - `DELETE /CartographicConversionSet/{id}`

- Minimal request payloads (schemas in `Model`):

```json
// CartographicProjection (minimal skeleton)
{
  "MetaInfo": { "ID": "c0ffee00-0000-0000-0000-000000000001" },
  "Name": "UTM 32N",
  "ProjectionType": "UTM",
  "Zone": 32,
  "IsSouth": false
}
```

```json
// CartographicConversionSet (minimal skeleton)
{
  "MetaInfo": { "ID": "c0ffee00-0000-0000-0000-000000000002" },
  "Name": "Demo conversion",
  "CartographicProjectionID": "c0ffee00-0000-0000-0000-000000000001",
  "CartographicCoordinateList": [
    { "Northing": 6740000.0, "Easting": 300000.0, "VerticalDepth": 0.0 }
  ]
}
```

Consult Scalar API reference for the authoritative schema and examples.

## Dependencies

- Project: references `Model` (`Service/Service.csproj:1`) for conversion logic and data types.
- Packages: `Microsoft.Data.Sqlite`, `Microsoft.OpenApi`, `Microsoft.AspNetCore.OpenApi`/`Scalar`.
- Build-time: `Microsoft.Extensions.ApiDescription.Server` exports OpenAPI during Debug builds.

## Integration

- Model: Implements conversion algorithms (DotSpatial) and data types consumed by controllers.
- ModelSharedIn: Provides generated DTOs used inside `Model` (e.g., `GeodeticDatum`/`Spheroid`).
- ModelSharedOut: Generated from Service OpenAPI; places merged OpenAPI at `Service/wwwroot/json-schema/CartographicProjectionMergedModel.json` and produces a C# client for consumers.
- WebApp: Calls Service endpoints using the `ModelSharedOut` client; does not reference `Model` directly.

## Operations & Maintenance

- Database: stored in `home/CartographicProjection.db` (auto‑created). `DatabaseCleanerService` removes old records daily.
- Reverse proxy: `Program.cs` handles `X-Forwarded-Host` to generate correct Swagger `servers` URLs.

## Deployed Endpoints

- Swagger (dev): https://dev.digiwells.no/CartographicProjection/api/swagger
- Swagger (prod): https://app.digiwells.no/CartographicProjection/api/swagger
- Example API (dev): https://dev.digiwells.no/CartographicProjection/api/CartographicConversionSet
- Example API (prod): https://app.digiwells.no/CartographicProjection/api/CartographicConversionSet

## Source & Template

- Generated from NORCE Drilling and Well Modelling .NET template.
- Template repo: https://github.com/NORCE-DrillingAndWells/Templates
- Template docs: https://github.com/NORCE-DrillingAndWells/DrillingAndWells/wiki/.NET-Templates

## Funding

The current work has been funded by the Research Council of Norway and Industry partners in the SFI Digiwells (2020–2028) centre for research‑based innovation.

## Contributors

- Eric Cayeux — NORCE Energy Modelling and Automation
- Gilles Pelfrene — NORCE Energy Modelling and Automation
- Andrew Holsaeter — NORCE Energy Modelling and Automation
- Lucas Volpi — NORCE Energy Modelling and Automation

