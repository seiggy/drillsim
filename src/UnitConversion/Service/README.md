# UnitConversion microservice

The UnitConversion microservice is packaged as a docker container named:

``osdcunitconversionservice``

It is available on dockerhub, under the digiwells organization, at:

https://hub.docker.com/?namespace=digiwells

The API (OpenApi schema) of the microservice is available and testable at:

https://app.digiwells.no/UnitConversion/api/swagger

The microservice itself is available at:

https://app.digiwells.no/UnitConversion/api/PhysicalQuantity
https://app.digiwells.no/UnitConversion/api/UnitSystem
https://app.digiwells.no/UnitConversion/api/UnitConversionSet
https://app.digiwells.no/UnitConversion/api/UnitSystemConversionSet

## Configuration and Docker volume

The service stores its SQLite database and local runtime files under the shared `home/` location. In Docker this folder is mounted as `/home`.

The Docker image reads an optional external configuration file from:

```
/home/UnitConversion.Service.json
```

This path can be overridden with the `UNITCONVERSION_EXTERNAL_CONFIG` environment variable. Use a shared volume for `/home` when running the container so the database, vector database, optional external configuration, and generated MCP hub instance id survive container restarts.

The Docker image also carries a seed copy of the MCP vector document database at `/app/seed/UnitConversionVectors.db`. At startup, the service copies that seed into `/home/UnitConversionVectors.db` only when the runtime file is missing. This keeps `/home` persistent while avoiding the Kubernetes volume mount hiding the database bundled in the image.

Example:

```bash
docker run -p 8080:8080 -e ASPNETCORE_URLS=http://+:8080 -v %CD%/home:/home osdcunitconversionservice
```

## MCP server

Alongside the REST API, the microservice exposes a [Model Context Protocol](https://modelcontextprotocol.io/) server that allows MCP compatible clients to call domain specific tools over HTTP (or WebSocket) transports. The public entry point is:

```
https://app.digiwells.no/UnitConversion/api/mcp
```

When running the service locally the default endpoint is:

```
http://localhost:5002/UnitConversion/api/mcp
```

### Supported tools

| Tool name | Description |
|-----------|-------------|
| `search_physical_quantities` | Return ranked canonical-name and synonym matches with UUIDs and hierarchy summaries |
| `get_physical_quantity` | Retrieve dimensions, SI precision, units, synonyms, hierarchy, and symbolic conversion definitions by UUID |
| `convert_values` | Pure, non-persisting conversion of one or more values between named or UUID-addressed unit choices |
| `convert_between_unit_systems` | Pure, non-persisting conversion using the choices selected by two unit systems |
| `list_unit_systems` / `get_unit_system` | Paginate unit-system summaries and retrieve one complete mapping |
| `create_unit_system`, `replace_unit_system`, `delete_unit_system` | Persistently manage custom unit systems |
| `search_documentation` | Return ranked MCP resource links and similarity scores for a textual query |

The tool descriptions, input/output schemas, and annotations distinguish three workflows:

- `convert_values` converts an array between two compatible unit choices. UUIDs take precedence over names, and units may be inherited from a compatible parent quantity. Numeric results remain unrounded; display strings use the requested quantity's `MeaningfulPrecisionInSI`.
- `convert_between_unit_systems` converts an array using the choices selected by two unit systems and returns the actual resolved source and target units.
- Unit-system mutation tools persist data and carry MCP read-only, destructive, and idempotency annotations so clients can apply appropriate approval behavior.

The MCP conversion tools calculate directly through the conversion model and do not create temporary database records. Persistent conversion-set workflows remain available through the REST API.

For example, `RateOfPenetrationDrilling` derives from `Velocity`. ROP directly lists only the common drilling units, whereas `Velocity` also defines `furlong per fortnight`. A request for `30 meter per hour` to `furlongs per fortnight` can inherit the target unit from `Velocity`, return the full numeric result, and format it using the ROP meaningful precision. The physical-quantity search/get tools expose this relationship in `hierarchy` metadata.

`search_documentation` expects a nomic-ai/nomic-embed-text compatible endpoint (default `http://localhost:8080/embeddings`). Configure `VectorDocumentSearch:Nomic:*` or the `NOMIC_API_KEY` environment variable if the inference server requires authentication, and ensure the vector database was generated with the same model/dimension pair. Results contain structured ranking data and MCP resource-link content blocks.

### Example request

You can call tools over HTTP by POSTing a JSON-RPC 2.0 request. For example, to convert a value between two unit choices:

```bash
curl -X POST \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -d '{
        "jsonrpc": "2.0",
        "id": 1,
        "method": "tools/call",
        "params": {
          "name": "convert_values",
          "arguments": {
            "physicalQuantity": "Mud Density",
            "unitIn": "kilogram per cubic metre",
            "unitOut": "pound per gallon",
            "values": [1200]
          }
        }
      }' \
  http://localhost:5002/UnitConversion/api/mcp
```

Clients that prefer Server Sent Events (SSE) or WebSockets can switch the transport mode during the MCP handshake—the service accepts both HTTP streaming and WebSocket sessions.

### MCP hub registration

The service can optionally register its MCP endpoint on an MCP hub. Enable this through the external configuration file:

```json
{
  "McpHub": {
    "Enabled": true,
    "HubBaseUrl": "https://mcp-hub.example.com/api",
    "RegistrationEndpoint": "McpMicroservice",
    "RetryIntervalSeconds": 60,
    "PublicBaseUrl": "https://dev.digiwells.no",
    "ServiceName": "UnitConversion",
    "InstanceId": "",
    "UnregisterOnShutdown": true
  }
}
```

When enabled, the service registers a fixed UnitConversion service type id, a configured or persisted instance id, and MCP URLs derived from `PublicBaseUrl`:

- `PublicBaseUrl + "/UnitConversion/api/mcp"`
- `PublicBaseUrl` converted to `ws`/`wss` plus `"/UnitConversion/api/mcp/ws"`

If `HubBaseUrl` or `PublicBaseUrl` is missing, registration is skipped. If the hub is configured but unreachable, registration is retried every `RetryIntervalSeconds` seconds. On graceful shutdown, the service attempts to unregister its instance when `UnregisterOnShutdown` is true.

# Funding

The current work has been funded by the [Research Council of Norway](https://www.forskningsradet.no/) and [Industry partners](https://www.digiwells.no/about/board/) in the framework of the cent for research-based innovation [SFI Digiwells (2020-2028)](https://www.digiwells.no/) focused on Digitalization, Drilling Engineering and GeoSteering. 

# Contributors

**Eric Cayeux**, *NORCE Energy Modelling and Automation*

**Gilles Pelfrene**, *NORCE Energy Modelling and Automation*
