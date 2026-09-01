# Model

The Model project defines the core data model and projection logic used by the CartographicProjection microservice. It provides types to describe cartographic projections, perform forward/inverse transformations between geodetic and cartographic coordinates, and compute related quantities like grid convergence.

- Key files: `Model/CartographicProjection.cs:1`, `Model/CartographicProjectionType.cs:1`, `Model/CartographicConversionSet.cs:1`, `Model/CartographicCoordinate.cs:1`, `Model/Model.csproj:1`

## Purpose

- Encapsulate projection parameters in `CartographicProjection` and map them to PROJ.4 strings.
- Enumerate supported projections via `ProjectionType` and provide per‑projection parameter flags (`CartographicProjectionType`).
- Convert between latitude/longitude (in a specified geodetic datum) and easting/northing (in the specified cartographic projection) using `DotSpatial.Projections`.
- Compute grid convergence at locations and build projection strings from spheroid parameters.

## How It Fits In

- Service: `Service/Service.csproj` references `Model` to expose HTTP endpoints for conversions.
- Shared DTOs: `Model` depends on `ModelSharedIn` (generated client models) for geodetic types such as `GeodeticDatum`, `GeodeticCoordinate` and `Spheroid`.
- Clients: `ModelSharedOut` (generated from the Service OpenAPI) is used by `ServiceTest` and `WebApp` to call the microservice. The WebApp does not reference `Model` directly.
- Tests: `ModelTest` targets `Model` and is the place for unit tests of projection logic.

## Dependencies

- DotSpatial.Projections.NetStandard (projection math)
- OSDC.DotnetLibraries.General.* and Drilling.DrillingProperties (shared utilities and domain types)
- Project reference to `../ModelSharedIn/ModelSharedIn.csproj`

See `Model/Model.csproj:1` for exact package versions.

## Key Types

- `CartographicProjection` (`Model/CartographicProjection.cs:1`)
  - Holds projection parameters (e.g., `ProjectionType`, `Zone`, `IsSouth`, `LongitudeOrigin`, `LatitudeOrigin`, `Scaling`, `FalseEasting`, `FalseNorthing`, etc.).
  - Builds a PROJ.4 string via `GetProj4String()` considering only parameters applicable to the chosen `ProjectionType`.
  - Angle properties are interpreted in radians; distances are meters.
- `CartographicProjectionType` (`Model/CartographicProjectionType.cs:1`)
  - `ProjectionType` enum lists supported projections (UTM, Mercator, Lambert variants, TM, etc.).
  - Indicates which parameters apply per projection and maps to the `+proj=...` token for PROJ.4.
- `CartographicConversionSet` (`Model/CartographicConversionSet.cs:1`)
  - Forward/inverse single‑point and batch conversions using DotSpatial:
    - `ToCarto(CartographicProjection, GeodeticDatum, latitudeRad, longitudeRad, out northing, out easting)`
    - `FromCarto(CartographicProjection, GeodeticDatum, northing, easting, out latitudeRad, out longitudeRad)`
    - `CalculateGridConvergence(...)` to compute grid convergence at a location.
  - `GetProj4String(Spheroid)` to build spheroid parameters (`+a`, `+b`, `+f`, `+rf`, `+e`, `+es`).
- `CartographicCoordinate` (`Model/CartographicCoordinate.cs:1`)
  - Container for a single pair of cartographic and/or geodetic coordinates.

## Usage Examples

Notes:
- Angular quantities are radians; easting/northing are meters.
- `GeodeticDatum` and `Spheroid` are provided by `NORCE.Drilling.CartographicProjection.ModelShared` (from `ModelSharedIn`). Populate them with your target datum (e.g., WGS84 spheroid parameters).

Example 1 — Project a geodetic point to UTM easting/northing:

```csharp
using NORCE.Drilling.CartographicProjection.Model;
using NORCE.Drilling.CartographicProjection.ModelShared; // GeodeticDatum, Spheroid

// Define the cartographic projection (UTM Zone 32N)
var projection = new CartographicProjection
{
    ProjectionType = ProjectionType.UTM,
    Zone = 32,
    IsSouth = false
};

// Define the geodetic datum (populate Spheroid with your datum values, e.g., WGS84)
var datum = new GeodeticDatum
{
    Spheroid = new Spheroid
    {
        // Minimal example: set semi-major axis and inverse flattening
        // (Fill in values as available in your context)
        // SemiMajorAxis.DiracDistributionValue.Value.Value = 6378137.0;
        // InverseFlattening.DiracDistributionValue.Value.Value = 298.257223563;
    }
};

// Input latitude/longitude in radians
double latitudeRad = 60.0 * Math.PI / 180.0;
double longitudeRad = 5.0 * Math.PI / 180.0;

// Convert to cartographic coordinates
var set = new CartographicConversionSet();
set.ToCarto(projection, datum, latitudeRad, longitudeRad, out double? northing, out double? easting);

Console.WriteLine($"N={northing}, E={easting}");
```

Example 2 — Inverse: from easting/northing back to geodetic:

```csharp
set.FromCarto(projection, datum, northing!.Value, easting!.Value, out double? latRad, out double? lonRad);
Console.WriteLine($"Lat={latRad}, Lon={lonRad}");
```

Example 3 — Grid convergence at a point:

```csharp
set.CalculateGridConvergence(projection, datum, northing!.Value, easting!.Value, latitudeRad, longitudeRad, out double? gridConv);
Console.WriteLine($"Grid convergence (rad) = {gridConv}");
```

Example 4 — Inspect the PROJ.4 string used for the transformation:

```csharp
string proj4 = projection.GetProj4String() + CartographicConversionSet.GetProj4String(datum.Spheroid);
Console.WriteLine(proj4);
```

Batch conversions are also supported via list overloads on `ToCarto(...)` and `FromCarto(...)`.

## Build & Test

- Build: `dotnet build Model/Model.csproj`
- Tests (projection logic): `dotnet test ModelTest/ModelTest.csproj`

## Documentation

The project includes a DocFX configuration (`Model/docfx.json:1`). If you have `docfx` installed locally, you can build the API docs from the `Model` directory:

- Generate metadata and site: `docfx docfx.json`
- Output: `Model/_site`

## Integration Details

- Service (`Service/Service.csproj:1`)
  - References `Model` and exposes endpoints that call the conversion methods.
  - Builds and publishes OpenAPI, which `ModelSharedOut` consumes to generate client models.
- ModelSharedIn (`ModelSharedIn/ModelSharedIn.csproj:1`)
  - Provides generated DTOs used by `Model` (`GeodeticDatum`, `Spheroid`, `GeodeticCoordinate`, etc.).
- ModelSharedOut (`ModelSharedOut/ModelSharedOut.csproj:1`)
  - Generates shared client models for consumers (used by `ServiceTest` and `WebApp`).
- WebApp (`WebApp/WebApp.csproj:1`)
  - Calls the Service API; does not reference `Model` directly.

## Tips & Conventions

- Angles: radians (e.g., use `Math.PI / 180.0` to convert degrees to radians).
- Units: meters for linear quantities.
- UTM specifics: set `Zone` (1–60) and `IsSouth` for southern hemisphere.
- Only parameters that are marked as used by the selected `ProjectionType` are emitted to the PROJ.4 string.

