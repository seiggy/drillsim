# Service

Stateless ASP.NET Core host for the Earth Magnetic Field REST and MCP APIs.

- `GET /EarthMagneticField/api/EarthMagneticField`: discovery and installed-model information.
- `POST /EarthMagneticField/api/EarthMagneticField/Evaluate`: atomic synchronous WMM2025/IGRF14 batch evaluation.
- `GET /EarthMagneticField/api/EarthMagneticField/ModelInfo`: full model provenance.
- `GET /EarthMagneticField/api/EarthMagneticFieldUsageStatistics`: process-replica REST counters.
- `/EarthMagneticField/api/mcp`, `/health/*`, `/metrics`, `/swagger`, and `/json-schema/EarthMagneticFieldMergedModel.json`.

Configuration section `EarthMagneticField` supports `MaximumSamplesPerRequest` (default 10000) and optional `ModelDirectory`. MCP exposes only `ping`, `earth_magnetic_field_get_model_info`, and `earth_magnetic_field_evaluate`; usage statistics are excluded.

Author: Eric Cayeux

Company: NORCE Research
