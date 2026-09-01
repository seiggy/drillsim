# Unit conversion

The UnitConversion repository contains tools to handle unit conversions of a wide variety of physical quantities, unit systems, and units. The repository hosts:

- a [.NET library (nuget package)](https://www.nuget.org/packages/OSDC.UnitConversion.Conversion) that handles unit conversions of base physical quantities
- a [.NET library (nuget package)](https://www.nuget.org/packages/OSDC.UnitConversion.Conversion.UnitSystem) that handles unit conversion sets of base physical quantities
- a [.NET library (nuget package)](https://www.nuget.org/packages/OSDC.UnitConversion.Conversion.DrillingEngineering) that handles unit conversions of physical quantities specific to the drilling engineering field
- a [.NET library (nuget package)](https://www.nuget.org/packages/OSDC.UnitConversion.Conversion.UnitSystem.DrillingEngineering) that handles unit conversion sets of physical quantities specific to the drilling engineering field

- a microservice to handle unit conversions programmatically
  - [microservice API](https://dev.digiwells.no/UnitConversion/api/swagger) (swagger)
  - [PhysicalQuantity endpoint](https://dev.digiwells.no/UnitConversion/api/PhysicalQuantity) 
  - [UnitSystem endpoint](https://dev.digiwells.no/UnitConversion/api/UnitSystem)
  - [UnitSystemConversionSet endpoint](https://dev.digiwells.no/UnitConversion/api/UnitSystemConversionSet)
  - [UnitConversionSet endpoint](https://dev.digiwells.no/UnitConversion/api/UnitConversionSet)
  - MCP server entry point: `https://app.digiwells.no/UnitConversion/api/mcp` (see details below)
  - optional MCP hub registration configured from the shared `home/` volume

- a client user interface to handle unit conversions 
  - [PhysicalQuantity endpoint](https://dev.digiwells.no/UnitConversion/webapp/PhysicalQuantity)
  - [UnitConversionSet endpoint](https://dev.digiwells.no/UnitConversion/webapp/UnitConversion)

## Model Context Protocol server

The UnitConversion service hosts a [Model Context Protocol](https://modelcontextprotocol.io/) server alongside the REST API. MCP-compatible clients can call tools over HTTP (streaming) or WebSocket transports using the endpoint:

```
https://app.digiwells.no/UnitConversion/api/mcp
```

When running locally with the default profile, the entry point is:

```
http://localhost:5002/UnitConversion/api/mcp
```

The service image includes a seed MCP vector document database under `/app/seed/UnitConversionVectors.db`. On startup this seed is copied to `/home/UnitConversionVectors.db` only if the runtime database is missing, so the `/home` volume can remain persistent without hiding the bundled seed data.

### Available tools

The following tool names are exposed through MCP:

- `ping` – connectivity probe returning a `pong` payload
- Physical quantity utilities: `get_all_physical_quantity_id`, `get_physical_quantity_by_id`, `get_all_physical_quantity`, `find_physical_quantity_id_by_name`
- Unit conversions for individual quantities: `convert_unit_value`
- Unit system utilities: `get_all_unit_system_id`, `get_unit_system_by_id`, `get_all_unit_system_light`, `get_all_unit_system`, `find_unit_system_id_by_name`, `post_unit_system`, `put_unit_system_by_id`, `delete_unit_system_by_id`
- Unit system conversions: `convert_unit_system_value`
- Unit conversion set management: `get_all_unit_conversion_set_id`, `get_all_unit_conversion_set_meta_info`, `get_unit_conversion_set_by_id`, `get_all_unit_conversion_set`, `post_unit_conversion_set`, `put_unit_conversion_set_by_id`, `delete_unit_conversion_set_by_id`
- Unit system conversion set management: `get_all_unit_system_conversion_set_id`, `get_all_unit_system_conversion_set_meta_info`, `get_unit_system_conversion_set_by_id`, `get_all_unit_system_conversion_set`, `post_unit_system_conversion_set`, `put_unit_system_conversion_set_by_id`, `delete_unit_system_conversion_set_by_id`

The two convenience conversion tools create, retrieve, and automatically delete temporary calculation cases. The `post_*_conversion_set` tools are the persistent batch alternative: create a case with a caller-assigned UUID, retrieve its calculated `DataOut` and `DataOutString` fields by that UUID, then delete it explicitly when appropriate. MCP discovery provides complete nested schemas for conversion sets and unit-system `Choices` mappings.

### Example JSON-RPC request

```bash
curl -X POST \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -d '{
        "jsonrpc": "2.0",
        "id": 1,
        "method": "tools.call",
        "params": {
          "name": "convert_unit_system_value",
          "arguments": {
            "physicalQuantity": "Mud Density",
            "unitSystemIn": "SI",
            "unitSystemOut": "Metric",
            "value": 1200
          }
        }
      }' \
  http://localhost:5002/UnitConversion/api/mcp
```

Clients can also initiate WebSocket sessions (via `GET /UnitConversion/api/mcp/ws`) and use SSE/HTTP streaming; the transport is negotiated automatically by the ModelContextProtocol client libraries.

### MCP hub registration

The service can publish its MCP endpoint to an MCP hub. The Docker image reads optional external configuration from `/home/UnitConversion.Service.json`, or from the path specified by `UNITCONVERSION_EXTERNAL_CONFIG`. The shared `/home` volume also stores the SQLite database, vector database, and generated MCP hub instance id.

Minimal external configuration:

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

The registered MCP URLs are derived from `PublicBaseUrl` as `/UnitConversion/api/mcp` and `/UnitConversion/api/mcp/ws`. If the hub is unavailable, the service retries every `RetryIntervalSeconds` seconds.

# Data model

<details>
   <summary><h3>Physical quantities</h3></summary>

The UnitConversion repository contains tools to handle unit conversions of a wide variety of physical quantities:

- either 206 base physical quantities, called `BasePhysicalQuantity`

- or 192 physical quantities specific to the drilling engineering field, called `PhysicalQuantity`

- note that `PhysicalQuantity` extends `BasePhysicalQuantity` and hence encompasses it, so that the drilling catalog opens access to a total of **398 physical quantities**.

- a drilling-specific quantity can itself derive from a more general physical quantity. For example, `RateOfPenetrationDrillingQuantity` derives from `VelocityQuantity`. The specialised quantity exposes a curated list of common drilling units and defines domain-specific `MeaningfulPrecisionInSI`; compatible units absent from that list can be resolved from its parent hierarchy. Conversion retains the specialised quantity for precision semantics rather than replacing it with the parent quantity.

- see the overview of physical quantities by technical fields [below](https://github.com/Open-Source-Drilling-Community/UnitConversion/blob/main/README.md#list-of-physical-quantities); the generated `EnumerationQuantities.cs` files are the authoritative complete catalogs.

</details>

<details>
   <summary><h3>Unit conversion classes</h3></summary>

Unit conversions can be handled:

- either through `UnitSystemConversionSet` that converts a user-defined collection physical quantities from one unit system to another

  - `UnitSystemConversionSet` class is useful for web pages that rely on a user-selected unit system
  
  - a unit system is a collection of quantities where each `PhysicalQuantity` is assigned one pre-defined `UnitChoice`

    - `BaseUnitSystem` are standard unit systems, i.e. *SI*, *US*, *Imperial*, *Metric*

    - `UnitSystem` are custom unit systems that allow the user to map every `PhysicalQuantity` to a preferred `UnitChoice`
  
    - note that `UnitSystem` extends `BaseUnitSystem` and hence encompasses it
 
 - or through `UnitConversionSet` that converts a user-defined collection physical quantities from one unit choice to another
 
   - `UnitConversionSet` class is more versatile and useful for unit conversions that technical people must perform in their every day work

</details>

<details>
   <summary><h3>Hybrid data access</h3></summary>
   
- physical quantities and standard unit systems are loaded in memory on microservice startup, through the nuget packages [OSDC.UnitConversion.Conversion.DrillingEngineering](https://www.nuget.org/packages/OSDC.UnitConversion.Conversion) and [OSDC.UnitConversion.Conversion](https://www.nuget.org/packages/OSDC.UnitConversion.Conversion.DrillingEngineering)

- custom unit systems, unit system conversion sets, and unit conversion sets are loaded through http requests sent to the database of the containerized microservice

</details>

<details>
   <summary><h3>BasePhysicalQuantity and BaseUnitSystem</h3></summary>
   
- base physical quantities are loaded in memory on microservice startup through `BasePhysicalQuantity.AvailableBasePhysicalQuantities`. Each `BasePhysicalQuantity` holds a reference on a list of `UnitChoice` which simply represents units available for this quantity.

- base unit systems (*SI*, *Metric*, *Imperial*, *US*) are also loaded in memory on microservice startup through `BaseUnitSystem.SIBaseUnitSystem` for example. Each of them holds a reference on a `Dictionary<string, string>` that assigns one `UnitChoice` to each `BasePhysicalQuantity`

- class diagram is as follows

<p align="center">
<img src="https://github.com/user-attachments/assets/d9a36340-dedf-4266-89c1-93e07eacec20" width="800">
</p>



- `PhysicalQuantity` extends `BasePhysicalQuantity` and `UnitSystem` extends `BaseUnitSystem` so that the same considerations as above apply

- class diagram is as follows


<p align="center">
<img src="https://github.com/user-attachments/assets/b42b933b-2617-46ae-8071-057f6cb52544" width="500">
</p>


- all these classes are identified with a simple `Guid`. The `MetaInfo` concept (see nuget package [OSDC.DotnetLibraries.General.DataManagement](https://www.nuget.org/packages/OSDC.DotnetLibraries.General.DataManagement)) is not relevant here, due to the fact that these classes are loaded in memory on startup, and hence do not need to be identified through a network of microservices.

</details>

<details>
   <summary><h3>Unit conversion classes</h3></summary>

- the base class that performs `double` to `double?` unit conversions is `ValueConversion`

- `ValueConversion` is aggregated in two intermediate super-classes
  - `QuantityConversion` that holds a reference on the `Guid` of a `PhysicalQuantity`
  - `QuantityUnitConversion` that holds a reference on both the `Guid` of a `PhysicalQuantity`, the `Guid` of a `UnitChoice` for the input data, and the `Guid` of a `UnitChoice` for the output data
  
- `QuantityConversion` is itself aggregated in the `UnitSystemConversionSet` super-class that holds a reference on
  - the `Guid` of an input `UnitSystem`
  - the `Guid` of an output `UnitSystem`
  - a *MetaInfo* used to identify the conversion set in the microservice architecture and store it in the database
  
- `QuantityUnitConversion` is itself aggregated in the `UnitConversionSet` super-class that simply holds a reference on a *MetaInfo* used to identify the conversion set in the microservice architecture and store it in the database
  - no unit system or unit choice identifier are necessary at the `UnitConversionSet` level, since unit choices are already decided at the level of the `QuantityUnitConversion`
  
- therefore, `UnitSystemConversionSet` is useful to convert quantities from a unit system to another and `UnitConversionSet` is more versatile as it allows to tune the unit choice at the level of each quantity aggregated in the list. In particular, with the `UnitConversionSet`, it is possible to convert 2 quantities of the same type according to different unit choices. Whereas, with the `UnitSystemConversionSet`, all quantities of the same type must stick to the unit choice of the selected input or output `UnitSystem`

- class diagram is as follows

<p align="center">
<img src="https://github.com/user-attachments/assets/3df456cc-668e-4336-8d58-1887bbf8d85a" width="800">
</p>

   
</details>

# List of physical quantities

<details>
   <summary><h3>BasePhysicalQuantity</h3></summary>
   
|Physical Quantity Name|Usual Names||||
|---|---|---|---|---|
|Acceleration|Acceleration||||
|AmountSubstance|Amount Substance||||
|AngleMagneticFluxDensity|Angle Magnetic Flux Density||||
|AngleVariationGradient|Angle Variation Gradient||||
|AngularAcceleration|Angular Acceleration||||
|AngularVelocity|Angular Velocity||||
|Area|Area||||
|Compressibility|Compressibility||||
|Curvature|Curvature||||
|Density|Density||||
|DensityGradientDepth|Density Gradient Depth||||
|DensityGradientTemperature|Density Gradient Temperature||||
|DensityRateOfChange|Density Rate of Change||||
|Dimensionless|Dimensionless||||||
|DynamicViscosity|Dynamic Viscosity||||
|EarthMagneticFluxDensity|Earth Magnetic Flux Density||||
|ElectricalCapacitance|Electrical Capacitance||||
|ElectricalCurrent|Electrical Current||||
|ElectricTension|Electric Tension||||
|ElongationGradient|Elongation Gradient||||
|Energy|Energy||||
|EnergyDensity|EnergyDensiy||||
|FluidShearRate|Fluid Shear Rate||||
|FluidShearStress|Fluid Shear Stress||||
|Force|Force||||
|ForceGradient|Force Gradient||||
|Frequency|Frequency||||
|FrequencyRateOfChange|Frequency Rate Of Change||||
|GravitationalLoad|Gravitational Load||||
|HeatTransferCoefficient|Heat Transfer Coefficient||||
|HydraulicConductivity|Hydraulic Conductivity||||
|ImageScale|Image Scale||||
|InterfacialTension|Interfacial Tension||||
|LargeVolume|Large Volume||||
|Length|Length||||
|LuminousIntensity|Luminous Intensity||||
|MagneticFlux|Magnetic Flux||||
|MagneticFluxDensity|Magnetic Flux Density||||
|Mass|Mass||||
|MassGradient|Mass Gradient||||
|MassRate|Mass Rate||||
|MaterialStrength|Material Strength||||
|Permeability|Permeability||||
|PlaneAngle|Plane Angle||||
|Porosity|Porosity||||
|Power|Power||||
|Pressure|Pressure||||
|PressureGradient|Pressure Gradient||||
|PressureLossConstant|Pressure Loss Constant||||
|Proportion|Proportion||||
|RandomWalk|Random Walk||||
|RelativeTemperature|Relative Temperature||||
|Resistivity|Resistivity||||
|RheologyConsistencyIndex|Rheology Consistency Index||||
|RotationFrequency|Rotation Frequency|Rotary speed|RPM|
|RotationFrequencyRateOfChange|Rotation Frequency Rate Of Change||||
|SmallDiameter|Small Diameter||||
|SmallLength|Small Length||||
|SmallProportion|Small Proportion||||
|SmallRotationFrequency|Small Rotation Frequency|Small rotary speed|Small RPM|
|SmallTorque|Small Torque||||
|SolidAngle|Solid Angle||||
|SpecificHeatCapacity|Specific Heat Capacity||||
|SpecificHeatCapacityTemperatureGradient|Specific Heat Capacity Temperature Gradient||||
|StandardDimensionless|Standard Dimensionless||||
|StandardLength|Standard Length||||
|StandardProportion|Standard Proportion||||
|Temperature|Temperature||||
|TemperatureGradient|Temperature Gradient||||
|Tension|Tension||||
|ThermalConductivity|Thermal Conductivity||||
|ThermalConductivityTemperatureGradient|Thermal Conductivity Temperature Gradient||||
|Time|Time||||
|Torque|Torque|Bending moment|||
|Velocity|Velocity||||
|Volume|Volume||||
|VolumetricFlowRate|Volumetric Flow Rate|Flow Rate (volumetric)|||
|VolumetricFlowRateRateOfChange|Volumetric Flow Rate Rate Of Change||||
|WaveNumber|Wave Number||||
|YoungModulus|Young Modulus||||
   
</details>

<details>
   <summary><h3>PhysicalQuantity, drilling-specific only</h3></summary>
   
|Physical Quantity Name|Usual Names||||
|---|---|---|---|---|
|AccelerationDrilling|Acceleration (drilling)||||||
|AngleVariationGradientDrilling|Angle Variation Gradient (drilling)|||
|AngularAccelerationDrilling|Angular Acceleration (drilling)||||||
|AngularVelocityDrilling|Angular Velocity (drilling)||||||
|AreaDrilling|Area (drilling)||||||
|AxialVelocityDrilling|Axial Velocity (drilling)||||||
|BlockVelocity|Block Velocity (drilling)||||||
|CableDiameter|Cable Diameter (drilling)||||||
|CapillaryPressure|Capillary Pressure (drilling)||||||
|CompressibilityDrilling|Compressibility (drilling)||||||
|CurvatureDrilling|Curvature (drilling)|DLS|Dogleg severity||
|DensityDrilling|Density (drilling)||||||
|DensityGradientDepthDrilling|Density Gradient Depth (drilling)||||||
|DensityGradientTemperatureDrilling|Density Gradient Temperature (drilling)||||||
|DensityRateOfChangeDrilling|Density Rate of Change (drilling)||||||
|Depth|Depth (drilling)||||||
|DrillStemMaterialStrength|Drill Stem Material Strength (drilling)||||||
|DrillStringMagneticFluxDrilling|Drill String Magnetic Flux (drilling)||||||
|DurationDrilling|Duration (drilling)||||||
|DynamicViscosityDrilling|Dynamic Viscosity (drilling)||||||
|ElongationGradientDrilling|Elongation Gradient (drilling)||||||
|EnergyDensityDrilling|Energy Density (drilling)||||||
|FluidVelocityDrilling|Fluid Velocity (drilling)||||||
|ForceDrilling|Force (drilling)||||||
|ForceGradientDrilling|Force Gradient (drilling)||||||
|FormationResistivity|Formation Resistivity (drilling)||||||
|FormationStrength|Formation Strength (drilling)||||||
|GammaRay|Gamma Ray (drilling)||||||
|GasShow|Gas Show (drilling)||||||
|GasVolumetricFlowRate|Gas Volumetric Flow Rate (drilling)||||||
|HeatTransferCoefficientDrilling|Heat Transfer Coefficient (drilling)||||||
|Height|Height (drilling)||||||
|HookLoad|Hook Load (drilling)||||||
|HydraulicConductivityDrilling|Hydraulic Conductivity (drilling)||||||
|InterfacialTensionDrilling|Interfacial Tension (drilling)||||||
|MassDrilling|Mass (drilling)||||||
|MassGradientDrilling|Mass Gradient (drilling)||||||
|MassRateDrilling|Mass Rate (drilling)||||||
|NozzleDiameter|Nozzle Diameter (drilling)||||||
|PipeDiameter|Pipe Diameter (drilling)||||||
|PlaneAngleDrilling|Plane Angle (drilling)||||||
|PoreDiameter|Pore Diameter (drilling)||||||
|PoreSurface|Pore Surface (drilling)||||||
|Position|Position (drilling)||||||
|PowerDrilling|Power (drilling)||||||
|PressureDrilling|Pressure (drilling)|MSE (as a pressure)|||
|PressureGradientDrilling|Pressure Gradient (drilling)||||||
|PressureLossConstantDrilling|Pressure Loss Constant (drilling)||||||
|RandomWalkDrilling|Random Walk (drilling)||||||
|RateOfPenetration|Rate Of Penetration (drilling)|ROP|||
|RotationFrequencyRateOfChangeDrilling|Rotation Frequency Rate Of Change (drilling)||||||
|ShockRate|Shock Rate (drilling)||||||
|SpecificHeatCapacityDrilling|Specific Heat Capacity (drilling)||||||
|SpecificHeatCapacityTemperatureGradientDrilling|Specific Heat Capacity Temperature Gradient (drilling)|||
|StickDurationDrilling|Stick Duration (drilling)||||||
|SurveyInstrumentAngleMagneticFluxDensityDrilling|Survey Instrument Angle Magnetic Flux Density (drilling)||||||
|SurveyInstrumentAngularVelocityDrilling|Survey Instrument Angular Velocity (drilling)||||||
|SurveyInstrumentReciprocalLengthDrilling|Survey Instrument Reciprocal Length (drilling)||||||
|TemperatureDrilling|Temperature (drilling)||||||
|TemperatureGradientDrilling|Temperature Gradient (drilling)||||||
|TensionDrilling|Tension (drilling)||||||
|ThermalConductivityDrilling|Thermal Conductivity (drilling)||||||
|ThermalConductivityTemperatureGradientDrilling|Thermal Conductivity Temperature Gradient (drilling)||||||
|TorqueDrilling|Torque (drilling)|Bending moment (drilling)|TOB|BOB|
|VolumeDrilling|Volume (drilling)||||||
|VolumetricFlowRateDrilling|Volumetric Flow Rate (drilling)|Flow Rate (volumetric, drilling)||||
|VolumetricFlowRateOfChangeDrilling|Volumetric Flow Rate Of Change (drilling)||||||
|WeightOnBit|Weight On Bit (drilling)|WOB||||

</details>

<details>
   <summary><h3>PhysicalQuantity, by technical field</h3></summary>
   
<table class="table-fixed">
  <tr>
    <th>Technical Field</th>
    <th>Physical Quantity Names</th>
  </tr>
  <tr>
    <td>Chemistry and Material Properties</td>
    <td>AmountSubstance, DensityGradientDepth, DensityGradientDepthDrilling, DensityGradientTemperature, DensityGradientTemperatureDrilling, DensityDrilling, DensitySpeedDrilling, FormationResistivity, FormationStrength, GasShow, Mass, MassDrilling, MassGradient, MassGradientDrilling, MassRate, MassRateDrilling, Porosity, Proportion, Resistivity, RheologyConsistencyIndex, ShockRate, SmallProportion, SpecificHeatCapacity, SpecificHeatCapacityDrilling, SpecificHeatCapacityTemperatureGradient, SpecificHeatCapacityTemperatureGradientDrilling</td>
  </tr>
  <tr>
    <td>Drilling Technology</td>
    <td>AccelerationDrilling, AngleVariationGradientDrilling, AngularVelocityDrilling, AreaDrilling, AxialVelocityDrilling, CompressibilityDrilling, CurvatureDrilling, DensityDrilling, DensityGradientDepthDrilling, DensityGradientTemperatureDrilling, DensitySpeedDrilling, DrillStringMagneticFlux, DrillStemMaterialStrength, DurationDrilling, DynamicViscosityDrilling, ElongationGradientDrilling, FluidVelocityDrilling, ForceDrilling, ForceGradientDrilling, HeatTransferCoefficientDrilling, HydraulicConductivityDrilling, InterfacialTensionDrilling, MagneticFluxDensityDrilling, PlaneAngleDrilling, PowerDrilling, PressureDrilling, PressureGradientDrilling, PressureLossConstantDrilling, RandomWalkDrilling, RotationFrequencyRateOfChangeDrilling, StickDurationDrilling, SurveyInstrumentAngularVelocityDrilling, SurveyInstrumentMagneticFluxDensityDrilling, SurveyInstrumentReciprocalLengthDrilling, TemperatureDrilling, TemperatureGradientDrilling, ThermalConductivityDrilling, ThermalConductivityTemperatureGradientDrilling, TorqueDrilling, VolumetricFlowRateDrilling, VolumetricFlowRateOfChangeDrilling</td>
  </tr>
  <tr>
    <td>Electromagnetism</td>
    <td>AngleMagneticFluxDensity, EarthMagneticFluxDensity, ElectricalCurrent, ElectricTension, MagneticFlux, MagneticFluxDensity, SurveyInstrumentMagneticFluxDensityDrilling</td>
  </tr>
  <tr>
    <td>Fluid Dynamics and Hydraulics</td>
    <td>Capacitance, Depth, Density, Dimensionless, FluidShearRate, FluidShearStress, FlowRate, FluidVelocityDrilling, HydraulicConductivity, HydraulicConductivityDrilling, InterfacialTension, InterfacialTensionDrilling, Permeability, PressureLossConstant, PressureLossConstantDrilling, VolumetricFlowRateRateOfChange, WaveNumber, VolumetricFlowRateDrilling, VolumetricFlowRateOfChangeDrilling</td>
  </tr>
  <tr>
    <td>Geometric and Dimensional Analysis</td>
    <td>Area, Curvature, CurvatureDrilling, Dimensionless, ImageScale, Length, PlaneAngle, PlaneAngleDrilling, SolidAngle</td>
  </tr>
  <tr>
    <td>Instrumentation and Measurement</td>
    <td>Frequency, FrequencyRateOfChange, GammaRay, Height, ImageScale, LuminousIntensity, RelativeTemperature, StandardDimensionless, StandardLength, StandardProportion</td>
  </tr>
  <tr>
    <td></td>
    <td></td>
  </tr>
  <tr>
    <td>Mechanics</td>
    <td>Acceleration, AngularVelocity, BlockVelocity, DensitySpeed, Force, ForceGradient, GravitationalLoad, HookLoad, Position, RandomWalk, RateOfPenetration, RotationFrequency, RotationFrequencyRateOfChange, SmallRotationFrequency, SmallTorque, Torque, Velocity, WeightOnBit, YoungModulus</td>
  </tr>
  <tr>
    <td>Structural and Material Science</td>
    <td>CableDiameter, DrillStemMaterialStrength, ElongationGradient, LargeVolume, MaterialStrength, NozzleDiameter, PoreDiameter, PoreSurface, SmallDiameter, SmallLength, SmallProportion, StandardDimensionless, StandardLength, StandardProportion, Volume</td>
  </tr>
  <tr>
    <td>Thermodynamics and Heat Transfer</td>
    <td>CapillaryPressure, Compressibility, Energy, HeatTransferCoefficient, HeatTransferCoefficientDrilling, SpecificHeatCapacity, SpecificHeatCapacityDrilling, SpecificHeatCapacityTemperatureGradient, SpecificHeatCapacityTemperatureGradientDrilling, Temperature, TemperatureGradient, ThermalConductivity, ThermalConductivityDrilling, ThermalConductivityTemperatureGradient, ThermalConductivityTemperatureGradientDrilling</td>
  </tr>
</table>
   
</details>

# Funding

The current work has been funded by the [Research Council of Norway](https://www.forskningsradet.no/) and [Industry partners](https://www.digiwells.no/about/board/) in the framework of the cent for research-based innovation [SFI Digiwells (2020-2028)](https://www.digiwells.no/) focused on Digitalization, Drilling Engineering and GeoSteering.

# Contributors

**Eric Cayeux**, *NORCE Energy Modelling and Automation*

**Gilles Pelfrene**, *NORCE Energy Modelling and Automation*

# License

Part of it is hereby donated **without any limit or warranty** to the Society of Petroleum (SPE) Open Source Drilling Community under the **MIT** license, a sub-committee of the Drilling System Automation Technical Section. Anyone is thus **free to use** the source code of this repository **under its own responsibility**.
