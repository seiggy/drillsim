# OSDC.UnitConversion.DrillingRazorMudComponents

MudBlazor components for displaying and editing drilling-engineering values with unit-system conversion and optional engineering reference conversion.

The package is built for Blazor and targets `net8.0`. It uses the drilling unit systems from `OSDC.UnitConversion.Conversion.UnitSystem.DrillingEngineering` and MudBlazor controls for the UI.

## Features

- Select a drilling unit system from a `MudSelect`.
- Display or edit scalar values in the selected unit system while storing values in SI.
- Display or edit plane angles as degree, arc-minute, arc-second fields.
- Optionally convert values between engineering references:
  - Pressure: absolute pressure and gauge pressure.
  - Azimuth: true north, grid north, and magnetic north.
  - Depth: WGS84, rotary table, sea/water level, mean sea level, ground/mud line, and well-head.
  - Position: WGS84, well-head, cluster-reference, field, and cartographic grid.
  - Geodetic datum: WGS84 and cartographic projection datum.
- Refresh child components automatically when the selected unit system or reference changes.

## Installation

Install the NuGet package:

```powershell
dotnet add package OSDC.UnitConversion.DrillingRazorMudComponents
```

The package depends on:

- `MudBlazor`
- `OSDC.UnitConversion.Conversion.UnitSystem.DrillingEngineering`
- `OSDC.DotnetLibraries.General.DataManagement`

In the consuming Blazor app, make sure MudBlazor services and styles are configured as required by MudBlazor.

## Basic Usage

Wrap unit-aware child components in `MudUnitAndReferenceChoiceTag`. The wrapper provides the selected unit system and selected references through a cascading value.

```razor
<MudUnitAndReferenceChoiceTag @bind-UnitSystemName="unitSystemName">
    <MudInputWithUnit QuantityLabel="Measured depth"
                      QuantityName="MeasuredDepth"
                      DrillingSignalReference="DrillingSignalReferenceType.Depth"
                      @bind-SIValueNullable="measuredDepth" />

    <MudSpanWithUnit QuantityName="Pressure"
                     DrillingSignalReference="DrillingSignalReferenceType.Pressure"
                     SIValue="pressure"
                     UseUnitLabel="true" />
</MudUnitAndReferenceChoiceTag>

@code {
    private string? unitSystemName;
    private double? measuredDepth;
    private double? pressure;
}
```

The values bound to `SIValue` or `SIValueNullable` are SI values. The components display and parse values in the currently selected unit system and reference.

## Components

### MudUnitAndReferenceChoiceTag

`MudUnitAndReferenceChoiceTag` is the parent component. It renders the unit-system selector and any reference selectors that are enabled by supplying a reference source.

Common parameters:

| Parameter | Purpose |
| --- | --- |
| `UnitSystemName` / `UnitSystemNameChanged` | Selected unit-system name. |
| `DepthReferenceName` / `DepthReferenceNameChanged` | Selected depth reference. |
| `PositionReferenceName` / `PositionReferenceNameChanged` | Selected position reference. |
| `GeodeticReferenceName` / `GeodeticReferenceNameChanged` | Selected geodetic datum reference. |
| `AzimuthReferenceName` / `AzimuthReferenceNameChanged` | Selected azimuth reference. |
| `PressureReferenceName` / `PressureReferenceNameChanged` | Selected pressure reference. |
| `HttpHost`, `HttpBasePath`, `HttpController` | Optional endpoint information for loading custom unit systems. If omitted, built-in drilling unit systems are used. |

Reference selectors are optional. A selector is hidden when it has only the default choice.

### MudInputWithUnit

Single-field numeric editor.

Important parameters:

| Parameter | Purpose |
| --- | --- |
| `QuantityLabel` | Label shown in the input. |
| `QuantityName` | Unit-conversion quantity name. |
| `SIValue` / `SIValueChanged` | Non-nullable SI value binding. |
| `SIValueNullable` / `SIValueNullableChanged` | Nullable SI value binding. |
| `DrillingSignalReference` | Reference behavior: `Independent`, `Pressure`, `Azimuth`, `Depth`, `Position`, or `Geodetic`. |
| `PositionDirection` | Direction for position/geodetic values: `North` or `East`. |
| `MinValue`, `MaxValue` | Optional validation bounds in SI/reference source coordinates. |

### MudInputWithUnitAdornment

Single-field numeric editor with the converted unit label shown as a MudBlazor adornment.

It supports the same conversion/reference parameters as `MudInputWithUnit`, plus:

| Parameter | Purpose |
| --- | --- |
| `Disabled` | Disables the input. |
| `ReadOnly` | Makes the input read-only. |
| `Immediate` | Forwards MudBlazor immediate binding behavior. |

### MudSpanWithUnit

Read-only single-field display of a numeric value. It supports `QuantityName`, `SIValue`, `UseUnitLabel`, `DrillingSignalReference`, and `PositionDirection`.

### MudSpanUnitLabel

Read-only display of only the current unit label. It follows the selected unit system and the selected reference type.

### MudInputAngleWithUnit

Degree, arc-minute, arc-second editor for plane angles.

It can be used standalone, or inside `MudUnitAndReferenceChoiceTag`. When used for latitude/longitude datum conversion, set:

- `QuantityName`
- `DrillingSignalReference="DrillingSignalReferenceType.Geodetic"`
- `PositionDirection="PositionDirectionType.North"` for latitude
- `PositionDirection="PositionDirectionType.East"` for longitude

### MudInputAngleWithUnitAdornment

Degree, arc-minute, arc-second editor with a caption label. It supports the same geodetic parameters as `MudInputAngleWithUnit`.

### MudSpanAngleWithUnit

Read-only degree, arc-minute, arc-second display. It supports geodetic datum conversion when `QuantityName`, `DrillingSignalReference`, and `PositionDirection` are provided.

## Reference Types

Use `DrillingSignalReferenceType` to tell child components which reference conversion should be applied.

```csharp
public enum DrillingSignalReferenceType
{
    Independent,
    Pressure,
    Azimuth,
    Depth,
    Position,
    Geodetic
}
```

`PositionDirectionType` is used for both position and geodetic values:

```csharp
public enum PositionDirectionType
{
    Independent,
    North,
    East
}
```

For position references:

- `North` selects northing offsets.
- `East` selects easting offsets.

For geodetic references:

- `North` selects latitude offsets.
- `East` selects longitude offsets.

## Optional Reference Sources

Reference choices are enabled by passing objects that implement the relevant source interfaces to `MudUnitAndReferenceChoiceTag`.

### Pressure

```csharp
public interface IGaugeReferencePressureSource
{
    double? GaugeReferencePressure { get; set; }
}
```

When provided, the pressure selector offers:

- `Absolute Pressure`
- `Gauge Pressure`

### Azimuth

```csharp
public interface IGridConvergenceSource
{
    double? GridConvergence { get; set; }
}

public interface IMagneticDeclinationSource
{
    double? MagneticDeclination { get; set; }
}
```

The azimuth selector always includes `True North`. It also includes `Grid North` and/or `Magnetic North` when the corresponding source is supplied.

### Depth

```csharp
public interface IRotaryTableDepthReferenceSource
{
    double? RotaryTableDepthReference { get; set; }
}

public interface ISeaWaterLevelDepthReferenceSource
{
    double? SeaWaterLevelDepthReference { get; set; }
}

public interface IMeanSeaLevelDepthReferenceSource
{
    double? MeanSeaLevelDepthReference { get; set; }
}

public interface IGroundMudLineDepthReferenceSource
{
    double? GroundMudLineDepthReference { get; set; }
}

public interface IWellHeadDepthReferenceSource
{
    double? WellHeadDepthReference { get; set; }
}
```

The depth selector always includes `WGS84`. Additional choices are added from the provided sources.

### Position

```csharp
public interface IWellHeadPositionReferenceSource
{
    double? WellHeadNorthPositionReference { get; set; }
    double? WellHeadEastPositionReference { get; set; }
}

public interface IClusterPositionReferenceSource
{
    double? ClusterNorthPositionReference { get; set; }
    double? ClusterEastPositionReference { get; set; }
}

public interface IFieldPositionReferenceSource
{
    double? FieldNorthPositionReference { get; set; }
    double? FieldEastPositionReference { get; set; }
}

public interface ICartographicGridPositionReferenceSource
{
    double? CartographicGridNorthPositionReference { get; set; }
    double? CartographicGridEastPositionReference { get; set; }
}
```

The position selector always includes `WGS84`. Additional choices are:

- `Well-head`
- `Cluster-reference`
- `Field`
- `Cartographic`

### Geodetic Datum

```csharp
public interface ICartographicProjectionDatumGeodeticReferenceSource
{
    double? CartographicProjectionDatumLatitudeReference { get; set; }
    double? CartographicProjectionDatumLongitudeReference { get; set; }
}
```

The geodetic selector always includes `WGS84`. When the cartographic projection datum source is supplied, it also includes `Cartographic Projection Datum`.

Latitude and longitude offsets are expected in SI angle units. Use `PositionDirectionType.North` for latitude and `PositionDirectionType.East` for longitude.

## Example: Depth, Position, and Geodetic Datum

```razor
<MudUnitAndReferenceChoiceTag @bind-UnitSystemName="unitSystemName"
                              RotaryTableDepthReferenceSource="referenceModel"
                              WellHeadPositionReferenceSource="referenceModel"
                              CartographicProjectionDatumGeodeticReferenceSource="referenceModel">
    <MudInputWithUnit QuantityLabel="TVD"
                      QuantityName="Depth"
                      DrillingSignalReference="DrillingSignalReferenceType.Depth"
                      @bind-SIValueNullable="tvd" />

    <MudInputWithUnitAdornment QuantityLabel="North position"
                               QuantityName="Length"
                               DrillingSignalReference="DrillingSignalReferenceType.Position"
                               PositionDirection="PositionDirectionType.North"
                               @bind-SIValueNullable="north" />

    <MudInputAngleWithUnitAdornment QuantityLabel="Latitude"
                                    QuantityName="PlaneAngle"
                                    DrillingSignalReference="DrillingSignalReferenceType.Geodetic"
                                    PositionDirection="PositionDirectionType.North"
                                    @bind-SIValueNullable="latitude" />

    <MudInputAngleWithUnitAdornment QuantityLabel="Longitude"
                                    QuantityName="PlaneAngle"
                                    DrillingSignalReference="DrillingSignalReferenceType.Geodetic"
                                    PositionDirection="PositionDirectionType.East"
                                    @bind-SIValueNullable="longitude" />
</MudUnitAndReferenceChoiceTag>

@code {
    private string? unitSystemName;
    private double? tvd;
    private double? north;
    private double? latitude;
    private double? longitude;

    private ReferenceModel referenceModel = new();

    private sealed class ReferenceModel :
        IRotaryTableDepthReferenceSource,
        IWellHeadPositionReferenceSource,
        ICartographicProjectionDatumGeodeticReferenceSource
    {
        public double? RotaryTableDepthReference { get; set; } = 25.0;

        public double? WellHeadNorthPositionReference { get; set; } = 100.0;
        public double? WellHeadEastPositionReference { get; set; } = 50.0;

        public double? CartographicProjectionDatumLatitudeReference { get; set; } = 0.0;
        public double? CartographicProjectionDatumLongitudeReference { get; set; } = 0.0;
    }
}
```

## Conversion Methods on MudUnitAndReferenceChoiceTag

Child components call these methods through the cascading parent. They are also available when custom child components need the same behavior.

| Method family | Purpose |
| --- | --- |
| `FromSI`, `ToSI`, `GetUnitLabel` | Unit conversion without reference conversion. |
| `FromAbsolutePressureSI`, `ToAbsolutePressureSI`, `GetPressureUnitLabel` | Pressure reference conversion. |
| `FromTrueNorthAzimuthSI`, `ToTrueNorthAzimuthSI`, `GetAzimuthUnitLabel` | Azimuth reference conversion. |
| `FromWGS84DepthSI`, `ToWGS84DepthSI`, `GetDepthUnitLabel` | Depth reference conversion. |
| `FromWGS84PositionSI`, `ToWGS84PositionSI`, `GetPositionUnitLabel` | Position reference conversion. |
| `FromWGS84GeodeticSI`, `ToWGS84GeodeticSI`, `GetGeodeticUnitLabel` | Geodetic datum conversion. |

## Notes

- Values stored in bound properties are SI values.
- Reference source values are expected in SI units for the relevant quantity.
- Optional reference selectors appear only when at least one non-default reference source is supplied.
- For geodetic datum conversion, the current implementation follows the same offset-based style as position references: the cartographic projection datum latitude/longitude references are added when displaying from WGS84 and subtracted when converting back to WGS84.
