using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion
{
  public partial class BasePhysicalQuantity : Object
  {
     public new enum QuantityEnum 
       {
         AbsorbedDose,  // AbsorbedDose
         Acceleration,  // Acceleration
         AmountSubstancePerArea,  // AmountSubstancePerArea
         AmountSubstancePerAreaRateOfChange,  // AmountSubstancePerAreaRateOfChange
         AmountSubstancePerVolume,  // AmountSubstancePerVolume
         AmountSubstance,  // AmountSubstance
         AmountSubstanceRate,  // AmountSubstanceRate
         AngleGradientPerLength,  // AngleGradientPerLength
         AngleMagneticFluxDensity,  // AngleMagneticFluxDensity
         AngularAcceleration,  // AngularAcceleration
         AngularVelocityPerVolumetricFlowRate,  // AngularVelocityPerVolumetricFlowRate
         AngularVelocity,  // AngularVelocity
         AreaPerAmountSubstance,  // AreaPerAmountSubstance
         AreaPerMass,  // AreaPerMass
         AreaPerVolume,  // AreaPerVolume
         Area,  // Area
         AreaRateOfChange,  // AreaRateOfChange
         BendingMomentGradient,  // BendingMomentGradient
         BendingMoment,  // BendingMoment
         Compressibility,  // Compressibility
         ConsistencyIndexRheology,  // ConsistencyIndexRheology
         Curvature,  // Curvature
         Dimensionless,  // Dimensionless
         DynamicViscosity,  // DynamicViscosity
         ElectricCapacitance,  // ElectricCapacitance
         ElectricCurrent,  // ElectricCurrent
         ElectricResistivity,  // ElectricResistivity
         ElectricTension,  // ElectricTension
         ElongationGradientPerLength,  // ElongationGradientPerLength
         EnergyDensity,  // EnergyDensity
         Energy,  // Energy
         MassDensityGradientPerLength,  // MassDensityGradientPerLength
         MassDensityGradientPerTemperature,  // MassDensityGradientPerTemperature
         MassDensity,  // MassDensity
         MassDensityRateOfChange,  // MassDensityRateOfChange
         ForceGradientPerLength,  // ForceGradientPerLength
         Force,  // Force
         Frequency,  // Frequency
         FrequencyRateOfChange,  // FrequencyRateOfChange
         HeatTransferCoefficient,  // HeatTransferCoefficient
         ImageScale,  // ImageScale
         InterfacialTension,  // InterfacialTension
         Length,  // Length
         LuminousIntensity,  // LuminousIntensity
         MagneticFluxDensity,  // MagneticFluxDensity
         MagneticFlux,  // MagneticFlux
         MassGradientPerLength,  // MassGradientPerLength
         MassRate,  // MassRate
         MaterialStrength,  // MaterialStrength
         PorousMediumPermeability,  // PorousMediumPermeability
         PlaneAngle,  // PlaneAngle
         Power,  // Power
         PressureGradientPerLength,  // PressureGradientPerLength
         PressureLossConstant,  // PressureLossConstant
         Pressure,  // Pressure
         Proportion,  // Proportion
         RandomWalk,  // RandomWalk
         RelativeTemperature,  // RelativeTemperature
         SolidAngle,  // SolidAngle
         IsobaricSpecificHeatCapacity,  // IsobaricSpecificHeatCapacity
         IsobaricSpecificHeatCapacityGradientPerTemperature,  // IsobaricSpecificHeatCapacityGradientPerTemperature
         Stress,  // Stress
         TemperatureGradientPerLength,  // TemperatureGradientPerLength
         Temperature,  // Temperature
         ThermalConductivity,  // ThermalConductivity
         ThermalConductivityGradientPerTemperature,  // ThermalConductivityGradientPerTemperature
         Time,  // Time
         TorqueGradientPerLength,  // TorqueGradientPerLength
         Torque,  // Torque
         Velocity,  // Velocity
         Volume,  // Volume
         VolumetricFlowRateOfChange,  // VolumetricFlowRateOfChange
         VolumetricFlowRate,  // VolumetricFlowRate
         WaveNumber,  // WaveNumber
         Mass,  // Mass
         ForceRateOfChange,  // ForceRateOfChange
         PressureRateOfChange,  // PressureRateOfChange
         TorqueRateOfChange,  // TorqueRateOfChange
         MomentOfArea,  // MomentOfArea
         MomentOfInertia,  // MomentOfInertia
         SpecificVolume,  // SpecificVolume
         SpecificVolumeSquared,  // SpecificVolumeSquared
         MassDensityGradientPerPressure,  // MassDensityGradientPerPressure
         MassDensityGradientPerPressureSquared,  // MassDensityGradientPerPressureSquared
         MassDensityGradientPerPressureTemperature,  // MassDensityGradientPerPressureTemperature
         MassDensityGradientPerPressureSquaredTemperature,  // MassDensityGradientPerPressureSquaredTemperature
         PowerRateOfChange,  // PowerRateOfChange
         ProportionRateOfChange,  // ProportionRateOfChange
         DataStorage,  // DataStorage
         DataTransferRate,  // DataTransferRate
         Diffusivity,  // Diffusivity
         DigitalSymbolRate,  // DigitalSymbolRate
         DoseEquivalent,  // DoseEquivalent
         DoseEquivalentRate,  // DoseEquivalentRate
         ElectricChargePerArea,  // ElectricChargePerArea
         ElectricChargePerMass,  // ElectricChargePerMass
         ElectricChargePerVolume,  // ElectricChargePerVolume
         ElectricCharge,  // ElectricCharge
         ElectricConductance,  // ElectricConductance
         ElectricConductivity,  // ElectricConductivity
         ElectricCurrentDensity,  // ElectricCurrentDensity
         ElectricDipoleMoment,  // ElectricDipoleMoment
         ElectricFieldStrength,  // ElectricFieldStrength
         ElectricResistanceGradientPerLength,  // ElectricResistanceGradientPerLength
         ElectromagneticMoment,  // ElectromagneticMoment
         ForceArea,  // ForceArea
         ForcePerVolume,  // ForcePerVolume
         HeatCapacity,  // HeatCapacity
         Illuminance,  // Illuminance
         Inductance,  // Inductance
         KinematicViscosity,  // KinematicViscosity
         LengthPerAngle,  // LengthPerAngle
         LengthPerMass,  // LengthPerMass
         LengthPerPressure,  // LengthPerPressure
         LengthPerTemperature,  // LengthPerTemperature
         LengthPerVolume,  // LengthPerVolume
         LightExposure,  // LightExposure
         LinearThermalExpansionCoefficient,  // LinearThermalExpansionCoefficient
         Luminance,  // Luminance
         LuminousEfficacy,  // LuminousEfficacy
         LuminousEnergy,  // LuminousEnergy
         LuminousExitance,  // LuminousExitance
         LuminousFlux,  // LuminousFlux
         MagneticFieldStrength,  // MagneticFieldStrength
         MagneticFluxDensityGradientPerLength,  // MagneticFluxDensityGradientPerLength
         MagneticPermeability,  // MagneticPermeability
         MagneticVectorPotential,  // MagneticVectorPotential
         MassLength,  // MassLength
         MassPerArea,  // MassPerArea
         MassRateGradientPerLength,  // MassRateGradientPerLength
         MassRatePerArea,  // MassRatePerArea
         MolarEnergy,  // MolarEnergy
         MolarHeatCapacity,  // MolarHeatCapacity
         MolarMass,  // MolarMass
         MolarVolume,  // MolarVolume
         Momentum,  // Momentum
         PermeabilityLength,  // PermeabilityLength
         Permittivity,  // Permittivity
         PlaneAnglePerVolume,  // PlaneAnglePerVolume
         PowerPerArea,  // PowerPerArea
         PowerPerVolume,  // PowerPerVolume
         ProductivityIndex,  // ProductivityIndex
         Radiance,  // Radiance
         RadiantIntensity,  // RadiantIntensity
         Radioactivity,  // Radioactivity
         ReciprocalArea,  // ReciprocalArea
         ReciprocalElectricTension,  // ReciprocalElectricTension
         ReciprocalForce,  // ReciprocalForce
         ReciprocalMass,  // ReciprocalMass
         ReciprocalTime,  // ReciprocalTime
         ReciprocalVolume,  // ReciprocalVolume
         Reluctance,  // Reluctance
         SectionModulus,  // SectionModulus
         SpecificActivity,  // SpecificActivity
         SpecificEnergy,  // SpecificEnergy
         SpecificProductivityIndex,  // SpecificProductivityIndex
         ThermalConductance,  // ThermalConductance
         ThermalInsulance,  // ThermalInsulance
         ThermalResistance,  // ThermalResistance
         TimePerLength,  // TimePerLength
         TimePerMass,  // TimePerMass
         TimePerVolume,  // TimePerVolume
         VolumeGradientPerLength,  // VolumeGradientPerLength
         VolumePerArea,  // VolumePerArea
         VolumePerAreaRateOfChange,  // VolumePerAreaRateOfChange
         VolumePerLengthRateOfChange,  // VolumePerLengthRateOfChange
         VolumePerPressure,  // VolumePerPressure
         VolumePerPressureRateOfChange,  // VolumePerPressureRateOfChange
         VolumePerVolumeRateOfChange,  // VolumePerVolumeRateOfChange
         VolumetricFlowRateGradientPerLength,  // VolumetricFlowRateGradientPerLength
         VolumetricFlowRatePerArea,  // VolumetricFlowRatePerArea
         VolumetricHeatTransferCoefficient,  // VolumetricHeatTransferCoefficient
         VolumetricThermalExpansionCoefficient,  // VolumetricThermalExpansionCoefficient
         ElectricalMobility,  // ElectricalMobility
         VolumePerAngle,  // VolumePerAngle
         Mobility,  // Mobility
         EnergyPerArea,  // EnergyPerArea
         MassPerEnergy,  // MassPerEnergy
         PressurePerVolume,  // PressurePerVolume
         PressureTimePerVolume,  // PressureTimePerVolume
         RelativeTemperaturePerPressure,  // RelativeTemperaturePerPressure
         TemperatureRateOfChange,  // TemperatureRateOfChange
         VolumePerEnergy,  // VolumePerEnergy
         WorkGradient,  // WorkGradient
         DiameterSmall,  // DiameterSmall
         DimensionLessStandard,  // DimensionLessStandard
         EarthMagneticFluxDensity,  // EarthMagneticFluxDensity
         ElasticModulus,  // ElasticModulus
         LengthSmall,  // LengthSmall
         RotationalFrequency,  // RotationalFrequency
         HydraulicConductivity,  // HydraulicConductivity
         VolumeLarge,  // VolumeLarge
         RotationalFrequencyRateOfChange,  // RotationalFrequencyRateOfChange
         Tension,  // Tension
         ProportionStandard,  // ProportionStandard
         LengthStandard,  // LengthStandard
         FluidShearRate,  // FluidShearRate
         FluidShearStress,  // FluidShearStress
         TorqueSmall,  // TorqueSmall
         RotationalFrequencySmall,  // RotationalFrequencySmall
         ProportionSmall,  // ProportionSmall
         Porosity,  // Porosity
         StrokeFrequency,  // StrokeFrequency
         ShockRate,  // ShockRate
         PlaneAngleGeodesic,  // PlaneAngleGeodesic
         PlaneAngleStandard // PlaneAngleStandard
       }
    protected static new Dictionary<QuantityEnum, Guid> enumLookUp_ = new Dictionary<QuantityEnum, Guid>()
    {
         {QuantityEnum.AbsorbedDose, new Guid("230dfcc5-efaa-4d09-b1c2-d64557bc59ef")},  // AbsorbedDose
         {QuantityEnum.Acceleration, new Guid("454a7b6b-a921-428e-8aa7-a4a636a58e34")},  // Acceleration
         {QuantityEnum.AmountSubstancePerArea, new Guid("4d6cfb0d-27ce-42df-a46e-4555268608b8")},  // AmountSubstancePerArea
         {QuantityEnum.AmountSubstancePerAreaRateOfChange, new Guid("ad4557d8-15fc-46e5-821e-e908210b5040")},  // AmountSubstancePerAreaRateOfChange
         {QuantityEnum.AmountSubstancePerVolume, new Guid("0ee126c6-c0e5-46cf-8427-b4d2eddf0d62")},  // AmountSubstancePerVolume
         {QuantityEnum.AmountSubstance, new Guid("200be1eb-c278-447c-9b15-32d20fc778b9")},  // AmountSubstance
         {QuantityEnum.AmountSubstanceRate, new Guid("4356fa06-b519-4c6b-8f62-23c0f5a04fa1")},  // AmountSubstanceRate
         {QuantityEnum.AngleGradientPerLength, new Guid("aed9c464-1073-448b-be62-a6a0c2a53dbc")},  // AngleGradientPerLength
         {QuantityEnum.AngleMagneticFluxDensity, new Guid("03bb57e6-ca8b-4741-a211-9cf57c8fd177")},  // AngleMagneticFluxDensity
         {QuantityEnum.AngularAcceleration, new Guid("8b33d305-f77e-4631-9818-7ef574bd0c02")},  // AngularAcceleration
         {QuantityEnum.AngularVelocityPerVolumetricFlowRate, new Guid("0a2e0354-d0ed-4963-b902-14e653f9a956")},  // AngularVelocityPerVolumetricFlowRate
         {QuantityEnum.AngularVelocity, new Guid("688ccd2b-6a30-4ccc-8580-a80c3a5803fa")},  // AngularVelocity
         {QuantityEnum.AreaPerAmountSubstance, new Guid("96f11a30-1115-47a7-8b59-6c4c94d0e2d0")},  // AreaPerAmountSubstance
         {QuantityEnum.AreaPerMass, new Guid("54417926-5232-4ee1-8dde-1c1e0689834d")},  // AreaPerMass
         {QuantityEnum.AreaPerVolume, new Guid("2b07d4f7-057b-44dd-9887-b54de6c73ea8")},  // AreaPerVolume
         {QuantityEnum.Area, new Guid("2a892bab-1b39-4ae4-b2d2-989621b09557")},  // Area
         {QuantityEnum.AreaRateOfChange, new Guid("922903d3-499c-4fc9-9d0e-da30abed1eff")},  // AreaRateOfChange
         {QuantityEnum.BendingMomentGradient, new Guid("5be8f51c-b97c-4a8e-a859-08459bd26f55")},  // BendingMomentGradient
         {QuantityEnum.BendingMoment, new Guid("82b03224-e2af-47e9-bcf5-810d2506f4e2")},  // BendingMoment
         {QuantityEnum.Compressibility, new Guid("1e7af8b8-0267-4d5d-a162-59123a8fde14")},  // Compressibility
         {QuantityEnum.ConsistencyIndexRheology, new Guid("05571702-00e6-47d7-8590-fd3983645406")},  // ConsistencyIndexRheology
         {QuantityEnum.Curvature, new Guid("bbfe7349-8cf5-4ca0-8a84-ffe66d7f33d0")},  // Curvature
         {QuantityEnum.Dimensionless, new Guid("790ae2cd-170c-4908-b2b9-163ba95f5b43")},  // Dimensionless
         {QuantityEnum.DynamicViscosity, new Guid("c830517f-5915-4a8f-ba83-bd102c0a935f")},  // DynamicViscosity
         {QuantityEnum.ElectricCapacitance, new Guid("9b284ff7-57bb-4ee0-bdbc-5fb7b80f3ae3")},  // ElectricCapacitance
         {QuantityEnum.ElectricCurrent, new Guid("a322deae-e965-41bf-b4fe-a7530d33c9a0")},  // ElectricCurrent
         {QuantityEnum.ElectricResistivity, new Guid("c6c87a27-c04d-4658-8a71-1e46eb3bfd80")},  // ElectricResistivity
         {QuantityEnum.ElectricTension, new Guid("da5094a4-7481-4246-9def-1bd3b6f893a1")},  // ElectricTension
         {QuantityEnum.ElongationGradientPerLength, new Guid("3c6176f8-8f74-4fbf-bb65-207ed8b0a120")},  // ElongationGradientPerLength
         {QuantityEnum.EnergyDensity, new Guid("9e82436a-392e-428a-8ee9-0998027c3c46")},  // EnergyDensity
         {QuantityEnum.Energy, new Guid("3be49c73-d2d1-40a2-b15f-07a1606d8179")},  // Energy
         {QuantityEnum.MassDensityGradientPerLength, new Guid("037e0326-5095-4c82-ba1b-4df594243cda")},  // MassDensityGradientPerLength
         {QuantityEnum.MassDensityGradientPerTemperature, new Guid("2d788f1e-db66-49c3-8eb6-313152cd8e3c")},  // MassDensityGradientPerTemperature
         {QuantityEnum.MassDensity, new Guid("5754358c-9359-4bb0-8eb4-08602d19c6af")},  // MassDensity
         {QuantityEnum.MassDensityRateOfChange, new Guid("be272506-8c7a-4eff-9a05-ad4d07f36e11")},  // MassDensityRateOfChange
         {QuantityEnum.ForceGradientPerLength, new Guid("e5212340-1147-4cad-9f71-5cd9d4208ffd")},  // ForceGradientPerLength
         {QuantityEnum.Force, new Guid("af9fd237-14d8-4b75-8d0b-34ea8961c20b")},  // Force
         {QuantityEnum.Frequency, new Guid("8a1ff3d9-95c9-43e1-abb4-4ae9b8df861e")},  // Frequency
         {QuantityEnum.FrequencyRateOfChange, new Guid("e9d5bfe9-428b-4df0-9fe5-d9ad17e6a0cb")},  // FrequencyRateOfChange
         {QuantityEnum.HeatTransferCoefficient, new Guid("08c247bc-a55b-460e-a9a7-150faf10bdff")},  // HeatTransferCoefficient
         {QuantityEnum.ImageScale, new Guid("a3f230b0-a70b-40dd-9305-39e63bb1821b")},  // ImageScale
         {QuantityEnum.InterfacialTension, new Guid("6c2da24b-fa92-415d-9161-150de87dad4c")},  // InterfacialTension
         {QuantityEnum.Length, new Guid("96058475-80c4-4394-b21a-afd2fb1594c8")},  // Length
         {QuantityEnum.LuminousIntensity, new Guid("fd02d171-cd96-4a41-84cc-431b50ba879b")},  // LuminousIntensity
         {QuantityEnum.MagneticFluxDensity, new Guid("b9a3f96b-8861-4b03-9c8a-3c0d7d6ec139")},  // MagneticFluxDensity
         {QuantityEnum.MagneticFlux, new Guid("0d36345b-624d-47c1-9d20-e627a6c6c13a")},  // MagneticFlux
         {QuantityEnum.MassGradientPerLength, new Guid("b8694592-8f8d-4684-b0ba-c88de50c8486")},  // MassGradientPerLength
         {QuantityEnum.MassRate, new Guid("3dd05c4c-3d6d-49ae-a878-5a5e4a6e7acf")},  // MassRate
         {QuantityEnum.MaterialStrength, new Guid("d9ca8230-a07a-45c0-ba67-051b70607c40")},  // MaterialStrength
         {QuantityEnum.PorousMediumPermeability, new Guid("413da2c1-ebad-454a-ae14-1a8620f8f59c")},  // PorousMediumPermeability
         {QuantityEnum.PlaneAngle, new Guid("751a8f44-d938-4319-a422-a753962fd91f")},  // PlaneAngle
         {QuantityEnum.Power, new Guid("6fd69f30-a219-4d56-a1dd-000d8175e2ed")},  // Power
         {QuantityEnum.PressureGradientPerLength, new Guid("62eb6afe-bd7d-48dd-b4fd-de40e9f3c632")},  // PressureGradientPerLength
         {QuantityEnum.PressureLossConstant, new Guid("6417f6e0-969d-43f2-bee6-249199ec1697")},  // PressureLossConstant
         {QuantityEnum.Pressure, new Guid("0f282508-9223-489d-86e6-36307f987045")},  // Pressure
         {QuantityEnum.Proportion, new Guid("10d2d588-19b8-4822-9240-e1d278d99e32")},  // Proportion
         {QuantityEnum.RandomWalk, new Guid("e3d17133-1c98-4ef2-8b1b-f0d935a4c1e4")},  // RandomWalk
         {QuantityEnum.RelativeTemperature, new Guid("58dadec7-7858-414b-8d7b-66504d5c2793")},  // RelativeTemperature
         {QuantityEnum.SolidAngle, new Guid("26a7767a-ea4d-417e-a1ef-b7fe674dcd3f")},  // SolidAngle
         {QuantityEnum.IsobaricSpecificHeatCapacity, new Guid("e5c75fa9-0102-42dc-bb0c-830fe9fca2b9")},  // IsobaricSpecificHeatCapacity
         {QuantityEnum.IsobaricSpecificHeatCapacityGradientPerTemperature, new Guid("3a317540-3db4-47a1-a566-33b6f39b7540")},  // IsobaricSpecificHeatCapacityGradientPerTemperature
         {QuantityEnum.Stress, new Guid("e4aa819b-a385-418b-bbca-cfb1421093f5")},  // Stress
         {QuantityEnum.TemperatureGradientPerLength, new Guid("4c1819d5-008b-4613-b62a-3f5d91b08ee7")},  // TemperatureGradientPerLength
         {QuantityEnum.Temperature, new Guid("16130f2d-72a8-44a5-beaa-adbb5a1a7b21")},  // Temperature
         {QuantityEnum.ThermalConductivity, new Guid("ca23212e-8f2d-4041-89f6-ac8bfa8604fa")},  // ThermalConductivity
         {QuantityEnum.ThermalConductivityGradientPerTemperature, new Guid("5e509f43-8fb4-490e-b9a5-59d7393761c0")},  // ThermalConductivityGradientPerTemperature
         {QuantityEnum.Time, new Guid("7106f7cb-ddf2-4e2f-9e21-b19bc83eb248")},  // Time
         {QuantityEnum.TorqueGradientPerLength, new Guid("002104cd-25d6-438d-afe3-065ff392b294")},  // TorqueGradientPerLength
         {QuantityEnum.Torque, new Guid("3eb9e01e-48fa-430e-82c6-3aee4d359ac4")},  // Torque
         {QuantityEnum.Velocity, new Guid("b3fd17fe-ce71-4ef3-ac99-cf4f5756e81a")},  // Velocity
         {QuantityEnum.Volume, new Guid("69151432-d2ed-4473-a3dc-334f8e6daaa6")},  // Volume
         {QuantityEnum.VolumetricFlowRateOfChange, new Guid("7f4f645c-e23e-41bc-bbcc-1dbcef53318e")},  // VolumetricFlowRateOfChange
         {QuantityEnum.VolumetricFlowRate, new Guid("9c4eb2bc-413f-456e-ae6b-b1055be8e839")},  // VolumetricFlowRate
         {QuantityEnum.WaveNumber, new Guid("3709c98d-d471-41dd-bfde-81c4458757e5")},  // WaveNumber
         {QuantityEnum.Mass, new Guid("99d13248-c303-4b3d-b062-af98de701d6f")},  // Mass
         {QuantityEnum.ForceRateOfChange, new Guid("2f28f6d5-5b01-4fd0-9924-bf84250f6092")},  // ForceRateOfChange
         {QuantityEnum.PressureRateOfChange, new Guid("611830b0-739e-42ef-8215-98e0a4e1df3b")},  // PressureRateOfChange
         {QuantityEnum.TorqueRateOfChange, new Guid("e94ee582-62bd-472b-9188-1f423729e99e")},  // TorqueRateOfChange
         {QuantityEnum.MomentOfArea, new Guid("669f44f8-ed5f-43e0-9b63-adf1b6b9e865")},  // MomentOfArea
         {QuantityEnum.MomentOfInertia, new Guid("cf7c69b0-b4d7-45d2-9d7d-4714996424c0")},  // MomentOfInertia
         {QuantityEnum.SpecificVolume, new Guid("ad0b041e-4bfe-4e4a-9c9f-1b800d2332ba")},  // SpecificVolume
         {QuantityEnum.SpecificVolumeSquared, new Guid("a266621c-b583-443f-ae53-1ad46d90252b")},  // SpecificVolumeSquared
         {QuantityEnum.MassDensityGradientPerPressure, new Guid("54da476c-5a35-4274-93bf-1a2d8eede435")},  // MassDensityGradientPerPressure
         {QuantityEnum.MassDensityGradientPerPressureSquared, new Guid("885ebdc2-2800-462e-93fa-cbaaffd12b6e")},  // MassDensityGradientPerPressureSquared
         {QuantityEnum.MassDensityGradientPerPressureTemperature, new Guid("1f5a6169-f514-4d86-a030-956efc8cb4f1")},  // MassDensityGradientPerPressureTemperature
         {QuantityEnum.MassDensityGradientPerPressureSquaredTemperature, new Guid("2d4b23e0-01ea-472f-85c1-1ced4d6507a6")},  // MassDensityGradientPerPressureSquaredTemperature
         {QuantityEnum.PowerRateOfChange, new Guid("32774627-7eb2-4cab-8270-ae075ea5f335")},  // PowerRateOfChange
         {QuantityEnum.ProportionRateOfChange, new Guid("ea1d1297-7726-474c-8bc0-804b8f3b1f4a")},  // ProportionRateOfChange
         {QuantityEnum.DataStorage, new Guid("62c44d7b-45ca-48d0-8715-692edadf31b7")},  // DataStorage
         {QuantityEnum.DataTransferRate, new Guid("f7b2946a-8d28-4d5f-9a88-b255c6811b59")},  // DataTransferRate
         {QuantityEnum.Diffusivity, new Guid("a88883dc-2db9-4f2d-b34b-61f0f4da910d")},  // Diffusivity
         {QuantityEnum.DigitalSymbolRate, new Guid("d005f39e-8afb-40c0-a2ed-0305743e600d")},  // DigitalSymbolRate
         {QuantityEnum.DoseEquivalent, new Guid("b681bb4a-4350-4a9c-8cf5-344226311185")},  // DoseEquivalent
         {QuantityEnum.DoseEquivalentRate, new Guid("d94f46ab-9d1e-41b4-9e09-2172fdb085ff")},  // DoseEquivalentRate
         {QuantityEnum.ElectricChargePerArea, new Guid("14a547f7-dfb7-4162-875d-656c2a24743d")},  // ElectricChargePerArea
         {QuantityEnum.ElectricChargePerMass, new Guid("ea5ffd73-5045-4a48-a0a8-8cce3e82d5fb")},  // ElectricChargePerMass
         {QuantityEnum.ElectricChargePerVolume, new Guid("f58d46c1-bef9-41df-9273-62b3894c0e27")},  // ElectricChargePerVolume
         {QuantityEnum.ElectricCharge, new Guid("a41a4b57-58ef-481f-b959-29dcf6b69dd7")},  // ElectricCharge
         {QuantityEnum.ElectricConductance, new Guid("d787f9a4-7582-46dd-aa4c-52aec757b612")},  // ElectricConductance
         {QuantityEnum.ElectricConductivity, new Guid("17cc6e0e-aa7f-4d35-bd42-bf23c2bcb41b")},  // ElectricConductivity
         {QuantityEnum.ElectricCurrentDensity, new Guid("96bc308d-e056-4aec-bc7d-8df207a0eddd")},  // ElectricCurrentDensity
         {QuantityEnum.ElectricDipoleMoment, new Guid("594992c4-b617-4879-8449-d0df6facab57")},  // ElectricDipoleMoment
         {QuantityEnum.ElectricFieldStrength, new Guid("9f7dc923-2e9d-46aa-95ec-8842af07ffd7")},  // ElectricFieldStrength
         {QuantityEnum.ElectricResistanceGradientPerLength, new Guid("9d65e20d-c896-494a-9533-c32938fa65dc")},  // ElectricResistanceGradientPerLength
         {QuantityEnum.ElectromagneticMoment, new Guid("4435dc43-c681-4e1f-ac4d-fe6bbf3f9c4f")},  // ElectromagneticMoment
         {QuantityEnum.ForceArea, new Guid("b08fb64a-9b44-4585-a5cc-5f8d3fcacb08")},  // ForceArea
         {QuantityEnum.ForcePerVolume, new Guid("595b57c6-2b90-4090-a9ee-0f3a1481edf4")},  // ForcePerVolume
         {QuantityEnum.HeatCapacity, new Guid("eb719933-d2de-4894-b72d-d5a66d89e54b")},  // HeatCapacity
         {QuantityEnum.Illuminance, new Guid("f0bdfefc-c4dd-4bf5-84a6-1abeee605b1a")},  // Illuminance
         {QuantityEnum.Inductance, new Guid("4b300ae8-d2b2-41cd-bcfa-6e023ee46de6")},  // Inductance
         {QuantityEnum.KinematicViscosity, new Guid("19c3e97f-6b3f-43b4-9201-5a91db440ae2")},  // KinematicViscosity
         {QuantityEnum.LengthPerAngle, new Guid("ffea77a1-f294-4c32-8cc2-6d6656071093")},  // LengthPerAngle
         {QuantityEnum.LengthPerMass, new Guid("b0dbfd93-df83-4351-abd4-a4afb4da3309")},  // LengthPerMass
         {QuantityEnum.LengthPerPressure, new Guid("3dbc03bb-db8b-49cb-b261-1e4dd657f594")},  // LengthPerPressure
         {QuantityEnum.LengthPerTemperature, new Guid("c6bd0e8f-1707-46e1-8d4a-7073b3b710e9")},  // LengthPerTemperature
         {QuantityEnum.LengthPerVolume, new Guid("7cf7f819-1c5a-4cff-a003-7cf6f5d2d9c7")},  // LengthPerVolume
         {QuantityEnum.LightExposure, new Guid("14e2d5f4-f868-46eb-8101-8a4867b57b4f")},  // LightExposure
         {QuantityEnum.LinearThermalExpansionCoefficient, new Guid("e2a898d5-5110-415e-84e9-4923003781da")},  // LinearThermalExpansionCoefficient
         {QuantityEnum.Luminance, new Guid("27adf373-4669-44aa-b524-2edbc27a3af3")},  // Luminance
         {QuantityEnum.LuminousEfficacy, new Guid("cea058e0-f4a8-4f46-966c-f460eff93031")},  // LuminousEfficacy
         {QuantityEnum.LuminousEnergy, new Guid("669c697c-b808-4873-a177-55c240c73619")},  // LuminousEnergy
         {QuantityEnum.LuminousExitance, new Guid("bf6a07d5-0f5f-4397-b537-4748c13d373a")},  // LuminousExitance
         {QuantityEnum.LuminousFlux, new Guid("e294517a-2b69-46f0-ae14-bb43b3f679bb")},  // LuminousFlux
         {QuantityEnum.MagneticFieldStrength, new Guid("db6a2e38-bbca-4ef0-b466-beee214160a6")},  // MagneticFieldStrength
         {QuantityEnum.MagneticFluxDensityGradientPerLength, new Guid("6ed74495-1719-440b-88b9-be740ef4d5b0")},  // MagneticFluxDensityGradientPerLength
         {QuantityEnum.MagneticPermeability, new Guid("406ec51a-b1a9-42f1-9ec8-3aeaec3fb7f3")},  // MagneticPermeability
         {QuantityEnum.MagneticVectorPotential, new Guid("b8845077-919a-4d8b-a856-b97f8cd8d74b")},  // MagneticVectorPotential
         {QuantityEnum.MassLength, new Guid("1b197346-779c-4d77-839b-0e28bcb96a81")},  // MassLength
         {QuantityEnum.MassPerArea, new Guid("79400a63-c7dd-475e-8114-dbc09b1c8d41")},  // MassPerArea
         {QuantityEnum.MassRateGradientPerLength, new Guid("05891b6e-c096-4901-b941-1c23697153d9")},  // MassRateGradientPerLength
         {QuantityEnum.MassRatePerArea, new Guid("36e93f6c-3812-403c-b3c6-fa2999b47980")},  // MassRatePerArea
         {QuantityEnum.MolarEnergy, new Guid("f839e9da-af77-4cea-95b2-40deffc6b1db")},  // MolarEnergy
         {QuantityEnum.MolarHeatCapacity, new Guid("c75d3d79-1040-4cb2-aa6a-9fe32d04a942")},  // MolarHeatCapacity
         {QuantityEnum.MolarMass, new Guid("92841dda-643c-4c0e-8f7a-46aec8183496")},  // MolarMass
         {QuantityEnum.MolarVolume, new Guid("2ba239f8-e96c-4a63-9fb2-a0dc2a045e08")},  // MolarVolume
         {QuantityEnum.Momentum, new Guid("cc3d8ab9-5131-43a7-84e5-52a553d2bf38")},  // Momentum
         {QuantityEnum.PermeabilityLength, new Guid("6306fee6-0383-4ab9-9044-ee72c21f14bd")},  // PermeabilityLength
         {QuantityEnum.Permittivity, new Guid("efe69117-c302-419f-acc1-836b295e845d")},  // Permittivity
         {QuantityEnum.PlaneAnglePerVolume, new Guid("79958c90-85a4-463e-88be-4e328d86907e")},  // PlaneAnglePerVolume
         {QuantityEnum.PowerPerArea, new Guid("c528cf86-98de-45b9-86db-5e4a8215db8d")},  // PowerPerArea
         {QuantityEnum.PowerPerVolume, new Guid("835d08b5-e817-44ab-867a-a55f7cc3f9fd")},  // PowerPerVolume
         {QuantityEnum.ProductivityIndex, new Guid("7320b04f-86cf-44b9-b386-d512fbf23240")},  // ProductivityIndex
         {QuantityEnum.Radiance, new Guid("d721aed3-da54-469e-961a-63100b2711c9")},  // Radiance
         {QuantityEnum.RadiantIntensity, new Guid("8884a026-e09f-4fba-8aa2-7af355d21e44")},  // RadiantIntensity
         {QuantityEnum.Radioactivity, new Guid("c1792f77-6c52-44ab-b811-9ceacb6dc5de")},  // Radioactivity
         {QuantityEnum.ReciprocalArea, new Guid("1fff82b1-2e6b-4f74-b814-a655ac4db93c")},  // ReciprocalArea
         {QuantityEnum.ReciprocalElectricTension, new Guid("4f8a80aa-8424-4daf-ba9e-a3096f25f758")},  // ReciprocalElectricTension
         {QuantityEnum.ReciprocalForce, new Guid("98977886-d1de-499e-b365-e0dba1c48bf0")},  // ReciprocalForce
         {QuantityEnum.ReciprocalMass, new Guid("87278911-3f4b-4a76-9d72-01adecda370f")},  // ReciprocalMass
         {QuantityEnum.ReciprocalTime, new Guid("87c47f73-1e79-44c5-87ce-d81514305184")},  // ReciprocalTime
         {QuantityEnum.ReciprocalVolume, new Guid("9967a650-3116-464a-9746-6b2e7abec518")},  // ReciprocalVolume
         {QuantityEnum.Reluctance, new Guid("fd9fb2d9-98da-4cd4-9ee4-7ae5cb5eaf1e")},  // Reluctance
         {QuantityEnum.SectionModulus, new Guid("1f785970-5655-40b3-b9eb-0721ab42b452")},  // SectionModulus
         {QuantityEnum.SpecificActivity, new Guid("7f6b0d3a-02e2-48af-9476-f303c62fd52c")},  // SpecificActivity
         {QuantityEnum.SpecificEnergy, new Guid("596254b1-4a99-43a8-b21d-b8b3ebca6335")},  // SpecificEnergy
         {QuantityEnum.SpecificProductivityIndex, new Guid("9d2c471c-4adc-42e9-8941-755fe16e4a2c")},  // SpecificProductivityIndex
         {QuantityEnum.ThermalConductance, new Guid("65bb59df-6dfc-430d-b7a6-28405c56a73f")},  // ThermalConductance
         {QuantityEnum.ThermalInsulance, new Guid("f5776adf-201c-4222-8a8f-b4bed4184a79")},  // ThermalInsulance
         {QuantityEnum.ThermalResistance, new Guid("72d2e7a8-926d-4a12-a1a1-34a8a4e591d3")},  // ThermalResistance
         {QuantityEnum.TimePerLength, new Guid("6ae01714-16a5-48d0-82f3-a466478d51e4")},  // TimePerLength
         {QuantityEnum.TimePerMass, new Guid("6c493b1b-89dc-4bd3-997b-8080f2381b7e")},  // TimePerMass
         {QuantityEnum.TimePerVolume, new Guid("0c8dc346-29f5-4468-9259-5b93246113e6")},  // TimePerVolume
         {QuantityEnum.VolumeGradientPerLength, new Guid("a9efb630-0e03-4047-a777-f2277cc66d5b")},  // VolumeGradientPerLength
         {QuantityEnum.VolumePerArea, new Guid("4304481e-e6c7-48e0-93f7-d3b7c077edc6")},  // VolumePerArea
         {QuantityEnum.VolumePerAreaRateOfChange, new Guid("bae8948e-2a16-4664-b426-fd29f3548c2b")},  // VolumePerAreaRateOfChange
         {QuantityEnum.VolumePerLengthRateOfChange, new Guid("da543737-1db8-44c1-bfa6-785095ac4a67")},  // VolumePerLengthRateOfChange
         {QuantityEnum.VolumePerPressure, new Guid("1b1a2335-7755-4e54-b931-adb70ae48c2d")},  // VolumePerPressure
         {QuantityEnum.VolumePerPressureRateOfChange, new Guid("ecb46cc0-4de0-4ca4-88b9-d1051dea3579")},  // VolumePerPressureRateOfChange
         {QuantityEnum.VolumePerVolumeRateOfChange, new Guid("af6ccc7a-2ae6-4dd6-becb-ea425e461991")},  // VolumePerVolumeRateOfChange
         {QuantityEnum.VolumetricFlowRateGradientPerLength, new Guid("7bb02c43-2159-4ca4-b6cd-3e6addd90534")},  // VolumetricFlowRateGradientPerLength
         {QuantityEnum.VolumetricFlowRatePerArea, new Guid("883203d0-f253-4247-85f0-7193e100379c")},  // VolumetricFlowRatePerArea
         {QuantityEnum.VolumetricHeatTransferCoefficient, new Guid("10e62909-a85f-42de-a18b-fc941faed451")},  // VolumetricHeatTransferCoefficient
         {QuantityEnum.VolumetricThermalExpansionCoefficient, new Guid("6ff516c8-6817-4355-bf29-408fc20d7320")},  // VolumetricThermalExpansionCoefficient
         {QuantityEnum.ElectricalMobility, new Guid("e3b12404-1c06-46ea-ac83-503c168c91cd")},  // ElectricalMobility
         {QuantityEnum.VolumePerAngle, new Guid("b7289689-e15e-40c5-9d8c-7b07b1677881")},  // VolumePerAngle
         {QuantityEnum.Mobility, new Guid("0dc5157f-9f12-4ef7-b4d4-5f7a3616183c")},  // Mobility
         {QuantityEnum.EnergyPerArea, new Guid("31fb561e-bda5-4459-b7aa-5701a8cc93e2")},  // EnergyPerArea
         {QuantityEnum.MassPerEnergy, new Guid("cefe77c3-88c2-491e-aadf-a62218e1329b")},  // MassPerEnergy
         {QuantityEnum.PressurePerVolume, new Guid("b4f048cf-90f2-4ef2-b4fa-c0cf57eb6044")},  // PressurePerVolume
         {QuantityEnum.PressureTimePerVolume, new Guid("bcd53761-f5f0-4e95-927d-55003466b0eb")},  // PressureTimePerVolume
         {QuantityEnum.RelativeTemperaturePerPressure, new Guid("bbaa29a4-af78-428d-895a-fc192d9ff1ca")},  // RelativeTemperaturePerPressure
         {QuantityEnum.TemperatureRateOfChange, new Guid("57ed8659-32a4-4812-b324-67883f627836")},  // TemperatureRateOfChange
         {QuantityEnum.VolumePerEnergy, new Guid("d1bdac66-5afb-4514-b019-bd99ef87166e")},  // VolumePerEnergy
         {QuantityEnum.WorkGradient, new Guid("670a0017-e2ac-45c5-a6a4-965ab686a950")},  // WorkGradient
         {QuantityEnum.DiameterSmall, new Guid("d07d00aa-35aa-41c6-a52d-ad51c3f4e97f")},  // DiameterSmall
         {QuantityEnum.DimensionLessStandard, new Guid("5d356437-ab4e-4de7-8219-1f4988315dee")},  // DimensionLessStandard
         {QuantityEnum.EarthMagneticFluxDensity, new Guid("ed95aca5-aaf9-4822-b045-342ffcd06ca7")},  // EarthMagneticFluxDensity
         {QuantityEnum.ElasticModulus, new Guid("7ffbcc35-b46f-4656-baf5-c92be501f0ec")},  // ElasticModulus
         {QuantityEnum.LengthSmall, new Guid("3bb73c6f-c40e-4e54-b59a-962bec9aafed")},  // LengthSmall
         {QuantityEnum.RotationalFrequency, new Guid("f6f7ab6f-3003-49d2-a17d-92a0f81938f2")},  // RotationalFrequency
         {QuantityEnum.HydraulicConductivity, new Guid("04df2b82-aff0-485a-855e-3d2aa53e12eb")},  // HydraulicConductivity
         {QuantityEnum.VolumeLarge, new Guid("f8ab1afa-7b99-403b-9410-93598bbb4089")},  // VolumeLarge
         {QuantityEnum.RotationalFrequencyRateOfChange, new Guid("ed24a9f7-b70d-4f39-a992-241f25e1a77e")},  // RotationalFrequencyRateOfChange
         {QuantityEnum.Tension, new Guid("7c4e127d-aa65-4796-a962-c2c666c4fdd0")},  // Tension
         {QuantityEnum.ProportionStandard, new Guid("97555d61-9fc3-4769-9143-6bc2bf51b2d7")},  // ProportionStandard
         {QuantityEnum.LengthStandard, new Guid("3716ad37-2b0c-4c0b-8936-6c9cdb47ad1d")},  // LengthStandard
         {QuantityEnum.FluidShearRate, new Guid("d3aa72c5-2fc0-4024-902e-6775d63f3231")},  // FluidShearRate
         {QuantityEnum.FluidShearStress, new Guid("b8f8f4f5-1925-4eaf-87c2-2adfdf618454")},  // FluidShearStress
         {QuantityEnum.TorqueSmall, new Guid("adf7b170-8d24-4c9f-93e1-40179f361d8c")},  // TorqueSmall
         {QuantityEnum.RotationalFrequencySmall, new Guid("b7ab1664-3d56-4ae5-842a-e4d6d0475ef9")},  // RotationalFrequencySmall
         {QuantityEnum.ProportionSmall, new Guid("875392e2-ef43-45f7-a19b-19c51eaba248")},  // ProportionSmall
         {QuantityEnum.Porosity, new Guid("2f6516a1-47cc-498f-8271-e84150183665")},  // Porosity
         {QuantityEnum.StrokeFrequency, new Guid("86fd37e4-3ebf-42ec-9eb2-1e65f7abf29e")},  // StrokeFrequency
         {QuantityEnum.ShockRate, new Guid("0076d96f-bfc3-4f98-8541-4fd12e4bcbff")},  // ShockRate
         {QuantityEnum.PlaneAngleGeodesic, new Guid("fc88c9ff-fa0a-4406-b397-015f3ca062bc")},  // PlaneAngleGeodesic
         {QuantityEnum.PlaneAngleStandard, new Guid("3773fc19-be1a-4604-91f8-6e4aefb44757")} // PlaneAngleStandard
    };
  }
}

namespace OSDC.UnitConversion.Conversion
{
  public partial class AbsorbedDoseQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Gray,  // gray
         Milligray,  // milligray
         Rad // rad
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Gray, new Guid("4ee9a141-da57-40fd-90c2-f224da907d70")},  // gray
         {UnitChoicesEnum.Milligray, new Guid("35957208-54e9-407d-b4b8-781ac093036b")},  // milligray
         {UnitChoicesEnum.Rad, new Guid("d367e5fd-3943-4a44-b4a7-c2c4d0eec004")} // rad
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class AccelerationQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         MetrePerSecondSquared,  // metre per second squared
         FootPerSecondSquared,  // foot per second squared
         CentimetrePerSecondSquared,  // centimetre per second squared
         CentimetrePerHourPerSecond,  // centimetre per hour per second
         CentimetrePerMinutePerSecond,  // centimetre per minute per second
         FootPerHourPerSecond,  // foot per hour per second
         FootPerMinutePerSecond,  // foot per minute per second
         Galileo,  // galileo
         GravityStandard,  // gravity standard
         InchPerHourPerSecond,  // inch per hour per second
         InchPerMinutePerSecond,  // inch per minute per second
         InchPerSecondSquared,  // inch per second squared
         KnotPerSecond,  // knot per second
         MetrePerSecondPerMillisecond,  // metre per second per millisecond
         MilePerHourPerSecond,  // mile per hour per second
         MilePerMinutePerSecond,  // mile per minute per second
         MilePerSecondSquared,  // mile per second squared
         KilometrePerSecondSquared,  // kilometre per second squared
         KilometrePerHourPerSecond,  // kilometre per hour per second
         KilometrePerMinutePerSecond // kilometre per minute per second
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.MetrePerSecondSquared, new Guid("20515c0b-8a90-4e76-a2b3-cab213db5a06")},  // metre per second squared
         {UnitChoicesEnum.FootPerSecondSquared, new Guid("74f20b52-6c15-40e2-ae23-5493326fc879")},  // foot per second squared
         {UnitChoicesEnum.CentimetrePerSecondSquared, new Guid("4dc923c6-81db-4bd1-aa2c-933966ebbef4")},  // centimetre per second squared
         {UnitChoicesEnum.CentimetrePerHourPerSecond, new Guid("b469c349-fd35-4802-b408-dd6f86d62d27")},  // centimetre per hour per second
         {UnitChoicesEnum.CentimetrePerMinutePerSecond, new Guid("e681e123-03d7-42eb-9ef5-33da76a6d78c")},  // centimetre per minute per second
         {UnitChoicesEnum.FootPerHourPerSecond, new Guid("ccdc1097-4386-4e78-beff-438807b0d52c")},  // foot per hour per second
         {UnitChoicesEnum.FootPerMinutePerSecond, new Guid("eea429f4-ada7-4c40-adc0-4054dde8549a")},  // foot per minute per second
         {UnitChoicesEnum.Galileo, new Guid("a88f1f6d-e025-47b8-802a-0c802f730824")},  // galileo
         {UnitChoicesEnum.GravityStandard, new Guid("94dcb02d-04d9-4613-b3c0-d3bdcc3ce382")},  // gravity standard
         {UnitChoicesEnum.InchPerHourPerSecond, new Guid("16cc4dc1-0fb0-4bfa-acc2-55a754ab47f9")},  // inch per hour per second
         {UnitChoicesEnum.InchPerMinutePerSecond, new Guid("e33ae152-f371-4b4c-aaf7-51e5635cbcaa")},  // inch per minute per second
         {UnitChoicesEnum.InchPerSecondSquared, new Guid("2c872353-889f-444a-a182-1a1d3fd09bb3")},  // inch per second squared
         {UnitChoicesEnum.KnotPerSecond, new Guid("eefbcd14-1031-4763-8a5d-a9fc8c4fc833")},  // knot per second
         {UnitChoicesEnum.MetrePerSecondPerMillisecond, new Guid("ecefe9c4-9d38-4fd8-8f56-482f7c54874e")},  // metre per second per millisecond
         {UnitChoicesEnum.MilePerHourPerSecond, new Guid("622bde1c-57b2-4845-8a14-c252a0f6c0df")},  // mile per hour per second
         {UnitChoicesEnum.MilePerMinutePerSecond, new Guid("f58cace4-7e55-4218-ad64-5c3e420efe02")},  // mile per minute per second
         {UnitChoicesEnum.MilePerSecondSquared, new Guid("4f91adee-52ad-420b-94a8-5b45ab0399a9")},  // mile per second squared
         {UnitChoicesEnum.KilometrePerSecondSquared, new Guid("8b77b272-564a-4637-83cf-bf9d08d231db")},  // kilometre per second squared
         {UnitChoicesEnum.KilometrePerHourPerSecond, new Guid("9f74f510-f9ce-4393-9d9d-83518519e42a")},  // kilometre per hour per second
         {UnitChoicesEnum.KilometrePerMinutePerSecond, new Guid("aaaba47c-f11e-44fa-b5df-e981f6356f5a")} // kilometre per minute per second
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class AmountSubstancePerAreaQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         MolePerSquareMetre,  // mole per square metre
         MillimolePerSquareMetre // millimole per square metre
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.MolePerSquareMetre, new Guid("d146aedf-876f-4f5c-a766-378d77ebad03")},  // mole per square metre
         {UnitChoicesEnum.MillimolePerSquareMetre, new Guid("126b36a2-550d-4eaf-9170-cf95bbf5e7f1")} // millimole per square metre
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class AmountSubstancePerAreaRateOfChangeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         MolePerSquareMetreSecond,  // mole per square metre second
         MillimolePerSquareMetreSecond // millimole per square metre second
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.MolePerSquareMetreSecond, new Guid("c9dcb204-c8a6-4f0a-80df-680054f8d2d1")},  // mole per square metre second
         {UnitChoicesEnum.MillimolePerSquareMetreSecond, new Guid("bfa7e432-587b-4a31-b84f-950ce89ff6d2")} // millimole per square metre second
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class AmountSubstancePerVolumeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         MolePerCubicMetre,  // mole per cubic metre
         MolePerLitre,  // mole per litre
         MillimolePerLitre // millimole per litre
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.MolePerCubicMetre, new Guid("fd710071-6493-47fa-84a3-565ea9284423")},  // mole per cubic metre
         {UnitChoicesEnum.MolePerLitre, new Guid("2835bc29-d482-4737-9d6c-a2e90d40e748")},  // mole per litre
         {UnitChoicesEnum.MillimolePerLitre, new Guid("3229ec79-0e78-43a0-b725-76f2694338a7")} // millimole per litre
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class AmountSubstanceQuantity : SymbolizedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Mole,  // mole
         Decimole,  // decimole
         Centimole,  // centimole
         Millimole,  // millimole
         Micromole,  // micromole
         Nanomole,  // nanomole
         Picomole,  // picomole
         Kilomole // kilomole
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Mole, new Guid("d1072bb3-265e-400a-91b2-9c49616dc3de")},  // mole
         {UnitChoicesEnum.Decimole, new Guid("b458e56b-edc3-48c3-8858-75d507f1f1f2")},  // decimole
         {UnitChoicesEnum.Centimole, new Guid("4d83c0db-ddc4-4087-ae50-076148976cad")},  // centimole
         {UnitChoicesEnum.Millimole, new Guid("0a83a1b6-cef4-4899-bcd4-4aa1e10d232a")},  // millimole
         {UnitChoicesEnum.Micromole, new Guid("ec2669ad-9742-4667-8a14-76e00c47a41e")},  // micromole
         {UnitChoicesEnum.Nanomole, new Guid("dc8d0034-ecfa-4dbf-a24c-f1851c4aaf9c")},  // nanomole
         {UnitChoicesEnum.Picomole, new Guid("642555ea-37f0-49b9-9f21-8dd616d477c4")},  // picomole
         {UnitChoicesEnum.Kilomole, new Guid("01157606-070e-41a2-8c78-84a7ae950bd6")} // kilomole
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class AmountSubstanceRateQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         MolePerSecond,  // mole per second
         MolePerMinute,  // mole per minute
         KilomolePerHour // kilomole per hour
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.MolePerSecond, new Guid("c98f922e-202b-4089-8669-f1fcdced34e5")},  // mole per second
         {UnitChoicesEnum.MolePerMinute, new Guid("9b687210-a450-4d7e-9f50-0314e0584990")},  // mole per minute
         {UnitChoicesEnum.KilomolePerHour, new Guid("3dec2a32-1d28-405d-bfde-569cd50a0982")} // kilomole per hour
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class AngleGradientPerLengthQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         RadianPerMetre,  // radian per metre
         DegreePerMetre,  // degree per metre
         DegreePerCentimetre,  // degree per centimetre
         DegreePerFoot,  // degree per foot
         DegreePerInch,  // degree per inch
         DegreePerDecimetre,  // degree per decimetre
         DegreePerMillimetre,  // degree per millimetre
         RadianPerMillimetre,  // radian per millimetre
         RadianPerCentimetre,  // radian per centimetre
         RadianPerDecimetre,  // radian per decimetre
         RadianPerFoot,  // radian per foot
         RadianPerInch // radian per inch
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.RadianPerMetre, new Guid("5d9782b6-c4c7-47ca-a86b-dce3f63c3747")},  // radian per metre
         {UnitChoicesEnum.DegreePerMetre, new Guid("2fcd4219-8879-4494-9563-5173ec2292fa")},  // degree per metre
         {UnitChoicesEnum.DegreePerCentimetre, new Guid("7f4f63d6-5ea8-4c6b-8be4-81f52b7060c7")},  // degree per centimetre
         {UnitChoicesEnum.DegreePerFoot, new Guid("23bf7716-5779-4607-aef7-1e0eeb7f201b")},  // degree per foot
         {UnitChoicesEnum.DegreePerInch, new Guid("271db65d-2a9f-4fec-a52a-21e13e106dd4")},  // degree per inch
         {UnitChoicesEnum.DegreePerDecimetre, new Guid("452edd17-d501-487b-8cc1-90c08f7b1417")},  // degree per decimetre
         {UnitChoicesEnum.DegreePerMillimetre, new Guid("5cc72a73-70c0-4ccf-83ae-38e8a45391b4")},  // degree per millimetre
         {UnitChoicesEnum.RadianPerMillimetre, new Guid("dbd20525-128b-43c5-9de4-a8e604cbf6bf")},  // radian per millimetre
         {UnitChoicesEnum.RadianPerCentimetre, new Guid("5552abca-e21b-48ca-aedc-4518a32b8de3")},  // radian per centimetre
         {UnitChoicesEnum.RadianPerDecimetre, new Guid("47e72ab7-444d-4d4b-8cd2-01d2fb8efa2d")},  // radian per decimetre
         {UnitChoicesEnum.RadianPerFoot, new Guid("e1ab7dd2-48c7-4ac8-ac5e-bc50fdcae5df")},  // radian per foot
         {UnitChoicesEnum.RadianPerInch, new Guid("c36cf9c1-d4f2-4654-99eb-5d84eac21c66")} // radian per inch
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class AngleMagneticFluxDensityQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         RadianTesla,  // radian tesla
         RadianGauss,  // radian gauss
         RadianMilligauss,  // radian milligauss
         RadianMillitesla,  // radian millitesla
         RadianMicrotesla,  // radian microtesla
         RadianNanotesla,  // radian nanotesla
         RadianMaxwellPerSquareCentimetre,  // radian maxwell per square centimetre
         RadianWeberPerSquareMetre,  // radian weber per square metre
         DegreeTesla,  // degree tesla
         DegreeGauss,  // degree gauss
         DegreeMilligauss,  // degree milligauss
         DegreeMillitesla,  // degree millitesla
         DegreeMicrotesla,  // degree microtesla
         DegreeNanotesla,  // degree nanotesla
         DegreeMaxwellPerSquareCentimetre,  // degree maxwell per square centimetre
         DegreeWeberPerSquareMetre // degree weber per square metre
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.RadianTesla, new Guid("242bd328-96ef-4a1b-9a78-1e2eb8366955")},  // radian tesla
         {UnitChoicesEnum.RadianGauss, new Guid("aa726b90-bd4b-420c-ae71-6f2f1fde3b58")},  // radian gauss
         {UnitChoicesEnum.RadianMilligauss, new Guid("352a5b84-306d-4e38-898a-58705683fdf0")},  // radian milligauss
         {UnitChoicesEnum.RadianMillitesla, new Guid("663639b3-48b8-4c04-a2eb-6ae2e16daa9b")},  // radian millitesla
         {UnitChoicesEnum.RadianMicrotesla, new Guid("b445e592-e0ca-490f-8880-9708e4e96a01")},  // radian microtesla
         {UnitChoicesEnum.RadianNanotesla, new Guid("b4aeee40-29fa-463a-80a4-e10fa42c743f")},  // radian nanotesla
         {UnitChoicesEnum.RadianMaxwellPerSquareCentimetre, new Guid("02d06899-5d89-4669-a4c2-35adb9ec3924")},  // radian maxwell per square centimetre
         {UnitChoicesEnum.RadianWeberPerSquareMetre, new Guid("409e8e85-0870-4529-ae0c-95ab6506c445")},  // radian weber per square metre
         {UnitChoicesEnum.DegreeTesla, new Guid("13df770f-6e77-4de4-91c6-137206e53fbb")},  // degree tesla
         {UnitChoicesEnum.DegreeGauss, new Guid("73bb1de0-ccc0-42e6-b88e-44ecfb6fe7e4")},  // degree gauss
         {UnitChoicesEnum.DegreeMilligauss, new Guid("e74c9ae3-6e17-48e1-896b-e6c1877d68d7")},  // degree milligauss
         {UnitChoicesEnum.DegreeMillitesla, new Guid("812a3461-ae4a-405b-ae5d-73eb23e8a71f")},  // degree millitesla
         {UnitChoicesEnum.DegreeMicrotesla, new Guid("50782201-236e-4537-843b-121e8dca28c6")},  // degree microtesla
         {UnitChoicesEnum.DegreeNanotesla, new Guid("0d9bf20d-2b10-4e73-ae8e-3d3e91862ec0")},  // degree nanotesla
         {UnitChoicesEnum.DegreeMaxwellPerSquareCentimetre, new Guid("092e8231-a6e6-4b29-bdd6-2ae490aa583a")},  // degree maxwell per square centimetre
         {UnitChoicesEnum.DegreeWeberPerSquareMetre, new Guid("2d7e1d60-6401-41c0-b436-612116be9ad4")} // degree weber per square metre
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class AngularAccelerationQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         RadianPerSecondSquared,  // radian per second squared
         DegreePerSecondSquared,  // degree per second squared
         RadianPerDayPerSecond,  // radian per day per second
         RadianPerHourPerSecond,  // radian per hour per second
         RadianPerMinutePerSecond,  // radian per minute per second
         DegreePerDayPerSecond,  // degree per day per second
         DegreePerHourPerSecond,  // degree per hour per second
         DegreePerMinutePerSecond,  // degree per minute per second
         RadianPerSecondPerMinute,  // radian per second per minute
         DegreePerSecondPerMinute,  // degree per second per minute
         RadianPerDayPerMinute,  // radian per day per minute
         RadianPerHourPerMinute,  // radian per hour per minute
         RadianPerMinuteSquared,  // radian per minute squared
         DegreePerDayPerMinute,  // degree per day per minute
         DegreePerHourPerMinute,  // degree per hour per minute
         DegreePerMinuteSquared,  // degree per minute squared
         RadianPerSecondPerHour,  // radian per second per hour
         DegreePerSecondPerHour,  // degree per second per hour
         RadianPerDayPerHour,  // radian per day per hour
         RadianPerHourSquared,  // radian per hour squared
         RadianPerMinutePerHour,  // radian per minute per hour
         DegreePerDayPerHour,  // degree per day per hour
         DegreePerHourSquared,  // degree per hour squared
         DegreePerMinutePerHour,  // degree per minute per hour
         RadianPerSecondPerDay,  // radian per second per day
         DegreePerSecondPerDay,  // degree per second per day
         RadianPerDaySquared,  // radian per day squared
         RadianPerHourPerDay,  // radian per hour per day
         RadianPerMinutePerDay,  // radian per minute per day
         DegreePerDaySquared,  // degree per day squared
         DegreePerHourPerDay,  // degree per hour per day
         DegreePerMinutePerDay // degree per minute per day
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.RadianPerSecondSquared, new Guid("15d7ab2b-c0c3-4d33-8242-670ba2f937ff")},  // radian per second squared
         {UnitChoicesEnum.DegreePerSecondSquared, new Guid("6fcc944b-fd7e-4368-baa4-3bb8eeba63a2")},  // degree per second squared
         {UnitChoicesEnum.RadianPerDayPerSecond, new Guid("76b421a7-87e5-4fdf-a280-8e5aea80e0d0")},  // radian per day per second
         {UnitChoicesEnum.RadianPerHourPerSecond, new Guid("70b2f838-b8d2-425f-bea1-0a841c520ba8")},  // radian per hour per second
         {UnitChoicesEnum.RadianPerMinutePerSecond, new Guid("fdf50c96-cef9-466a-80d6-747b99dad734")},  // radian per minute per second
         {UnitChoicesEnum.DegreePerDayPerSecond, new Guid("838a73aa-fd19-42ac-9c72-c62573cda91b")},  // degree per day per second
         {UnitChoicesEnum.DegreePerHourPerSecond, new Guid("cbfff738-14e6-47ad-8b21-41eeeea41439")},  // degree per hour per second
         {UnitChoicesEnum.DegreePerMinutePerSecond, new Guid("f6b4276b-64a8-46f9-a831-fd14a61c34a0")},  // degree per minute per second
         {UnitChoicesEnum.RadianPerSecondPerMinute, new Guid("6bc32ba1-3a66-415a-bc0f-d9292ac77ab6")},  // radian per second per minute
         {UnitChoicesEnum.DegreePerSecondPerMinute, new Guid("2bb42b52-ab2d-4d2f-8ebb-1294f9b35b89")},  // degree per second per minute
         {UnitChoicesEnum.RadianPerDayPerMinute, new Guid("62a8e46b-3701-4375-be57-f5d53df23d87")},  // radian per day per minute
         {UnitChoicesEnum.RadianPerHourPerMinute, new Guid("77f3ea1e-8280-4881-befe-08dc1065a94f")},  // radian per hour per minute
         {UnitChoicesEnum.RadianPerMinuteSquared, new Guid("61ec8b9b-f1c7-4798-bf37-d7f697d0ea8e")},  // radian per minute squared
         {UnitChoicesEnum.DegreePerDayPerMinute, new Guid("22b8d910-ce73-4ce4-87b3-761aa17059d6")},  // degree per day per minute
         {UnitChoicesEnum.DegreePerHourPerMinute, new Guid("414785c2-4d81-472e-a020-7eca9913ebd2")},  // degree per hour per minute
         {UnitChoicesEnum.DegreePerMinuteSquared, new Guid("49b1ab89-8a0c-4bd9-b693-7db0e14b86e9")},  // degree per minute squared
         {UnitChoicesEnum.RadianPerSecondPerHour, new Guid("6bb6033c-3391-4e68-9cc3-63e9d8b03eb3")},  // radian per second per hour
         {UnitChoicesEnum.DegreePerSecondPerHour, new Guid("cdd641f0-2b50-4cb4-88c1-256ac2f5e2b5")},  // degree per second per hour
         {UnitChoicesEnum.RadianPerDayPerHour, new Guid("1ff0952d-4395-4dd6-b101-99b890a46ee8")},  // radian per day per hour
         {UnitChoicesEnum.RadianPerHourSquared, new Guid("f105faab-2b48-402a-98f2-004b99adb5a7")},  // radian per hour squared
         {UnitChoicesEnum.RadianPerMinutePerHour, new Guid("c5713507-731e-482a-bd4e-d86ef21d4ae5")},  // radian per minute per hour
         {UnitChoicesEnum.DegreePerDayPerHour, new Guid("7e8596f9-657f-4eb6-937d-091a55bd0e34")},  // degree per day per hour
         {UnitChoicesEnum.DegreePerHourSquared, new Guid("fad47c71-53d9-4a88-b720-6a953092b41b")},  // degree per hour squared
         {UnitChoicesEnum.DegreePerMinutePerHour, new Guid("a4870e3b-95b7-4c8c-b86a-ffe1d2d6e524")},  // degree per minute per hour
         {UnitChoicesEnum.RadianPerSecondPerDay, new Guid("52462419-b494-473f-a46d-270ae253f076")},  // radian per second per day
         {UnitChoicesEnum.DegreePerSecondPerDay, new Guid("dab003cb-6906-4f9f-8551-e8f72d3a1c4d")},  // degree per second per day
         {UnitChoicesEnum.RadianPerDaySquared, new Guid("6027756a-1c1e-47e8-b938-0bc4e4b25a24")},  // radian per day squared
         {UnitChoicesEnum.RadianPerHourPerDay, new Guid("a31be21e-1f83-4e9b-aaa7-56a2f797c3d6")},  // radian per hour per day
         {UnitChoicesEnum.RadianPerMinutePerDay, new Guid("3335414d-1a69-477b-b27f-61f464a7ca13")},  // radian per minute per day
         {UnitChoicesEnum.DegreePerDaySquared, new Guid("a77c1dea-6fe9-43cc-9f53-75d87e00e458")},  // degree per day squared
         {UnitChoicesEnum.DegreePerHourPerDay, new Guid("69437581-725e-4a66-b945-abaac133be0a")},  // degree per hour per day
         {UnitChoicesEnum.DegreePerMinutePerDay, new Guid("5a8a9fe1-9866-4c71-95a5-23dcb4726235")} // degree per minute per day
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class AngularVelocityPerVolumetricFlowRateQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         RadianPerSecondPerCubicMetrePerSecond,  // radian per second per cubic metre per second
         RevolutionPerMinutePerLitrePerMinute,  // revolution per minute per litre per minute
         RevolutionPerMinutePerGallonUSPerMinute,  // revolution per minute per gallon US per minute
         RevolutionPerMinutePerGallonUKPerMinute // revolution per minute per gallon UK per minute
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.RadianPerSecondPerCubicMetrePerSecond, new Guid("9b2339aa-e707-4db5-9654-31caa9eecc17")},  // radian per second per cubic metre per second
         {UnitChoicesEnum.RevolutionPerMinutePerLitrePerMinute, new Guid("b730c2a5-964d-4275-8a73-ecf594739c1a")},  // revolution per minute per litre per minute
         {UnitChoicesEnum.RevolutionPerMinutePerGallonUSPerMinute, new Guid("6534bd66-5155-4a88-a3c7-9b3e8081d11d")},  // revolution per minute per gallon US per minute
         {UnitChoicesEnum.RevolutionPerMinutePerGallonUKPerMinute, new Guid("c4034096-6ae6-48da-8bcc-ceedb876b5c5")} // revolution per minute per gallon UK per minute
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class AngularVelocityQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         RadianPerSecond,  // radian per second
         DegreePerSecond,  // degree per second
         RadianPerDay,  // radian per day
         RadianPerHour,  // radian per hour
         RadianPerMinute,  // radian per minute
         DegreePerDay,  // degree per day
         DegreePerHour,  // degree per hour
         DegreePerMinute,  // degree per minute
         RevolutionPerSecond,  // revolution per second
         RevolutionPerMinute,  // revolution per minute
         RevolutionPerHour,  // revolution per hour
         ThousandRevolutionPerSecond,  // thousand revolution per second
         ThousandRevolutionPerMinute,  // thousand revolution per minute
         ThousandRevolutionPerHour,  // thousand revolution per hour
         StrokePerSecond,  // stroke per second
         StrokePerMinute,  // stroke per minute
         StrokePerHour,  // stroke per hour
         ThousandStrokePerSecond,  // thousand stroke per second
         ThousandStrokePerMinute,  // thousand stroke per minute
         ThousandStrokePerHour // thousand stroke per hour
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.RadianPerSecond, new Guid("8889fafb-cc58-4c4e-a52d-696219dfcf4a")},  // radian per second
         {UnitChoicesEnum.DegreePerSecond, new Guid("c6c0676e-c8a6-407d-be44-1691fd6b5d9e")},  // degree per second
         {UnitChoicesEnum.RadianPerDay, new Guid("53f2a55b-beb6-4e4d-b696-e817c5b0eaed")},  // radian per day
         {UnitChoicesEnum.RadianPerHour, new Guid("c2f0e82b-236b-4eb5-9c77-b2c500ab60ab")},  // radian per hour
         {UnitChoicesEnum.RadianPerMinute, new Guid("5267eb8c-36bc-4ab9-bd62-26f05f852c35")},  // radian per minute
         {UnitChoicesEnum.DegreePerDay, new Guid("ec049b3d-134b-44a3-9746-0419a368beef")},  // degree per day
         {UnitChoicesEnum.DegreePerHour, new Guid("44cc8b17-2f87-49c9-8ab7-7f4ed36d9eb6")},  // degree per hour
         {UnitChoicesEnum.DegreePerMinute, new Guid("0a3ad50c-bbfa-456c-9613-84f37b4a80f1")},  // degree per minute
         {UnitChoicesEnum.RevolutionPerSecond, new Guid("00a96665-b967-4982-8b6b-1ca3671f8c9a")},  // revolution per second
         {UnitChoicesEnum.RevolutionPerMinute, new Guid("fe846c15-ddba-480f-93f0-16af45f7b9ce")},  // revolution per minute
         {UnitChoicesEnum.RevolutionPerHour, new Guid("950585c6-bc50-4f51-becb-2f840e217c4f")},  // revolution per hour
         {UnitChoicesEnum.ThousandRevolutionPerSecond, new Guid("4c7c26e6-874c-4da0-b26d-095e59938bf0")},  // thousand revolution per second
         {UnitChoicesEnum.ThousandRevolutionPerMinute, new Guid("4ac0f148-241b-42bd-b55e-88a27d12f860")},  // thousand revolution per minute
         {UnitChoicesEnum.ThousandRevolutionPerHour, new Guid("ee395b6a-95bc-4fd1-b6fe-0eb26046d595")},  // thousand revolution per hour
         {UnitChoicesEnum.StrokePerSecond, new Guid("23fd9599-d210-4050-8c61-fc18a5087db3")},  // stroke per second
         {UnitChoicesEnum.StrokePerMinute, new Guid("979b3170-be8b-42ee-a7d5-ecf9d9f1869d")},  // stroke per minute
         {UnitChoicesEnum.StrokePerHour, new Guid("114d2d67-d080-4c46-85c6-9047cd0e2d7a")},  // stroke per hour
         {UnitChoicesEnum.ThousandStrokePerSecond, new Guid("edb3d772-8ed9-499a-a2db-e5835176fb1b")},  // thousand stroke per second
         {UnitChoicesEnum.ThousandStrokePerMinute, new Guid("bf130c38-70bb-4bd1-8e21-ae3b3043bc96")},  // thousand stroke per minute
         {UnitChoicesEnum.ThousandStrokePerHour, new Guid("756ac21d-94c9-4501-947f-0bf275528fb5")} // thousand stroke per hour
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class AreaPerAmountSubstanceQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         SquareMetrePerMole,  // square metre per mole
         SquareCentimetrePerMole // square centimetre per mole
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.SquareMetrePerMole, new Guid("db0b80ff-86c9-4b33-997c-0228fe1e9d4f")},  // square metre per mole
         {UnitChoicesEnum.SquareCentimetrePerMole, new Guid("2432bd35-7c69-415c-a7ab-cb48d61f1380")} // square centimetre per mole
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class AreaPerMassQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         SquareMetrePerKilogram,  // square metre per kilogram
         SquareCentimetrePerGram,  // square centimetre per gram
         SquareFootPerPound // square foot per pound
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.SquareMetrePerKilogram, new Guid("ff216a2c-2fae-423f-a1d2-d14c070ffb06")},  // square metre per kilogram
         {UnitChoicesEnum.SquareCentimetrePerGram, new Guid("b61df260-ea70-48a5-aa07-c209eb0eafeb")},  // square centimetre per gram
         {UnitChoicesEnum.SquareFootPerPound, new Guid("e71612bc-3e04-4768-a755-ade8184f1a26")} // square foot per pound
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class AreaPerVolumeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         SquareMetrePerCubicMetre,  // square metre per cubic metre
         SquareCentimetrePerCubicCentimetre,  // square centimetre per cubic centimetre
         SquareFootPerCubicFoot // square foot per cubic foot
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.SquareMetrePerCubicMetre, new Guid("da913573-b78c-4862-8404-65f5ee9c41ed")},  // square metre per cubic metre
         {UnitChoicesEnum.SquareCentimetrePerCubicCentimetre, new Guid("dde6d485-718e-417e-b460-805953a12c39")},  // square centimetre per cubic centimetre
         {UnitChoicesEnum.SquareFootPerCubicFoot, new Guid("244f9a97-e0fe-48ad-9985-6e5ee3b80b1b")} // square foot per cubic foot
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class AreaQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         SquareMetre,  // square metre
         SquareKilometre,  // square kilometre
         Hectare,  // hectare
         SquareDecametre,  // square decametre
         SquareDecimetre,  // square decimetre
         SquareCentimetre,  // square centimetre
         SquareMillimetre,  // square millimetre
         SquareMicrometre,  // square micrometre
         SquareFoot,  // square foot
         SquareInch,  // square inch
         SquareYard,  // square yard
         Acre,  // acre
         SquareMile // square mile
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.SquareMetre, new Guid("6225a0d7-d2f1-4bb1-9721-5b260bac26ee")},  // square metre
         {UnitChoicesEnum.SquareKilometre, new Guid("6353513a-6e38-4a58-b20c-d3e8d7b70fb8")},  // square kilometre
         {UnitChoicesEnum.Hectare, new Guid("14313265-7985-4010-a19a-5ffaebe05092")},  // hectare
         {UnitChoicesEnum.SquareDecametre, new Guid("df9417fc-1c08-4c76-a177-e8ea803b2e2f")},  // square decametre
         {UnitChoicesEnum.SquareDecimetre, new Guid("125fd8d6-d1eb-4826-a952-5219603409ab")},  // square decimetre
         {UnitChoicesEnum.SquareCentimetre, new Guid("d74bb2bc-9c86-4be4-bff1-88cac7b1049b")},  // square centimetre
         {UnitChoicesEnum.SquareMillimetre, new Guid("0b87d221-284a-4e8c-8a60-50c522f9ade4")},  // square millimetre
         {UnitChoicesEnum.SquareMicrometre, new Guid("bec98c97-72c7-4485-9138-058ed14e7fbe")},  // square micrometre
         {UnitChoicesEnum.SquareFoot, new Guid("5a59332e-17b3-4fa2-9527-12d06a2b4248")},  // square foot
         {UnitChoicesEnum.SquareInch, new Guid("294bc6d0-5be7-4c70-95f3-ad9dc50f02cf")},  // square inch
         {UnitChoicesEnum.SquareYard, new Guid("ae3df24c-e5db-4b88-9e81-228f29855f1b")},  // square yard
         {UnitChoicesEnum.Acre, new Guid("bc94456a-b8b9-49ac-b349-eaded6c984c6")},  // acre
         {UnitChoicesEnum.SquareMile, new Guid("5bbe8c59-cce9-47c8-b357-c5a15610af72")} // square mile
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class AreaRateOfChangeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         SquareMetrePerSecond,  // square metre per second
         SquareMetrePerMinute,  // square metre per minute
         SquareFootPerSecond // square foot per second
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.SquareMetrePerSecond, new Guid("3b0a761b-3bb1-4b64-9540-2e5425ed8920")},  // square metre per second
         {UnitChoicesEnum.SquareMetrePerMinute, new Guid("937e0918-c2aa-445d-8a3b-179464b9ded2")},  // square metre per minute
         {UnitChoicesEnum.SquareFootPerSecond, new Guid("efaeef1f-5dfe-4b28-99b4-d7ff104ffb3e")} // square foot per second
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class BendingMomentGradientQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         NewtonMetrePerMetre,  // newton metre per metre
         KilonewtonMetrePerMetre,  // kilonewton metre per metre
         FootPoundForcePerFoot,  // foot pound force per foot
         InchPoundForcePerInch // inch pound force per inch
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.NewtonMetrePerMetre, new Guid("8544e5b8-f182-4833-bbb4-4262f4f3c55f")},  // newton metre per metre
         {UnitChoicesEnum.KilonewtonMetrePerMetre, new Guid("807087da-e38c-4a04-97c9-cd8bb3ce1d98")},  // kilonewton metre per metre
         {UnitChoicesEnum.FootPoundForcePerFoot, new Guid("604de5ca-be77-4f34-8160-7b39303352bc")},  // foot pound force per foot
         {UnitChoicesEnum.InchPoundForcePerInch, new Guid("8036588e-ff60-4176-bc61-4cf91ac271d8")} // inch pound force per inch
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class BendingMomentQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         NewtonMetre,  // newton metre
         DecanewtonMetre,  // decanewton metre
         KilogramForceMetre,  // kilogram force metre
         KilonewtonMetre,  // kilonewton metre
         FootPound,  // foot pound
         KilofootPound,  // kilofoot pound
         NewtonDecimetre,  // newton decimetre
         NewtonCentimetre,  // newton centimetre
         NewtonMillimetre,  // newton millimetre
         InchPound // inch pound
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.NewtonMetre, new Guid("bd43cd1d-23db-4137-8f87-f00c686b1609")},  // newton metre
         {UnitChoicesEnum.DecanewtonMetre, new Guid("e63f5c69-880e-4ed2-985d-a6c1316a8ff5")},  // decanewton metre
         {UnitChoicesEnum.KilogramForceMetre, new Guid("ac7a836f-daf8-4373-b926-dde5b65ef005")},  // kilogram force metre
         {UnitChoicesEnum.KilonewtonMetre, new Guid("62a7ba7e-e9e4-4ad4-8faa-3e80ffd79c76")},  // kilonewton metre
         {UnitChoicesEnum.FootPound, new Guid("ac12e954-14db-4a0d-8d40-7196b5a819b5")},  // foot pound
         {UnitChoicesEnum.KilofootPound, new Guid("fd9c2e34-fcd9-4fb0-9b2f-995dc2d15467")},  // kilofoot pound
         {UnitChoicesEnum.NewtonDecimetre, new Guid("6da5c57f-34ae-4b7f-9511-cf08812e6092")},  // newton decimetre
         {UnitChoicesEnum.NewtonCentimetre, new Guid("bf09633d-103f-4ca2-8200-b27f6200df01")},  // newton centimetre
         {UnitChoicesEnum.NewtonMillimetre, new Guid("a31d03fe-2397-40d0-a647-8e81faa4298f")},  // newton millimetre
         {UnitChoicesEnum.InchPound, new Guid("cfbb3f04-27e9-42b8-ae7d-074ced09831e")} // inch pound
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class CompressibilityQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         InversePascal,  // inverse pascal
         InverseBar,  // inverse bar
         InversePoundPerSquareInch,  // inverse pound per square inch
         InverseAtmosphere // inverse atmosphere
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.InversePascal, new Guid("0e259998-c8bb-4a4d-b281-afb8008b2693")},  // inverse pascal
         {UnitChoicesEnum.InverseBar, new Guid("4a0f6df4-0d2d-489b-80f1-511be7713101")},  // inverse bar
         {UnitChoicesEnum.InversePoundPerSquareInch, new Guid("282ab24c-6c43-4710-8e52-433bdf90cc2e")},  // inverse pound per square inch
         {UnitChoicesEnum.InverseAtmosphere, new Guid("92c19398-ac0f-41fc-8a22-516c3dc59c82")} // inverse atmosphere
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ConsistencyIndexRheologyQuantity : DynamicViscosityQuantity
  {
    public new enum UnitChoicesEnum
      {
         PascalSecond,  // pascal second
         MicropascalSecond,  // micropascal second
         Centipoise,  // centipoise
         Micropoise,  // micropoise
         PoundSecondPer100SquareFoot,  // pound second per 100 square foot
         DyneSecondPerSquareCentimetre // dyne second per square centimetre
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.PascalSecond, new Guid("5707caa4-e293-430d-9575-425305060fcc")},  // pascal second
         {UnitChoicesEnum.MicropascalSecond, new Guid("ba54cce5-29ad-464a-9263-ae4cfa96328d")},  // micropascal second
         {UnitChoicesEnum.Centipoise, new Guid("a71ef873-6ea2-4922-a100-231177de0e85")},  // centipoise
         {UnitChoicesEnum.Micropoise, new Guid("5cae22bd-1294-4aa7-9666-a9a2080d53e8")},  // micropoise
         {UnitChoicesEnum.PoundSecondPer100SquareFoot, new Guid("b48720b9-8eb5-4b5c-8da1-ca2312fdff01")},  // pound second per 100 square foot
         {UnitChoicesEnum.DyneSecondPerSquareCentimetre, new Guid("90ce61e5-46db-47f9-9c22-1c0f19068132")} // dyne second per square centimetre
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class CurvatureQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         RadianPerMetre,  // radian per metre
         DegreePer10m,  // degree per 10m
         DegreePer30m,  // degree per 30m
         DegreePer30ft,  // degree per 30ft
         DegreePer100ft,  // degree per 100ft
         DegreePerFoot,  // degree per foot
         RadianPerFoot,  // radian per foot
         DegreePerMetre,  // degree per metre
         DegreePerDecimeter,  // degree per decimeter
         DegreePerCentimeter,  // degree per centimeter
         DegreePerMillimeter,  // degree per millimeter
         DegreePerMicrometer,  // degree per micrometer
         DegreePerNanometer,  // degree per nanometer
         DegreePerDecameter,  // degree per decameter
         DegreePerHectometer,  // degree per hectometer
         DegreePerKilometer,  // degree per kilometer
         RadianPerDecimeter,  // radian per decimeter
         RadianPerCentimeter,  // radian per centimeter
         RadianPerMillimeter,  // radian per millimeter
         RadianPerMicrometer,  // radian per micrometer
         RadianPerNanometer,  // radian per nanometer
         RadianPerDecameter,  // radian per decameter
         RadianPerHectometer,  // radian per hectometer
         RadianPerKilometer,  // radian per kilometer
         DegreePerYard,  // degree per yard
         DegreePerMile,  // degree per mile
         RadianPerYard,  // radian per yard
         RadianPerMile // radian per mile
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.RadianPerMetre, new Guid("cbba2f64-9914-4c00-a3fe-3c2aef560225")},  // radian per metre
         {UnitChoicesEnum.DegreePer10m, new Guid("c62408d6-c3c5-47bd-8cb0-4fa9faf51598")},  // degree per 10m
         {UnitChoicesEnum.DegreePer30m, new Guid("98a4d593-c959-49fb-ba1a-b1aa61830cd7")},  // degree per 30m
         {UnitChoicesEnum.DegreePer30ft, new Guid("284dca7d-b1fc-4e40-ab08-a3e0fa2ef9d0")},  // degree per 30ft
         {UnitChoicesEnum.DegreePer100ft, new Guid("5aa20ab5-6800-48db-abb1-4c3538b0972d")},  // degree per 100ft
         {UnitChoicesEnum.DegreePerFoot, new Guid("363a6781-5829-4046-95d8-ce1e844343fc")},  // degree per foot
         {UnitChoicesEnum.RadianPerFoot, new Guid("1428743e-927c-4f7a-9e15-62d37790ad51")},  // radian per foot
         {UnitChoicesEnum.DegreePerMetre, new Guid("7c47f046-0499-4108-937d-abb504883259")},  // degree per metre
         {UnitChoicesEnum.DegreePerDecimeter, new Guid("ed1db27f-5f42-4678-8d0b-8e4e91208a83")},  // degree per decimeter
         {UnitChoicesEnum.DegreePerCentimeter, new Guid("0440cec4-b070-4b03-a065-453c2fafbf24")},  // degree per centimeter
         {UnitChoicesEnum.DegreePerMillimeter, new Guid("c6e0bc05-2ee5-4dd1-85fa-71de7a235ef4")},  // degree per millimeter
         {UnitChoicesEnum.DegreePerMicrometer, new Guid("996e89d6-3b4a-4893-81f5-e4f539e93cf8")},  // degree per micrometer
         {UnitChoicesEnum.DegreePerNanometer, new Guid("e15c78f0-7a6c-47e5-bd3f-4f02571fabe5")},  // degree per nanometer
         {UnitChoicesEnum.DegreePerDecameter, new Guid("970e611b-87ee-4006-aee7-c6d500c33ff0")},  // degree per decameter
         {UnitChoicesEnum.DegreePerHectometer, new Guid("5cca6250-fd0e-47a7-a64f-01b8025950ad")},  // degree per hectometer
         {UnitChoicesEnum.DegreePerKilometer, new Guid("91ef582e-ee8b-4f48-a1f6-c5d96eb634ca")},  // degree per kilometer
         {UnitChoicesEnum.RadianPerDecimeter, new Guid("c6d2c02d-83a6-4043-9a7d-392415a74b12")},  // radian per decimeter
         {UnitChoicesEnum.RadianPerCentimeter, new Guid("f101a0b8-f710-4010-b9a5-6ced681bcf0a")},  // radian per centimeter
         {UnitChoicesEnum.RadianPerMillimeter, new Guid("80e5d9d6-c1fa-4273-8134-33ff4bc46b40")},  // radian per millimeter
         {UnitChoicesEnum.RadianPerMicrometer, new Guid("777ca6bc-48da-4353-a00d-0bf0931a4328")},  // radian per micrometer
         {UnitChoicesEnum.RadianPerNanometer, new Guid("eceb51ee-d3ad-4a42-a877-6b20f927fc01")},  // radian per nanometer
         {UnitChoicesEnum.RadianPerDecameter, new Guid("2e8079f1-260e-4524-be2f-cee1195c7fdd")},  // radian per decameter
         {UnitChoicesEnum.RadianPerHectometer, new Guid("cb2c22fb-ac24-4c65-96d3-2437c8942662")},  // radian per hectometer
         {UnitChoicesEnum.RadianPerKilometer, new Guid("e9a7a647-092a-4349-bb58-0d096a1477c7")},  // radian per kilometer
         {UnitChoicesEnum.DegreePerYard, new Guid("9cb7c45f-56d8-4b03-b514-6b5fc687919c")},  // degree per yard
         {UnitChoicesEnum.DegreePerMile, new Guid("d155bdde-da22-4f3a-9050-9f5f7e69b57d")},  // degree per mile
         {UnitChoicesEnum.RadianPerYard, new Guid("7409e987-f5cb-4d24-93a8-50fc993fe949")},  // radian per yard
         {UnitChoicesEnum.RadianPerMile, new Guid("59e78258-2739-41b4-aa50-8e077bad6678")} // radian per mile
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class DimensionlessQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Dimensionless // dimensionless
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Dimensionless, new Guid("8744b0f7-2866-42d8-bf6c-b619ac87b945")} // dimensionless
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class DynamicViscosityQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         PascalSecond,  // pascal second
         MillipascalSecond,  // millipascal second
         MicropascalSecond,  // micropascal second
         Poise,  // poise
         Centipoise,  // centipoise
         Millipoise,  // millipoise
         Micropoise,  // micropoise
         PoundSecondPerSquareFoot,  // pound second per square foot
         PoundSecondPer100SquareFoot,  // pound second per 100 square foot
         PoundSecondPerSquareInch,  // pound second per square inch
         DyneSecondPerSquareCentimetre // dyne second per square centimetre
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.PascalSecond, new Guid("5707caa4-e293-430d-9575-425305060fcc")},  // pascal second
         {UnitChoicesEnum.MillipascalSecond, new Guid("640f2693-dd92-472b-b2ec-0052fc7b7a11")},  // millipascal second
         {UnitChoicesEnum.MicropascalSecond, new Guid("ba54cce5-29ad-464a-9263-ae4cfa96328d")},  // micropascal second
         {UnitChoicesEnum.Poise, new Guid("79e600d1-05f1-46ef-b47a-951b04f6666e")},  // poise
         {UnitChoicesEnum.Centipoise, new Guid("a71ef873-6ea2-4922-a100-231177de0e85")},  // centipoise
         {UnitChoicesEnum.Millipoise, new Guid("f20c7109-bf60-413b-8f41-6f1d2f3bff45")},  // millipoise
         {UnitChoicesEnum.Micropoise, new Guid("5cae22bd-1294-4aa7-9666-a9a2080d53e8")},  // micropoise
         {UnitChoicesEnum.PoundSecondPerSquareFoot, new Guid("1397525b-b5b6-4b3c-82e8-b562f01e9a42")},  // pound second per square foot
         {UnitChoicesEnum.PoundSecondPer100SquareFoot, new Guid("b48720b9-8eb5-4b5c-8da1-ca2312fdff01")},  // pound second per 100 square foot
         {UnitChoicesEnum.PoundSecondPerSquareInch, new Guid("70b7471d-6a62-4ce9-8def-ceb3d6d7495f")},  // pound second per square inch
         {UnitChoicesEnum.DyneSecondPerSquareCentimetre, new Guid("90ce61e5-46db-47f9-9c22-1c0f19068132")} // dyne second per square centimetre
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ElectricCapacitanceQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Farad,  // farad
         CoulombPerVolt,  // coulomb per volt
         Millifarad,  // millifarad
         Microfarad,  // microfarad
         Nanofarad,  // nanofarad
         Picofarad // picofarad
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Farad, new Guid("11dd208b-bcf2-4415-b7a9-4161791166c7")},  // farad
         {UnitChoicesEnum.CoulombPerVolt, new Guid("81ae5717-d834-4f25-800e-c42c3bcb48af")},  // coulomb per volt
         {UnitChoicesEnum.Millifarad, new Guid("12c8b1ad-d38a-4dbe-b418-7f3b31c23ff6")},  // millifarad
         {UnitChoicesEnum.Microfarad, new Guid("a5974c82-68ac-4166-81b0-123f3ae84701")},  // microfarad
         {UnitChoicesEnum.Nanofarad, new Guid("c1af9df8-69d5-41d3-9027-3c89aa151777")},  // nanofarad
         {UnitChoicesEnum.Picofarad, new Guid("1a9b9112-8a9f-4c80-a2ad-ebe5d9af5eef")} // picofarad
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ElectricCurrentQuantity : SymbolizedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Ampere,  // ampere
         CoulombPerSecond,  // coulomb per second
         SiemensVolt,  // siemens volt
         VoltPerOhm,  // volt per ohm
         WattPerVolt,  // watt per volt
         WeberPerHenry,  // weber per henry
         Deciampere,  // deciampere
         Centiampere,  // centiampere
         Milliampere,  // milliampere
         Microampere,  // microampere
         Nanoampere,  // nanoampere
         Picoampere,  // picoampere
         Biot,  // biot
         Abampere,  // abampere
         Kiloampere,  // kiloampere
         Megaampere,  // megaampere
         Gigaampere,  // gigaampere
         Teraampere,  // teraampere
         Statampere // statampere
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Ampere, new Guid("1cd2ef0e-baf8-43fb-9fac-64f84560d0a9")},  // ampere
         {UnitChoicesEnum.CoulombPerSecond, new Guid("0a9cc349-3bac-4f44-9a9b-3940ae595f03")},  // coulomb per second
         {UnitChoicesEnum.SiemensVolt, new Guid("52bc6bd9-f25d-470b-9068-b6e87f1f2ed0")},  // siemens volt
         {UnitChoicesEnum.VoltPerOhm, new Guid("100dd38e-19ad-465c-a995-0fb1174e506b")},  // volt per ohm
         {UnitChoicesEnum.WattPerVolt, new Guid("29464509-67a2-4062-b78a-8156e54cfa88")},  // watt per volt
         {UnitChoicesEnum.WeberPerHenry, new Guid("bf3285f5-34be-4592-822d-f6ffc3ce4858")},  // weber per henry
         {UnitChoicesEnum.Deciampere, new Guid("fdca0a23-f088-4a93-8bfa-4776d19dc26e")},  // deciampere
         {UnitChoicesEnum.Centiampere, new Guid("f057be23-0f56-4a5f-bb39-3ec032b66ff8")},  // centiampere
         {UnitChoicesEnum.Milliampere, new Guid("a2b3179e-3003-48eb-82aa-80fbfb2cfe39")},  // milliampere
         {UnitChoicesEnum.Microampere, new Guid("fb4dfa70-2011-468a-9c4a-06aba3754fc2")},  // microampere
         {UnitChoicesEnum.Nanoampere, new Guid("f5f75652-d393-4328-87f0-6132fd8dc786")},  // nanoampere
         {UnitChoicesEnum.Picoampere, new Guid("9c5c1ea2-89bc-48f8-83ab-fbde7b1f3721")},  // picoampere
         {UnitChoicesEnum.Biot, new Guid("4648ec96-2c82-4aa2-8de3-d6eb105f470e")},  // biot
         {UnitChoicesEnum.Abampere, new Guid("d589a05d-d6a4-4d2d-9ec7-3a02d0de2ef0")},  // abampere
         {UnitChoicesEnum.Kiloampere, new Guid("5a500f57-a5d1-41c2-9b8d-b08757420bb8")},  // kiloampere
         {UnitChoicesEnum.Megaampere, new Guid("978af2aa-e776-43d6-bfe6-4055b6d602e8")},  // megaampere
         {UnitChoicesEnum.Gigaampere, new Guid("eb1b76cd-4863-4cf3-b421-1cd80d2fb0b4")},  // gigaampere
         {UnitChoicesEnum.Teraampere, new Guid("4bf45d0a-e177-45b9-8a40-f8f51d5b15c6")},  // teraampere
         {UnitChoicesEnum.Statampere, new Guid("7d9a22ac-62d8-476d-8429-bc41febbe707")} // statampere
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ElectricResistivityQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         OhmMetre,  // ohm metre
         KiloOhmMetre,  // kilo ohm metre
         MegaOhmMetre,  // mega ohm metre
         GigaOhmMetre // giga ohm metre
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.OhmMetre, new Guid("fb07d86d-d69f-46ca-892c-17ec45adffcb")},  // ohm metre
         {UnitChoicesEnum.KiloOhmMetre, new Guid("c58ce3f0-7389-4c36-b291-55fa5ceb9962")},  // kilo ohm metre
         {UnitChoicesEnum.MegaOhmMetre, new Guid("cf90cab7-e973-469a-9727-08bfa7f708e6")},  // mega ohm metre
         {UnitChoicesEnum.GigaOhmMetre, new Guid("eecfdf24-7a8e-4783-a627-d4387831767d")} // giga ohm metre
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ElectricTensionQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Volt,  // volt
         Millivolt,  // millivolt
         Centivolt,  // centivolt
         Microvolt,  // microvolt
         Nanovolt,  // nanovolt
         Picovolt,  // picovolt
         Kilovolt,  // kilovolt
         Megavolt,  // megavolt
         Gigavolt // gigavolt
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Volt, new Guid("618fafff-fc9c-4d22-a64d-b7579931aa93")},  // volt
         {UnitChoicesEnum.Millivolt, new Guid("f186f5d4-2b0b-4cfe-b24c-0c02d3155cf8")},  // millivolt
         {UnitChoicesEnum.Centivolt, new Guid("ee01194e-dee3-4b50-8312-5fde3c8f774e")},  // centivolt
         {UnitChoicesEnum.Microvolt, new Guid("ede7e093-3e7d-429a-8c22-3b35ab5b20f2")},  // microvolt
         {UnitChoicesEnum.Nanovolt, new Guid("86dfcbe1-af8c-4081-b6ed-481eb44ab890")},  // nanovolt
         {UnitChoicesEnum.Picovolt, new Guid("19fb81d7-4991-4902-a1fd-55420789ac59")},  // picovolt
         {UnitChoicesEnum.Kilovolt, new Guid("6ffc60bc-ec9f-44d4-961b-79d9e593bf64")},  // kilovolt
         {UnitChoicesEnum.Megavolt, new Guid("3342ddbc-b1b2-46f8-addc-216ce94a616a")},  // megavolt
         {UnitChoicesEnum.Gigavolt, new Guid("1ce4342f-d4d2-44b2-81e9-73502c7ca10f")} // gigavolt
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ElongationGradientPerLengthQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         MetrePerMetre,  // metre per metre
         DecimetrePerMetre,  // decimetre per metre
         CentimetrePerMetre,  // centimetre per metre
         MillimetrePerMetre,  // millimetre per metre
         MicrometrePerMetre,  // micrometre per metre
         MetrePerKilometre,  // metre per kilometre
         DecimetrePerKilometre,  // decimetre per kilometre
         CentimetrePerKilometre,  // centimetre per kilometre
         MillimetrePerKilometre,  // millimetre per kilometre
         MicrometrePerKilometre,  // micrometre per kilometre
         InchPerFoot,  // inch per foot
         InchPerYard,  // inch per yard
         InchPerMile,  // inch per mile
         FootPerFoot,  // foot per foot
         FootPerYard,  // foot per yard
         FootPerMile,  // foot per mile
         YardPerFoot,  // yard per foot
         YardPerYard,  // yard per yard
         YardPerMile // yard per mile
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.MetrePerMetre, new Guid("cc12967b-4c7d-4c70-95cf-d2f23bd6f76b")},  // metre per metre
         {UnitChoicesEnum.DecimetrePerMetre, new Guid("337d67e2-b28c-4ab8-9817-3f9951fdb67b")},  // decimetre per metre
         {UnitChoicesEnum.CentimetrePerMetre, new Guid("4a7ce144-b35f-401f-bfbc-276b7c4ec4a9")},  // centimetre per metre
         {UnitChoicesEnum.MillimetrePerMetre, new Guid("4e241b59-388a-428f-82e7-b9971f9e1df5")},  // millimetre per metre
         {UnitChoicesEnum.MicrometrePerMetre, new Guid("bbd912b2-06e2-46fe-82da-af87bab150dc")},  // micrometre per metre
         {UnitChoicesEnum.MetrePerKilometre, new Guid("5b583a4c-7838-48e7-8201-420f43eef9e1")},  // metre per kilometre
         {UnitChoicesEnum.DecimetrePerKilometre, new Guid("7dc93254-9a25-4215-b2bc-9f2d8dc14d6e")},  // decimetre per kilometre
         {UnitChoicesEnum.CentimetrePerKilometre, new Guid("f539c676-e969-4235-9524-42e860e84966")},  // centimetre per kilometre
         {UnitChoicesEnum.MillimetrePerKilometre, new Guid("59f50e71-7796-4559-9e55-7fc420d9c20c")},  // millimetre per kilometre
         {UnitChoicesEnum.MicrometrePerKilometre, new Guid("73d8d799-d9f5-40f9-9216-4bc0bbf1c044")},  // micrometre per kilometre
         {UnitChoicesEnum.InchPerFoot, new Guid("3df1af23-afd0-41ed-9442-a3af6ae944d2")},  // inch per foot
         {UnitChoicesEnum.InchPerYard, new Guid("ec1fbeee-cbf4-41f0-94d8-573e241c22b2")},  // inch per yard
         {UnitChoicesEnum.InchPerMile, new Guid("998afd92-de3a-4f10-90b6-a252b8e59181")},  // inch per mile
         {UnitChoicesEnum.FootPerFoot, new Guid("a53ffdb6-a2db-4984-85aa-53763ba3aabb")},  // foot per foot
         {UnitChoicesEnum.FootPerYard, new Guid("ba9062f9-68be-4b9c-ba61-57c8543ed6f9")},  // foot per yard
         {UnitChoicesEnum.FootPerMile, new Guid("89b73d98-2818-43c5-8d31-8aa1bb78d3bc")},  // foot per mile
         {UnitChoicesEnum.YardPerFoot, new Guid("a6c2cf06-e21a-4387-90db-89d7d46b1b28")},  // yard per foot
         {UnitChoicesEnum.YardPerYard, new Guid("56f75af0-7213-43d9-b23f-bc74da6382e9")},  // yard per yard
         {UnitChoicesEnum.YardPerMile, new Guid("3283a57e-ec6d-4487-ab32-cdc0c5de2020")} // yard per mile
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class EnergyDensityQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         JoulePerCubicMetre,  // joule per cubic metre
         JoulePerLitre,  // joule per litre
         KilojoulePerCubicMetre,  // kilojoule per cubic metre
         KilojoulePerLitre,  // kilojoule per litre
         MegajoulePerCubicMetre,  // megajoule per cubic metre
         MegajoulePerLitre,  // megajoule per litre
         GigajoulePerCubicMetre,  // gigajoule per cubic metre
         GigajoulePerLitre,  // gigajoule per litre
         CaloriePerCubicMetre,  // calorie per cubic metre
         CaloriePerLitre,  // calorie per litre
         KilocaloriePerCubicMetre,  // kilocalorie per cubic metre
         KilocaloriePerLitre,  // kilocalorie per litre
         JoulePerCubicFoot,  // joule per cubic foot
         KilojoulePerCubicFoot,  // kilojoule per cubic foot
         MegajoulePerCubicFoot,  // megajoule per cubic foot
         GigajoulePerCubicFoot,  // gigajoule per cubic foot
         CaloriePerCubicFoot,  // calorie per cubic foot
         KilocaloriePerCubicFoot,  // kilocalorie per cubic foot
         JoulePerCubicInch,  // joule per cubic inch
         KilojoulePerCubicInch,  // kilojoule per cubic inch
         MegajoulePerCubicInch,  // megajoule per cubic inch
         GigajoulePerCubicInch,  // gigajoule per cubic inch
         CaloriePerCubicInch,  // calorie per cubic inch
         KilocaloriePerCubicInch,  // kilocalorie per cubic inch
         JoulePerGallonUK,  // joule per gallon (UK)
         KilojoulePerGallonUK,  // kilojoule per gallon (UK)
         MegajoulePerGallonUK,  // megajoule per gallon (UK)
         GigajoulePerGallonUK,  // gigajoule per gallon (UK)
         CaloriePerGallonUK,  // calorie per gallon (UK)
         KilocaloriePerGallonUK,  // kilocalorie per gallon (UK)
         JoulePerGallonUS,  // joule per gallon (US)
         KilojoulePerGallonUS,  // kilojoule per gallon (US)
         MegajoulePerGallonUS,  // megajoule per gallon (US)
         GigajoulePerGallonUS,  // gigajoule per gallon (US)
         CaloriePerGallonUS,  // calorie per gallon (US)
         KilocaloriePerGallonUS,  // kilocalorie per gallon (US)
         BritishThermalUnitPerCubicMetre,  // british thermal unit per cubic metre
         BritishThermalUnitPerLitre,  // british thermal unit per litre
         BritishThermalUnitPerCubicFoot,  // british thermal unit per cubic foot
         BritishThermalUnitPerCubicInch,  // british thermal unit per cubic inch
         BritishThermalUnitPerGallonUK,  // british thermal unit per gallon (UK)
         BritishThermalUnitPerGallonUS,  // british thermal unit per gallon (US)
         KiloBritishThermalUnitPerCubicMetre,  // kilo british thermal unit per cubic metre
         KiloBritishThermalUnitPerLitre,  // kilo british thermal unit per litre
         KiloBritishThermalUnitPerCubicFoot,  // kilo british thermal unit per cubic foot
         KiloBritishThermalUnitPerCubicInch,  // kilo british thermal unit per cubic inch
         KiloBritishThermalUnitPerGallonUK,  // kilo british thermal unit per gallon (UK)
         KiloBritishThermalUnitPerGallonUS // kilo british thermal unit per gallon (US)
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.JoulePerCubicMetre, new Guid("bac0aae1-8b3b-403d-b1ea-874a74da849a")},  // joule per cubic metre
         {UnitChoicesEnum.JoulePerLitre, new Guid("c3ae0b19-e3b1-4433-8075-ffb1444f447d")},  // joule per litre
         {UnitChoicesEnum.KilojoulePerCubicMetre, new Guid("ebc0ab24-af1b-4315-a41e-c66ef83e02f0")},  // kilojoule per cubic metre
         {UnitChoicesEnum.KilojoulePerLitre, new Guid("5cfaa1c0-7155-4eb6-b1ab-48db0dab2043")},  // kilojoule per litre
         {UnitChoicesEnum.MegajoulePerCubicMetre, new Guid("a5d1096b-9900-46ff-8a81-17313266dfdd")},  // megajoule per cubic metre
         {UnitChoicesEnum.MegajoulePerLitre, new Guid("f57f2f8d-3998-4ff3-ab50-bf1e8fdf99f4")},  // megajoule per litre
         {UnitChoicesEnum.GigajoulePerCubicMetre, new Guid("7728780b-66c7-4832-b905-25cb4e0b3edf")},  // gigajoule per cubic metre
         {UnitChoicesEnum.GigajoulePerLitre, new Guid("37e4f478-4e24-4735-9c31-30047d48828b")},  // gigajoule per litre
         {UnitChoicesEnum.CaloriePerCubicMetre, new Guid("ea4ac69b-351f-43d6-b6f0-847e6a73a069")},  // calorie per cubic metre
         {UnitChoicesEnum.CaloriePerLitre, new Guid("66d4c95b-5281-45d2-ba67-ee513f64f8dc")},  // calorie per litre
         {UnitChoicesEnum.KilocaloriePerCubicMetre, new Guid("b8d2b560-8541-4ad0-bb63-e2f755e92cb3")},  // kilocalorie per cubic metre
         {UnitChoicesEnum.KilocaloriePerLitre, new Guid("5bb757ee-b4b4-4340-b05b-d15988369ea0")},  // kilocalorie per litre
         {UnitChoicesEnum.JoulePerCubicFoot, new Guid("5c91efe6-c268-4d31-bff8-768344290db6")},  // joule per cubic foot
         {UnitChoicesEnum.KilojoulePerCubicFoot, new Guid("2b21249b-3eb3-4f86-9d05-407e42d8d1c5")},  // kilojoule per cubic foot
         {UnitChoicesEnum.MegajoulePerCubicFoot, new Guid("6bf78807-e679-4930-9e73-e52968821d9b")},  // megajoule per cubic foot
         {UnitChoicesEnum.GigajoulePerCubicFoot, new Guid("97e4a46b-11f6-4cb5-a24f-f2b004ad06cd")},  // gigajoule per cubic foot
         {UnitChoicesEnum.CaloriePerCubicFoot, new Guid("3efcbcd4-9f17-476d-9a47-43600ab93236")},  // calorie per cubic foot
         {UnitChoicesEnum.KilocaloriePerCubicFoot, new Guid("ff0ebc90-50fa-4c50-8583-0879a5b2380c")},  // kilocalorie per cubic foot
         {UnitChoicesEnum.JoulePerCubicInch, new Guid("daba8c83-b6f5-40bb-8c9d-e476e5d1bce2")},  // joule per cubic inch
         {UnitChoicesEnum.KilojoulePerCubicInch, new Guid("5beeb10a-314a-4c15-858d-0e900d4803e1")},  // kilojoule per cubic inch
         {UnitChoicesEnum.MegajoulePerCubicInch, new Guid("309da835-8bcb-4bdf-bbd7-aad194ade23a")},  // megajoule per cubic inch
         {UnitChoicesEnum.GigajoulePerCubicInch, new Guid("698b690d-b747-48da-8433-5ac0b25598d3")},  // gigajoule per cubic inch
         {UnitChoicesEnum.CaloriePerCubicInch, new Guid("6ae5cf98-0fcc-4426-9c72-481d1b1992ce")},  // calorie per cubic inch
         {UnitChoicesEnum.KilocaloriePerCubicInch, new Guid("66375de4-842e-4813-b79e-3e62cf7a97cc")},  // kilocalorie per cubic inch
         {UnitChoicesEnum.JoulePerGallonUK, new Guid("1c3b4a46-1cfa-44e4-b10e-4ed0f74e2994")},  // joule per gallon (UK)
         {UnitChoicesEnum.KilojoulePerGallonUK, new Guid("a5086581-3c86-43f5-acd2-845bc35ebc33")},  // kilojoule per gallon (UK)
         {UnitChoicesEnum.MegajoulePerGallonUK, new Guid("45c985ee-25be-4bd6-88de-38c98f042dbe")},  // megajoule per gallon (UK)
         {UnitChoicesEnum.GigajoulePerGallonUK, new Guid("e09aff93-785e-4bae-b9c5-17961bfd6642")},  // gigajoule per gallon (UK)
         {UnitChoicesEnum.CaloriePerGallonUK, new Guid("c36ef596-5a06-4ace-a119-baae9f761e29")},  // calorie per gallon (UK)
         {UnitChoicesEnum.KilocaloriePerGallonUK, new Guid("92c90ba8-7b15-4f6c-980f-f1ca44fb556a")},  // kilocalorie per gallon (UK)
         {UnitChoicesEnum.JoulePerGallonUS, new Guid("357a5df6-6df1-43fa-8be8-652e8d97db7c")},  // joule per gallon (US)
         {UnitChoicesEnum.KilojoulePerGallonUS, new Guid("5d74db73-4d3f-49b8-9a6b-c8eef3b9e287")},  // kilojoule per gallon (US)
         {UnitChoicesEnum.MegajoulePerGallonUS, new Guid("38d388bf-bbb8-4c84-9ec8-e78c6d81ae07")},  // megajoule per gallon (US)
         {UnitChoicesEnum.GigajoulePerGallonUS, new Guid("b3360ab2-cd97-4e2f-b29d-1470c0b34c1f")},  // gigajoule per gallon (US)
         {UnitChoicesEnum.CaloriePerGallonUS, new Guid("30d202d9-0a80-4d71-b4c8-e434d714ee9a")},  // calorie per gallon (US)
         {UnitChoicesEnum.KilocaloriePerGallonUS, new Guid("05fa77bf-3dcb-4df0-bf4d-7394e9f8854d")},  // kilocalorie per gallon (US)
         {UnitChoicesEnum.BritishThermalUnitPerCubicMetre, new Guid("550355a8-bfbe-4045-ab32-28325513db51")},  // british thermal unit per cubic metre
         {UnitChoicesEnum.BritishThermalUnitPerLitre, new Guid("2458887d-ee5f-4502-9446-cb6a7ae55c9d")},  // british thermal unit per litre
         {UnitChoicesEnum.BritishThermalUnitPerCubicFoot, new Guid("ce9d74ec-ecfc-454a-a00f-818e59c55895")},  // british thermal unit per cubic foot
         {UnitChoicesEnum.BritishThermalUnitPerCubicInch, new Guid("dd01b982-428b-4798-9cf6-091317d8fe93")},  // british thermal unit per cubic inch
         {UnitChoicesEnum.BritishThermalUnitPerGallonUK, new Guid("49e1a251-6a46-4715-a399-8953b25b7ce8")},  // british thermal unit per gallon (UK)
         {UnitChoicesEnum.BritishThermalUnitPerGallonUS, new Guid("f03e145b-26fc-4597-8cd6-1f468e574644")},  // british thermal unit per gallon (US)
         {UnitChoicesEnum.KiloBritishThermalUnitPerCubicMetre, new Guid("982cde20-c1a6-4dcf-8e37-c3784a427bc0")},  // kilo british thermal unit per cubic metre
         {UnitChoicesEnum.KiloBritishThermalUnitPerLitre, new Guid("2a64cc42-f2ab-43df-858f-04659d9a1306")},  // kilo british thermal unit per litre
         {UnitChoicesEnum.KiloBritishThermalUnitPerCubicFoot, new Guid("fdf14475-d109-4450-b5c0-9462c7ef8ec8")},  // kilo british thermal unit per cubic foot
         {UnitChoicesEnum.KiloBritishThermalUnitPerCubicInch, new Guid("8d6b0b9d-8f02-4a6a-9df5-575ebebaaba3")},  // kilo british thermal unit per cubic inch
         {UnitChoicesEnum.KiloBritishThermalUnitPerGallonUK, new Guid("24f9799f-4918-4f1c-9155-6b87c99703b0")},  // kilo british thermal unit per gallon (UK)
         {UnitChoicesEnum.KiloBritishThermalUnitPerGallonUS, new Guid("d8751a32-b0bd-425a-a9df-0ef7f5f0cb1f")} // kilo british thermal unit per gallon (US)
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class EnergyQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Joule,  // joule
         Kilojoule,  // kilojoule
         Megajoule,  // megajoule
         Gigajoule,  // gigajoule
         Calorie,  // calorie
         Kilocalorie,  // kilocalorie
         BritishThermalUnit,  // british thermal unit
         KiloBritishThermalUnit,  // kilo british thermal unit
         MegaBritishThermalUnit // mega british thermal unit
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Joule, new Guid("c653b7de-0386-467c-8d25-60bb0f6a7076")},  // joule
         {UnitChoicesEnum.Kilojoule, new Guid("4b0cf63a-84af-4232-b7a1-7531ec1d47b0")},  // kilojoule
         {UnitChoicesEnum.Megajoule, new Guid("c4fdba05-7269-4098-8b33-bd8e50c67126")},  // megajoule
         {UnitChoicesEnum.Gigajoule, new Guid("c8781145-3c6c-4d87-9567-b0e6ec2821a2")},  // gigajoule
         {UnitChoicesEnum.Calorie, new Guid("3f020e89-3146-4f3f-9b9b-eecda4400b12")},  // calorie
         {UnitChoicesEnum.Kilocalorie, new Guid("e4e916fe-9e79-47c9-97e5-3e8458358578")},  // kilocalorie
         {UnitChoicesEnum.BritishThermalUnit, new Guid("8548500e-e3a9-4e36-aecb-024836b8a012")},  // british thermal unit
         {UnitChoicesEnum.KiloBritishThermalUnit, new Guid("b8e1ba3f-d374-4220-85a6-7a066d91dd26")},  // kilo british thermal unit
         {UnitChoicesEnum.MegaBritishThermalUnit, new Guid("329c7fef-b5da-489f-a973-9ec2efb82a19")} // mega british thermal unit
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MassDensityGradientPerLengthQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         KilogramPerCubicMetrePerMetre,  // kilogram per cubic metre per metre
         SpecificGravityPerMetre,  // specific gravity per metre
         SpecificGravityPer10Metre,  // specific gravity per 10 metre
         SpecificGravityPer30Metre,  // specific gravity per 30 metre
         SpecificGravityPer100Metre,  // specific gravity per 100 metre
         GramPerCubicCentimetrePer100Metre,  // gram per cubic centimetre per 100 metre
         PoundPerGallonUKPerFoot,  // pound per gallon (UK) per foot
         PoundPerGallonUKPer30Foot,  // pound per gallon (UK) per 30 foot
         PoundPerGallonUKPer100Foot,  // pound per gallon (UK) per 100 foot
         PoundPerGallonUSPerFoot,  // pound per gallon (US) per foot
         PoundPerGallonUSPer30Foot,  // pound per gallon (US) per 30 foot
         PoundPerGallonUSPer100Foot,  // pound per gallon (US) per 100 foot
         KilogramPerCubicMetrePer10Metre,  // kilogram per cubic metre per 10 metre
         KilogramPerCubicMetrePer30Metre,  // kilogram per cubic metre per 30 metre
         KilogramPerCubicMetrePer100Metre,  // kilogram per cubic metre per 100 metre
         GramPerCubicCentimetrePerMetre,  // gram per cubic centimetre per metre
         GramPerCubicCentimetrePer10Metre,  // gram per cubic centimetre per 10 metre
         GramPerCubicCentimetrePer30Metre,  // gram per cubic centimetre per 30 metre
         PoundPerCubicFootPerFoot,  // pound per cubic foot per foot
         PoundPerCubicFootPer30Foot,  // pound per cubic foot per 30 foot
         PoundPerCubicFootPer100Foot,  // pound per cubic foot per 100 foot
         PoundPerCubicInchPerFoot,  // pound per cubic inch per foot
         PoundPerCubicInchPer30Foot,  // pound per cubic inch per 30 foot
         PoundPerCubicInchPer100Foot,  // pound per cubic inch per 100 foot
         PoundPerCubicYardPerFoot,  // pound per cubic yard per foot
         PoundPerCubicYardPer30Foot,  // pound per cubic yard per 30 foot
         PoundPerCubicYardPer100Foot // pound per cubic yard per 100 foot
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.KilogramPerCubicMetrePerMetre, new Guid("00707a6a-2e33-4214-9f8c-3e64eaa82ec1")},  // kilogram per cubic metre per metre
         {UnitChoicesEnum.SpecificGravityPerMetre, new Guid("07964c1e-b0d5-4785-bee4-8b4b8882b8b2")},  // specific gravity per metre
         {UnitChoicesEnum.SpecificGravityPer10Metre, new Guid("4af1c9f0-480c-4e80-a62b-c6b57b486c3f")},  // specific gravity per 10 metre
         {UnitChoicesEnum.SpecificGravityPer30Metre, new Guid("f8499728-220b-4b2d-94b2-3dc2cdfa6a92")},  // specific gravity per 30 metre
         {UnitChoicesEnum.SpecificGravityPer100Metre, new Guid("af987ef2-c8e5-470a-bc53-b2fff05d2c6a")},  // specific gravity per 100 metre
         {UnitChoicesEnum.GramPerCubicCentimetrePer100Metre, new Guid("361f976c-6271-41d2-8da3-6b4009cf5e06")},  // gram per cubic centimetre per 100 metre
         {UnitChoicesEnum.PoundPerGallonUKPerFoot, new Guid("f2e67c73-3706-4c14-b23a-afe474b2ecbe")},  // pound per gallon (UK) per foot
         {UnitChoicesEnum.PoundPerGallonUKPer30Foot, new Guid("684acd16-b420-4952-bc42-ffb47044074d")},  // pound per gallon (UK) per 30 foot
         {UnitChoicesEnum.PoundPerGallonUKPer100Foot, new Guid("f4b6b8a9-c222-4ac9-a6bb-072a9ca7d567")},  // pound per gallon (UK) per 100 foot
         {UnitChoicesEnum.PoundPerGallonUSPerFoot, new Guid("56128f8e-f59e-4f30-927b-35acb6ab44b1")},  // pound per gallon (US) per foot
         {UnitChoicesEnum.PoundPerGallonUSPer30Foot, new Guid("389150f0-4602-4468-bba3-a8eaf1d36ca0")},  // pound per gallon (US) per 30 foot
         {UnitChoicesEnum.PoundPerGallonUSPer100Foot, new Guid("658a9698-d34b-4a56-9ee3-3cf6e46a52a3")},  // pound per gallon (US) per 100 foot
         {UnitChoicesEnum.KilogramPerCubicMetrePer10Metre, new Guid("2d0ed7e8-2b80-46ff-9566-bd1429aa3129")},  // kilogram per cubic metre per 10 metre
         {UnitChoicesEnum.KilogramPerCubicMetrePer30Metre, new Guid("dccaa4b1-cf9f-4075-a9f2-50931e38af01")},  // kilogram per cubic metre per 30 metre
         {UnitChoicesEnum.KilogramPerCubicMetrePer100Metre, new Guid("ccca234e-8626-4f75-beed-4da4abad1317")},  // kilogram per cubic metre per 100 metre
         {UnitChoicesEnum.GramPerCubicCentimetrePerMetre, new Guid("91fe264e-6f5f-4a4d-b7f7-1532810ad5bd")},  // gram per cubic centimetre per metre
         {UnitChoicesEnum.GramPerCubicCentimetrePer10Metre, new Guid("836701f1-1fbd-4ae3-aba8-17a97379272c")},  // gram per cubic centimetre per 10 metre
         {UnitChoicesEnum.GramPerCubicCentimetrePer30Metre, new Guid("faaa4f2f-4dd4-419a-a985-ea16c5fc6d49")},  // gram per cubic centimetre per 30 metre
         {UnitChoicesEnum.PoundPerCubicFootPerFoot, new Guid("2c75c006-8ab5-475e-87aa-f5b0501b5ad7")},  // pound per cubic foot per foot
         {UnitChoicesEnum.PoundPerCubicFootPer30Foot, new Guid("cd7e9b61-06e9-41ca-b1dd-c2dd0b2cce55")},  // pound per cubic foot per 30 foot
         {UnitChoicesEnum.PoundPerCubicFootPer100Foot, new Guid("bd3c10c7-aa1b-438a-a61d-791f05de5a64")},  // pound per cubic foot per 100 foot
         {UnitChoicesEnum.PoundPerCubicInchPerFoot, new Guid("8997f813-9692-4f1a-b281-42260799f2ab")},  // pound per cubic inch per foot
         {UnitChoicesEnum.PoundPerCubicInchPer30Foot, new Guid("394876ee-1779-4ac0-a306-ad64fd9b640f")},  // pound per cubic inch per 30 foot
         {UnitChoicesEnum.PoundPerCubicInchPer100Foot, new Guid("11826f55-a121-498c-b03d-e51431f00476")},  // pound per cubic inch per 100 foot
         {UnitChoicesEnum.PoundPerCubicYardPerFoot, new Guid("09c728a5-da92-4a0c-a5bb-3aa166c2564d")},  // pound per cubic yard per foot
         {UnitChoicesEnum.PoundPerCubicYardPer30Foot, new Guid("82a23b2f-56a9-4345-97ec-61e6ec250915")},  // pound per cubic yard per 30 foot
         {UnitChoicesEnum.PoundPerCubicYardPer100Foot, new Guid("6f82e6f2-2cb8-467f-8baa-6d766056acf7")} // pound per cubic yard per 100 foot
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MassDensityGradientPerTemperatureQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         KilogramPerCubicMetrePerKelvin,  // kilogram per cubic metre per kelvin
         SpecificGravityPerCelsius,  // specific gravity per celsius
         GramPerCubicCentimetrePerCelsius,  // gram per cubic centimetre per celsius
         PoundPerGallonUKPerCelsius,  // pound per gallon (UK) per celsius
         PoundPerGallonUSPerFahrenheit,  // pound per gallon (US) per fahrenheit
         PoundPerGallonUKPerFahrenheit,  // pound per gallon (UK) per fahrenheit
         PoundPerGallonUSPerCelsius,  // pound per gallon (US) per celsius
         PoundPerCubicFootPerCelsius,  // pound per cubic foot per celsius
         PoundPerCubicFootPerFahrenheit,  // pound per cubic foot per fahrenheit
         PoundPerCubicInchPerCelsius,  // pound per cubic inch per celsius
         PoundPerCubicInchPerFahrenheit,  // pound per cubic inch per fahrenheit
         PoundPerCubicYardPerCelsius,  // pound per cubic yard per celsius
         PoundPerCubicYeardPerFahrenheit // pound per cubic yeard per fahrenheit
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.KilogramPerCubicMetrePerKelvin, new Guid("8b947453-ebe8-4fa9-b59a-87557150e1cf")},  // kilogram per cubic metre per kelvin
         {UnitChoicesEnum.SpecificGravityPerCelsius, new Guid("2b1d68c0-4e75-4e9d-92a1-37d501e7cb3e")},  // specific gravity per celsius
         {UnitChoicesEnum.GramPerCubicCentimetrePerCelsius, new Guid("e78e2b25-e0a7-4c06-b6df-60f97f767a20")},  // gram per cubic centimetre per celsius
         {UnitChoicesEnum.PoundPerGallonUKPerCelsius, new Guid("edac57eb-7535-447f-bcf9-0c6709b6ae3b")},  // pound per gallon (UK) per celsius
         {UnitChoicesEnum.PoundPerGallonUSPerFahrenheit, new Guid("397a5f98-842e-4d86-8fb7-f4f2f82e720b")},  // pound per gallon (US) per fahrenheit
         {UnitChoicesEnum.PoundPerGallonUKPerFahrenheit, new Guid("24485846-7944-4903-a5c5-975298daf450")},  // pound per gallon (UK) per fahrenheit
         {UnitChoicesEnum.PoundPerGallonUSPerCelsius, new Guid("8b642465-acee-4a4a-9cb5-6fc16ace5bc3")},  // pound per gallon (US) per celsius
         {UnitChoicesEnum.PoundPerCubicFootPerCelsius, new Guid("592a60a7-71e1-40dc-bfe0-573e407b893c")},  // pound per cubic foot per celsius
         {UnitChoicesEnum.PoundPerCubicFootPerFahrenheit, new Guid("64b97848-5c42-49ec-a09e-05c7bd0cea6b")},  // pound per cubic foot per fahrenheit
         {UnitChoicesEnum.PoundPerCubicInchPerCelsius, new Guid("10e845fe-c8c1-4847-bf6a-874c1f746325")},  // pound per cubic inch per celsius
         {UnitChoicesEnum.PoundPerCubicInchPerFahrenheit, new Guid("586d023b-3c87-4354-bce9-5704c8d1ae0a")},  // pound per cubic inch per fahrenheit
         {UnitChoicesEnum.PoundPerCubicYardPerCelsius, new Guid("ea5147c2-d35b-4e0c-8c47-e9f04a6e0fa1")},  // pound per cubic yard per celsius
         {UnitChoicesEnum.PoundPerCubicYeardPerFahrenheit, new Guid("e15f8f82-0d58-487a-9d70-8f14f3606177")} // pound per cubic yeard per fahrenheit
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MassDensityQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         KilogramPerCubicMetre,  // kilogram per cubic metre
         GramPerCubicMetre,  // gram per cubic metre
         SpecificGravity,  // specific gravity
         GramPerCubicCentimetre,  // gram per cubic centimetre
         PoundPerGallonUK,  // pound per gallon (UK)
         PoundPerGallonUS,  // pound per gallon (US)
         PoundPerCubicFoot,  // pound per cubic foot
         PoundPerCubicInch,  // pound per cubic inch
         PoundPerCubicYard // pound per cubic yard
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.KilogramPerCubicMetre, new Guid("8e175ca0-08f6-441d-afcf-a58bbe429abf")},  // kilogram per cubic metre
         {UnitChoicesEnum.GramPerCubicMetre, new Guid("8c5b7fc3-0ade-4e85-9646-71ec5fcb869a")},  // gram per cubic metre
         {UnitChoicesEnum.SpecificGravity, new Guid("da94ba95-4494-45af-bf99-31f00031c680")},  // specific gravity
         {UnitChoicesEnum.GramPerCubicCentimetre, new Guid("64f1b0d8-609f-4ed9-99da-52e18fe97450")},  // gram per cubic centimetre
         {UnitChoicesEnum.PoundPerGallonUK, new Guid("75ecf787-8778-4d74-afc7-498e117d27bf")},  // pound per gallon (UK)
         {UnitChoicesEnum.PoundPerGallonUS, new Guid("dcc01dd0-4610-42c7-9a55-2ddeb45ef6da")},  // pound per gallon (US)
         {UnitChoicesEnum.PoundPerCubicFoot, new Guid("f7479c8c-8d03-460b-bfa3-2b68808be935")},  // pound per cubic foot
         {UnitChoicesEnum.PoundPerCubicInch, new Guid("d549658a-76ab-4507-8a9e-e62a5cf47e23")},  // pound per cubic inch
         {UnitChoicesEnum.PoundPerCubicYard, new Guid("8199e187-5283-42cc-891e-b3887c3aa450")} // pound per cubic yard
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MassDensityRateOfChangeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         KilogramPerCubicMetrePerSecond,  // kilogram per cubic metre per second
         SpecificGravityPerSecond,  // specific gravity per second
         SpecificGravityPerMinute,  // specific gravity per minute
         SpecificGravityPerHour,  // specific gravity per hour
         GramPerCubicCentimetrePerSecond,  // gram per cubic centimetre per second
         GramPerCubicCentimetrePerMinute,  // gram per cubic centimetre per minute
         GramPerCubicCentimetrePerHour,  // gram per cubic centimetre per hour
         PoundPerGallonUKPerSecond,  // pound per gallon (UK) per second
         PoundPerGallonUKPerMinute,  // pound per gallon (UK) per minute
         PoundPerGallonUKPerHour,  // pound per gallon (UK) per hour
         PoundPerGallonUSPerSecond,  // pound per gallon (US) per second
         PoundPerGallonUSPerMinute,  // pound per gallon (US) per minute
         PoundPerGallonUSPerHour // pound per gallon (US) per hour
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.KilogramPerCubicMetrePerSecond, new Guid("d80197aa-f0b2-4a26-a5a4-b132a248c377")},  // kilogram per cubic metre per second
         {UnitChoicesEnum.SpecificGravityPerSecond, new Guid("dec0a290-ffd8-4fc0-ae11-3a6068469791")},  // specific gravity per second
         {UnitChoicesEnum.SpecificGravityPerMinute, new Guid("9c314f49-3297-4f7b-9cf3-5da32ba2f1cc")},  // specific gravity per minute
         {UnitChoicesEnum.SpecificGravityPerHour, new Guid("ce5d34b0-d7ab-48a4-87c1-9a43fabf5c06")},  // specific gravity per hour
         {UnitChoicesEnum.GramPerCubicCentimetrePerSecond, new Guid("e26f57a2-9659-40fd-a670-38a3b83fd36f")},  // gram per cubic centimetre per second
         {UnitChoicesEnum.GramPerCubicCentimetrePerMinute, new Guid("93777f41-0e47-46aa-9ab6-413987553817")},  // gram per cubic centimetre per minute
         {UnitChoicesEnum.GramPerCubicCentimetrePerHour, new Guid("c8d6a682-00ca-4d0f-b603-bf2d755f4b31")},  // gram per cubic centimetre per hour
         {UnitChoicesEnum.PoundPerGallonUKPerSecond, new Guid("e5a712d2-b874-4e7a-873e-a4f4f3ec7a67")},  // pound per gallon (UK) per second
         {UnitChoicesEnum.PoundPerGallonUKPerMinute, new Guid("e79c74b9-774d-4695-81d5-75042f268b96")},  // pound per gallon (UK) per minute
         {UnitChoicesEnum.PoundPerGallonUKPerHour, new Guid("5b461e39-d632-4f5e-b7e7-ef30745e5070")},  // pound per gallon (UK) per hour
         {UnitChoicesEnum.PoundPerGallonUSPerSecond, new Guid("eee6814a-d701-4cd8-b392-ebfedde20e11")},  // pound per gallon (US) per second
         {UnitChoicesEnum.PoundPerGallonUSPerMinute, new Guid("c047d53d-38f3-4dce-b590-1c9ab700a3ea")},  // pound per gallon (US) per minute
         {UnitChoicesEnum.PoundPerGallonUSPerHour, new Guid("822327d4-9f7c-4e91-9528-4eff5dfc9643")} // pound per gallon (US) per hour
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ForceGradientPerLengthQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         NewtonPerMetre,  // newton per metre
         NewtonPer30Metre,  // newton per 30 metre
         NewtonPer10Metre,  // newton per 10 metre
         NewtonPerDecimetre,  // newton per decimetre
         NewtonPerCentimetre,  // newton per centimetre
         NewtonPerMillimetre,  // newton per millimetre
         DecanewtonPerMetre,  // decanewton per metre
         DecanewtonPer30Metre,  // decanewton per 30 metre
         DecanewtonPer10Metre,  // decanewton per 10 metre
         DecanewtonPerDecimetre,  // decanewton per decimetre
         DecanewtonPerCentimetre,  // decanewton per centimetre
         DecanewtonPerMillimetre,  // decanewton per millimetre
         KilonewtonPerMetre,  // kilonewton per metre
         KilonewtonPer30Metre,  // kilonewton per 30 metre
         KilonewtonPer10Metre,  // kilonewton per 10 metre
         KilonewtonPerDecimetre,  // kilonewton per decimetre
         KilonewtonPerCentimetre,  // kilonewton per centimetre
         KilonewtonPerMillimetre,  // kilonewton per millimetre
         PoundPerFoot,  // pound per foot
         PoundPerInch,  // pound per inch
         KilopoundPerFoot,  // kilopound per foot
         KilopoundPerInch,  // kilopound per inch
         PoundPer30Foot,  // pound per 30 foot
         PoundPer100Foot,  // pound per 100 foot
         KilopoundPer30Foot,  // kilopound per 30 foot
         KilopoundPer100Foot // kilopound per 100 foot
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.NewtonPerMetre, new Guid("e503a1d3-1815-4321-8087-6e3d6dc641c8")},  // newton per metre
         {UnitChoicesEnum.NewtonPer30Metre, new Guid("be16e271-5ce7-445b-a8db-9014a6acc22b")},  // newton per 30 metre
         {UnitChoicesEnum.NewtonPer10Metre, new Guid("46defe0c-4a00-45d1-83bb-f898e00a78c5")},  // newton per 10 metre
         {UnitChoicesEnum.NewtonPerDecimetre, new Guid("dcd21076-ecb6-481e-8b8b-1cd5ccc68915")},  // newton per decimetre
         {UnitChoicesEnum.NewtonPerCentimetre, new Guid("739cb2cd-2c9f-4efc-ad17-306b8f09de57")},  // newton per centimetre
         {UnitChoicesEnum.NewtonPerMillimetre, new Guid("9375f700-72fb-4212-a51d-0f4500e7b13c")},  // newton per millimetre
         {UnitChoicesEnum.DecanewtonPerMetre, new Guid("2566918f-f1b1-4ffb-906b-adb3680812e1")},  // decanewton per metre
         {UnitChoicesEnum.DecanewtonPer30Metre, new Guid("20de7177-2099-4f86-89da-fdfa68bf67ed")},  // decanewton per 30 metre
         {UnitChoicesEnum.DecanewtonPer10Metre, new Guid("4f30206a-b381-4a28-9e2d-fafc026e71d5")},  // decanewton per 10 metre
         {UnitChoicesEnum.DecanewtonPerDecimetre, new Guid("cf20b9bb-aab1-4f1a-832c-1cfbe8ffc825")},  // decanewton per decimetre
         {UnitChoicesEnum.DecanewtonPerCentimetre, new Guid("47704d55-35cc-4bfc-9f93-7cf7f29c81ac")},  // decanewton per centimetre
         {UnitChoicesEnum.DecanewtonPerMillimetre, new Guid("1f418c90-f2e6-4bc8-8c06-f281e56ef6cc")},  // decanewton per millimetre
         {UnitChoicesEnum.KilonewtonPerMetre, new Guid("9ec7912e-9506-43ce-9089-80000d7ddd3f")},  // kilonewton per metre
         {UnitChoicesEnum.KilonewtonPer30Metre, new Guid("b08fae49-fdc3-409e-8b0f-3349ab189dc9")},  // kilonewton per 30 metre
         {UnitChoicesEnum.KilonewtonPer10Metre, new Guid("f57cb3e9-4da5-4960-aff6-a27167276e4a")},  // kilonewton per 10 metre
         {UnitChoicesEnum.KilonewtonPerDecimetre, new Guid("f3033c1b-1be8-4110-832a-4b60c31043e6")},  // kilonewton per decimetre
         {UnitChoicesEnum.KilonewtonPerCentimetre, new Guid("4db740c5-df92-4f65-b0da-2119ad80cbfc")},  // kilonewton per centimetre
         {UnitChoicesEnum.KilonewtonPerMillimetre, new Guid("14578d1b-6d43-441a-8f1b-aa77ab10a9bf")},  // kilonewton per millimetre
         {UnitChoicesEnum.PoundPerFoot, new Guid("516e4b02-2f1a-49a7-8cd9-3fa4e28c8fce")},  // pound per foot
         {UnitChoicesEnum.PoundPerInch, new Guid("8a5772d2-1253-4269-958a-af9f779aecc6")},  // pound per inch
         {UnitChoicesEnum.KilopoundPerFoot, new Guid("bf63e80f-97df-48d1-afbf-c83415654e44")},  // kilopound per foot
         {UnitChoicesEnum.KilopoundPerInch, new Guid("fa6a4a38-b070-48d1-a747-be22ab0e57b6")},  // kilopound per inch
         {UnitChoicesEnum.PoundPer30Foot, new Guid("0d0926be-19fa-4687-88d1-35f1acc58717")},  // pound per 30 foot
         {UnitChoicesEnum.PoundPer100Foot, new Guid("dcaa5f41-da2f-49d2-be41-80fb6f0a06ec")},  // pound per 100 foot
         {UnitChoicesEnum.KilopoundPer30Foot, new Guid("27a355cf-36ae-458d-acbd-2a5ad931bbab")},  // kilopound per 30 foot
         {UnitChoicesEnum.KilopoundPer100Foot, new Guid("0d5c841e-b259-4fdf-93d7-e39cca391adb")} // kilopound per 100 foot
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ForceQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Newton,  // newton
         Decanewton,  // decanewton
         Kilonewton,  // kilonewton
         Kilodecanewton,  // kilodecanewton
         KilogramForce,  // kilogram force
         TonneForce,  // tonne force
         PoundForce,  // pound force
         KilopoundForce // kilopound force
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Newton, new Guid("2e6b218c-0f85-4e8d-b9c5-73b78d207ef8")},  // newton
         {UnitChoicesEnum.Decanewton, new Guid("fc48e3a8-deb9-4cf6-aaad-5b18f7e37972")},  // decanewton
         {UnitChoicesEnum.Kilonewton, new Guid("e30d6f94-7887-4746-8d4f-eb720239b702")},  // kilonewton
         {UnitChoicesEnum.Kilodecanewton, new Guid("8ad7ae81-602b-4694-bc6d-c18f520f1266")},  // kilodecanewton
         {UnitChoicesEnum.KilogramForce, new Guid("ea771c51-4078-4aa6-b2df-db6f77a140ad")},  // kilogram force
         {UnitChoicesEnum.TonneForce, new Guid("6d7771d3-01cf-40f0-b5bc-3165cd0e6bea")},  // tonne force
         {UnitChoicesEnum.PoundForce, new Guid("c738ced5-1c99-42ec-9c47-59e7d6455ffa")},  // pound force
         {UnitChoicesEnum.KilopoundForce, new Guid("fa385f22-3ed9-4f34-ab0c-193e3ac79375")} // kilopound force
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class FrequencyQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Hertz,  // hertz
         ReciprocalSecond,  // reciprocal second
         Kilohertz,  // kilohertz
         Megahertz,  // megahertz
         Gigahertz,  // gigahertz
         Terahertz,  // terahertz
         Rpm,  // rpm
         Spm,  // spm
         RotationPerSecond,  // rotation per second
         StrokePerSecond,  // stroke per second
         StrokePerHour,  // stroke per hour
         RotationPerHour,  // rotation per hour
         ShockPerSecond,  // shock per second
         ShockPerMinute,  // shock per minute
         ShockPerHour,  // shock per hour
         RadianPerSecond,  // radian per second
         DegreePerSecond,  // degree per second
         RadianPerDay,  // radian per day
         RadianPerHour,  // radian per hour
         RadianPerMinute,  // radian per minute
         DegreePerDay,  // degree per day
         DegreePerHour,  // degree per hour
         DegreePerMinute // degree per minute
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Hertz, new Guid("7c572c06-0699-4187-9d0c-397f479fe93d")},  // hertz
         {UnitChoicesEnum.ReciprocalSecond, new Guid("39240f8f-8c82-4026-9db7-f72ec60cb4c9")},  // reciprocal second
         {UnitChoicesEnum.Kilohertz, new Guid("acf483c1-5d7a-4914-afa2-de7abed9be3e")},  // kilohertz
         {UnitChoicesEnum.Megahertz, new Guid("6dea9f29-d4f4-49a7-86fe-0205d4bab45e")},  // megahertz
         {UnitChoicesEnum.Gigahertz, new Guid("655ee4f9-1782-4ec0-894a-afff9b75cac7")},  // gigahertz
         {UnitChoicesEnum.Terahertz, new Guid("9ca52ae4-2fc5-4e60-b774-79c73442de13")},  // terahertz
         {UnitChoicesEnum.Rpm, new Guid("30880b5f-803d-412e-9736-62dca3ba5bbd")},  // rpm
         {UnitChoicesEnum.Spm, new Guid("426b000b-235f-41d5-8806-b2e47fbfb53b")},  // spm
         {UnitChoicesEnum.RotationPerSecond, new Guid("6e0ff63e-ef67-440d-a0f7-a17ff73cfff2")},  // rotation per second
         {UnitChoicesEnum.StrokePerSecond, new Guid("fe114eaf-117a-480e-9dbc-2db244b6d210")},  // stroke per second
         {UnitChoicesEnum.StrokePerHour, new Guid("b0f63a0c-9a53-4bdc-9166-03eb4254d3d8")},  // stroke per hour
         {UnitChoicesEnum.RotationPerHour, new Guid("cdc5dd34-dc2d-4bd8-85ac-13f6d71ea188")},  // rotation per hour
         {UnitChoicesEnum.ShockPerSecond, new Guid("b5318133-64e9-43c7-b7bf-3c86140fe7aa")},  // shock per second
         {UnitChoicesEnum.ShockPerMinute, new Guid("6ccbee46-cb8a-4777-b1d2-e88eedd24f73")},  // shock per minute
         {UnitChoicesEnum.ShockPerHour, new Guid("0c0d4ecb-ee11-4b57-9bc7-70860637232e")},  // shock per hour
         {UnitChoicesEnum.RadianPerSecond, new Guid("1da89c19-8dba-44c4-bff5-d1a7bcf97269")},  // radian per second
         {UnitChoicesEnum.DegreePerSecond, new Guid("dd8dc22c-5dd3-494e-8ff8-fed249a354bb")},  // degree per second
         {UnitChoicesEnum.RadianPerDay, new Guid("9d9be8e7-a4ff-4540-9e83-ecde31c24d6b")},  // radian per day
         {UnitChoicesEnum.RadianPerHour, new Guid("aaab4534-864c-4864-91de-e094f7c332b9")},  // radian per hour
         {UnitChoicesEnum.RadianPerMinute, new Guid("e2d71725-cebb-4c24-8cb8-4685390bdead")},  // radian per minute
         {UnitChoicesEnum.DegreePerDay, new Guid("a53ece85-ca3b-421a-aa71-6661edd30fa2")},  // degree per day
         {UnitChoicesEnum.DegreePerHour, new Guid("9f885257-5f15-4a4d-b5be-7d2f0bc0538e")},  // degree per hour
         {UnitChoicesEnum.DegreePerMinute, new Guid("61bc0b74-78f4-486f-b725-03ae520a6e8c")} // degree per minute
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class FrequencyRateOfChangeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         HertzPerSecond,  // hertz per second
         KiloHertzPerSecond,  // kilo hertz per second
         MegaHertzPerSecond,  // mega hertz per second
         GigaHertzPerSecond,  // giga hertz per second
         HertzPerMinute,  // hertz per minute
         KiloHertzPerMinute,  // kilo hertz per minute
         MegaHertzPerMinute,  // mega hertz per minute
         GigaHertzPerMinute,  // giga hertz per minute
         HertzPerHour,  // hertz per hour
         KiloHertzPerHour,  // kilo hertz per hour
         MegaHertzPerHour,  // mega hertz per hour
         GigaHertzPerHour,  // giga hertz per hour
         HertzPerDay,  // hertz per day
         KiloHertzPerDay,  // kilo hertz per day
         MegaHertzPerDay,  // mega hertz per day
         GigaHertzPerDay,  // giga hertz per day
         HertzPerYear,  // hertz per year
         KiloHertzPerYear,  // kilo hertz per year
         MegaHertzPerYear,  // mega hertz per year
         GigaHertzPerYear,  // giga hertz per year
         RpmPerSecond,  // rpm per second
         SpmPerSecond // spm per second
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.HertzPerSecond, new Guid("4d7e4b49-df76-4259-a96c-8c1250d5ecdd")},  // hertz per second
         {UnitChoicesEnum.KiloHertzPerSecond, new Guid("e197e7ca-93f7-4348-9508-74e61ce97f94")},  // kilo hertz per second
         {UnitChoicesEnum.MegaHertzPerSecond, new Guid("8c9671f4-54b6-40a0-94c1-5cfb25378f88")},  // mega hertz per second
         {UnitChoicesEnum.GigaHertzPerSecond, new Guid("46ad2062-982c-461f-95d8-ddd888e5d4f8")},  // giga hertz per second
         {UnitChoicesEnum.HertzPerMinute, new Guid("af3fcbbf-4fc8-4b5d-b555-33340d3c2f0f")},  // hertz per minute
         {UnitChoicesEnum.KiloHertzPerMinute, new Guid("0fabfb82-03fb-4855-aaea-578e36c9c7cf")},  // kilo hertz per minute
         {UnitChoicesEnum.MegaHertzPerMinute, new Guid("97c4e6e3-a8b3-4aa6-a742-1900a239e282")},  // mega hertz per minute
         {UnitChoicesEnum.GigaHertzPerMinute, new Guid("8d8d140d-00cd-4e80-aaa5-8d2d5ddcbc73")},  // giga hertz per minute
         {UnitChoicesEnum.HertzPerHour, new Guid("424100d5-ab81-4061-9429-74a9e3638453")},  // hertz per hour
         {UnitChoicesEnum.KiloHertzPerHour, new Guid("0963dc43-168a-483c-be3f-3c9054b0c692")},  // kilo hertz per hour
         {UnitChoicesEnum.MegaHertzPerHour, new Guid("a1b30880-ba44-4675-b808-6d93ba8aa8d2")},  // mega hertz per hour
         {UnitChoicesEnum.GigaHertzPerHour, new Guid("cd42ca67-9d8b-411c-bcce-e9e5ce6d1259")},  // giga hertz per hour
         {UnitChoicesEnum.HertzPerDay, new Guid("fe28723d-23e5-45f3-b286-50705746d643")},  // hertz per day
         {UnitChoicesEnum.KiloHertzPerDay, new Guid("0dc10fed-83a5-4570-a997-f2422d71d7fd")},  // kilo hertz per day
         {UnitChoicesEnum.MegaHertzPerDay, new Guid("c5743df5-a0be-41d2-99a1-b1f760940007")},  // mega hertz per day
         {UnitChoicesEnum.GigaHertzPerDay, new Guid("56e88229-8197-4ca2-aa69-e4100234d344")},  // giga hertz per day
         {UnitChoicesEnum.HertzPerYear, new Guid("1195a495-ea6e-4b5a-92b6-6ef0d2ca23d5")},  // hertz per year
         {UnitChoicesEnum.KiloHertzPerYear, new Guid("2e2a0d0f-5658-4ba2-8799-53bb06f197e7")},  // kilo hertz per year
         {UnitChoicesEnum.MegaHertzPerYear, new Guid("665c1c2a-57f6-4696-8b7b-524f8ad6084f")},  // mega hertz per year
         {UnitChoicesEnum.GigaHertzPerYear, new Guid("2c756b88-bbed-4650-8307-86bc7513caee")},  // giga hertz per year
         {UnitChoicesEnum.RpmPerSecond, new Guid("762b5d58-a1ba-40cb-8776-2004613d15fb")},  // rpm per second
         {UnitChoicesEnum.SpmPerSecond, new Guid("abcb24f7-c949-41dd-bf7d-acc23dc7e5e3")} // spm per second
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class HeatTransferCoefficientQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         WattPerSquareMetrePerKelvin,  // watt per square metre per kelvin
         BritishThermalUnitPerHourPerSquareFootPerDegreeFahrenheit // british thermal unit per hour per square foot per degree fahrenheit
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.WattPerSquareMetrePerKelvin, new Guid("e1737353-c10b-46cd-aa4e-9c90afb2f01e")},  // watt per square metre per kelvin
         {UnitChoicesEnum.BritishThermalUnitPerHourPerSquareFootPerDegreeFahrenheit, new Guid("6963db25-2bd9-4017-9c83-cc578a11abbf")} // british thermal unit per hour per square foot per degree fahrenheit
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ImageScaleQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         DotPerMetre,  // dot per metre
         DotPerInch,  // dot per inch
         DotPerMillimetre,  // dot per millimetre
         DotPerMicrometre // dot per micrometre
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.DotPerMetre, new Guid("acc723b8-083c-49f3-a208-184d2da3347d")},  // dot per metre
         {UnitChoicesEnum.DotPerInch, new Guid("e042b571-b7d0-477d-abf6-8b8998e5ba6c")},  // dot per inch
         {UnitChoicesEnum.DotPerMillimetre, new Guid("6d4d5f26-8812-4002-a2bf-27ec7871c1f4")},  // dot per millimetre
         {UnitChoicesEnum.DotPerMicrometre, new Guid("76e21d1d-54f5-4bbb-81c6-1b92b8b30bfe")} // dot per micrometre
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class InterfacialTensionQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         NewtonPerMetre,  // newton per metre
         MillinewtonPerMetre,  // millinewton per metre
         DynePerCentimetre,  // dyne per centimetre
         PoundPerSecondSquared // pound per second squared
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.NewtonPerMetre, new Guid("7ee9eca6-2704-442a-bd50-c8a0826da932")},  // newton per metre
         {UnitChoicesEnum.MillinewtonPerMetre, new Guid("7b1b363c-cbb0-4499-9d7c-762adc43e690")},  // millinewton per metre
         {UnitChoicesEnum.DynePerCentimetre, new Guid("a3c12fb9-6936-44bf-ad66-f4139163d11b")},  // dyne per centimetre
         {UnitChoicesEnum.PoundPerSecondSquared, new Guid("03db472b-b8e8-4ad0-b2b1-b8970686210c")} // pound per second squared
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class LengthQuantity : SymbolizedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Metre,  // metre
         Decimetre,  // decimetre
         Centimetre,  // centimetre
         Millimetre,  // millimetre
         Micrometre,  // micrometre
         Nanometre,  // nanometre
         Ångstrøm,  // ångstrøm
         Picometre,  // picometre
         Decametre,  // decametre
         Hectometre,  // hectometre
         Kilometre,  // kilometre
         AstronomicalUnit,  // astronomical unit
         LightYear,  // light year
         Parsec,  // parsec
         Foot,  // foot
         USSurveyFoot,  // US survey foot
         Inch,  // inch
         Yard,  // yard
         Fathom,  // fathom
         Surveyor_sChain,  // surveyor's chain
         Mile,  // mile
         InternationalNauticalMile,  // international nautical mile
         UKNauticalMile,  // UK nautical mile
         ScandinavianMile,  // scandinavian mile
         InchPer32,  // inch per 32
         Mil,  // mil
         Thou,  // thou
         Hand,  // hand
         Furlong // furlong
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Metre, new Guid("cc442e11-bb28-4e51-9074-87df66050d8a")},  // metre
         {UnitChoicesEnum.Decimetre, new Guid("e84c1968-cc63-412e-82c1-93ed39a43c01")},  // decimetre
         {UnitChoicesEnum.Centimetre, new Guid("96a3d4b4-c321-4528-92c0-7a52646b6461")},  // centimetre
         {UnitChoicesEnum.Millimetre, new Guid("0b2094f1-ba22-4b7b-888a-7a6b5da2ba25")},  // millimetre
         {UnitChoicesEnum.Micrometre, new Guid("60820c6d-d721-49b8-ba40-a75343aa0f2f")},  // micrometre
         {UnitChoicesEnum.Nanometre, new Guid("0d181caf-8121-46a8-bfa7-2cb7457d9db9")},  // nanometre
         {UnitChoicesEnum.Ångstrøm, new Guid("0db6a73f-c58f-415c-ac0d-e56137b28d5a")},  // ångstrøm
         {UnitChoicesEnum.Picometre, new Guid("f305ce05-7bd1-4d67-a834-e9b932ca586e")},  // picometre
         {UnitChoicesEnum.Decametre, new Guid("0ff9e118-e7d5-4ace-b140-eb3383812b21")},  // decametre
         {UnitChoicesEnum.Hectometre, new Guid("cb5c81a3-b9da-42b5-a2ea-0509df445d01")},  // hectometre
         {UnitChoicesEnum.Kilometre, new Guid("93aee1b8-653d-4841-b948-10460cb84334")},  // kilometre
         {UnitChoicesEnum.AstronomicalUnit, new Guid("0471d74c-cc44-45bd-be0a-aaad6c05f0d0")},  // astronomical unit
         {UnitChoicesEnum.LightYear, new Guid("fc43d439-576f-430c-855e-60b28f70856e")},  // light year
         {UnitChoicesEnum.Parsec, new Guid("0565c7e8-11cb-48de-8d7a-d58c89955d0f")},  // parsec
         {UnitChoicesEnum.Foot, new Guid("b4adebce-d0cd-417a-b38c-ab4a2e38233a")},  // foot
         {UnitChoicesEnum.USSurveyFoot, new Guid("eaf5909f-c68e-4346-9517-1dafad48b161")},  // US survey foot
         {UnitChoicesEnum.Inch, new Guid("0a6e2349-6f90-4ac5-baed-ccdaf5e5b919")},  // inch
         {UnitChoicesEnum.Yard, new Guid("7b9156e4-7cce-41cf-a251-95412f4d91a5")},  // yard
         {UnitChoicesEnum.Fathom, new Guid("6be602af-ea19-41cc-af7f-8263564c3c3b")},  // fathom
         {UnitChoicesEnum.Surveyor_sChain, new Guid("f101708b-ab63-4f21-ac87-4b5b3615eb30")},  // surveyor's chain
         {UnitChoicesEnum.Mile, new Guid("95736fd3-878b-4d93-9a78-ee6f20619628")},  // mile
         {UnitChoicesEnum.InternationalNauticalMile, new Guid("4c297035-e0cf-4bfe-aa63-d835170e8e25")},  // international nautical mile
         {UnitChoicesEnum.UKNauticalMile, new Guid("3b7a50d6-dc58-4cd7-9a5b-96dba59eceaa")},  // UK nautical mile
         {UnitChoicesEnum.ScandinavianMile, new Guid("22e6763c-4105-4a4c-9dfe-044256a107d1")},  // scandinavian mile
         {UnitChoicesEnum.InchPer32, new Guid("758ae28a-7484-4e0c-91af-aa105bcd744f")},  // inch per 32
         {UnitChoicesEnum.Mil, new Guid("648502a7-47e0-42dc-aacc-2e1789b0f1ce")},  // mil
         {UnitChoicesEnum.Thou, new Guid("2ce1b2d1-8157-4ad5-ae85-5d6c634f5c68")},  // thou
         {UnitChoicesEnum.Hand, new Guid("608205f3-8c52-40f2-9796-a586d1cd62da")},  // hand
         {UnitChoicesEnum.Furlong, new Guid("7ce130d1-8147-409f-99b3-bf22b0aae4cc")} // furlong
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class LuminousIntensityQuantity : SymbolizedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Candela,  // candela
         LumenPerSteradian,  // lumen per steradian
         Millicandela,  // millicandela
         Kilocandela,  // kilocandela
         Hefnerkerze,  // hefnerkerze
         InternationalCandle,  // international candle
         DecimalCandle,  // decimal candle
         BerlinerLichteinheit,  // berliner lichteinheit
         DVWGCandle,  // DVWG candle
         Violle,  // violle
         Carcel // carcel
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Candela, new Guid("28411995-11f2-4967-92ed-5077237f17e1")},  // candela
         {UnitChoicesEnum.LumenPerSteradian, new Guid("5683bf23-cd97-4141-9bf4-62a43750ceda")},  // lumen per steradian
         {UnitChoicesEnum.Millicandela, new Guid("b722e2e1-bcc4-4d6c-ad47-dfd24bb66543")},  // millicandela
         {UnitChoicesEnum.Kilocandela, new Guid("f1159794-14ab-49bb-80de-0164c8172c1f")},  // kilocandela
         {UnitChoicesEnum.Hefnerkerze, new Guid("8059d89c-1ed5-43d3-a9dc-a11de6cd0f8d")},  // hefnerkerze
         {UnitChoicesEnum.InternationalCandle, new Guid("fa25c6d3-c832-42a1-8490-c31131378ee2")},  // international candle
         {UnitChoicesEnum.DecimalCandle, new Guid("a07a3c15-4679-4a6a-a79b-64fe27fa5799")},  // decimal candle
         {UnitChoicesEnum.BerlinerLichteinheit, new Guid("ffd07aaa-486b-495d-bb63-a93d122c35e4")},  // berliner lichteinheit
         {UnitChoicesEnum.DVWGCandle, new Guid("b2fa4a9a-4c5d-4a31-8002-a7bc9e857af5")},  // DVWG candle
         {UnitChoicesEnum.Violle, new Guid("1e53e27a-3e4f-4b68-833f-c7a05fdf094e")},  // violle
         {UnitChoicesEnum.Carcel, new Guid("70b8902f-8a35-4398-b4ba-1e2b4858264f")} // carcel
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MagneticFluxDensityQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Tesla,  // tesla
         Gauss,  // gauss
         Milligauss,  // milligauss
         Millitesla,  // millitesla
         Microtesla,  // microtesla
         Nanotesla,  // nanotesla
         MaxwellPerSquareCentimetre,  // maxwell per square centimetre
         WeberPerSquareMetre // weber per square metre
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Tesla, new Guid("33c3b59d-9876-4918-9f31-f22de88d7bde")},  // tesla
         {UnitChoicesEnum.Gauss, new Guid("c09cd87d-8a84-45d0-88d3-20bb5cc48559")},  // gauss
         {UnitChoicesEnum.Milligauss, new Guid("41ace729-a2ff-4047-adc3-375829de64c6")},  // milligauss
         {UnitChoicesEnum.Millitesla, new Guid("9b6d864e-6775-4668-a59d-e1ab432f8960")},  // millitesla
         {UnitChoicesEnum.Microtesla, new Guid("c6b30197-be6b-41b7-803d-a8de61338612")},  // microtesla
         {UnitChoicesEnum.Nanotesla, new Guid("9bef9def-8cd3-4f7b-b991-290d3441b3d4")},  // nanotesla
         {UnitChoicesEnum.MaxwellPerSquareCentimetre, new Guid("d1b202cb-87c6-417a-947c-5247e5cdfe82")},  // maxwell per square centimetre
         {UnitChoicesEnum.WeberPerSquareMetre, new Guid("fefe997a-f3a6-4663-a1de-32889ee0cf15")} // weber per square metre
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MagneticFluxQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Weber,  // weber
         Milliweber,  // milliweber
         Microweber,  // microweber
         VoltSecond,  // volt second
         UnitPole,  // unit pole
         Megaline,  // megaline
         Kiloline,  // kiloline
         Line,  // line
         Maxwell,  // maxwell
         TeslaSquareMetre,  // tesla square metre
         TeslaSquareCentimetre,  // tesla square centimetre
         GaussSquareCentimetre,  // gauss square centimetre
         MagneticFluxQuantum // magnetic flux quantum
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Weber, new Guid("d790689d-e7dd-43d8-a3c0-c17ccc8073e5")},  // weber
         {UnitChoicesEnum.Milliweber, new Guid("94f03fc7-b0bb-4356-8787-c9e33f6559d2")},  // milliweber
         {UnitChoicesEnum.Microweber, new Guid("200b9de7-0635-40eb-8ebd-9cef7c01ac10")},  // microweber
         {UnitChoicesEnum.VoltSecond, new Guid("430305c3-d672-4d68-9b16-d0517243a870")},  // volt second
         {UnitChoicesEnum.UnitPole, new Guid("3bac78d4-5601-4cb2-bea1-01d952597a4d")},  // unit pole
         {UnitChoicesEnum.Megaline, new Guid("cca39e15-ee2e-4b8f-8843-527b329f3e81")},  // megaline
         {UnitChoicesEnum.Kiloline, new Guid("85862477-e913-4bcf-9d24-8248ec975d43")},  // kiloline
         {UnitChoicesEnum.Line, new Guid("40d608dd-b19f-4489-aac3-a3a6b7a55413")},  // line
         {UnitChoicesEnum.Maxwell, new Guid("8c1fcd01-4a3d-469a-a019-d3b35f7ef8b5")},  // maxwell
         {UnitChoicesEnum.TeslaSquareMetre, new Guid("f6da9f32-0738-4014-aac6-fdc5935fd436")},  // tesla square metre
         {UnitChoicesEnum.TeslaSquareCentimetre, new Guid("312b97ea-6167-47b5-a046-c6c202fb7eb4")},  // tesla square centimetre
         {UnitChoicesEnum.GaussSquareCentimetre, new Guid("a0dc1e92-7e84-401f-bca2-a6eb618ef604")},  // gauss square centimetre
         {UnitChoicesEnum.MagneticFluxQuantum, new Guid("f768bd79-1119-401c-b0df-39a5207273e0")} // magnetic flux quantum
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MassGradientPerLengthQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         KilogramPerMetre,  // kilogram per metre
         PoundPerFoot,  // pound per foot
         GramPerMetre // gram per metre
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.KilogramPerMetre, new Guid("c8b4a6ea-29bf-4e5a-b7ce-9142cefc0752")},  // kilogram per metre
         {UnitChoicesEnum.PoundPerFoot, new Guid("6fdf4cb6-a43b-482d-9bc8-d4ad49770f9e")},  // pound per foot
         {UnitChoicesEnum.GramPerMetre, new Guid("0cea1e32-9adb-4882-9070-d027cd0eef8e")} // gram per metre
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MassRateQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         KilogramPerSecond,  // kilogram per second
         KilogramPerMinute,  // kilogram per minute
         KilogramPerHour,  // kilogram per hour
         KilogramPerYear,  // kilogram per year
         PoundPerSecond,  // pound per second
         PoundPerMinute,  // pound per minute
         PoundPerHour,  // pound per hour
         PoundPerYear // pound per year
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.KilogramPerSecond, new Guid("a2daceb8-7705-4c97-9945-b354ea1ff78d")},  // kilogram per second
         {UnitChoicesEnum.KilogramPerMinute, new Guid("b776ae6f-5b86-462c-b815-2608d7e98192")},  // kilogram per minute
         {UnitChoicesEnum.KilogramPerHour, new Guid("736e4fcd-434f-4442-b025-a480a1532543")},  // kilogram per hour
         {UnitChoicesEnum.KilogramPerYear, new Guid("0ce50feb-a755-4a62-a50b-4af417bc2702")},  // kilogram per year
         {UnitChoicesEnum.PoundPerSecond, new Guid("48ac7515-ce4e-4ed6-a198-fe3ed3451a38")},  // pound per second
         {UnitChoicesEnum.PoundPerMinute, new Guid("92d18443-9357-42cf-86d2-fa78996c838a")},  // pound per minute
         {UnitChoicesEnum.PoundPerHour, new Guid("d4e0791c-eb4c-47a4-9e71-af3ad1b707cc")},  // pound per hour
         {UnitChoicesEnum.PoundPerYear, new Guid("a461e40b-48ea-49b1-8a55-8e75b26fbb8e")} // pound per year
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MaterialStrengthQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Pascal,  // pascal
         Megapascal,  // megapascal
         Gigapascal,  // gigapascal
         Psi,  // psi
         PoundPer100SquareFoot,  // pound per 100 square foot
         MegapoundPerSquareInch // megapound per square inch
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Pascal, new Guid("159e99d3-c79d-4dc6-974f-05cc38af001e")},  // pascal
         {UnitChoicesEnum.Megapascal, new Guid("38b95b61-a825-4393-a0e8-ecd686575735")},  // megapascal
         {UnitChoicesEnum.Gigapascal, new Guid("c9aa0a18-02ac-42a0-9afe-8a08b4f03331")},  // gigapascal
         {UnitChoicesEnum.Psi, new Guid("4adf2a33-05c3-49bb-ba61-59dd76f4621e")},  // psi
         {UnitChoicesEnum.PoundPer100SquareFoot, new Guid("eb1e2a52-3de3-4338-ad4d-40e8ce90e40b")},  // pound per 100 square foot
         {UnitChoicesEnum.MegapoundPerSquareInch, new Guid("197a8b98-190d-4d45-91d7-85af12deab02")} // megapound per square inch
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class PorousMediumPermeabilityQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         SquareMetre,  // square metre
         Darcy,  // darcy
         Millidarcy,  // millidarcy
         Microdarcy,  // microdarcy
         Nanodarcy // nanodarcy
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.SquareMetre, new Guid("5e27ad4a-b541-4807-9a36-4bd159b33f52")},  // square metre
         {UnitChoicesEnum.Darcy, new Guid("9a89bcc3-dc77-4e3a-a492-fcdabc24ec41")},  // darcy
         {UnitChoicesEnum.Millidarcy, new Guid("8d7a6767-6c6b-4daf-8617-d35e4055d457")},  // millidarcy
         {UnitChoicesEnum.Microdarcy, new Guid("b552f28d-c68a-4c59-853c-fe6e03dd5f4c")},  // microdarcy
         {UnitChoicesEnum.Nanodarcy, new Guid("f35b05c7-8b2f-4194-9336-d42cec5f3ba5")} // nanodarcy
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class PlaneAngleQuantity : SymbolizedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Radian,  // radian
         Milliradian,  // milliradian
         Degree,  // degree
         Grad,  // grad
         Gon,  // gon
         Circle,  // circle
         Revolution,  // revolution
         Quadrant,  // quadrant
         Sextant,  // sextant
         Octant,  // octant
         ArcMinute,  // arc minute
         ArcSecond // arc second
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Radian, new Guid("a71fc712-342a-48c2-8e45-b56ee31c7ae0")},  // radian
         {UnitChoicesEnum.Milliradian, new Guid("34a37faf-dfb9-4a34-899c-c9fa78f295a5")},  // milliradian
         {UnitChoicesEnum.Degree, new Guid("023a3393-a01e-499f-967a-a76b1a78d586")},  // degree
         {UnitChoicesEnum.Grad, new Guid("584314cf-a10f-49b6-a5e9-1cfa0ec0f355")},  // grad
         {UnitChoicesEnum.Gon, new Guid("feefeed5-2df2-4c66-84f1-0de998ba44db")},  // gon
         {UnitChoicesEnum.Circle, new Guid("e27aeec3-667d-41bb-9bd2-60bf213f8b7b")},  // circle
         {UnitChoicesEnum.Revolution, new Guid("e613477b-f8bc-45c7-8ccc-391a7f33af05")},  // revolution
         {UnitChoicesEnum.Quadrant, new Guid("dedbbea6-40e7-439a-9409-220fee4c148a")},  // quadrant
         {UnitChoicesEnum.Sextant, new Guid("ce4197b4-6d9d-488b-a360-98899a82837e")},  // sextant
         {UnitChoicesEnum.Octant, new Guid("8f78bfce-cad9-4a77-aa5f-3a5fecc89364")},  // octant
         {UnitChoicesEnum.ArcMinute, new Guid("e1ce9562-ecd0-46e2-82e2-bcec1b6ac113")},  // arc minute
         {UnitChoicesEnum.ArcSecond, new Guid("bea092da-34d6-4130-bc65-41fb7702597a")} // arc second
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class PowerQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Watt,  // watt
         Decawatt,  // decawatt
         Hectowatt,  // hectowatt
         Kilowatt,  // kilowatt
         Megawatt,  // megawatt
         Gigawatt,  // gigawatt
         Terawatt,  // terawatt
         Petawatt,  // petawatt
         Exawatt,  // exawatt
         Deciwatt,  // deciwatt
         Centiwatt,  // centiwatt
         Milliwatt,  // milliwatt
         Microwatt,  // microwatt
         Nanowatt,  // nanowatt
         Picowatt,  // picowatt
         Femtowatt,  // femtowatt
         Attowatt // attowatt
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Watt, new Guid("9d986a0c-700f-4448-a48c-a028bbd22049")},  // watt
         {UnitChoicesEnum.Decawatt, new Guid("fa888306-f2ef-420a-9ce2-8c56fe64ea3c")},  // decawatt
         {UnitChoicesEnum.Hectowatt, new Guid("1f159f0d-635a-4bc8-9020-6c09d72b3f63")},  // hectowatt
         {UnitChoicesEnum.Kilowatt, new Guid("016b23c7-0231-45bb-8723-5d3d4cc5c054")},  // kilowatt
         {UnitChoicesEnum.Megawatt, new Guid("5719b5f3-5c24-46d2-8ccd-0a06cc6b49ae")},  // megawatt
         {UnitChoicesEnum.Gigawatt, new Guid("ba67ba92-cdf5-46a8-a5f5-56c1ad102417")},  // gigawatt
         {UnitChoicesEnum.Terawatt, new Guid("b3e60a20-9e0f-479b-903b-16b22d86a515")},  // terawatt
         {UnitChoicesEnum.Petawatt, new Guid("bafba6b7-8a58-46b0-b4c7-c9a008c5e8f4")},  // petawatt
         {UnitChoicesEnum.Exawatt, new Guid("457950e4-0d4c-4f18-87ae-c35a7d2f512a")},  // exawatt
         {UnitChoicesEnum.Deciwatt, new Guid("6a3cd886-1c2c-41c8-8214-b21aff588b1e")},  // deciwatt
         {UnitChoicesEnum.Centiwatt, new Guid("ac6c67e1-0912-44f2-9496-ed82aca2b925")},  // centiwatt
         {UnitChoicesEnum.Milliwatt, new Guid("4b9e8b24-6c84-423e-8f79-b2bec161f219")},  // milliwatt
         {UnitChoicesEnum.Microwatt, new Guid("f0345b17-3e67-4c27-a787-69cd6feb7b1b")},  // microwatt
         {UnitChoicesEnum.Nanowatt, new Guid("622ee208-1b04-42c4-ba6e-552e6e328e02")},  // nanowatt
         {UnitChoicesEnum.Picowatt, new Guid("5b46567b-0571-4ca7-90d5-6304a0b7f938")},  // picowatt
         {UnitChoicesEnum.Femtowatt, new Guid("325622ea-c161-4f4f-9ee4-86d9e802f21c")},  // femtowatt
         {UnitChoicesEnum.Attowatt, new Guid("7bc1807f-90ac-41b0-a15f-9d1c81101f6d")} // attowatt
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class PressureGradientPerLengthQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         PascalPerMetre,  // pascal per metre
         BarPerMetre,  // bar per metre
         PsiPerMetre,  // psi per metre
         PsiPerFoot // psi per foot
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.PascalPerMetre, new Guid("f5a37831-4a70-44de-af34-4f6ce1a54af3")},  // pascal per metre
         {UnitChoicesEnum.BarPerMetre, new Guid("73a70891-87cf-44fc-8437-94938f034eec")},  // bar per metre
         {UnitChoicesEnum.PsiPerMetre, new Guid("2235a51b-cdf2-4f53-9664-b7a968dbbba3")},  // psi per metre
         {UnitChoicesEnum.PsiPerFoot, new Guid("b99cef5c-d6df-4803-b52b-6050cf7e7ff8")} // psi per foot
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class PressureLossConstantQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         PressureLossConstantSI,  // pressure loss constant SI
         PressureLossConstantMetric,  // pressure loss constant metric
         PressureLossConstantUK,  // pressure loss constant UK
         PressureLossConstantUS // pressure loss constant US
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.PressureLossConstantSI, new Guid("e0b334c4-2e44-4b1b-891f-9deae86a4d17")},  // pressure loss constant SI
         {UnitChoicesEnum.PressureLossConstantMetric, new Guid("043fbd34-1e4f-45bc-9935-b1797b606fd6")},  // pressure loss constant metric
         {UnitChoicesEnum.PressureLossConstantUK, new Guid("d5a97f2d-cb2f-449f-8f60-0ad292a01b87")},  // pressure loss constant UK
         {UnitChoicesEnum.PressureLossConstantUS, new Guid("b5cb21d1-0e71-4ab2-8d9d-42de21753edc")} // pressure loss constant US
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class PressureQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Pascal,  // pascal
         Kilopascal,  // kilopascal
         Bar,  // bar
         Millibar,  // millibar
         Microbar,  // microbar
         PoundPerSquareInch,  // pound per square inch
         PoundPer100SquareFoot,  // pound per 100 square foot
         KilopoundPerSquareInch,  // kilopound per square inch
         StandardAtmosphere,  // standard atmosphere
         PoundPerSquareFoot,  // pound per square foot
         Megapascal,  // megapascal
         Gigapascal,  // gigapascal
         NewtonPerSquareMetre,  // newton per square metre
         NewtonPerSquareCentimetre,  // newton per square centimetre
         NewtonPerSquareMillimetre,  // newton per square millimetre
         KilonewtonPerSquareMetre,  // kilonewton per square metre
         MegapoundPerSquareInch,  // megapound per square inch
         Torr,  // torr
         CentimetreMercuryAtZeroDegreeCelsius,  // centimetre mercury at zero degree celsius
         MillimetreMercuryAtZeroDegreeCelsius,  // millimetre mercury at zero degree celsius
         InchMercuryAt32DegreeFahrenheit,  // inch mercury at 32 degree fahrenheit
         InchMercuryAt60DegreeFahrenheit,  // inch mercury at 60 degree fahrenheit
         CentimetreWaterAt4DegreeCelsius,  // centimetre water at 4 degree celsius
         MillimetreWaterAt4DegreeCelsius,  // millimetre water at 4 degree celsius
         InchWaterAt4DegreeCelsius,  // inch water at 4 degree celsius
         FootWaterAt4DegreeCelsius,  // foot water at 4 degree celsius
         DynePerSquareCentimetre // dyne per square centimetre
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Pascal, new Guid("4f8ebaf4-cd1b-4714-a609-0a9fbe44cafb")},  // pascal
         {UnitChoicesEnum.Kilopascal, new Guid("a41c04f5-198b-4a04-b90a-5700412a2a29")},  // kilopascal
         {UnitChoicesEnum.Bar, new Guid("0d182739-f8f6-47a6-afcb-71feac973307")},  // bar
         {UnitChoicesEnum.Millibar, new Guid("43e4fe86-948d-4765-a69d-513ce6dc2b5b")},  // millibar
         {UnitChoicesEnum.Microbar, new Guid("7fb9e41f-4748-4457-b8b9-efb73da52d94")},  // microbar
         {UnitChoicesEnum.PoundPerSquareInch, new Guid("afce482e-a8cf-47f8-85c1-22595d5b5485")},  // pound per square inch
         {UnitChoicesEnum.PoundPer100SquareFoot, new Guid("e3b95821-d782-4f12-a492-489cbcd6d2a1")},  // pound per 100 square foot
         {UnitChoicesEnum.KilopoundPerSquareInch, new Guid("a07b5fe5-87e3-4422-afe1-f54de24deeb8")},  // kilopound per square inch
         {UnitChoicesEnum.StandardAtmosphere, new Guid("93839971-33f2-43e9-82eb-9f869846f999")},  // standard atmosphere
         {UnitChoicesEnum.PoundPerSquareFoot, new Guid("35b28889-c076-4274-b200-cf7732b17aa3")},  // pound per square foot
         {UnitChoicesEnum.Megapascal, new Guid("4ef28797-f416-4d97-b36a-711ea848bcc0")},  // megapascal
         {UnitChoicesEnum.Gigapascal, new Guid("5c81fb9b-36ad-47b7-9a8e-c999f7fdbfe3")},  // gigapascal
         {UnitChoicesEnum.NewtonPerSquareMetre, new Guid("101e92c3-47ab-4d55-8982-93061bc82dea")},  // newton per square metre
         {UnitChoicesEnum.NewtonPerSquareCentimetre, new Guid("2aa59deb-84d9-41c5-969f-8c8bb9d0c369")},  // newton per square centimetre
         {UnitChoicesEnum.NewtonPerSquareMillimetre, new Guid("e5e9cb06-38a8-4ac2-a8a5-8b74689a31a8")},  // newton per square millimetre
         {UnitChoicesEnum.KilonewtonPerSquareMetre, new Guid("eaa46677-af1c-4922-bf61-d82f2925534b")},  // kilonewton per square metre
         {UnitChoicesEnum.MegapoundPerSquareInch, new Guid("bb49de0a-fbf3-4914-b6b9-fc60ab502522")},  // megapound per square inch
         {UnitChoicesEnum.Torr, new Guid("f5afdfee-624e-46fa-b798-0ab1b04d2181")},  // torr
         {UnitChoicesEnum.CentimetreMercuryAtZeroDegreeCelsius, new Guid("412602dc-837b-4fab-afc9-3bf4798a9bed")},  // centimetre mercury at zero degree celsius
         {UnitChoicesEnum.MillimetreMercuryAtZeroDegreeCelsius, new Guid("d91f64fe-4df4-4ddd-943c-d985fbd1659b")},  // millimetre mercury at zero degree celsius
         {UnitChoicesEnum.InchMercuryAt32DegreeFahrenheit, new Guid("ab729585-0716-4f24-9502-fcd07ba051bc")},  // inch mercury at 32 degree fahrenheit
         {UnitChoicesEnum.InchMercuryAt60DegreeFahrenheit, new Guid("83ed97cc-526c-41cc-be78-ea0c86412080")},  // inch mercury at 60 degree fahrenheit
         {UnitChoicesEnum.CentimetreWaterAt4DegreeCelsius, new Guid("a1bac4cc-f37c-4aa5-aec6-ede0b4c52f09")},  // centimetre water at 4 degree celsius
         {UnitChoicesEnum.MillimetreWaterAt4DegreeCelsius, new Guid("a46b3ef6-fe2a-4ff3-bc2d-7a26661ce45e")},  // millimetre water at 4 degree celsius
         {UnitChoicesEnum.InchWaterAt4DegreeCelsius, new Guid("3015f436-b35d-455c-af23-b9bc4dd857da")},  // inch water at 4 degree celsius
         {UnitChoicesEnum.FootWaterAt4DegreeCelsius, new Guid("52de6721-dfec-4a54-861c-e74da72c8470")},  // foot water at 4 degree celsius
         {UnitChoicesEnum.DynePerSquareCentimetre, new Guid("04ca59b8-90e1-4903-ac82-ee95cac0ca38")} // dyne per square centimetre
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ProportionQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Proportion,  // proportion
         Percent,  // percent
         PerThousand,  // per thousand
         PartPerMillion // part per million
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Proportion, new Guid("03eb339b-61aa-4b42-aa35-4a20c547fdb9")},  // proportion
         {UnitChoicesEnum.Percent, new Guid("1a825e84-bc53-4da8-a089-118fdf40b8f7")},  // percent
         {UnitChoicesEnum.PerThousand, new Guid("141465a2-9c3c-4dda-82ec-eb35e72250c2")},  // per thousand
         {UnitChoicesEnum.PartPerMillion, new Guid("af33bf27-c3b8-4746-8b08-826ed1d21792")} // part per million
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class RandomWalkQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         RadianPerSquareRootSecond,  // radian per square root second
         RadianPerSquareRootMinute,  // radian per square root minute
         RadianPerSquareRootHour,  // radian per square root hour
         RadianPerSquareRootDay,  // radian per square root day
         DegreePerSquareRootSecond,  // degree per square root second
         DegreePerSquareRootMinute,  // degree per square root minute
         DegreePerSquareRootHour,  // degree per square root hour
         DegreePerSquareRootDay // degree per square root day
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.RadianPerSquareRootSecond, new Guid("557ea59e-a1da-438b-b04d-ccfc5539f87f")},  // radian per square root second
         {UnitChoicesEnum.RadianPerSquareRootMinute, new Guid("ccc41b4e-2efb-4760-969f-94614248374f")},  // radian per square root minute
         {UnitChoicesEnum.RadianPerSquareRootHour, new Guid("e296c410-e278-4586-af95-bae6fe4f0673")},  // radian per square root hour
         {UnitChoicesEnum.RadianPerSquareRootDay, new Guid("fb4a74f9-a648-4310-a424-9c85036bbc41")},  // radian per square root day
         {UnitChoicesEnum.DegreePerSquareRootSecond, new Guid("87a0a4e3-a2f5-4f84-b845-c7e6276e1655")},  // degree per square root second
         {UnitChoicesEnum.DegreePerSquareRootMinute, new Guid("e8e3a988-4219-44a5-ae89-ce115a239d04")},  // degree per square root minute
         {UnitChoicesEnum.DegreePerSquareRootHour, new Guid("ab6b85cf-54e5-4c3b-a330-f65d7e3bb926")},  // degree per square root hour
         {UnitChoicesEnum.DegreePerSquareRootDay, new Guid("8f806d0f-3741-4aa8-9f37-54b4f80e307c")} // degree per square root day
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class RelativeTemperatureQuantity : TemperatureQuantity
  {
    public new enum UnitChoicesEnum
      {
         Kelvin,  // kelvin
         RelativeCelsius,  // relative celsius
         Rankine // rankine
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Kelvin, new Guid("8fc5fa10-2d89-4064-8ace-b852d9a8d31f")},  // kelvin
         {UnitChoicesEnum.RelativeCelsius, new Guid("10ea31a1-e661-41c9-9a3d-245904b73599")},  // relative celsius
         {UnitChoicesEnum.Rankine, new Guid("62f3ffbc-eda3-400a-9fb7-8d021771f0fa")} // rankine
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class SolidAngleQuantity : SymbolizedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Steradian,  // steradian
         Spat,  // spat
         DegreeSquared // degree squared
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Steradian, new Guid("aee057f5-3235-4976-b6e6-a57179f0173e")},  // steradian
         {UnitChoicesEnum.Spat, new Guid("44abc0c0-d564-442a-ac80-b08c9d499867")},  // spat
         {UnitChoicesEnum.DegreeSquared, new Guid("ad4b94e8-1a86-42ab-bfc6-9cc7ff7a835f")} // degree squared
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class IsobaricSpecificHeatCapacityQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         JoulePerKilogramKelvin,  // joule per kilogram kelvin
         JoulePerGramKelvin,  // joule per gram kelvin
         JoulePerGramDegreeCelsius,  // joule per gram degree celsius
         CaloriePerGramDegreeCelsius,  // calorie per gram degree celsius
         BritishThermalUnitPerPoundDegreeFahrenheit,  // british thermal unit per pound degree fahrenheit
         KilocaloriePerGramDegreeCelsius // kilocalorie per gram degree celsius
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.JoulePerKilogramKelvin, new Guid("52d9523e-546b-41dd-b283-a125447433a3")},  // joule per kilogram kelvin
         {UnitChoicesEnum.JoulePerGramKelvin, new Guid("0c38001b-ecba-4920-ac75-e4644d8feced")},  // joule per gram kelvin
         {UnitChoicesEnum.JoulePerGramDegreeCelsius, new Guid("5b620d63-2269-42d3-8385-edca04c7ea70")},  // joule per gram degree celsius
         {UnitChoicesEnum.CaloriePerGramDegreeCelsius, new Guid("bb241c58-e76c-4d96-81c1-356b3f2ad397")},  // calorie per gram degree celsius
         {UnitChoicesEnum.BritishThermalUnitPerPoundDegreeFahrenheit, new Guid("ad9274f2-4c1a-45fe-97c1-710f00deca16")},  // british thermal unit per pound degree fahrenheit
         {UnitChoicesEnum.KilocaloriePerGramDegreeCelsius, new Guid("b283ecf7-20e4-4a6c-b62b-b07f56fa6614")} // kilocalorie per gram degree celsius
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         JoulePerKilogramSquaredKelvin,  // joule per kilogram squared kelvin
         JoulePerGramSquaredKelvin,  // joule per gram squared kelvin
         JoulePerGramDegreeSquaredCelsius,  // joule per gram degree squared celsius
         CaloriePerGramDegreeSquaredCelsius,  // calorie per gram degree squared celsius
         BritishThermalUnitPerPoundSquaredDegreeFahrenheit // british thermal unit per pound squared degree fahrenheit 
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.JoulePerKilogramSquaredKelvin, new Guid("9570fd84-ff2e-4a74-93b7-39bcf6558301")},  // joule per kilogram squared kelvin
         {UnitChoicesEnum.JoulePerGramSquaredKelvin, new Guid("69520d03-c7c3-483f-bbbb-6bdf3cf74463")},  // joule per gram squared kelvin
         {UnitChoicesEnum.JoulePerGramDegreeSquaredCelsius, new Guid("9ed03436-3032-4bee-a145-fd03b6236816")},  // joule per gram degree squared celsius
         {UnitChoicesEnum.CaloriePerGramDegreeSquaredCelsius, new Guid("ad3fe4d1-3286-4313-9f45-f2110b7ca6f2")},  // calorie per gram degree squared celsius
         {UnitChoicesEnum.BritishThermalUnitPerPoundSquaredDegreeFahrenheit, new Guid("57264532-79b7-4a19-8ffe-617bba781be3")} // british thermal unit per pound squared degree fahrenheit 
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class StressQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Pascal,  // pascal
         Kilopascal,  // kilopascal
         Bar,  // bar
         Millibar,  // millibar
         Microbar,  // microbar
         PoundPerSquareInch,  // pound per square inch
         PoundPer100SquareFoot,  // pound per 100 square foot
         KilopoundPerSquareInch,  // kilopound per square inch
         PoundPerSquareFoot,  // pound per square foot
         Megapascal,  // megapascal
         Gigapascal,  // gigapascal
         NewtonPerSquareMetre,  // newton per square metre
         NewtonPerSquareCentimetre,  // newton per square centimetre
         NewtonPerSquareMillimetre,  // newton per square millimetre
         KilonewtonPerSquareMetre,  // kilonewton per square metre
         MegapoundPerSquareInch,  // megapound per square inch
         DynePerSquareCentimetre // dyne per square centimetre
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Pascal, new Guid("7a4c7d2e-62f1-43c7-9c9d-8ff8664b0d98")},  // pascal
         {UnitChoicesEnum.Kilopascal, new Guid("8f070021-4cc7-424d-a325-e2e57fc82874")},  // kilopascal
         {UnitChoicesEnum.Bar, new Guid("69864a1c-bb6b-400e-be3b-527bc94a9a96")},  // bar
         {UnitChoicesEnum.Millibar, new Guid("cf58a57a-381b-4864-9ab3-bbe42589d871")},  // millibar
         {UnitChoicesEnum.Microbar, new Guid("b3ae1880-5d17-4f4b-b837-c6dc13c44cae")},  // microbar
         {UnitChoicesEnum.PoundPerSquareInch, new Guid("0e385d5b-5d3a-4360-8695-a934f0152a09")},  // pound per square inch
         {UnitChoicesEnum.PoundPer100SquareFoot, new Guid("d1aade96-1038-4902-9c4a-95f96933d54d")},  // pound per 100 square foot
         {UnitChoicesEnum.KilopoundPerSquareInch, new Guid("02b3acd6-0715-4ef6-b8e4-6134a3cdc3a6")},  // kilopound per square inch
         {UnitChoicesEnum.PoundPerSquareFoot, new Guid("2d835d44-2ffd-4239-b0dd-c9c36a763d4a")},  // pound per square foot
         {UnitChoicesEnum.Megapascal, new Guid("b6de095b-2800-4faf-931b-e8b2b9b2e35f")},  // megapascal
         {UnitChoicesEnum.Gigapascal, new Guid("213a896e-47e4-4745-baed-c28861f938bb")},  // gigapascal
         {UnitChoicesEnum.NewtonPerSquareMetre, new Guid("23d4c68e-a606-4fc0-a2b8-74998f6c2862")},  // newton per square metre
         {UnitChoicesEnum.NewtonPerSquareCentimetre, new Guid("b42eccea-4c35-42af-ba98-5101a3c10b6b")},  // newton per square centimetre
         {UnitChoicesEnum.NewtonPerSquareMillimetre, new Guid("9f96d22c-9021-4ed6-9904-344d6cd2417a")},  // newton per square millimetre
         {UnitChoicesEnum.KilonewtonPerSquareMetre, new Guid("ff05dc39-74f1-4bc8-b2d4-e9ee518d3e43")},  // kilonewton per square metre
         {UnitChoicesEnum.MegapoundPerSquareInch, new Guid("c1b8c4db-7a1e-4201-b0aa-e23d1df40871")},  // megapound per square inch
         {UnitChoicesEnum.DynePerSquareCentimetre, new Guid("eee0197b-0fbd-4a21-8023-61403c9417fe")} // dyne per square centimetre
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class TemperatureGradientPerLengthQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         KelvinPerMetre,  // kelvin per metre
         CelsiusPerMetre,  // celsius per metre
         CelsiusPer10Metre,  // celsius per 10 metre
         CelsiusPer30Metre,  // celsius per 30 metre
         CelsiusPer100Metre,  // celsius per 100 metre
         CelsiusPerFoot,  // celsius per foot
         CelsiusPer30Foot,  // celsius per 30 foot
         CelsiusPer100Foot,  // celsius per 100 foot
         FahrenheitPerFoot,  // fahrenheit per foot
         FahrenheitPer30Foot,  // fahrenheit per 30 foot
         FahrenheitPer100Foot // fahrenheit per 100 foot
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.KelvinPerMetre, new Guid("f1fe19d2-12e3-43d1-ba97-3ef9e8ec9e73")},  // kelvin per metre
         {UnitChoicesEnum.CelsiusPerMetre, new Guid("40dbbdfe-b680-403a-8326-2c217ba85d52")},  // celsius per metre
         {UnitChoicesEnum.CelsiusPer10Metre, new Guid("5e4ff2bf-4788-4258-bd4a-7b18a13364ff")},  // celsius per 10 metre
         {UnitChoicesEnum.CelsiusPer30Metre, new Guid("d17464c4-a7ef-4dcd-b783-bafe6e9b92de")},  // celsius per 30 metre
         {UnitChoicesEnum.CelsiusPer100Metre, new Guid("b47f299a-913a-46b7-ad20-c683fa0f02d0")},  // celsius per 100 metre
         {UnitChoicesEnum.CelsiusPerFoot, new Guid("e7b05420-41f6-4812-bc54-9c14f05a9cbd")},  // celsius per foot
         {UnitChoicesEnum.CelsiusPer30Foot, new Guid("bea3df4f-78e9-4e1a-bbee-22086da043b4")},  // celsius per 30 foot
         {UnitChoicesEnum.CelsiusPer100Foot, new Guid("f9bae95a-b282-44a7-8ae0-54728ef3c7a3")},  // celsius per 100 foot
         {UnitChoicesEnum.FahrenheitPerFoot, new Guid("d08596f1-77c4-4a8e-9245-6bf563fa7345")},  // fahrenheit per foot
         {UnitChoicesEnum.FahrenheitPer30Foot, new Guid("a1664cb0-db5c-4933-9b57-d075c4975f46")},  // fahrenheit per 30 foot
         {UnitChoicesEnum.FahrenheitPer100Foot, new Guid("232e2d6d-cb65-4b56-9277-457e4ff678fa")} // fahrenheit per 100 foot
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class TemperatureQuantity : SymbolizedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Kelvin,  // kelvin
         Celsius,  // celsius
         Fahrenheit,  // fahrenheit
         Rankine,  // rankine
         Réaumur // réaumur
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Kelvin, new Guid("8fc5fa10-2d89-4064-8ace-b852d9a8d31f")},  // kelvin
         {UnitChoicesEnum.Celsius, new Guid("5d69048e-fe18-4923-9341-cb80c2ccf8cc")},  // celsius
         {UnitChoicesEnum.Fahrenheit, new Guid("55c289ab-6975-439f-9b7a-fdca6d219a9f")},  // fahrenheit
         {UnitChoicesEnum.Rankine, new Guid("b4d6c55d-cf05-46e1-a09b-d0b26eba634a")},  // rankine
         {UnitChoicesEnum.Réaumur, new Guid("968def6c-bc85-49b0-84a8-3ac7ad37efc6")} // réaumur
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ThermalConductivityQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         WattPerMetreKelvin,  // watt per metre kelvin
         CaloriePerMetreSecondDegreeCelsius,  // calorie per metre second degree celsius
         CaloriePerCentimetreSecondDegreeCelsius,  // calorie per centimetre second degree celsius
         BritishThermalUnitPerHourFootDegreeFahrenheit,  // british thermal unit per hour foot degree fahrenheit
         BritishThermalUnitInchPerHourSquareFootDegreeFahrenheit // british thermal unit inch per hour square foot degree fahrenheit
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.WattPerMetreKelvin, new Guid("3ddba24f-4ccf-4cb1-af6c-2829cac3b88f")},  // watt per metre kelvin
         {UnitChoicesEnum.CaloriePerMetreSecondDegreeCelsius, new Guid("d0386fc4-b97b-4874-8c8d-66e093c391ea")},  // calorie per metre second degree celsius
         {UnitChoicesEnum.CaloriePerCentimetreSecondDegreeCelsius, new Guid("5f8706ed-d938-4715-a0ca-2afff423f6e6")},  // calorie per centimetre second degree celsius
         {UnitChoicesEnum.BritishThermalUnitPerHourFootDegreeFahrenheit, new Guid("43169695-8f6e-42ad-8c07-566dc7651edb")},  // british thermal unit per hour foot degree fahrenheit
         {UnitChoicesEnum.BritishThermalUnitInchPerHourSquareFootDegreeFahrenheit, new Guid("c79c2b27-c956-49a3-9caf-8653017777ca")} // british thermal unit inch per hour square foot degree fahrenheit
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ThermalConductivityGradientPerTemperatureQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         WattPerMetreKelvinPerKelvin,  // watt per metre kelvin per kelvin
         CaloriePerMetreSecondDegreeCelsiusSquared,  // calorie per metre second degree celsius squared
         CaloriePerCentimetreSecondDegreeCelsiusSquared,  // calorie per centimetre second degree celsius squared
         BritishThermalUnitPerHourFootDegreeFahrenheitSquared,  // british thermal unit per hour foot degree fahrenheit squared
         BritishThermalUnitInchPerHourSquareFootDegreeFahrenheitSquared // british thermal unit inch per hour square foot degree fahrenheit squared
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.WattPerMetreKelvinPerKelvin, new Guid("0459940e-d71f-4b01-9ea6-eeb05d754af2")},  // watt per metre kelvin per kelvin
         {UnitChoicesEnum.CaloriePerMetreSecondDegreeCelsiusSquared, new Guid("eb08ff8c-d542-440f-a4c7-610653018910")},  // calorie per metre second degree celsius squared
         {UnitChoicesEnum.CaloriePerCentimetreSecondDegreeCelsiusSquared, new Guid("6c21a6cd-61fe-4086-95a7-ad6d6820c96e")},  // calorie per centimetre second degree celsius squared
         {UnitChoicesEnum.BritishThermalUnitPerHourFootDegreeFahrenheitSquared, new Guid("b79509ea-8c03-4538-9974-208f7e0ee40e")},  // british thermal unit per hour foot degree fahrenheit squared
         {UnitChoicesEnum.BritishThermalUnitInchPerHourSquareFootDegreeFahrenheitSquared, new Guid("918b4e34-3986-427f-8bb6-c09740a7c299")} // british thermal unit inch per hour square foot degree fahrenheit squared
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class TimeQuantity : SymbolizedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Second,  // second
         Millisecond,  // millisecond
         Microsecond,  // microsecond
         Shake,  // shake
         Nanosecond,  // nanosecond
         Picosecond,  // picosecond
         Minute,  // minute
         Hour,  // hour
         Day,  // day
         Week,  // week
         Fortnight,  // fortnight
         MonthCommon,  // month common
         MonthSideral,  // month sideral
         MonthSynodic,  // month synodic
         QuarterCommon,  // quarter common
         YearCommon,  // year common
         YearAverageGregorian,  // year average gregorian
         YearJulian,  // year julian
         YearLeap,  // year leap
         YearTropical,  // year tropical
         Decade,  // decade
         Century,  // century
         Millenia,  // millenia
         MillionYear // million year
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Second, new Guid("eac42b09-cc85-4e29-9aaf-05fe73bca2aa")},  // second
         {UnitChoicesEnum.Millisecond, new Guid("1c1b150f-80a0-47da-836c-a583c4fa4b74")},  // millisecond
         {UnitChoicesEnum.Microsecond, new Guid("18cf5c8f-bdd4-4575-a74b-f8321c567edb")},  // microsecond
         {UnitChoicesEnum.Shake, new Guid("f2b06fdd-ddfe-430b-8107-11597bdfdf2f")},  // shake
         {UnitChoicesEnum.Nanosecond, new Guid("6bf1a718-637c-4e86-ae3e-426d2a1a9195")},  // nanosecond
         {UnitChoicesEnum.Picosecond, new Guid("9e10f905-4dc5-4670-9adf-278afd7d4131")},  // picosecond
         {UnitChoicesEnum.Minute, new Guid("4b9dc241-388b-4b7a-b862-48db43234c4a")},  // minute
         {UnitChoicesEnum.Hour, new Guid("f0d815e4-9bef-4422-94e6-1de52216b611")},  // hour
         {UnitChoicesEnum.Day, new Guid("85442621-bb56-4e2a-8e77-2b72409ff84f")},  // day
         {UnitChoicesEnum.Week, new Guid("4dd50f01-60b3-4d44-82ea-ff8ededd797d")},  // week
         {UnitChoicesEnum.Fortnight, new Guid("bc87f864-3dc1-4f1a-87bc-4123a47c53dc")},  // fortnight
         {UnitChoicesEnum.MonthCommon, new Guid("41cceaa2-1a1d-40f1-9195-5183be9770d4")},  // month common
         {UnitChoicesEnum.MonthSideral, new Guid("2e7446c0-5b0e-44e1-9a27-f0bc7d8aeb98")},  // month sideral
         {UnitChoicesEnum.MonthSynodic, new Guid("31edcda9-df8f-4d15-83a9-7dafd8a7e404")},  // month synodic
         {UnitChoicesEnum.QuarterCommon, new Guid("71f0e01a-c1a2-49ba-a25b-c11854f8867c")},  // quarter common
         {UnitChoicesEnum.YearCommon, new Guid("38481414-3b9d-472d-ac31-04b00dcc9d5c")},  // year common
         {UnitChoicesEnum.YearAverageGregorian, new Guid("fc33008b-9517-440f-a56b-189c5d80621b")},  // year average gregorian
         {UnitChoicesEnum.YearJulian, new Guid("281f6c7b-da23-4aab-89e1-994e52280658")},  // year julian
         {UnitChoicesEnum.YearLeap, new Guid("c84c1293-82d4-481b-8f6e-8bb8b81e6273")},  // year leap
         {UnitChoicesEnum.YearTropical, new Guid("e93c0b95-68b1-4ecc-9b27-f07a9a53ad49")},  // year tropical
         {UnitChoicesEnum.Decade, new Guid("b7d3b041-7119-45a6-a5ea-e05d7cb3c68b")},  // decade
         {UnitChoicesEnum.Century, new Guid("5cf296b6-48bf-4cc1-bf79-0220100ef1db")},  // century
         {UnitChoicesEnum.Millenia, new Guid("c235c8fc-f15b-45ff-a1b7-aab46ccea159")},  // millenia
         {UnitChoicesEnum.MillionYear, new Guid("d0281a3c-86dd-4d05-b856-cb7fa67c0f4d")} // million year
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class TorqueGradientPerLengthQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         NewtonMetrePerMetre,  // newton metre per metre
         DecanewtonMetrePerMetre,  // decanewton metre per metre
         KilogramForceMetrePerMetre,  // kilogram force metre per metre
         KilonewtonMetrePerMetre,  // kilonewton metre per metre
         FootPoundPerMetre,  // foot pound per metre
         KilofootPoundPerMetre,  // kilofoot pound per metre
         NewtonDecimetrePerMetre,  // newton decimetre per metre
         NewtonCentimetrePerMetre,  // newton centimetre per metre
         NewtonMillimetrePerMetre,  // newton millimetre per metre
         InchPoundPerMetre,  // inch pound per metre
         NewtonMetrePerDecimetre,  // Newton metre per decimetre
         DecanewtonMetrePerDecimetre,  // decanewton metre per decimetre
         KilogramForceMetrePerDecimetre,  // kilogram force metre per decimetre
         KilonewtonMetrePerDecimetre,  // kilonewton metre per decimetre
         FootPoundPerDecimetre,  // foot pound per decimetre
         KilofootPoundPerDecimetre,  // kilofoot pound per decimetre
         NewtonDecimetrePerDecimetre,  // newton decimetre per decimetre
         NewtonCentimetrePerDecimetre,  // newton centimetre per decimetre
         NewtonMillimetrePerDecimetre,  // newton millimetre per decimetre
         InchPoundPerDecimetre,  // inch pound per decimetre
         NewtonMetrePerCentimetre,  // Newton metre per centimetre
         DecanewtonMetrePerCentimetre,  // decanewton metre per centimetre
         KilogramForceMetrePerCentimetre,  // kilogram force metre per centimetre
         KilonewtonMetrePerCentimetre,  // kilonewton metre per centimetre
         FootPoundPerCentimetre,  // foot pound per centimetre
         KilofootPoundPerCentimetre,  // kilofoot pound per centimetre
         NewtonDecimetrePerCentimetre,  // newton decimetre per centimetre
         NewtonCentimetrePerCentimetre,  // newton centimetre per centimetre
         NewtonMillimetrePerCentimetre,  // newton millimetre per centimetre
         InchPoundPerCentimetre,  // inch pound per centimetre
         NewtonMetrePerMillimetre,  // Newton metre per millimetre
         DecanewtonMetrePerMillimetre,  // decanewton metre per millimetre
         KilogramForceMetrePerMillimetre,  // kilogram force metre per millimetre
         KilonewtonMetrePerMillimetre,  // kilonewton metre per millimetre
         FootPoundPerMillimetre,  // foot pound per millimetre
         KilofootPoundPerMillimetre,  // kilofoot pound per millimetre
         NewtonDecimetrePerMillimetre,  // newton decimetre per millimetre
         NewtonCentimetrePerMillimetre,  // newton centimetre per millimetre
         NewtonMillimetrePerMillimetre,  // newton millimetre per millimetre
         InchPoundPerMillimetre,  // inch pound per millimetre
         NewtonMetrePerFoot,  // Newton metre per foot
         DecanewtonMetrePerFoot,  // decanewton metre per foot
         KilogramForceMetrePerFoot,  // kilogram force metre per foot
         KilonewtonMetrePerFoot,  // kilonewton metre per foot
         FootPoundPerFoot,  // foot pound per foot
         KilofootPoundPerFoot,  // kilofoot pound per foot
         NewtonDecimetrePerFoot,  // newton decimetre per foot
         NewtonCentimetrePerFoot,  // newton centimetre per foot
         NewtonMillimetrePerFoot,  // newton millimetre per foot
         InchPoundPerFoot,  // inch pound per foot
         NewtonMetrePerInch,  // Newton metre per inch
         DecanewtonMetrePerInch,  // decanewton metre per inch
         KilogramForceMetrePerInch,  // kilogram force metre per inch
         KilonewtonMetrePerInch,  // kilonewton metre per inch
         FootPoundPerInch,  // foot pound per inch
         KilofootPoundPerInch,  // kilofoot pound per inch
         NewtonDecimetrePerInch,  // newton decimetre per inch
         NewtonCentimetrePerInch,  // newton centimetre per inch
         NewtonMillimetrePerInch,  // newton millimetre per inch
         InchPoundPerInch // inch pound per inch
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.NewtonMetrePerMetre, new Guid("33baa8d7-6987-4217-959b-1e3aa5b04752")},  // newton metre per metre
         {UnitChoicesEnum.DecanewtonMetrePerMetre, new Guid("50a1ea8d-9a46-4e24-9e9f-dad66e8bb9ca")},  // decanewton metre per metre
         {UnitChoicesEnum.KilogramForceMetrePerMetre, new Guid("66f7449d-5a06-4dd0-bf27-0bed2d2e4bed")},  // kilogram force metre per metre
         {UnitChoicesEnum.KilonewtonMetrePerMetre, new Guid("d07e4c6c-fea3-4545-b020-9cc2402e1ca5")},  // kilonewton metre per metre
         {UnitChoicesEnum.FootPoundPerMetre, new Guid("e3b4fe22-6590-4b2d-b2fa-0250f1ca8b26")},  // foot pound per metre
         {UnitChoicesEnum.KilofootPoundPerMetre, new Guid("e9bff76e-5388-4ea0-85af-62c772d919c5")},  // kilofoot pound per metre
         {UnitChoicesEnum.NewtonDecimetrePerMetre, new Guid("87ef9e2b-7e3b-4bda-a406-9f0b7f06e8fa")},  // newton decimetre per metre
         {UnitChoicesEnum.NewtonCentimetrePerMetre, new Guid("c03b845d-f2e5-4a16-afca-efab2591c526")},  // newton centimetre per metre
         {UnitChoicesEnum.NewtonMillimetrePerMetre, new Guid("fb1bb6bb-9c4a-4ecd-99fc-4af502271614")},  // newton millimetre per metre
         {UnitChoicesEnum.InchPoundPerMetre, new Guid("18259cf1-1f96-4ace-b7ad-657a78254baf")},  // inch pound per metre
         {UnitChoicesEnum.NewtonMetrePerDecimetre, new Guid("5ceaa09f-0de4-4025-ba23-d3b76f55a8b1")},  // Newton metre per decimetre
         {UnitChoicesEnum.DecanewtonMetrePerDecimetre, new Guid("902dd7b3-0b4f-40a3-a089-02bb39367219")},  // decanewton metre per decimetre
         {UnitChoicesEnum.KilogramForceMetrePerDecimetre, new Guid("2c83dced-5b36-49f1-bd4a-c95d558fb868")},  // kilogram force metre per decimetre
         {UnitChoicesEnum.KilonewtonMetrePerDecimetre, new Guid("a3949720-a023-4c5d-9a5e-4194af30005f")},  // kilonewton metre per decimetre
         {UnitChoicesEnum.FootPoundPerDecimetre, new Guid("a6b11edb-e9bc-4a7e-9af9-ace7ca62b93b")},  // foot pound per decimetre
         {UnitChoicesEnum.KilofootPoundPerDecimetre, new Guid("92cb8e61-c58d-461a-a69c-ad9fe7324e2a")},  // kilofoot pound per decimetre
         {UnitChoicesEnum.NewtonDecimetrePerDecimetre, new Guid("61261367-5cb0-4038-bcfb-6c8395758a21")},  // newton decimetre per decimetre
         {UnitChoicesEnum.NewtonCentimetrePerDecimetre, new Guid("1eee8ef4-cb7b-451a-9658-d1704ccf81d2")},  // newton centimetre per decimetre
         {UnitChoicesEnum.NewtonMillimetrePerDecimetre, new Guid("6c49e6aa-7d4c-4c92-96e5-5e3f26c3a367")},  // newton millimetre per decimetre
         {UnitChoicesEnum.InchPoundPerDecimetre, new Guid("e638f5ee-bbf9-4e7b-ae6a-9613eb9792cc")},  // inch pound per decimetre
         {UnitChoicesEnum.NewtonMetrePerCentimetre, new Guid("d8c63694-bf19-48be-b9bd-0f5b462ce2ec")},  // Newton metre per centimetre
         {UnitChoicesEnum.DecanewtonMetrePerCentimetre, new Guid("e87d21e6-2191-4cee-aea8-1929df1d8bd0")},  // decanewton metre per centimetre
         {UnitChoicesEnum.KilogramForceMetrePerCentimetre, new Guid("6ee38a6b-907d-4de9-94f1-0d979ef58340")},  // kilogram force metre per centimetre
         {UnitChoicesEnum.KilonewtonMetrePerCentimetre, new Guid("2149c60b-d6a8-4056-9818-f7fe6d10c409")},  // kilonewton metre per centimetre
         {UnitChoicesEnum.FootPoundPerCentimetre, new Guid("730e5c03-816e-4f88-b7bf-632a8a30c3ca")},  // foot pound per centimetre
         {UnitChoicesEnum.KilofootPoundPerCentimetre, new Guid("6249a894-93d9-45b6-a188-eb2d4bef800e")},  // kilofoot pound per centimetre
         {UnitChoicesEnum.NewtonDecimetrePerCentimetre, new Guid("0dbdd140-66d3-4f47-bbea-f5025b804b20")},  // newton decimetre per centimetre
         {UnitChoicesEnum.NewtonCentimetrePerCentimetre, new Guid("2f7c8e32-f865-4b68-8a3c-4d8c862fd5f2")},  // newton centimetre per centimetre
         {UnitChoicesEnum.NewtonMillimetrePerCentimetre, new Guid("830ae4b8-76d5-404a-b0c7-90db357a68ec")},  // newton millimetre per centimetre
         {UnitChoicesEnum.InchPoundPerCentimetre, new Guid("6822172d-adf0-4a71-b883-f4bc825ee9ea")},  // inch pound per centimetre
         {UnitChoicesEnum.NewtonMetrePerMillimetre, new Guid("a6416087-d525-4d98-aa45-2006ceb4a474")},  // Newton metre per millimetre
         {UnitChoicesEnum.DecanewtonMetrePerMillimetre, new Guid("6acab23d-3952-4eed-b1a0-7c38f03109b0")},  // decanewton metre per millimetre
         {UnitChoicesEnum.KilogramForceMetrePerMillimetre, new Guid("cdd4e6aa-ee0b-4679-97be-82553960efd1")},  // kilogram force metre per millimetre
         {UnitChoicesEnum.KilonewtonMetrePerMillimetre, new Guid("1335bebf-fcda-4a39-afa8-7de3ed24fa0c")},  // kilonewton metre per millimetre
         {UnitChoicesEnum.FootPoundPerMillimetre, new Guid("73a284d2-8900-44bb-96ab-897416a525e1")},  // foot pound per millimetre
         {UnitChoicesEnum.KilofootPoundPerMillimetre, new Guid("bad7b651-25f9-4687-875f-7624388228d6")},  // kilofoot pound per millimetre
         {UnitChoicesEnum.NewtonDecimetrePerMillimetre, new Guid("ecfa262e-1242-4c36-8325-b98de1ef4ffd")},  // newton decimetre per millimetre
         {UnitChoicesEnum.NewtonCentimetrePerMillimetre, new Guid("0f0cd3a8-84ec-4b58-b100-3f413bea1e05")},  // newton centimetre per millimetre
         {UnitChoicesEnum.NewtonMillimetrePerMillimetre, new Guid("b20d7cec-c1f6-4f64-9b60-e77ea699d940")},  // newton millimetre per millimetre
         {UnitChoicesEnum.InchPoundPerMillimetre, new Guid("cd28eba2-c7ea-40d0-ac32-fb67a8f581bc")},  // inch pound per millimetre
         {UnitChoicesEnum.NewtonMetrePerFoot, new Guid("2ce8e697-3a8a-4a73-ac23-4730790b4812")},  // Newton metre per foot
         {UnitChoicesEnum.DecanewtonMetrePerFoot, new Guid("c6e8f7e7-0239-47bc-b230-0f5870c94b82")},  // decanewton metre per foot
         {UnitChoicesEnum.KilogramForceMetrePerFoot, new Guid("7b2d82cc-0ac5-4945-9914-c91e62ac61dd")},  // kilogram force metre per foot
         {UnitChoicesEnum.KilonewtonMetrePerFoot, new Guid("8b5be3db-bc7a-4107-b751-53d3d2772eb8")},  // kilonewton metre per foot
         {UnitChoicesEnum.FootPoundPerFoot, new Guid("85a75741-c967-4e10-b195-01e5e7297eda")},  // foot pound per foot
         {UnitChoicesEnum.KilofootPoundPerFoot, new Guid("65606e85-199d-4fee-8cd4-97715431d868")},  // kilofoot pound per foot
         {UnitChoicesEnum.NewtonDecimetrePerFoot, new Guid("cdd6f7a0-954c-4826-8263-bb2d7fb06764")},  // newton decimetre per foot
         {UnitChoicesEnum.NewtonCentimetrePerFoot, new Guid("1767c385-e868-457a-b0be-aac0a4db42ff")},  // newton centimetre per foot
         {UnitChoicesEnum.NewtonMillimetrePerFoot, new Guid("ba4691ad-7575-4b9d-a6c6-9c98dde239ca")},  // newton millimetre per foot
         {UnitChoicesEnum.InchPoundPerFoot, new Guid("d5f1dfc6-80ec-4780-a4ad-8df68c179eee")},  // inch pound per foot
         {UnitChoicesEnum.NewtonMetrePerInch, new Guid("394cc997-ae71-47d1-91be-8aa69fdb71d7")},  // Newton metre per inch
         {UnitChoicesEnum.DecanewtonMetrePerInch, new Guid("1f8786b1-4aae-45b9-9875-7fe63bddb6cb")},  // decanewton metre per inch
         {UnitChoicesEnum.KilogramForceMetrePerInch, new Guid("df62fa6d-b983-4960-ad7b-853e00ccf45c")},  // kilogram force metre per inch
         {UnitChoicesEnum.KilonewtonMetrePerInch, new Guid("cbc02a54-98e8-4b10-a028-152a5c92f2ce")},  // kilonewton metre per inch
         {UnitChoicesEnum.FootPoundPerInch, new Guid("9c9771ff-d194-4e9f-b663-7f817da4d207")},  // foot pound per inch
         {UnitChoicesEnum.KilofootPoundPerInch, new Guid("70978437-cf05-4138-8389-ec633fdc1fce")},  // kilofoot pound per inch
         {UnitChoicesEnum.NewtonDecimetrePerInch, new Guid("0f34a9e2-dc04-4ce7-ac42-6cc12e2c98b5")},  // newton decimetre per inch
         {UnitChoicesEnum.NewtonCentimetrePerInch, new Guid("622d36f4-6fed-4d16-a246-14477f724bca")},  // newton centimetre per inch
         {UnitChoicesEnum.NewtonMillimetrePerInch, new Guid("31169945-1b67-4225-a2c6-6e850c39cb2f")},  // newton millimetre per inch
         {UnitChoicesEnum.InchPoundPerInch, new Guid("2e16e0be-2037-413d-9f43-6316a24d1fca")} // inch pound per inch
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class TorqueQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         NewtonMetre,  // newton metre
         DecanewtonMetre,  // decanewton metre
         KilogramForceMetre,  // kilogram force metre
         KilonewtonMetre,  // kilonewton metre
         FootPound,  // foot pound
         KilofootPound,  // kilofoot pound
         NewtonDecimetre,  // newton decimetre
         NewtonCentimetre,  // newton centimetre
         NewtonMillimetre,  // newton millimetre
         InchPound // inch pound
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.NewtonMetre, new Guid("50b017fa-8d81-4076-a485-61de1d8301b5")},  // newton metre
         {UnitChoicesEnum.DecanewtonMetre, new Guid("501ce02c-8a5d-46e6-9ab6-ce443df70402")},  // decanewton metre
         {UnitChoicesEnum.KilogramForceMetre, new Guid("282f97a0-df2a-4016-9ab0-796db49ff384")},  // kilogram force metre
         {UnitChoicesEnum.KilonewtonMetre, new Guid("2e417a6e-1acc-4901-8704-7dfeb3f67546")},  // kilonewton metre
         {UnitChoicesEnum.FootPound, new Guid("700d9fc7-17e1-4ad6-84f1-39cacbe5fe51")},  // foot pound
         {UnitChoicesEnum.KilofootPound, new Guid("ee9be6ed-df75-4915-be6a-e3941dacd6bd")},  // kilofoot pound
         {UnitChoicesEnum.NewtonDecimetre, new Guid("e70db590-c5fb-4ab9-a2c4-3ad611cb7f63")},  // newton decimetre
         {UnitChoicesEnum.NewtonCentimetre, new Guid("4acf4542-8df0-4f57-a852-7c0184dbeec9")},  // newton centimetre
         {UnitChoicesEnum.NewtonMillimetre, new Guid("a933225d-7c8e-4ce4-b0ea-4c1c6e9f7e34")},  // newton millimetre
         {UnitChoicesEnum.InchPound, new Guid("0d40553e-d8c4-4b75-ad05-61199b15a0a1")} // inch pound
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class VelocityQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         MetrePerSecond,  // metre per second
         MetrePerMinute,  // metre per minute
         CentimetrePerSecond,  // centimetre per second
         MetrePerHour,  // metre per hour
         MetrePerDay,  // metre per day
         FootPerHour,  // foot per hour
         FootPerDay,  // foot per day
         FootPerMinute,  // foot per minute
         FootPerSecond,  // foot per second
         InchPerSecond,  // inch per second
         MilePerHour,  // mile per hour
         KilometrePerHour,  // kilometre per hour
         KilometrePerMinute,  // kilometre per minute
         KilometrePerSecond,  // kilometre per second
         KilometrePerDay,  // kilometre per day
         MilePerMinute,  // mile per minute
         MilePerSecond,  // mile per second
         MilePerDay,  // mile per day
         InchPerMinute,  // inch per minute
         InchPerHour,  // inch per hour
         InchPerDay,  // inch per day
         CentimetrePerMinute,  // centimetre per minute
         CentimetrePerHour,  // centimetre per hour
         CentimetrePerDay,  // centimetre per day
         MillimetrePerSecond,  // millimetre per second
         MillimetrePerMinute,  // millimetre per minute
         MillimetrePerHour,  // millimetre per hour
         MillimetrePerDay,  // millimetre per day
         DecimetrePerSecond,  // decimetre per second
         DecimetrePerMinute,  // decimetre per minute
         DecimetrePerHour,  // decimetre per hour
         DecimetrePerDay,  // decimetre per day
         FurlongPerFortnight // furlong per fortnight
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.MetrePerSecond, new Guid("919ac736-9a37-45d1-8c02-54bc453d65dc")},  // metre per second
         {UnitChoicesEnum.MetrePerMinute, new Guid("824d3b5b-1e51-446a-99a4-39c02377f303")},  // metre per minute
         {UnitChoicesEnum.CentimetrePerSecond, new Guid("d32f84d7-f076-49ab-8bd5-1ccd27e0eba6")},  // centimetre per second
         {UnitChoicesEnum.MetrePerHour, new Guid("b4867c19-0668-4043-b3b9-f666f7552b02")},  // metre per hour
         {UnitChoicesEnum.MetrePerDay, new Guid("aa10055f-1ef6-460d-841c-b6dc69f6a7f2")},  // metre per day
         {UnitChoicesEnum.FootPerHour, new Guid("adb3b459-aa7e-4639-ad07-6d19c80f8170")},  // foot per hour
         {UnitChoicesEnum.FootPerDay, new Guid("aa58ec87-0dbc-4ed4-b8aa-8553b02c7b14")},  // foot per day
         {UnitChoicesEnum.FootPerMinute, new Guid("2d139d2c-1063-4f8d-99ae-bf71a98a1076")},  // foot per minute
         {UnitChoicesEnum.FootPerSecond, new Guid("6c9eef39-29f0-4d6d-ae7a-f9161d8fd4fa")},  // foot per second
         {UnitChoicesEnum.InchPerSecond, new Guid("8cd16c97-5c7a-4ee9-b59b-cbe2decd8ff9")},  // inch per second
         {UnitChoicesEnum.MilePerHour, new Guid("6c6d0be3-5b60-4b8a-9fd6-8b7afb261081")},  // mile per hour
         {UnitChoicesEnum.KilometrePerHour, new Guid("a1bab5e0-221c-4555-bd37-cf2b8004fd53")},  // kilometre per hour
         {UnitChoicesEnum.KilometrePerMinute, new Guid("b37519e1-5d78-4d34-ad7b-37bc3f0bc775")},  // kilometre per minute
         {UnitChoicesEnum.KilometrePerSecond, new Guid("3944bb76-5675-49bf-ae2f-143d3ff8e41a")},  // kilometre per second
         {UnitChoicesEnum.KilometrePerDay, new Guid("2d09bf7b-0f99-42c0-9732-f9923c11bde1")},  // kilometre per day
         {UnitChoicesEnum.MilePerMinute, new Guid("959dcb48-193b-48a9-9b86-554ea6b6e755")},  // mile per minute
         {UnitChoicesEnum.MilePerSecond, new Guid("5ec77a90-200b-4e6e-877b-8df0edb7adc2")},  // mile per second
         {UnitChoicesEnum.MilePerDay, new Guid("340ef6b0-53c2-447c-b8dd-f8f184bce71d")},  // mile per day
         {UnitChoicesEnum.InchPerMinute, new Guid("d6421f59-0d0f-49e3-9f2c-37590569beb4")},  // inch per minute
         {UnitChoicesEnum.InchPerHour, new Guid("06115ddb-4f51-41cd-a502-8c4f443d66b2")},  // inch per hour
         {UnitChoicesEnum.InchPerDay, new Guid("38991fcc-56f6-4447-bd1e-86159681e8d0")},  // inch per day
         {UnitChoicesEnum.CentimetrePerMinute, new Guid("b52fb69d-f8f7-4e46-9223-626e7497854d")},  // centimetre per minute
         {UnitChoicesEnum.CentimetrePerHour, new Guid("9a4d693e-cb18-4587-a465-48aec69369bf")},  // centimetre per hour
         {UnitChoicesEnum.CentimetrePerDay, new Guid("d34eba86-b8e2-4f28-92bb-8a26132ccfc6")},  // centimetre per day
         {UnitChoicesEnum.MillimetrePerSecond, new Guid("8d787bbf-81b0-4ba4-b913-c71cfe4b7025")},  // millimetre per second
         {UnitChoicesEnum.MillimetrePerMinute, new Guid("87a2da8b-a5e8-43f4-af18-859f6e8dc822")},  // millimetre per minute
         {UnitChoicesEnum.MillimetrePerHour, new Guid("4628ccfb-2837-40b3-9141-222af23fa7be")},  // millimetre per hour
         {UnitChoicesEnum.MillimetrePerDay, new Guid("c1540a11-a20e-43e2-9d1b-e173b928c94b")},  // millimetre per day
         {UnitChoicesEnum.DecimetrePerSecond, new Guid("0f9aa2e1-b66f-4728-bf57-79526ffce563")},  // decimetre per second
         {UnitChoicesEnum.DecimetrePerMinute, new Guid("980c51cc-a185-44a6-a69c-34f52e2b1fe2")},  // decimetre per minute
         {UnitChoicesEnum.DecimetrePerHour, new Guid("1d3b5a3c-81ff-4698-b92f-9b721f946220")},  // decimetre per hour
         {UnitChoicesEnum.DecimetrePerDay, new Guid("dcb77826-7550-4681-b3ce-a59cfdb7620d")},  // decimetre per day
         {UnitChoicesEnum.FurlongPerFortnight, new Guid("028ad001-b80d-49d8-8d18-8e10c1f0239f")} // furlong per fortnight
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class VolumeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         CubicMetre,  // cubic metre
         Litre,  // litre
         Decilitre,  // decilitre
         Centilitre,  // centilitre
         Millilitre,  // millilitre
         USGallon,  // US gallon
         UKGallon,  // UK gallon
         Barrel,  // barrel
         MillionCubicMetre,  // million cubic metre
         MillionLitre,  // million litre
         MillionUKGallon,  // million UK gallon
         MillionBarrel,  // million barrel
         ThousandStandardCubicFoot,  // thousand standard cubic foot
         MillionStandardCubicFoot,  // million standard cubic foot
         CubicFoot,  // cubic foot
         CubicInch,  // cubic inch
         MillionUSGallon // million US gallon
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.CubicMetre, new Guid("a465ba87-53d6-456c-8e74-315a1a212498")},  // cubic metre
         {UnitChoicesEnum.Litre, new Guid("3f713743-6248-4b0b-8f3b-c97b6fab76b1")},  // litre
         {UnitChoicesEnum.Decilitre, new Guid("3d7dde61-a9e9-4df6-8ee4-1eb4a4be1147")},  // decilitre
         {UnitChoicesEnum.Centilitre, new Guid("8d9baa02-5c3e-46f8-b909-7ca92d7bfa7a")},  // centilitre
         {UnitChoicesEnum.Millilitre, new Guid("6eb0d045-36e5-448d-be94-96a24a03f3e6")},  // millilitre
         {UnitChoicesEnum.USGallon, new Guid("1377d8ac-d203-48ed-bad4-733b0dc9d496")},  // US gallon
         {UnitChoicesEnum.UKGallon, new Guid("78f1cef7-c489-498c-96fb-d37474e242a9")},  // UK gallon
         {UnitChoicesEnum.Barrel, new Guid("b0f03925-d158-48ee-8c33-bf72c08cae68")},  // barrel
         {UnitChoicesEnum.MillionCubicMetre, new Guid("4c021db5-327b-44d3-9c4f-5a9b84af602f")},  // million cubic metre
         {UnitChoicesEnum.MillionLitre, new Guid("4f3f67df-28af-4398-966f-b23de678f50c")},  // million litre
         {UnitChoicesEnum.MillionUKGallon, new Guid("ab9a2938-f519-47c0-bcaa-d61c8fa23c7b")},  // million UK gallon
         {UnitChoicesEnum.MillionBarrel, new Guid("9d03120c-2c74-4666-8e24-98e143ab88db")},  // million barrel
         {UnitChoicesEnum.ThousandStandardCubicFoot, new Guid("e50b6a47-cf4c-4a74-8c1c-758000df5d67")},  // thousand standard cubic foot
         {UnitChoicesEnum.MillionStandardCubicFoot, new Guid("387b78ff-d51b-4684-b059-4c813407d767")},  // million standard cubic foot
         {UnitChoicesEnum.CubicFoot, new Guid("1da2384d-f463-4b08-9c0b-1de06b51268c")},  // cubic foot
         {UnitChoicesEnum.CubicInch, new Guid("dacec282-dacd-4687-9943-8fa741124116")},  // cubic inch
         {UnitChoicesEnum.MillionUSGallon, new Guid("a1d1c28d-8d55-417e-93af-e7302b68ed13")} // million US gallon
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class VolumetricFlowRateOfChangeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         CubicMetrePerSecondSquared,  // cubic metre per second squared
         LitrePerMinuteSquared,  // litre per minute squared
         LitrePerMinutePerSecond,  // litre per minute per second
         LitrePerSecondSquared,  // litre per second squared
         UKGallonPerMinuteSquared,  // UK gallon per minute squared
         UKGallonPerMinutePerSecond,  // UK gallon per minute per second
         USGallonPerMinuteSquared,  // US gallon per minute squared
         USGallonPerMinutePerSecond // US gallon per minute per second
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.CubicMetrePerSecondSquared, new Guid("aef20431-be0b-44ea-8770-a59db19b7f94")},  // cubic metre per second squared
         {UnitChoicesEnum.LitrePerMinuteSquared, new Guid("b27d2f54-a1f3-4abb-ba6d-a2a8b530049a")},  // litre per minute squared
         {UnitChoicesEnum.LitrePerMinutePerSecond, new Guid("e5a265b6-a9ba-4a09-ba08-b8c417b28ffb")},  // litre per minute per second
         {UnitChoicesEnum.LitrePerSecondSquared, new Guid("a899c06f-18dd-4d2a-9743-489f0af5be91")},  // litre per second squared
         {UnitChoicesEnum.UKGallonPerMinuteSquared, new Guid("c7c61175-e527-4403-8425-32f681367985")},  // UK gallon per minute squared
         {UnitChoicesEnum.UKGallonPerMinutePerSecond, new Guid("298e7a16-07a5-4b5b-a0de-3e49b31254b4")},  // UK gallon per minute per second
         {UnitChoicesEnum.USGallonPerMinuteSquared, new Guid("61885289-823d-4b26-bdf2-bc4744567bef")},  // US gallon per minute squared
         {UnitChoicesEnum.USGallonPerMinutePerSecond, new Guid("3c530e9a-9376-49d1-a6b5-0a6f93f4184b")} // US gallon per minute per second
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class VolumetricFlowRateQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         CubicMetrePerSecond,  // cubic metre per second
         LitrePerSecond,  // litre per second
         CubicFootPerSecond,  // cubic foot per second
         UKGallonPerSecond,  // UK gallon per second
         USGallonPerSecond,  // US gallon per second
         BarrelPerSecond,  // barrel per second
         CubicMetrePerMinute,  // cubic metre per minute
         LitrePerMinute,  // litre per minute
         CubicFootPerMinute,  // cubic foot per minute
         UKGallonPerMinute,  // UK gallon per minute
         USGallonPerMinute,  // US gallon per minute
         BarrelPerMinute,  // barrel per minute
         CubicMetrePerHour,  // cubic metre per hour
         LitrePerHour,  // litre per hour
         CubicFootPerHour,  // cubic foot per hour
         UKGallonPerHour,  // UK gallon per hour
         USGallonPerHour,  // US gallon per hour
         BarrelPerHour,  // barrel per hour
         CubicMetrePerDay,  // cubic metre per day
         MillionCubicMetrePerDay,  // million cubic metre per day
         UKGallonPerDay,  // UK gallon per day
         USGallonPerDay,  // US gallon per day
         MillionUKGallonPerDay,  // million UK gallon per day
         MillionUSGallonPerDay,  // million US gallon per day
         LitrePerDay,  // litre per day
         MillionLiterPerDay,  // million liter per day
         CubicFootPerDay,  // cubic foot per day
         ThousandStandardCubicFootPerDay,  // thousand standard cubic foot per day
         MillionStandardCubicFootPerDay,  // million standard cubic foot per day
         BarrelPerDay,  // barrel per day
         CubicMetrePerYear,  // cubic metre per year
         LitrePerYear,  // litre per year
         CubicFootPerYear,  // cubic foot per year
         UKGallonPerYear,  // UK gallon per year
         USGallonPerYear,  // US gallon per year
         BarrelPerYear // barrel per year
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.CubicMetrePerSecond, new Guid("fabe7a23-1ba2-4f5d-b3f5-d5729166c7a0")},  // cubic metre per second
         {UnitChoicesEnum.LitrePerSecond, new Guid("6dafb683-0f61-42ba-a22c-7bd5c9cea4ae")},  // litre per second
         {UnitChoicesEnum.CubicFootPerSecond, new Guid("a02fc355-34da-447b-ad1f-66cef4cc96d5")},  // cubic foot per second
         {UnitChoicesEnum.UKGallonPerSecond, new Guid("21ca44f5-ed4e-414d-b285-b38730600794")},  // UK gallon per second
         {UnitChoicesEnum.USGallonPerSecond, new Guid("21fe7e3a-bf92-4c94-b6d7-e2d30dfc4cd3")},  // US gallon per second
         {UnitChoicesEnum.BarrelPerSecond, new Guid("a73caac6-062e-4f79-8374-8fb2598b6842")},  // barrel per second
         {UnitChoicesEnum.CubicMetrePerMinute, new Guid("98eec130-224c-457a-a5dd-50b55a3a28ab")},  // cubic metre per minute
         {UnitChoicesEnum.LitrePerMinute, new Guid("e1ff1684-02ed-41c0-a82c-40b4a6d348b5")},  // litre per minute
         {UnitChoicesEnum.CubicFootPerMinute, new Guid("a81e08db-d8fd-4379-bc8e-c223c3fb71b3")},  // cubic foot per minute
         {UnitChoicesEnum.UKGallonPerMinute, new Guid("bb1d3fbc-efc1-4cf7-9c13-33c25a874224")},  // UK gallon per minute
         {UnitChoicesEnum.USGallonPerMinute, new Guid("ceca76e7-fd1e-4220-b072-9f40467a05e3")},  // US gallon per minute
         {UnitChoicesEnum.BarrelPerMinute, new Guid("3672c640-3924-4921-861b-d14c99643615")},  // barrel per minute
         {UnitChoicesEnum.CubicMetrePerHour, new Guid("d6607bed-0235-4838-87c8-2ff6c76be2ad")},  // cubic metre per hour
         {UnitChoicesEnum.LitrePerHour, new Guid("fe2d8a01-fd2b-4652-80bd-f0da2ce990fd")},  // litre per hour
         {UnitChoicesEnum.CubicFootPerHour, new Guid("54f30d1c-670a-4566-bfb2-3ce644d68878")},  // cubic foot per hour
         {UnitChoicesEnum.UKGallonPerHour, new Guid("85d47672-e1ce-4b33-be1d-eabc2ec1c4c1")},  // UK gallon per hour
         {UnitChoicesEnum.USGallonPerHour, new Guid("36892d41-4a11-4d32-9a8c-1e43ac1f8c6d")},  // US gallon per hour
         {UnitChoicesEnum.BarrelPerHour, new Guid("af1bd583-3a13-4d3a-8f65-ede4f8fe9789")},  // barrel per hour
         {UnitChoicesEnum.CubicMetrePerDay, new Guid("f512755c-166c-4346-a0f7-74f09703410f")},  // cubic metre per day
         {UnitChoicesEnum.MillionCubicMetrePerDay, new Guid("d09b4db7-9fbe-4018-aeb7-15286ccff8d6")},  // million cubic metre per day
         {UnitChoicesEnum.UKGallonPerDay, new Guid("334cf647-375e-4423-8ef4-e1171f938f9a")},  // UK gallon per day
         {UnitChoicesEnum.USGallonPerDay, new Guid("c0a47cc8-8ba1-47a1-ab94-d3e4361dd063")},  // US gallon per day
         {UnitChoicesEnum.MillionUKGallonPerDay, new Guid("f083576e-1d99-4243-bcfa-5ca6093b6931")},  // million UK gallon per day
         {UnitChoicesEnum.MillionUSGallonPerDay, new Guid("ffe9958a-3daa-472d-87ab-0b1217dcb1c5")},  // million US gallon per day
         {UnitChoicesEnum.LitrePerDay, new Guid("67628fe2-6a64-4ea6-94fe-5a34e7568b53")},  // litre per day
         {UnitChoicesEnum.MillionLiterPerDay, new Guid("1889c1dd-5aa3-45fe-b6cb-79a760e44832")},  // million liter per day
         {UnitChoicesEnum.CubicFootPerDay, new Guid("6b9a886b-b96a-473b-8dea-e850583ad5d8")},  // cubic foot per day
         {UnitChoicesEnum.ThousandStandardCubicFootPerDay, new Guid("627a75ed-2ab8-480a-96be-b9266d34ba5d")},  // thousand standard cubic foot per day
         {UnitChoicesEnum.MillionStandardCubicFootPerDay, new Guid("f1c76ab3-8e54-4ad1-a7fe-a09be2a61285")},  // million standard cubic foot per day
         {UnitChoicesEnum.BarrelPerDay, new Guid("9d36ce54-5bdc-4f4b-9948-dd65c58c9db3")},  // barrel per day
         {UnitChoicesEnum.CubicMetrePerYear, new Guid("f0e95734-b6a0-4081-b022-8c5bc0e7dd64")},  // cubic metre per year
         {UnitChoicesEnum.LitrePerYear, new Guid("d2b8abd2-cd97-4933-8e0a-54d8a4eef7ce")},  // litre per year
         {UnitChoicesEnum.CubicFootPerYear, new Guid("5ae7fcd3-fae0-4d81-abca-4c3d36d49e6d")},  // cubic foot per year
         {UnitChoicesEnum.UKGallonPerYear, new Guid("b7f54c43-a2ee-4b27-bccb-4c0e752e4caf")},  // UK gallon per year
         {UnitChoicesEnum.USGallonPerYear, new Guid("94558d1e-fbe2-4f06-a985-44210a1f0bc8")},  // US gallon per year
         {UnitChoicesEnum.BarrelPerYear, new Guid("0360ea30-fbf5-4dda-bf99-670dd6983420")} // barrel per year
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class WaveNumberQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         ReciprocalMetre,  // reciprocal metre
         ReciprocalDecimetre,  // reciprocal decimetre
         ReciprocalCentimetre,  // reciprocal centimetre
         ReciprocalMillimetre,  // reciprocal millimetre
         ReciprocalMicrometre,  // reciprocal micrometre
         ReciprocalNanometre,  // reciprocal nanometre
         ReciprocalÅngstrøm,  // reciprocal ångstrøm
         ReciprocalPicometre,  // reciprocal picometre
         ReciprocalDecametre,  // reciprocal decametre
         ReciprocalHectometre,  // reciprocal hectometre
         ReciprocalKilometre,  // reciprocal kilometre
         ReciprocalAstronomicalUnit,  // reciprocal astronomical unit
         ReciprocalLightYear,  // reciprocal light year
         ReciprocalParsec,  // reciprocal parsec
         ReciprocalFoot,  // reciprocal foot
         ReciprocalUSSurveyFoot,  // reciprocal US survey foot
         ReciprocalInch,  // reciprocal inch
         ReciprocalYard,  // reciprocal yard
         ReciprocalFathom,  // reciprocal fathom
         ReciprocalSurveyorsChain,  // reciprocal surveyors chain
         ReciprocalMile,  // reciprocal mile
         ReciprocalInternationalNauticalMile,  // reciprocal international nautical mile
         ReciprocalUKNauticalMile,  // reciprocal UK nautical mile
         ReciprocalScandinavianMile,  // reciprocal scandinavian mile
         ReciprocalInchPer32,  // reciprocal inch per 32
         ReciprocalMil,  // reciprocal mil
         ReciprocalThou,  // reciprocal thou
         ReciprocalHand,  // reciprocal hand
         ReciprocalFurlong // reciprocal furlong
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.ReciprocalMetre, new Guid("3cd38922-b99f-45bb-af6e-a38ebf1240f0")},  // reciprocal metre
         {UnitChoicesEnum.ReciprocalDecimetre, new Guid("cf8a931b-eaa9-4b0c-8894-53d54e93cba1")},  // reciprocal decimetre
         {UnitChoicesEnum.ReciprocalCentimetre, new Guid("4f2c38c2-86ff-4842-afdd-3a9fcf8a623e")},  // reciprocal centimetre
         {UnitChoicesEnum.ReciprocalMillimetre, new Guid("2d484d0f-7d29-48e9-8d5b-ff82fca6f1c5")},  // reciprocal millimetre
         {UnitChoicesEnum.ReciprocalMicrometre, new Guid("ddbbb734-ddd2-4d26-84ba-9995fff479e6")},  // reciprocal micrometre
         {UnitChoicesEnum.ReciprocalNanometre, new Guid("25412abd-9c3e-4b67-b809-d92926eb2b58")},  // reciprocal nanometre
         {UnitChoicesEnum.ReciprocalÅngstrøm, new Guid("7c28c943-c084-48f6-804c-87e6c6902b35")},  // reciprocal ångstrøm
         {UnitChoicesEnum.ReciprocalPicometre, new Guid("bca4fde9-e17a-4a27-9a3f-34beab644ee2")},  // reciprocal picometre
         {UnitChoicesEnum.ReciprocalDecametre, new Guid("a691338d-1916-4355-b6e1-3b1bff086c14")},  // reciprocal decametre
         {UnitChoicesEnum.ReciprocalHectometre, new Guid("4da19df4-0ff6-424b-a2ab-9d5811ba48ca")},  // reciprocal hectometre
         {UnitChoicesEnum.ReciprocalKilometre, new Guid("a4b4ed8e-a1c6-4e3f-9421-8770cec6ff42")},  // reciprocal kilometre
         {UnitChoicesEnum.ReciprocalAstronomicalUnit, new Guid("4d58ee46-e637-4f5a-a1ff-33f002154fec")},  // reciprocal astronomical unit
         {UnitChoicesEnum.ReciprocalLightYear, new Guid("81c8c5d9-a892-4702-921b-9946abbef6b0")},  // reciprocal light year
         {UnitChoicesEnum.ReciprocalParsec, new Guid("b8f6a954-7fe6-4f31-94cc-e53bb5603cd5")},  // reciprocal parsec
         {UnitChoicesEnum.ReciprocalFoot, new Guid("1d6a5284-d32f-4f5a-ad27-bfc0f71069aa")},  // reciprocal foot
         {UnitChoicesEnum.ReciprocalUSSurveyFoot, new Guid("16c3f209-e890-4b35-807e-7115545406e0")},  // reciprocal US survey foot
         {UnitChoicesEnum.ReciprocalInch, new Guid("95cd773d-b6da-4148-bd9c-bed66b4a72d8")},  // reciprocal inch
         {UnitChoicesEnum.ReciprocalYard, new Guid("be5f64c0-592a-4f3b-b2b5-6df8b9d2a31b")},  // reciprocal yard
         {UnitChoicesEnum.ReciprocalFathom, new Guid("ae9e314e-de19-405e-a897-6a68cd4845f6")},  // reciprocal fathom
         {UnitChoicesEnum.ReciprocalSurveyorsChain, new Guid("90da0d97-195c-4c30-85d8-51d70b75f071")},  // reciprocal surveyors chain
         {UnitChoicesEnum.ReciprocalMile, new Guid("acbb10a5-602f-423b-bc15-bdfd80cb7008")},  // reciprocal mile
         {UnitChoicesEnum.ReciprocalInternationalNauticalMile, new Guid("78474000-3cd8-4102-b7b4-360b4c2130fc")},  // reciprocal international nautical mile
         {UnitChoicesEnum.ReciprocalUKNauticalMile, new Guid("8a55784f-b5f4-4aa7-b5f9-d19742857349")},  // reciprocal UK nautical mile
         {UnitChoicesEnum.ReciprocalScandinavianMile, new Guid("f766575d-9bfa-45fb-8bfd-50f12a0e6a6a")},  // reciprocal scandinavian mile
         {UnitChoicesEnum.ReciprocalInchPer32, new Guid("ac9c8b52-22f0-476d-a038-023695c24f25")},  // reciprocal inch per 32
         {UnitChoicesEnum.ReciprocalMil, new Guid("09d27104-6452-4976-98e0-6fd087e22eb1")},  // reciprocal mil
         {UnitChoicesEnum.ReciprocalThou, new Guid("e732fd46-ddf3-433e-8baf-cc531ca69160")},  // reciprocal thou
         {UnitChoicesEnum.ReciprocalHand, new Guid("844ed023-9f47-4147-8b50-cf03c99071b5")},  // reciprocal hand
         {UnitChoicesEnum.ReciprocalFurlong, new Guid("79a3f3db-0ac8-4bcb-b93c-fca2a673147b")} // reciprocal furlong
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MassQuantity : SymbolizedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Kilogram,  // kilogram
         Hectogram,  // hectogram
         Decagram,  // decagram
         Gram,  // gram
         Centigram,  // centigram
         Milligram,  // milligram
         Microgram,  // microgram
         Nanogram,  // nanogram
         AtomMassUnit,  // atom mass unit
         TonneMetric,  // tonne metric
         Kilotonne,  // kilotonne
         Megatonne,  // megatonne
         Gigatonne,  // gigatonne
         Pound,  // pound
         Kilopound,  // kilopound
         Ounce,  // ounce
         Stone,  // stone
         TonUK,  // ton UK
         TonUS,  // ton US
         SolarMass,  // solar mass
         EarthMass,  // earth mass
         Grain,  // grain
         HundredWeights // hundred weights
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Kilogram, new Guid("ef4c5fc1-8774-4aea-b772-35aeae56413d")},  // kilogram
         {UnitChoicesEnum.Hectogram, new Guid("2fb79e4b-3eb5-4aa3-9f12-2c66b1784902")},  // hectogram
         {UnitChoicesEnum.Decagram, new Guid("1b3f72cb-55b1-4027-b6ad-309cd7d6c1a3")},  // decagram
         {UnitChoicesEnum.Gram, new Guid("049ba04e-4c70-41f5-bb29-6b54bb5b2103")},  // gram
         {UnitChoicesEnum.Centigram, new Guid("e56aa2fa-80b7-417f-8e08-91b9b8a1198c")},  // centigram
         {UnitChoicesEnum.Milligram, new Guid("322b0e70-c8e5-482e-a9db-682d15baacf9")},  // milligram
         {UnitChoicesEnum.Microgram, new Guid("eb831d52-2690-4b8a-a1a4-83e9bdb07dbc")},  // microgram
         {UnitChoicesEnum.Nanogram, new Guid("93db8c40-4dd0-46a4-ade6-db51bcbca66f")},  // nanogram
         {UnitChoicesEnum.AtomMassUnit, new Guid("f470168e-1e20-458e-b6da-6bee551cb6d6")},  // atom mass unit
         {UnitChoicesEnum.TonneMetric, new Guid("320b99ba-3115-42f5-939c-15a04d9e7e3c")},  // tonne metric
         {UnitChoicesEnum.Kilotonne, new Guid("2a767cda-fc61-4aa4-81dd-1a4f6d6af755")},  // kilotonne
         {UnitChoicesEnum.Megatonne, new Guid("92c4b624-4205-4596-aabf-1dd4aa442718")},  // megatonne
         {UnitChoicesEnum.Gigatonne, new Guid("51cd0591-d741-4769-bd22-e36959d1adcf")},  // gigatonne
         {UnitChoicesEnum.Pound, new Guid("e9e313ad-cb28-43fe-93fd-7f94dfee1878")},  // pound
         {UnitChoicesEnum.Kilopound, new Guid("777ff8ee-edc2-46d1-ac40-f097c1e1cd69")},  // kilopound
         {UnitChoicesEnum.Ounce, new Guid("4e64e69b-2276-46c8-a918-06ab6980178c")},  // ounce
         {UnitChoicesEnum.Stone, new Guid("6894dc1c-21e2-42aa-9569-759c0e6e6d6e")},  // stone
         {UnitChoicesEnum.TonUK, new Guid("059c7b81-ed11-410e-9466-4661011372d2")},  // ton UK
         {UnitChoicesEnum.TonUS, new Guid("443af797-a62f-4137-a852-ad1c9163dd7b")},  // ton US
         {UnitChoicesEnum.SolarMass, new Guid("432e73bf-a448-47f6-9c65-9339d5bac5a3")},  // solar mass
         {UnitChoicesEnum.EarthMass, new Guid("f9303406-dfce-45c4-9a1e-299d9bac1d4e")},  // earth mass
         {UnitChoicesEnum.Grain, new Guid("dad9b0a5-ce14-4132-b571-6365ab336bc2")},  // grain
         {UnitChoicesEnum.HundredWeights, new Guid("83810f2a-b260-41b3-bc13-5ef60290f214")} // hundred weights
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ForceRateOfChangeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         NewtonPerSecond,  // newton per second
         DecanewtonPerSecond,  // decanewton per second
         KilonewtonPerSecond,  // kilonewton per second
         KilodecanewtonPerSecond,  // kilodecanewton per second
         KilogramForcePerSecond,  // kilogram force per second
         PoundForcePerSecond,  // pound force per second
         KilopoundForcePerSecond,  // kilopound force per second
         NewtonPerMinute,  // newton per minute
         DecanewtonPerMinute,  // decanewton per minute
         KilonewtonPerMinute,  // kilonewton per minute
         KilodecanewtonPerMinute,  // kilodecanewton per minute
         KilogramForcePerMinute,  // kilogram force per minute
         PoundForcePerMinute,  // pound force per minute
         KilopoundForcePerMinute,  // kilopound force per minute
         NewtonPerHour,  // newton per hour
         DecanewtonPerHour,  // decanewton per hour
         KilonewtonPerHour,  // kilonewton per hour
         KilodecanewtonPerHour,  // kilodecanewton per hour
         KilogramForcePerHour,  // kilogram force per hour
         PoundForcePerHour,  // pound force per hour
         KilopoundForcePerHour // kilopound force per hour
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.NewtonPerSecond, new Guid("c766ff54-d778-4ee6-9c65-8467932efa60")},  // newton per second
         {UnitChoicesEnum.DecanewtonPerSecond, new Guid("1b7c3f4d-30ec-4d50-8063-0d1452c88615")},  // decanewton per second
         {UnitChoicesEnum.KilonewtonPerSecond, new Guid("5c99b2ac-51b7-4f9c-b82b-036f0b02492d")},  // kilonewton per second
         {UnitChoicesEnum.KilodecanewtonPerSecond, new Guid("f0ad684b-827c-43f9-8f6e-c9097bc82dd3")},  // kilodecanewton per second
         {UnitChoicesEnum.KilogramForcePerSecond, new Guid("e5dc01f1-09d4-4304-b065-8096026647e8")},  // kilogram force per second
         {UnitChoicesEnum.PoundForcePerSecond, new Guid("92ed16d5-3ea8-4102-a1cb-89f527d2b4a0")},  // pound force per second
         {UnitChoicesEnum.KilopoundForcePerSecond, new Guid("1f45684c-4582-4d5f-a5c5-950e4c9dbff7")},  // kilopound force per second
         {UnitChoicesEnum.NewtonPerMinute, new Guid("58df085b-9f10-4148-ad09-cb05bbcfa920")},  // newton per minute
         {UnitChoicesEnum.DecanewtonPerMinute, new Guid("130a7d93-a5d2-4d9b-bdb5-4c5784f61c79")},  // decanewton per minute
         {UnitChoicesEnum.KilonewtonPerMinute, new Guid("5841d94c-2349-4f51-a965-eb8cc3cc19d9")},  // kilonewton per minute
         {UnitChoicesEnum.KilodecanewtonPerMinute, new Guid("1a80c782-8438-43ac-ba6c-46b6b7abe761")},  // kilodecanewton per minute
         {UnitChoicesEnum.KilogramForcePerMinute, new Guid("323c8871-2f8f-41bd-9df8-50e3b50bf093")},  // kilogram force per minute
         {UnitChoicesEnum.PoundForcePerMinute, new Guid("924a79ab-743d-4c69-b5fb-b9a60bc70726")},  // pound force per minute
         {UnitChoicesEnum.KilopoundForcePerMinute, new Guid("1b5bc3fc-3784-4508-83d5-4a21b5e9fe84")},  // kilopound force per minute
         {UnitChoicesEnum.NewtonPerHour, new Guid("efa69e3c-b03b-4520-8ae8-e92ab6953141")},  // newton per hour
         {UnitChoicesEnum.DecanewtonPerHour, new Guid("1b0aaee4-9d74-4289-9f08-96c2d31a19f3")},  // decanewton per hour
         {UnitChoicesEnum.KilonewtonPerHour, new Guid("edd7e626-f9a4-42c5-bce3-5b72c1f3ca55")},  // kilonewton per hour
         {UnitChoicesEnum.KilodecanewtonPerHour, new Guid("ce5ff57d-e427-4e4f-aa11-c1f02118b3e1")},  // kilodecanewton per hour
         {UnitChoicesEnum.KilogramForcePerHour, new Guid("5c638813-fe28-47de-b7b5-a65760562b12")},  // kilogram force per hour
         {UnitChoicesEnum.PoundForcePerHour, new Guid("a1e5538f-a653-4a4b-8240-01b6a709a0d4")},  // pound force per hour
         {UnitChoicesEnum.KilopoundForcePerHour, new Guid("6041cb2d-b49a-46b4-87ef-1f89ddd89758")} // kilopound force per hour
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class PressureRateOfChangeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         PascalPerSecond,  // pascal per second
         KilopascalPerSecond,  // kilopascal per second
         BarPerSecond,  // bar per second
         MillibarPerSecond,  // millibar per second
         MicrobarPerSecond,  // microbar per second
         PoundPerSquareInchPerSecond,  // pound per square inch per second
         PoundPer100SquareFootPerSecond,  // pound per 100 square foot per second
         KilopoundPerSquareInchPerSecond,  // kilopound per square inch per second
         StandardAtmospherePerSecond,  // standard atmosphere per second
         PoundPerSquareFootPerSecond,  // pound per square foot per second
         MegapascalPerSecond,  // megapascal per second
         GigapascalPerSecond,  // gigapascal per second
         NewtonPerSquareMetrePerSecond,  // newton per square metre per second
         NewtonPerSquareCentimetrePerSecond,  // newton per square centimetre per second
         NewtonPerSquareMillimetrePerSecond,  // newton per square millimetre per second
         KilonewtonPerSquareMetrePerSecond,  // kilonewton per square metre per second
         MegapoundPerSquareInchPerSecond,  // megapound per square inch per second
         TorrPerSecond,  // torr per second
         CentimetreMercuryAtZeroDegreeCelsiusPerSecond,  // centimetre mercury at zero degree celsius per second
         MillimetreMercuryAtZeroDegreeCelsiusPerSecond,  // millimetre mercury at zero degree celsius per second
         InchMercuryAt32DegreeFahrenheitPerSecond,  // inch mercury at 32 degree fahrenheit per second
         InchMercuryAt60DegreeFahrenheitPerSecond,  // inch mercury at 60 degree fahrenheit per second
         CentimetreWaterAt4DegreeCelsiusPerSecond,  // centimetre water at 4 degree celsius per second
         MillimetreWaterAt4DegreeCelsiusPerSecond,  // millimetre water at 4 degree celsius per second
         InchWaterAt4DegreeCelsiusPerSecond,  // inch water at 4 degree celsius per second
         FootWaterAt4DegreeCelsiusPerSecond,  // foot water at 4 degree celsius per second
         DynePerSquareCentimetrePerSecond,  // dyne per square centimetre per second
         PascalPerMinute,  // pascal per minute
         KilopascalPerMinute,  // kilopascal per minute
         BarPerMinute,  // bar per minute
         MillibarPerMinute,  // millibar per minute
         MicrobarPerMinute,  // microbar per minute
         PoundPerSquareInchPerMinute,  // pound per square inch per minute
         PoundPer100SquareFootPerMinute,  // pound per 100 square foot per minute
         KilopoundPerSquareInchPerMinute,  // kilopound per square inch per minute
         StandardAtmospherePerMinute,  // standard atmosphere per minute
         PoundPerSquareFootPerMinute,  // pound per square foot per minute
         MegapascalPerMinute,  // megapascal per minute
         GigapascalPerMinute,  // gigapascal per minute
         NewtonPerSquareMetrePerMinute,  // newton per square metre per minute
         NewtonPerSquareCentimetrePerMinute,  // newton per square centimetre per minute
         NewtonPerSquareMillimetrePerMinute,  // newton per square millimetre per minute
         KilonewtonPerSquareMetrePerMinute,  // kilonewton per square metre per minute
         MegapoundPerSquareInchPerMinute,  // megapound per square inch per minute
         TorrPerMinute,  // torr per minute
         CentimetreMercuryAtZeroDegreeCelsiusPerMinute,  // centimetre mercury at zero degree celsius per minute
         MillimetreMercuryAtZeroDegreeCelsiusPerMinute,  // millimetre mercury at zero degree celsius per minute
         InchMercuryAt32DegreeFahrenheitPerMinute,  // inch mercury at 32 degree fahrenheit per minute
         InchMercuryAt60DegreeFahrenheitPerMinute,  // inch mercury at 60 degree fahrenheit per minute
         CentimetreWaterAt4DegreeCelsiusPerMinute,  // centimetre water at 4 degree celsius per minute
         MillimetreWaterAt4DegreeCelsiusPerMinute,  // millimetre water at 4 degree celsius per minute
         InchWaterAt4DegreeCelsiusPerMinute,  // inch water at 4 degree celsius per minute
         FootWaterAt4DegreeCelsiusPerMinute,  // foot water at 4 degree celsius per minute
         DynePerSquareCentimetrePerMinute,  // dyne per square centimetre per minute
         PascalPerHour,  // pascal per hour
         KilopascalPerHour,  // kilopascal per hour
         BarPerHour,  // bar per hour
         MillibarPerHour,  // millibar per hour
         MicrobarPerHour,  // microbar per hour
         PoundPerSquareInchPerHour,  // pound per square inch per hour
         PoundPer100SquareFootPerHour,  // pound per 100 square foot per hour
         KilopoundPerSquareInchPerHour,  // kilopound per square inch per hour
         StandardAtmospherePerHour,  // standard atmosphere per hour
         PoundPerSquareFootPerHour,  // pound per square foot per hour
         MegapascalPerHour,  // megapascal per hour
         GigapascalPerHour,  // gigapascal per hour
         NewtonPerSquareMetrePerHour,  // newton per square metre per hour
         NewtonPerSquareCentimetrePerHour,  // newton per square centimetre per hour
         NewtonPerSquareMillimetrePerHour,  // newton per square millimetre per hour
         KilonewtonPerSquareMetrePerHour,  // kilonewton per square metre per hour
         MegapoundPerSquareInchPerHour,  // megapound per square inch per hour
         TorrPerHour,  // torr per hour
         CentimetreMercuryAtZeroDegreeCelsiusPerHour,  // centimetre mercury at zero degree celsius per hour
         MillimetreMercuryAtZeroDegreeCelsiusPerHour,  // millimetre mercury at zero degree celsius per hour
         InchMercuryAt32DegreeFahrenheitPerHour,  // inch mercury at 32 degree fahrenheit per hour
         InchMercuryAt60DegreeFahrenheitPerHour,  // inch mercury at 60 degree fahrenheit per hour
         CentimetreWaterAt4DegreeCelsiusPerHour,  // centimetre water at 4 degree celsius per hour
         MillimetreWaterAt4DegreeCelsiusPerHour,  // millimetre water at 4 degree celsius per hour
         InchWaterAt4DegreeCelsiusPerHour,  // inch water at 4 degree celsius per hour
         FootWaterAt4DegreeCelsiusPerHour,  // foot water at 4 degree celsius per hour
         DynePerSquareCentimetrePerHour // dyne per square centimetre per hour
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.PascalPerSecond, new Guid("146c6da5-9de1-4c41-b6dd-9a7757b14ebf")},  // pascal per second
         {UnitChoicesEnum.KilopascalPerSecond, new Guid("92faa8ae-3f6f-4bd3-97ef-19709f9b7a43")},  // kilopascal per second
         {UnitChoicesEnum.BarPerSecond, new Guid("3bd0765c-d3ca-45e9-9818-d70dbd225fdc")},  // bar per second
         {UnitChoicesEnum.MillibarPerSecond, new Guid("ba3427d8-c516-40c8-8d4f-fdfe162414e3")},  // millibar per second
         {UnitChoicesEnum.MicrobarPerSecond, new Guid("65fb7735-63d5-42da-a730-1bdb4bd7f96a")},  // microbar per second
         {UnitChoicesEnum.PoundPerSquareInchPerSecond, new Guid("6c065cb9-edcc-4093-a81e-dcba0711ab0c")},  // pound per square inch per second
         {UnitChoicesEnum.PoundPer100SquareFootPerSecond, new Guid("c2f71235-2332-42ae-83a3-be1aeea10488")},  // pound per 100 square foot per second
         {UnitChoicesEnum.KilopoundPerSquareInchPerSecond, new Guid("90dd9c87-07f1-4f62-9098-029f78343309")},  // kilopound per square inch per second
         {UnitChoicesEnum.StandardAtmospherePerSecond, new Guid("3d6bbda4-a133-4bd4-bf37-afd2c56f5b02")},  // standard atmosphere per second
         {UnitChoicesEnum.PoundPerSquareFootPerSecond, new Guid("c25ea4df-6b1b-4cef-8ece-7565c8ae6739")},  // pound per square foot per second
         {UnitChoicesEnum.MegapascalPerSecond, new Guid("364ded63-6e4f-4d9b-ac57-d3bff57cc36a")},  // megapascal per second
         {UnitChoicesEnum.GigapascalPerSecond, new Guid("04567188-9a65-4289-ac76-2b346401ef39")},  // gigapascal per second
         {UnitChoicesEnum.NewtonPerSquareMetrePerSecond, new Guid("3efead2c-a99a-4efd-a534-d8221d3dbad4")},  // newton per square metre per second
         {UnitChoicesEnum.NewtonPerSquareCentimetrePerSecond, new Guid("db0bcb17-a1bd-4c68-b4ff-528194f9b766")},  // newton per square centimetre per second
         {UnitChoicesEnum.NewtonPerSquareMillimetrePerSecond, new Guid("78c99986-523b-4052-b995-64ada11779a0")},  // newton per square millimetre per second
         {UnitChoicesEnum.KilonewtonPerSquareMetrePerSecond, new Guid("012d7c45-41a8-45b3-9c40-3ab8333ed624")},  // kilonewton per square metre per second
         {UnitChoicesEnum.MegapoundPerSquareInchPerSecond, new Guid("cfd1514e-2707-4df5-b963-50390d1e2298")},  // megapound per square inch per second
         {UnitChoicesEnum.TorrPerSecond, new Guid("334a2e72-7dae-4904-ac5f-5e98dba8f191")},  // torr per second
         {UnitChoicesEnum.CentimetreMercuryAtZeroDegreeCelsiusPerSecond, new Guid("29f8c1be-1148-41ed-ad7c-eb7d6fe12800")},  // centimetre mercury at zero degree celsius per second
         {UnitChoicesEnum.MillimetreMercuryAtZeroDegreeCelsiusPerSecond, new Guid("8b0a6a79-0751-4aea-baa8-f685b36b5226")},  // millimetre mercury at zero degree celsius per second
         {UnitChoicesEnum.InchMercuryAt32DegreeFahrenheitPerSecond, new Guid("ef059b25-5fdd-481d-bdf2-3785c012b082")},  // inch mercury at 32 degree fahrenheit per second
         {UnitChoicesEnum.InchMercuryAt60DegreeFahrenheitPerSecond, new Guid("23dd84dd-b90a-442e-ad91-a1458fac47f7")},  // inch mercury at 60 degree fahrenheit per second
         {UnitChoicesEnum.CentimetreWaterAt4DegreeCelsiusPerSecond, new Guid("fafbcb83-8425-4e04-8bf6-cb58c64bdcd7")},  // centimetre water at 4 degree celsius per second
         {UnitChoicesEnum.MillimetreWaterAt4DegreeCelsiusPerSecond, new Guid("402ce428-d47a-493b-9b90-e3230a79da96")},  // millimetre water at 4 degree celsius per second
         {UnitChoicesEnum.InchWaterAt4DegreeCelsiusPerSecond, new Guid("11d5031a-06d5-4950-b877-cae03aff2669")},  // inch water at 4 degree celsius per second
         {UnitChoicesEnum.FootWaterAt4DegreeCelsiusPerSecond, new Guid("6c248d57-b406-4b85-b52d-1ccb5fca3c30")},  // foot water at 4 degree celsius per second
         {UnitChoicesEnum.DynePerSquareCentimetrePerSecond, new Guid("e003cc76-81e3-4e8e-8e80-aa03ccaec0b5")},  // dyne per square centimetre per second
         {UnitChoicesEnum.PascalPerMinute, new Guid("e598bc6c-1858-448e-b6c2-dbefdfe517a7")},  // pascal per minute
         {UnitChoicesEnum.KilopascalPerMinute, new Guid("1bd828ef-e6a8-4c6b-954e-83d076d81b5b")},  // kilopascal per minute
         {UnitChoicesEnum.BarPerMinute, new Guid("d5064ac5-0f02-437a-8d93-004ca9301b88")},  // bar per minute
         {UnitChoicesEnum.MillibarPerMinute, new Guid("76136213-1d78-4ac2-8f78-54dc62b815bc")},  // millibar per minute
         {UnitChoicesEnum.MicrobarPerMinute, new Guid("ee409d37-a87d-4e7c-a595-d01832e66918")},  // microbar per minute
         {UnitChoicesEnum.PoundPerSquareInchPerMinute, new Guid("34e3dd46-c61b-4109-91f9-704a94e4a827")},  // pound per square inch per minute
         {UnitChoicesEnum.PoundPer100SquareFootPerMinute, new Guid("9220c284-c1d6-41ed-a0c3-ee8e439cc5e2")},  // pound per 100 square foot per minute
         {UnitChoicesEnum.KilopoundPerSquareInchPerMinute, new Guid("fd5479cd-6a86-43ba-bbbf-142068a903ee")},  // kilopound per square inch per minute
         {UnitChoicesEnum.StandardAtmospherePerMinute, new Guid("cfdf265f-5e6f-4b1a-9169-a4a93a232821")},  // standard atmosphere per minute
         {UnitChoicesEnum.PoundPerSquareFootPerMinute, new Guid("d4aa7c92-885b-4c7e-977b-4ef9908b25a8")},  // pound per square foot per minute
         {UnitChoicesEnum.MegapascalPerMinute, new Guid("a1442a08-69f3-461e-82c5-e53e0322b266")},  // megapascal per minute
         {UnitChoicesEnum.GigapascalPerMinute, new Guid("882534c4-b00b-45e2-a26b-04f9f683a7e6")},  // gigapascal per minute
         {UnitChoicesEnum.NewtonPerSquareMetrePerMinute, new Guid("5dce7d3a-f8b8-4d08-a9d3-1f91b84829b9")},  // newton per square metre per minute
         {UnitChoicesEnum.NewtonPerSquareCentimetrePerMinute, new Guid("70d9df4b-a360-4b89-b433-cd2d9d0e9fe0")},  // newton per square centimetre per minute
         {UnitChoicesEnum.NewtonPerSquareMillimetrePerMinute, new Guid("3d587520-1609-4292-8f10-45758b59230d")},  // newton per square millimetre per minute
         {UnitChoicesEnum.KilonewtonPerSquareMetrePerMinute, new Guid("30466bc7-6978-4c96-a2cf-2158955dbfe7")},  // kilonewton per square metre per minute
         {UnitChoicesEnum.MegapoundPerSquareInchPerMinute, new Guid("8ad28da4-f7e5-4dbb-9f70-b06157686aae")},  // megapound per square inch per minute
         {UnitChoicesEnum.TorrPerMinute, new Guid("73b7ed13-4545-4d90-b4a5-83e2f9c8ebb7")},  // torr per minute
         {UnitChoicesEnum.CentimetreMercuryAtZeroDegreeCelsiusPerMinute, new Guid("08b7f12a-ef89-4a13-8b60-dd7cba0f586f")},  // centimetre mercury at zero degree celsius per minute
         {UnitChoicesEnum.MillimetreMercuryAtZeroDegreeCelsiusPerMinute, new Guid("fe387842-ca9e-4c35-860a-d377745f6aea")},  // millimetre mercury at zero degree celsius per minute
         {UnitChoicesEnum.InchMercuryAt32DegreeFahrenheitPerMinute, new Guid("6638070e-912d-42d7-b8b3-e4caafc2bb33")},  // inch mercury at 32 degree fahrenheit per minute
         {UnitChoicesEnum.InchMercuryAt60DegreeFahrenheitPerMinute, new Guid("a84bc253-3a56-494c-80da-d62fe5a3e617")},  // inch mercury at 60 degree fahrenheit per minute
         {UnitChoicesEnum.CentimetreWaterAt4DegreeCelsiusPerMinute, new Guid("2a18d804-e172-4b17-9f3f-becb8ebbac5f")},  // centimetre water at 4 degree celsius per minute
         {UnitChoicesEnum.MillimetreWaterAt4DegreeCelsiusPerMinute, new Guid("125bcf03-f190-41bd-a95d-e7bded0bc97e")},  // millimetre water at 4 degree celsius per minute
         {UnitChoicesEnum.InchWaterAt4DegreeCelsiusPerMinute, new Guid("57071e63-13ea-48d2-bfc2-fc82e6cca335")},  // inch water at 4 degree celsius per minute
         {UnitChoicesEnum.FootWaterAt4DegreeCelsiusPerMinute, new Guid("8a06ee9b-f646-4c3a-b087-309a0bd3f844")},  // foot water at 4 degree celsius per minute
         {UnitChoicesEnum.DynePerSquareCentimetrePerMinute, new Guid("ae854308-d419-452a-ba65-9f094ab0c2b5")},  // dyne per square centimetre per minute
         {UnitChoicesEnum.PascalPerHour, new Guid("bc8c071a-2c0a-4617-90c1-af5e00ec7e94")},  // pascal per hour
         {UnitChoicesEnum.KilopascalPerHour, new Guid("618f23ff-90af-416c-8d1a-a90bc421a6de")},  // kilopascal per hour
         {UnitChoicesEnum.BarPerHour, new Guid("387befbe-ad5a-46b5-a1a1-b55bebf96a12")},  // bar per hour
         {UnitChoicesEnum.MillibarPerHour, new Guid("c8b4a1d2-d4aa-4fb7-8b64-fd73b462d924")},  // millibar per hour
         {UnitChoicesEnum.MicrobarPerHour, new Guid("1c6a1561-2293-4e37-b6f7-cc01bf3f8e71")},  // microbar per hour
         {UnitChoicesEnum.PoundPerSquareInchPerHour, new Guid("94ce97e6-e2aa-4240-bb5f-e713abe880ad")},  // pound per square inch per hour
         {UnitChoicesEnum.PoundPer100SquareFootPerHour, new Guid("5900f7db-2994-4492-8779-89f54294aaa7")},  // pound per 100 square foot per hour
         {UnitChoicesEnum.KilopoundPerSquareInchPerHour, new Guid("837e7f8f-9e02-4438-9425-9d4aca0c227a")},  // kilopound per square inch per hour
         {UnitChoicesEnum.StandardAtmospherePerHour, new Guid("673a6598-498d-4e22-8cd5-972d0d7a52ac")},  // standard atmosphere per hour
         {UnitChoicesEnum.PoundPerSquareFootPerHour, new Guid("b7938985-f701-4245-9050-303a9e6d5c9f")},  // pound per square foot per hour
         {UnitChoicesEnum.MegapascalPerHour, new Guid("a7ed3518-a18f-4edf-b739-0b65961a3b60")},  // megapascal per hour
         {UnitChoicesEnum.GigapascalPerHour, new Guid("9f24b377-6f28-4c2c-8b64-c7ef2f6b7499")},  // gigapascal per hour
         {UnitChoicesEnum.NewtonPerSquareMetrePerHour, new Guid("bd39b1da-4c58-4e56-8a3c-e364bce4b38f")},  // newton per square metre per hour
         {UnitChoicesEnum.NewtonPerSquareCentimetrePerHour, new Guid("aa3f0b26-d6f3-4feb-bffa-fa48a4a3a1a7")},  // newton per square centimetre per hour
         {UnitChoicesEnum.NewtonPerSquareMillimetrePerHour, new Guid("378b7b8b-2d47-4a00-a817-ef9805b09169")},  // newton per square millimetre per hour
         {UnitChoicesEnum.KilonewtonPerSquareMetrePerHour, new Guid("42e9c18f-37d4-44b1-8c1d-86f3c524bf8b")},  // kilonewton per square metre per hour
         {UnitChoicesEnum.MegapoundPerSquareInchPerHour, new Guid("f4455f10-dc18-45b6-984f-7fd86a21d26f")},  // megapound per square inch per hour
         {UnitChoicesEnum.TorrPerHour, new Guid("4e1b41a0-f7cb-4ff2-b691-3a19dbc1d668")},  // torr per hour
         {UnitChoicesEnum.CentimetreMercuryAtZeroDegreeCelsiusPerHour, new Guid("ce403ddc-7b41-4abf-aea5-78ea2323595b")},  // centimetre mercury at zero degree celsius per hour
         {UnitChoicesEnum.MillimetreMercuryAtZeroDegreeCelsiusPerHour, new Guid("dc27fed5-7e21-4b87-9498-51598af273da")},  // millimetre mercury at zero degree celsius per hour
         {UnitChoicesEnum.InchMercuryAt32DegreeFahrenheitPerHour, new Guid("eb10ca92-df7f-4ef2-8c95-ac30a3d1a068")},  // inch mercury at 32 degree fahrenheit per hour
         {UnitChoicesEnum.InchMercuryAt60DegreeFahrenheitPerHour, new Guid("9f30715a-000c-4de8-8b75-206e9bf87713")},  // inch mercury at 60 degree fahrenheit per hour
         {UnitChoicesEnum.CentimetreWaterAt4DegreeCelsiusPerHour, new Guid("ed435708-418d-4a26-9b4c-e0f94ee63509")},  // centimetre water at 4 degree celsius per hour
         {UnitChoicesEnum.MillimetreWaterAt4DegreeCelsiusPerHour, new Guid("3dafbaa0-5907-4bf0-8808-b5bc0ec2bf6c")},  // millimetre water at 4 degree celsius per hour
         {UnitChoicesEnum.InchWaterAt4DegreeCelsiusPerHour, new Guid("084c8268-17a3-4d56-890e-aba0809772bc")},  // inch water at 4 degree celsius per hour
         {UnitChoicesEnum.FootWaterAt4DegreeCelsiusPerHour, new Guid("2a3ed612-1097-46ad-9b91-63d81c14b943")},  // foot water at 4 degree celsius per hour
         {UnitChoicesEnum.DynePerSquareCentimetrePerHour, new Guid("0493cff3-1e05-4ea5-8f3d-477f506b13f9")} // dyne per square centimetre per hour
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class TorqueRateOfChangeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         NewtonMetrePerSecond,  // newton metre per second
         DecanewtonMetrePerSecond,  // decanewton metre per second
         KilogramForceMetrePerSecond,  // kilogram force metre per second
         KilonewtonMetrePerSecond,  // kilonewton metre per second
         FootPoundPerSecond,  // foot pound per second
         KilofootPoundPerSecond,  // kilofoot pound per second
         NewtonDecimetrePerSecond,  // newton decimetre per second
         NewtonCentimetrePerSecond,  // newton centimetre per second
         NewtonMillimetrePerSecond,  // newton millimetre per second
         InchPoundPerSecond,  // inch pound per second
         NewtonMetrePerMinute,  // newton metre per minute
         DecanewtonMetrePerMinute,  // decanewton metre per minute
         KilogramForceMetrePerMinute,  // kilogram force metre per minute
         KilonewtonMetrePerMinute,  // kilonewton metre per minute
         FootPoundPerMinute,  // foot pound per minute
         KilofootPoundPerMinute,  // kilofoot pound per minute
         NewtonDecimetrePerMinute,  // newton decimetre per minute
         NewtonCentimetrePerMinute,  // newton centimetre per minute
         NewtonMillimetrePerMinute,  // newton millimetre per minute
         InchPoundPerMinute,  // inch pound per minute
         NewtonMetrePerHour,  // newton metre per hour
         DecanewtonMetrePerHour,  // decanewton metre per hour
         KilogramForceMetrePerHour,  // kilogram force metre per hour
         KilonewtonMetrePerHour,  // kilonewton metre per hour
         FootPoundPerHour,  // foot pound per hour
         KilofootPoundPerHour,  // kilofoot pound per hour
         NewtonDecimetrePerHour,  // newton decimetre per hour
         NewtonCentimetrePerHour,  // newton centimetre per hour
         NewtonMillimetrePerHour,  // newton millimetre per hour
         InchPoundPerHour // inch pound per hour
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.NewtonMetrePerSecond, new Guid("0af9bebb-adde-49b9-bf0b-0d5002e454a2")},  // newton metre per second
         {UnitChoicesEnum.DecanewtonMetrePerSecond, new Guid("a6672d76-f845-47ce-9d67-7f1242ba9f60")},  // decanewton metre per second
         {UnitChoicesEnum.KilogramForceMetrePerSecond, new Guid("6150a99d-34f0-438b-a8d3-038b8864c19f")},  // kilogram force metre per second
         {UnitChoicesEnum.KilonewtonMetrePerSecond, new Guid("0875163d-a610-4e0d-8e05-5fe56896e44f")},  // kilonewton metre per second
         {UnitChoicesEnum.FootPoundPerSecond, new Guid("e53f2ab8-0883-4a6f-ae67-9f660eb20368")},  // foot pound per second
         {UnitChoicesEnum.KilofootPoundPerSecond, new Guid("6166acf4-c3bd-439c-b4a9-6f0282c18731")},  // kilofoot pound per second
         {UnitChoicesEnum.NewtonDecimetrePerSecond, new Guid("7898b31c-821a-414d-9dcc-19822f3aef28")},  // newton decimetre per second
         {UnitChoicesEnum.NewtonCentimetrePerSecond, new Guid("13d51bb3-8f11-4fb5-a89a-0eac4fd26fe7")},  // newton centimetre per second
         {UnitChoicesEnum.NewtonMillimetrePerSecond, new Guid("173f9d03-f9c3-4d44-a889-52055887f8da")},  // newton millimetre per second
         {UnitChoicesEnum.InchPoundPerSecond, new Guid("914875df-9234-40be-a5eb-f02e29e0457b")},  // inch pound per second
         {UnitChoicesEnum.NewtonMetrePerMinute, new Guid("8c3ab891-e5bc-4fa1-9f14-a3250e062ef4")},  // newton metre per minute
         {UnitChoicesEnum.DecanewtonMetrePerMinute, new Guid("a1b76c0a-7ef2-46df-8d5b-0253a5dd42e8")},  // decanewton metre per minute
         {UnitChoicesEnum.KilogramForceMetrePerMinute, new Guid("91fa2fa9-d0dc-4d26-b694-2eac6ee7ad92")},  // kilogram force metre per minute
         {UnitChoicesEnum.KilonewtonMetrePerMinute, new Guid("746fdf99-afde-483d-88d4-e512b46efe3e")},  // kilonewton metre per minute
         {UnitChoicesEnum.FootPoundPerMinute, new Guid("66567700-9838-48f0-aa21-672c21380f57")},  // foot pound per minute
         {UnitChoicesEnum.KilofootPoundPerMinute, new Guid("dfa752eb-792f-4eb1-9eaa-e32f497a1ea2")},  // kilofoot pound per minute
         {UnitChoicesEnum.NewtonDecimetrePerMinute, new Guid("d4109f79-1724-4bba-8251-5847b5689037")},  // newton decimetre per minute
         {UnitChoicesEnum.NewtonCentimetrePerMinute, new Guid("5d526e83-6a91-4230-97cd-054167a7a3d7")},  // newton centimetre per minute
         {UnitChoicesEnum.NewtonMillimetrePerMinute, new Guid("5f1dbc46-8b88-4879-826f-f07836f018b8")},  // newton millimetre per minute
         {UnitChoicesEnum.InchPoundPerMinute, new Guid("72d76192-c058-44f6-b074-48291be96f5b")},  // inch pound per minute
         {UnitChoicesEnum.NewtonMetrePerHour, new Guid("c17eae23-0496-4fa1-a389-aa24828fa243")},  // newton metre per hour
         {UnitChoicesEnum.DecanewtonMetrePerHour, new Guid("3907090e-c10e-41dc-95cf-634e1a28bb11")},  // decanewton metre per hour
         {UnitChoicesEnum.KilogramForceMetrePerHour, new Guid("6295ae90-c198-499e-a90d-8dba22c77584")},  // kilogram force metre per hour
         {UnitChoicesEnum.KilonewtonMetrePerHour, new Guid("e35e63cc-09dc-4d8a-bb5d-b0a0c24998f8")},  // kilonewton metre per hour
         {UnitChoicesEnum.FootPoundPerHour, new Guid("b55029ec-fadc-46da-a3b8-a48a08f4d92f")},  // foot pound per hour
         {UnitChoicesEnum.KilofootPoundPerHour, new Guid("18156d8f-dd9f-4b63-a138-18827cafc14d")},  // kilofoot pound per hour
         {UnitChoicesEnum.NewtonDecimetrePerHour, new Guid("f19ae752-df39-48ae-bda3-c955116e5a01")},  // newton decimetre per hour
         {UnitChoicesEnum.NewtonCentimetrePerHour, new Guid("c7d197cf-831e-454d-aaf2-73399ff9afa2")},  // newton centimetre per hour
         {UnitChoicesEnum.NewtonMillimetrePerHour, new Guid("66c884c5-7596-4af7-919e-724b243336ae")},  // newton millimetre per hour
         {UnitChoicesEnum.InchPoundPerHour, new Guid("64f6b76d-4a99-4379-bdf3-59b96e468e84")} // inch pound per hour
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MomentOfAreaQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         MetresToTheFourthPower,  // metres to the fourth power
         CentimetresToTheFourthPower,  // centimetres to the fourth power
         InchesToTheFourthPower,  // inches to the fourth power
         FeetToTheFourthPower // feet to the fourth power
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.MetresToTheFourthPower, new Guid("f479bbab-bdd0-4c33-b13c-6dac1d57539b")},  // metres to the fourth power
         {UnitChoicesEnum.CentimetresToTheFourthPower, new Guid("1e94dc47-7563-4fb0-9749-e4c88523e1f4")},  // centimetres to the fourth power
         {UnitChoicesEnum.InchesToTheFourthPower, new Guid("86914b8d-6a5d-43ee-ad7c-e69fbf6d5087")},  // inches to the fourth power
         {UnitChoicesEnum.FeetToTheFourthPower, new Guid("d35362a3-1352-4dcd-b425-c376afcf4d36")} // feet to the fourth power
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MomentOfInertiaQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         KilogramMetreSquared,  // kilogram metre squared
         GramCentimetreSquared,  // gram centimetre squared
         PoundFootSquared,  // pound foot squared
         PoundInchSquared // pound inch squared
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.KilogramMetreSquared, new Guid("01c11147-677d-47d2-9167-59601d7961b2")},  // kilogram metre squared
         {UnitChoicesEnum.GramCentimetreSquared, new Guid("71e4e230-c611-4de9-b056-a1ef732b7fce")},  // gram centimetre squared
         {UnitChoicesEnum.PoundFootSquared, new Guid("103bd4aa-494a-4ec3-bf60-c3ce5bab364e")},  // pound foot squared
         {UnitChoicesEnum.PoundInchSquared, new Guid("ce8e3a4e-2cea-471a-a0dc-846523001be2")} // pound inch squared
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class SpecificVolumeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         CubicMetrePerKilogram,  // cubic metre per kilogram
         CubicMetrePerGram,  // cubic metre per gram
         CubicDecimetrePerGram,  // cubic decimetre per gram
         LitrePerGram,  // litre per gram
         DecilitrePerGram,  // decilitre per gram
         CentilitrePerGram,  // centilitre per gram
         MillilitrePerGram,  // millilitre per gram
         CubicCentimetrePerGram,  // cubic centimetre per gram
         CubicMillimetrePerGram,  // cubic millimetre per gram
         LitrePerKilogram,  // litre per kilogram
         DecilitrePerKilogram,  // decilitre per kilogram
         CentilitrePerKilogram,  // centilitre per kilogram
         MillilitrePerKilogram,  // millilitre per kilogram
         CubicCentimetrePerKilogram,  // cubic centimetre per kilogram
         CubicMillimetrePerKilogram,  // cubic millimetre per kilogram
         CubicYardPerPound,  // cubic yard per pound
         CubicFeetPerPound,  // cubic feet per pound
         CubicInchesPerPound,  // cubic inches per pound
         CubicYardPerOunce,  // cubic yard per ounce
         CubicFeetPerOunce,  // cubic feet per ounce
         CubicInchesPerOunce,  // cubic inches per ounce
         GallonUKPerOunce,  // gallon UK per ounce
         GallonUSPerOunce,  // gallon US per ounce
         GallonUKPerPound,  // gallon UK per pound
         GallonUSPerPound // gallon US per pound
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.CubicMetrePerKilogram, new Guid("0c321874-be1d-4ca7-8bfe-eac3c2b6e2f4")},  // cubic metre per kilogram
         {UnitChoicesEnum.CubicMetrePerGram, new Guid("72a258a9-bb34-46b2-8eb7-3123fb054669")},  // cubic metre per gram
         {UnitChoicesEnum.CubicDecimetrePerGram, new Guid("563b1574-04df-4285-a953-20d7da6b528f")},  // cubic decimetre per gram
         {UnitChoicesEnum.LitrePerGram, new Guid("d9206161-2a7a-45e8-9306-f5f9714bea84")},  // litre per gram
         {UnitChoicesEnum.DecilitrePerGram, new Guid("65c84088-e307-4ce2-adfa-bb380e639484")},  // decilitre per gram
         {UnitChoicesEnum.CentilitrePerGram, new Guid("e608ef34-e8b9-4134-8692-b72273ddd0af")},  // centilitre per gram
         {UnitChoicesEnum.MillilitrePerGram, new Guid("9cc34144-ca23-4d82-b725-c39e1607c356")},  // millilitre per gram
         {UnitChoicesEnum.CubicCentimetrePerGram, new Guid("5ac98a68-f85b-40b9-84d7-f6409bc79944")},  // cubic centimetre per gram
         {UnitChoicesEnum.CubicMillimetrePerGram, new Guid("a31a14cf-66d9-4789-8ea5-cca1f874a3f1")},  // cubic millimetre per gram
         {UnitChoicesEnum.LitrePerKilogram, new Guid("f4551a81-3856-434b-9247-4215d5782052")},  // litre per kilogram
         {UnitChoicesEnum.DecilitrePerKilogram, new Guid("07e7e24d-6538-4dd2-8c5d-d09e7cf2f006")},  // decilitre per kilogram
         {UnitChoicesEnum.CentilitrePerKilogram, new Guid("f0702462-ab52-4675-ba1e-0d7f86c8c3ea")},  // centilitre per kilogram
         {UnitChoicesEnum.MillilitrePerKilogram, new Guid("b87d48ef-91c1-48a8-8bac-41e774e3c3f0")},  // millilitre per kilogram
         {UnitChoicesEnum.CubicCentimetrePerKilogram, new Guid("7e258609-fb1f-4ad9-b712-9277427adaa5")},  // cubic centimetre per kilogram
         {UnitChoicesEnum.CubicMillimetrePerKilogram, new Guid("e72a515f-8edc-4049-973c-b9516aba6b61")},  // cubic millimetre per kilogram
         {UnitChoicesEnum.CubicYardPerPound, new Guid("cb0950fd-ebbb-459a-b542-f041b9388b1b")},  // cubic yard per pound
         {UnitChoicesEnum.CubicFeetPerPound, new Guid("7b5fe09f-170e-4a4f-bf30-77d4ffbbbd28")},  // cubic feet per pound
         {UnitChoicesEnum.CubicInchesPerPound, new Guid("8bf7c8f0-64fe-4785-be5f-5928906aea6a")},  // cubic inches per pound
         {UnitChoicesEnum.CubicYardPerOunce, new Guid("7027900f-a654-482a-8fc0-548f6d68c470")},  // cubic yard per ounce
         {UnitChoicesEnum.CubicFeetPerOunce, new Guid("ce834ba1-6ca8-4651-8fbd-1d008113ea1e")},  // cubic feet per ounce
         {UnitChoicesEnum.CubicInchesPerOunce, new Guid("a23d16bd-f9a2-475d-a76c-e6025940d631")},  // cubic inches per ounce
         {UnitChoicesEnum.GallonUKPerOunce, new Guid("cfe6a1db-5f38-4eb1-8a43-83dd959ee91c")},  // gallon UK per ounce
         {UnitChoicesEnum.GallonUSPerOunce, new Guid("216ba9e0-5c75-427a-8179-be65050bb773")},  // gallon US per ounce
         {UnitChoicesEnum.GallonUKPerPound, new Guid("2a509c09-5584-4df3-9b03-ef6afa56a0d3")},  // gallon UK per pound
         {UnitChoicesEnum.GallonUSPerPound, new Guid("8f6021d8-130a-4496-8a88-38b3b30aadfc")} // gallon US per pound
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class SpecificVolumeSquaredQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         CubicMetreSquaredPerKilogramSquared,  // cubic metre squared per kilogram squared
         CubicMetreSquaredPerGramSquared,  // cubic metre squared per gram squared
         CubicDecimetreSquaredPerGramSquared,  // cubic decimetre squared per gram squared
         LitreSquaredPerGramSquared,  // litre squared per gram squared
         DecilitreSquaredPerGramSquared,  // decilitre squared per gram squared
         CentilitreSquaredPerGramSquared,  // centilitre squared per gram squared
         MillilitreSquaredPerGramSquared,  // millilitre squared per gram squared
         CubicCentimetreSquaredPerGramSquared,  // cubic centimetre squared per gram squared
         CubicMillimetreSquaredPerGramSquared,  // cubic millimetre squared per gram squared
         LitreSquaredPerKilogramSquared,  // litre squared per kilogram squared
         DecilitreSquaredPerKilogramSquared,  // decilitre squared per kilogram squared
         CentilitreSquaredPerKilogramSquared,  // centilitre squared per kilogram squared
         MillilitreSquaredPerKilogramSquared,  // millilitre squared per kilogram squared
         CubicCentimetreSquaredPerKilogramSquared,  // cubic centimetre squared per kilogram squared
         CubicMillimetreSquaredPerKilogramSquared,  // cubic millimetre squared per kilogram squared
         CubicYardSquaredPerPoundSquared,  // cubic yard squared per pound squared
         CubicFeetSquaredPerPoundSquared,  // cubic feet squared per pound squared
         CubicInchesSquaredPerPoundSquared,  // cubic inches squared per pound squared
         CubicYardSquaredPerOunceSquared,  // cubic yard squared per ounce squared
         CubicFeetSquaredPerOunceSquared,  // cubic feet squared per ounce squared
         CubicInchesSquaredPerOunceSquared,  // cubic inches squared per ounce squared
         GallonUKSquaredPerOunceSquared,  // gallon UK squared per ounce squared
         GallonUSSquaredPerOunceSquared,  // gallon US squared per ounce squared
         GallonUKSquaredPerPoundSquared,  // gallon UK squared per pound squared
         GallonUSSquaredPerPoundSquared // gallon US squared per pound squared
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.CubicMetreSquaredPerKilogramSquared, new Guid("9c15bf12-237d-486f-bba4-a2fbfc4e8478")},  // cubic metre squared per kilogram squared
         {UnitChoicesEnum.CubicMetreSquaredPerGramSquared, new Guid("b3e7d7b9-5671-4a15-bb70-7885bd6540c5")},  // cubic metre squared per gram squared
         {UnitChoicesEnum.CubicDecimetreSquaredPerGramSquared, new Guid("f29c66ae-2cf0-459d-af24-34e62a350904")},  // cubic decimetre squared per gram squared
         {UnitChoicesEnum.LitreSquaredPerGramSquared, new Guid("5e37afb2-6def-422c-963c-6d7377421a66")},  // litre squared per gram squared
         {UnitChoicesEnum.DecilitreSquaredPerGramSquared, new Guid("7707bae3-e596-4d4f-9e52-c862236abe40")},  // decilitre squared per gram squared
         {UnitChoicesEnum.CentilitreSquaredPerGramSquared, new Guid("7e344fe0-8487-4d24-99e8-8ef370e01c99")},  // centilitre squared per gram squared
         {UnitChoicesEnum.MillilitreSquaredPerGramSquared, new Guid("119533f4-6a22-4d16-8927-6cf60b6919c1")},  // millilitre squared per gram squared
         {UnitChoicesEnum.CubicCentimetreSquaredPerGramSquared, new Guid("d1331578-5ea9-4e15-a687-0eaef32d5197")},  // cubic centimetre squared per gram squared
         {UnitChoicesEnum.CubicMillimetreSquaredPerGramSquared, new Guid("16048716-100d-4089-af27-3ba731defa11")},  // cubic millimetre squared per gram squared
         {UnitChoicesEnum.LitreSquaredPerKilogramSquared, new Guid("0b1b47d5-fdb3-47af-9f80-ea9193a60fa4")},  // litre squared per kilogram squared
         {UnitChoicesEnum.DecilitreSquaredPerKilogramSquared, new Guid("c4fe5cce-8106-4640-b70a-a915faf84317")},  // decilitre squared per kilogram squared
         {UnitChoicesEnum.CentilitreSquaredPerKilogramSquared, new Guid("c9130aa9-0f78-4e56-91b4-e8c397e48f34")},  // centilitre squared per kilogram squared
         {UnitChoicesEnum.MillilitreSquaredPerKilogramSquared, new Guid("7d9a4f75-0e77-4eb7-9690-dadad54690cb")},  // millilitre squared per kilogram squared
         {UnitChoicesEnum.CubicCentimetreSquaredPerKilogramSquared, new Guid("037ab139-0665-489c-8b5c-8183e738059c")},  // cubic centimetre squared per kilogram squared
         {UnitChoicesEnum.CubicMillimetreSquaredPerKilogramSquared, new Guid("6c6b8e30-4a7f-496d-9884-7c65124fb09e")},  // cubic millimetre squared per kilogram squared
         {UnitChoicesEnum.CubicYardSquaredPerPoundSquared, new Guid("be485c8d-8b4f-451b-bf5a-bef3713bc4a8")},  // cubic yard squared per pound squared
         {UnitChoicesEnum.CubicFeetSquaredPerPoundSquared, new Guid("b8599a96-d1bf-4832-aafb-4e79a2f7aa2f")},  // cubic feet squared per pound squared
         {UnitChoicesEnum.CubicInchesSquaredPerPoundSquared, new Guid("55d2064c-dc72-4c25-bbab-5d94b34dfada")},  // cubic inches squared per pound squared
         {UnitChoicesEnum.CubicYardSquaredPerOunceSquared, new Guid("41aa2db9-4162-4e89-ab3e-f516eee318fb")},  // cubic yard squared per ounce squared
         {UnitChoicesEnum.CubicFeetSquaredPerOunceSquared, new Guid("985c778c-2247-48cd-bf75-8658b0cce2e4")},  // cubic feet squared per ounce squared
         {UnitChoicesEnum.CubicInchesSquaredPerOunceSquared, new Guid("ed7d6e1d-2948-4e36-a165-8bb207f320c4")},  // cubic inches squared per ounce squared
         {UnitChoicesEnum.GallonUKSquaredPerOunceSquared, new Guid("57b49ba7-1aaa-4746-a7dc-7e6578fa2ea3")},  // gallon UK squared per ounce squared
         {UnitChoicesEnum.GallonUSSquaredPerOunceSquared, new Guid("1ddf2dc3-a5cd-457d-904d-cfead242bb05")},  // gallon US squared per ounce squared
         {UnitChoicesEnum.GallonUKSquaredPerPoundSquared, new Guid("b689bb7e-7334-41c6-b8de-afe74cec4dd0")},  // gallon UK squared per pound squared
         {UnitChoicesEnum.GallonUSSquaredPerPoundSquared, new Guid("109d07d6-b035-420a-9cee-fb44524ad0fb")} // gallon US squared per pound squared
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MassDensityGradientPerPressureQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         KilogramPerCubicMetrePerPascal,  // kilogram per cubic metre per pascal
         SpecificGravityPerPascal,  // specific gravity per pascal
         GramPerCubicCentimetrePerPascal,  // gram per cubic centimetre per pascal
         PoundPerGallonUKPerPascal,  // pound per gallon (UK) per pascal
         PoundPerGallonUSPerPascal,  // pound per gallon (US) per pascal
         PoundPerCubicFootPerPascal,  // pound per cubic foot per pascal
         PoundPerCubicInchPerPascal,  // pound per cubic inch per pascal
         PoundPerCubicYardPerPascal,  // pound per cubic yard per pascal
         KilogramPerCubicMetrePerBar,  // kilogram per cubic metre per bar
         SpecificGravityPerBar,  // specific gravity per bar
         GramPerCubicCentimetrePerBar,  // gram per cubic centimetre per bar
         PoundPerGallonUKPerBar,  // pound per gallon (UK) per bar
         PoundPerGallonUSPerBar,  // pound per gallon (US) per bar
         PoundPerCubicFootPerBar,  // pound per cubic foot per bar
         PoundPerCubicInchPerBar,  // pound per cubic inch per bar
         PoundPerCubicYardPerBar,  // pound per cubic yard per bar
         KilogramPerCubicMetrePerMegapascal,  // kilogram per cubic metre per megapascal
         SpecificGravityPerMegapascal,  // specific gravity per megapascal
         GramPerCubicCentimetrePerMegapascal,  // gram per cubic centimetre per megapascal
         PoundPerGallonUKPerMegapascal,  // pound per gallon (UK) per megapascal
         PoundPerGallonUSPerMegapascal,  // pound per gallon (US) per megapascal
         PoundPerCubicFootPerMegapascal,  // pound per cubic foot per megapascal
         PoundPerCubicInchPerMegapascal,  // pound per cubic inch per megapascal
         PoundPerCubicYardPerMegapascal,  // pound per cubic yard per megapascal
         KilogramPerCubicMetrePerGigapascal,  // kilogram per cubic metre per gigapascal
         SpecificGravityPerGigapascal,  // specific gravity per gigapascal
         GramPerCubicCentimetrePerGigapascal,  // gram per cubic centimetre per gigapascal
         PoundPerGallonUKPerGigapascal,  // pound per gallon (UK) per gigapascal
         PoundPerGallonUSPerGigapascal,  // pound per gallon (US) per gigapascal
         PoundPerCubicFootPerGigapascal,  // pound per cubic foot per gigapascal
         PoundPerCubicInchPerGigapascal,  // pound per cubic inch per gigapascal
         PoundPerCubicYardPerGigapascal,  // pound per cubic yard per gigapascal
         KilogramPerCubicMetrePerPoundPerSquareInch,  // kilogram per cubic metre per pound per square inch
         SpecificGravityPerPoundPerSquareInch,  // specific gravity per pound per square inch
         GramPerCubicCentimetrePerPoundPerSquareInch,  // gram per cubic centimetre per pound per square inch
         PoundPerGallonUKPerPoundPerSquareInch,  // pound per gallon (UK) per pound per square inch
         PoundPerGallonUSPerPoundPerSquareInch,  // pound per gallon (US) per pound per square inch
         PoundPerCubicFootPerPoundPerSquareInch,  // pound per cubic foot per pound per square inch
         PoundPerCubicInchPerPoundPerSquareInch,  // pound per cubic inch per pound per square inch
         PoundPerCubicYardPerPoundPerSquareInch // pound per cubic yard per pound per square inch
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.KilogramPerCubicMetrePerPascal, new Guid("48acbe7c-5fd3-4eaf-9667-adfbfda6c930")},  // kilogram per cubic metre per pascal
         {UnitChoicesEnum.SpecificGravityPerPascal, new Guid("619534ee-a9f5-4420-85ab-dfa218972250")},  // specific gravity per pascal
         {UnitChoicesEnum.GramPerCubicCentimetrePerPascal, new Guid("367f7fe4-b546-4963-accd-38043698916b")},  // gram per cubic centimetre per pascal
         {UnitChoicesEnum.PoundPerGallonUKPerPascal, new Guid("1dafde6f-b7f4-450c-bf9f-4d9063c53df5")},  // pound per gallon (UK) per pascal
         {UnitChoicesEnum.PoundPerGallonUSPerPascal, new Guid("0cb13b7b-d967-41ac-8d55-9dbd9c8a9004")},  // pound per gallon (US) per pascal
         {UnitChoicesEnum.PoundPerCubicFootPerPascal, new Guid("828a496b-d5be-42e2-8551-e49cd53b91e7")},  // pound per cubic foot per pascal
         {UnitChoicesEnum.PoundPerCubicInchPerPascal, new Guid("c8733719-56e5-4db2-a601-09e68bafdc18")},  // pound per cubic inch per pascal
         {UnitChoicesEnum.PoundPerCubicYardPerPascal, new Guid("4fc7d357-c230-4302-922f-05dc98c20953")},  // pound per cubic yard per pascal
         {UnitChoicesEnum.KilogramPerCubicMetrePerBar, new Guid("af23c11a-14cc-42f0-9693-dd8590db64a3")},  // kilogram per cubic metre per bar
         {UnitChoicesEnum.SpecificGravityPerBar, new Guid("c30e40c2-7fa4-47bd-8d62-4e8a33ef660c")},  // specific gravity per bar
         {UnitChoicesEnum.GramPerCubicCentimetrePerBar, new Guid("b49fd006-16ed-404f-afac-a78bd69c210d")},  // gram per cubic centimetre per bar
         {UnitChoicesEnum.PoundPerGallonUKPerBar, new Guid("0c7f7c23-6eee-4201-be51-3598b09f0ade")},  // pound per gallon (UK) per bar
         {UnitChoicesEnum.PoundPerGallonUSPerBar, new Guid("4810318e-813c-4671-92da-edfe64adc62a")},  // pound per gallon (US) per bar
         {UnitChoicesEnum.PoundPerCubicFootPerBar, new Guid("0e0ba903-3d8c-4bdd-82bc-0b4d9a79169d")},  // pound per cubic foot per bar
         {UnitChoicesEnum.PoundPerCubicInchPerBar, new Guid("4691ad34-b973-4f74-aae4-89c0af618805")},  // pound per cubic inch per bar
         {UnitChoicesEnum.PoundPerCubicYardPerBar, new Guid("bfef9341-2dfe-4567-9826-8388839e8bc2")},  // pound per cubic yard per bar
         {UnitChoicesEnum.KilogramPerCubicMetrePerMegapascal, new Guid("624fd78a-3d95-4f87-a539-ac07831690b4")},  // kilogram per cubic metre per megapascal
         {UnitChoicesEnum.SpecificGravityPerMegapascal, new Guid("42b5dcad-9c77-49fd-94eb-32033943511a")},  // specific gravity per megapascal
         {UnitChoicesEnum.GramPerCubicCentimetrePerMegapascal, new Guid("0dfd8f1c-162d-4e40-87c9-82f6f750d332")},  // gram per cubic centimetre per megapascal
         {UnitChoicesEnum.PoundPerGallonUKPerMegapascal, new Guid("3425cc44-360f-4bfa-80b2-4e5147ea0ae2")},  // pound per gallon (UK) per megapascal
         {UnitChoicesEnum.PoundPerGallonUSPerMegapascal, new Guid("2a151e18-61c8-405f-8264-29b872881731")},  // pound per gallon (US) per megapascal
         {UnitChoicesEnum.PoundPerCubicFootPerMegapascal, new Guid("50737d7b-51a7-44c4-9f3d-48555a7e0ac1")},  // pound per cubic foot per megapascal
         {UnitChoicesEnum.PoundPerCubicInchPerMegapascal, new Guid("550121b9-cab9-45a1-922c-3b1ee8faf2e3")},  // pound per cubic inch per megapascal
         {UnitChoicesEnum.PoundPerCubicYardPerMegapascal, new Guid("a960a378-bdac-42b4-9d31-023c655f4a18")},  // pound per cubic yard per megapascal
         {UnitChoicesEnum.KilogramPerCubicMetrePerGigapascal, new Guid("aacdc926-62ee-45db-aa38-fbe2cb22701c")},  // kilogram per cubic metre per gigapascal
         {UnitChoicesEnum.SpecificGravityPerGigapascal, new Guid("a18a8573-da1d-4e3a-b76b-4d73190a0754")},  // specific gravity per gigapascal
         {UnitChoicesEnum.GramPerCubicCentimetrePerGigapascal, new Guid("44504d3d-111a-4dc2-9788-bc6adc0c4f81")},  // gram per cubic centimetre per gigapascal
         {UnitChoicesEnum.PoundPerGallonUKPerGigapascal, new Guid("52f72092-045b-4f93-b4f7-223b07e6c8b0")},  // pound per gallon (UK) per gigapascal
         {UnitChoicesEnum.PoundPerGallonUSPerGigapascal, new Guid("037a8500-a3a7-44fc-a332-edb6e56be589")},  // pound per gallon (US) per gigapascal
         {UnitChoicesEnum.PoundPerCubicFootPerGigapascal, new Guid("adc6cd36-9dd2-4a09-be99-282cf23772ac")},  // pound per cubic foot per gigapascal
         {UnitChoicesEnum.PoundPerCubicInchPerGigapascal, new Guid("9e0ab271-6b7c-49c4-a0b4-db3562b658a8")},  // pound per cubic inch per gigapascal
         {UnitChoicesEnum.PoundPerCubicYardPerGigapascal, new Guid("15f23074-5d67-4955-8cb2-031e3de80096")},  // pound per cubic yard per gigapascal
         {UnitChoicesEnum.KilogramPerCubicMetrePerPoundPerSquareInch, new Guid("243a8789-24ba-407c-956a-7665b9ea5012")},  // kilogram per cubic metre per pound per square inch
         {UnitChoicesEnum.SpecificGravityPerPoundPerSquareInch, new Guid("1e56b53b-92a2-4dbc-87cf-143650a30895")},  // specific gravity per pound per square inch
         {UnitChoicesEnum.GramPerCubicCentimetrePerPoundPerSquareInch, new Guid("11a4f910-3071-4e9a-ac07-e7d106c52fd9")},  // gram per cubic centimetre per pound per square inch
         {UnitChoicesEnum.PoundPerGallonUKPerPoundPerSquareInch, new Guid("67712637-814b-4a0e-a858-c6b8bd864fc1")},  // pound per gallon (UK) per pound per square inch
         {UnitChoicesEnum.PoundPerGallonUSPerPoundPerSquareInch, new Guid("6234aef5-1534-47fa-b96b-4d0905832217")},  // pound per gallon (US) per pound per square inch
         {UnitChoicesEnum.PoundPerCubicFootPerPoundPerSquareInch, new Guid("c1d08a1b-9832-4d68-b47b-b113b8d06bf0")},  // pound per cubic foot per pound per square inch
         {UnitChoicesEnum.PoundPerCubicInchPerPoundPerSquareInch, new Guid("28fc5b3b-c94d-4afc-995f-6680dc3125f7")},  // pound per cubic inch per pound per square inch
         {UnitChoicesEnum.PoundPerCubicYardPerPoundPerSquareInch, new Guid("3e04b954-9156-405e-a058-16541687ee43")} // pound per cubic yard per pound per square inch
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MassDensityGradientPerPressureSquaredQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         KilogramPerCubicMetrePerPascalSquared,  // kilogram per cubic metre per pascal squared
         SpecificGravityPerPascalSquared,  // specific gravity per pascal squared
         GramPerCubicCentimetrePerPascalSquared,  // gram per cubic centimetre per pascal squared
         PoundPerGallonUKPerPascalSquared,  // pound per gallon (UK) per pascal squared
         PoundPerGallonUSPerPascalSquared,  // pound per gallon (US) per pascal squared
         PoundPerCubicFootPerPascalSquared,  // pound per cubic foot per pascal squared
         PoundPerCubicInchPerPascalSquared,  // pound per cubic inch per pascal squared
         PoundPerCubicYardPerPascalSquared,  // pound per cubic yard per pascal squared
         KilogramPerCubicMetrePerBarSquared,  // kilogram per cubic metre per bar squared
         SpecificGravityPerBarSquared,  // specific gravity per bar squared
         GramPerCubicCentimetrePerBarSquared,  // gram per cubic centimetre per bar squared
         PoundPerGallonUKPerBarSquared,  // pound per gallon (UK) per bar squared
         PoundPerGallonUSPerBarSquared,  // pound per gallon (US) per bar squared
         PoundPerCubicFootPerBarSquared,  // pound per cubic foot per bar squared
         PoundPerCubicInchPerBarSquared,  // pound per cubic inch per bar squared
         PoundPerCubicYardPerBarSquared,  // pound per cubic yard per bar squared
         KilogramPerCubicMetrePerMegapascalSquared,  // kilogram per cubic metre per megapascal squared
         SpecificGravityPerMegapascalSquared,  // specific gravity per megapascal squared
         GramPerCubicCentimetrePerMegapascalSquared,  // gram per cubic centimetre per megapascal squared
         PoundPerGallonUKPerMegapascalSquared,  // pound per gallon (UK) per megapascal squared
         PoundPerGallonUSPerMegapascalSquared,  // pound per gallon (US) per megapascal squared
         PoundPerCubicFootPerMegapascalSquared,  // pound per cubic foot per megapascal squared
         PoundPerCubicInchPerMegapascalSquared,  // pound per cubic inch per megapascal squared
         PoundPerCubicYardPerMegapascalSquared,  // pound per cubic yard per megapascal squared
         KilogramPerCubicMetrePerGigapascalSquared,  // kilogram per cubic metre per gigapascal squared
         SpecificGravityPerGigapascalSquared,  // specific gravity per gigapascal squared
         GramPerCubicCentimetrePerGigapascalSquared,  // gram per cubic centimetre per gigapascal squared
         PoundPerGallonUKPerGigapascalSquared,  // pound per gallon (UK) per gigapascal squared
         PoundPerGallonUSPerGigapascalSquared,  // pound per gallon (US) per gigapascal squared
         PoundPerCubicFootPerGigapascalSquared,  // pound per cubic foot per gigapascal squared
         PoundPerCubicInchPerGigapascalSquared,  // pound per cubic inch per gigapascal squared
         PoundPerCubicYardPerGigapascalSquared,  // pound per cubic yard per gigapascal squared
         KilogramPerCubicMetrePerPoundPerSquareInchSquared,  // kilogram per cubic metre per pound per square inch squared
         SpecificGravityPerPoundPerSquareInchSquared,  // specific gravity per pound per square inch squared
         GramPerCubicCentimetrePerPoundPerSquareInchSquared,  // gram per cubic centimetre per pound per square inch squared
         PoundPerGallonUKPerPoundPerSquareInchSquared,  // pound per gallon (UK) per pound per square inch squared
         PoundPerGallonUSPerPoundPerSquareInchSquared,  // pound per gallon (US) per pound per square inch squared
         PoundPerCubicFootPerPoundPerSquareInchSquared,  // pound per cubic foot per pound per square inch squared
         PoundPerCubicInchPerPoundPerSquareInchSquared,  // pound per cubic inch per pound per square inch squared
         PoundPerCubicYardPerPoundPerSquareInchSquared // pound per cubic yard per pound per square inch squared
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.KilogramPerCubicMetrePerPascalSquared, new Guid("f5e08eda-6f5f-480f-b4a1-c678e409c6e0")},  // kilogram per cubic metre per pascal squared
         {UnitChoicesEnum.SpecificGravityPerPascalSquared, new Guid("bbf15986-4411-4187-8186-de8da731a6b4")},  // specific gravity per pascal squared
         {UnitChoicesEnum.GramPerCubicCentimetrePerPascalSquared, new Guid("157c5898-353a-41f1-9924-0d8421cd8154")},  // gram per cubic centimetre per pascal squared
         {UnitChoicesEnum.PoundPerGallonUKPerPascalSquared, new Guid("8930d03e-184e-4bfa-88fa-755f728a3be1")},  // pound per gallon (UK) per pascal squared
         {UnitChoicesEnum.PoundPerGallonUSPerPascalSquared, new Guid("f93f8382-3a4c-454c-9575-c41ed6875656")},  // pound per gallon (US) per pascal squared
         {UnitChoicesEnum.PoundPerCubicFootPerPascalSquared, new Guid("de70ab66-52d6-4e5a-a0e7-a31c442c213f")},  // pound per cubic foot per pascal squared
         {UnitChoicesEnum.PoundPerCubicInchPerPascalSquared, new Guid("4f2d5894-6dc8-4c52-b442-976b8032e04e")},  // pound per cubic inch per pascal squared
         {UnitChoicesEnum.PoundPerCubicYardPerPascalSquared, new Guid("a22f8465-a519-47f9-80c5-5f633b5a3579")},  // pound per cubic yard per pascal squared
         {UnitChoicesEnum.KilogramPerCubicMetrePerBarSquared, new Guid("86d777df-9722-4fb6-861e-025c07589743")},  // kilogram per cubic metre per bar squared
         {UnitChoicesEnum.SpecificGravityPerBarSquared, new Guid("28cb683a-d3a8-4757-8123-cf093cb1d560")},  // specific gravity per bar squared
         {UnitChoicesEnum.GramPerCubicCentimetrePerBarSquared, new Guid("e8b6cc51-e9f8-4705-b486-20999de7d84a")},  // gram per cubic centimetre per bar squared
         {UnitChoicesEnum.PoundPerGallonUKPerBarSquared, new Guid("5245397a-58a4-40d1-8ba4-aa4ea9e3d7cd")},  // pound per gallon (UK) per bar squared
         {UnitChoicesEnum.PoundPerGallonUSPerBarSquared, new Guid("22450e27-b3c7-4a71-9f6e-f979217ca724")},  // pound per gallon (US) per bar squared
         {UnitChoicesEnum.PoundPerCubicFootPerBarSquared, new Guid("d2a23cb1-3ec6-416a-9a77-2c6e4f5ccd09")},  // pound per cubic foot per bar squared
         {UnitChoicesEnum.PoundPerCubicInchPerBarSquared, new Guid("ee8aff8d-f65e-4b09-8e25-5b94d7ad739b")},  // pound per cubic inch per bar squared
         {UnitChoicesEnum.PoundPerCubicYardPerBarSquared, new Guid("e1b7ea9b-d4ba-442b-99f8-a1eeebd35c1d")},  // pound per cubic yard per bar squared
         {UnitChoicesEnum.KilogramPerCubicMetrePerMegapascalSquared, new Guid("7e1a3e9d-ce0d-490c-a6b6-b5d9f3228929")},  // kilogram per cubic metre per megapascal squared
         {UnitChoicesEnum.SpecificGravityPerMegapascalSquared, new Guid("9327327d-fca5-47cf-8734-d231ee7e4ab5")},  // specific gravity per megapascal squared
         {UnitChoicesEnum.GramPerCubicCentimetrePerMegapascalSquared, new Guid("3081177b-3841-46c7-b736-7edeac6db903")},  // gram per cubic centimetre per megapascal squared
         {UnitChoicesEnum.PoundPerGallonUKPerMegapascalSquared, new Guid("8b641156-a14f-4aac-8b00-cfe61db83480")},  // pound per gallon (UK) per megapascal squared
         {UnitChoicesEnum.PoundPerGallonUSPerMegapascalSquared, new Guid("c5d8924d-100c-4556-aeeb-caa30b97b07d")},  // pound per gallon (US) per megapascal squared
         {UnitChoicesEnum.PoundPerCubicFootPerMegapascalSquared, new Guid("88e3ff83-e1fd-4757-bb41-f90d513d620b")},  // pound per cubic foot per megapascal squared
         {UnitChoicesEnum.PoundPerCubicInchPerMegapascalSquared, new Guid("3f36c155-dda0-4982-ae10-b4078bf7d7f3")},  // pound per cubic inch per megapascal squared
         {UnitChoicesEnum.PoundPerCubicYardPerMegapascalSquared, new Guid("6385dbbf-6738-4902-9389-12f2f3472423")},  // pound per cubic yard per megapascal squared
         {UnitChoicesEnum.KilogramPerCubicMetrePerGigapascalSquared, new Guid("d901c2a5-8e0d-4ce1-8507-fe4737e45753")},  // kilogram per cubic metre per gigapascal squared
         {UnitChoicesEnum.SpecificGravityPerGigapascalSquared, new Guid("b5fbf8a2-7064-43c7-bc22-d2c3607d36b3")},  // specific gravity per gigapascal squared
         {UnitChoicesEnum.GramPerCubicCentimetrePerGigapascalSquared, new Guid("44e94eae-9a75-48ff-a71b-b1057fc13667")},  // gram per cubic centimetre per gigapascal squared
         {UnitChoicesEnum.PoundPerGallonUKPerGigapascalSquared, new Guid("3a27a975-8879-4585-853b-6725f6829e57")},  // pound per gallon (UK) per gigapascal squared
         {UnitChoicesEnum.PoundPerGallonUSPerGigapascalSquared, new Guid("226d5137-7696-4a84-adc3-67cd8d26cfd2")},  // pound per gallon (US) per gigapascal squared
         {UnitChoicesEnum.PoundPerCubicFootPerGigapascalSquared, new Guid("11a8cbca-c51a-458d-9f04-0bc5b7ec442b")},  // pound per cubic foot per gigapascal squared
         {UnitChoicesEnum.PoundPerCubicInchPerGigapascalSquared, new Guid("f6bbac04-e3dc-4751-8ca5-d61490b88bbe")},  // pound per cubic inch per gigapascal squared
         {UnitChoicesEnum.PoundPerCubicYardPerGigapascalSquared, new Guid("407d3333-e09d-4219-8653-ccdb3fc1efde")},  // pound per cubic yard per gigapascal squared
         {UnitChoicesEnum.KilogramPerCubicMetrePerPoundPerSquareInchSquared, new Guid("ed837c52-b9d2-434e-8103-bb75c60b5dee")},  // kilogram per cubic metre per pound per square inch squared
         {UnitChoicesEnum.SpecificGravityPerPoundPerSquareInchSquared, new Guid("4a1459e4-ccfa-4563-ab43-af1ed407311e")},  // specific gravity per pound per square inch squared
         {UnitChoicesEnum.GramPerCubicCentimetrePerPoundPerSquareInchSquared, new Guid("f29096ad-ad1d-4f6e-9354-b9159600b5e8")},  // gram per cubic centimetre per pound per square inch squared
         {UnitChoicesEnum.PoundPerGallonUKPerPoundPerSquareInchSquared, new Guid("41979553-6c36-4f5a-9e62-a6e05bd3c9a2")},  // pound per gallon (UK) per pound per square inch squared
         {UnitChoicesEnum.PoundPerGallonUSPerPoundPerSquareInchSquared, new Guid("6f5b0146-ec8e-4224-9cb3-9ab440b6a006")},  // pound per gallon (US) per pound per square inch squared
         {UnitChoicesEnum.PoundPerCubicFootPerPoundPerSquareInchSquared, new Guid("af1a2b8a-b330-402d-9f9d-e30399b24926")},  // pound per cubic foot per pound per square inch squared
         {UnitChoicesEnum.PoundPerCubicInchPerPoundPerSquareInchSquared, new Guid("17051250-e815-4170-aa12-c222e89742e1")},  // pound per cubic inch per pound per square inch squared
         {UnitChoicesEnum.PoundPerCubicYardPerPoundPerSquareInchSquared, new Guid("f30510ee-c4bb-434b-a185-3805a543404f")} // pound per cubic yard per pound per square inch squared
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MassDensityGradientPerPressureTemperatureQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         KilogramPerCubicMetrePerPascalKelvin,  // kilogram per cubic metre per pascal kelvin
         SpecificGravityPerPascalKelvin,  // specific gravity per pascal kelvin
         GramPerCubicCentimetrePerPascalKelvin,  // gram per cubic centimetre per pascal kelvin
         PoundPerGallonUKPerPascalKelvin,  // pound per gallon (UK) per pascal kelvin
         PoundPerGallonUSPerPascalKelvin,  // pound per gallon (US) per pascal kelvin
         PoundPerCubicFootPerPascalKelvin,  // pound per cubic foot per pascal kelvin
         PoundPerCubicInchPerPascalKelvin,  // pound per cubic inch per pascal kelvin
         PoundPerCubicYardPerPascalKelvin,  // pound per cubic yard per pascal kelvin
         KilogramPerCubicMetrePerBarKelvin,  // kilogram per cubic metre per bar kelvin
         SpecificGravityPerBarKelvin,  // specific gravity per bar kelvin
         GramPerCubicCentimetrePerBarKelvin,  // gram per cubic centimetre per bar kelvin
         PoundPerGallonUKPerBarKelvin,  // pound per gallon (UK) per bar kelvin
         PoundPerGallonUSPerBarKelvin,  // pound per gallon (US) per bar kelvin
         PoundPerCubicFootPerBarKelvin,  // pound per cubic foot per bar kelvin
         PoundPerCubicInchPerBarKelvin,  // pound per cubic inch per bar kelvin
         PoundPerCubicYardPerBarKelvin,  // pound per cubic yard per bar kelvin
         KilogramPerCubicMetrePerMegapascalKelvin,  // kilogram per cubic metre per megapascal kelvin
         SpecificGravityPerMegapascalKelvin,  // specific gravity per megapascal kelvin
         GramPerCubicCentimetrePerMegapascalKelvin,  // gram per cubic centimetre per megapascal kelvin
         PoundPerGallonUKPerMegapascalKelvin,  // pound per gallon (UK) per megapascal kelvin
         PoundPerGallonUSPerMegapascalKelvin,  // pound per gallon (US) per megapascal kelvin
         PoundPerCubicFootPerMegapascalKelvin,  // pound per cubic foot per megapascal kelvin
         PoundPerCubicInchPerMegapascalKelvin,  // pound per cubic inch per megapascal kelvin
         PoundPerCubicYardPerMegapascalKelvin,  // pound per cubic yard per megapascal kelvin
         KilogramPerCubicMetrePerGigapascalKelvin,  // kilogram per cubic metre per gigapascal kelvin
         SpecificGravityPerGigapascalKelvin,  // specific gravity per gigapascal kelvin
         GramPerCubicCentimetrePerGigapascalKelvin,  // gram per cubic centimetre per gigapascal kelvin
         PoundPerGallonUKPerGigapascalKelvin,  // pound per gallon (UK) per gigapascal kelvin
         PoundPerGallonUSPerGigapascalKelvin,  // pound per gallon (US) per gigapascal kelvin
         PoundPerCubicFootPerGigapascalKelvin,  // pound per cubic foot per gigapascal kelvin
         PoundPerCubicInchPerGigapascalKelvin,  // pound per cubic inch per gigapascal kelvin
         PoundPerCubicYardPerGigapascalKelvin,  // pound per cubic yard per gigapascal kelvin
         KilogramPerCubicMetrePerPoundPerSquareInchKelvin,  // kilogram per cubic metre per pound per square inch kelvin
         SpecificGravityPerPoundPerSquareInchKelvin,  // specific gravity per pound per square inch kelvin
         GramPerCubicCentimetrePerPoundPerSquareInchKelvin,  // gram per cubic centimetre per pound per square inch kelvin
         PoundPerGallonUKPerPoundPerSquareInchKelvin,  // pound per gallon (UK) per pound per square inch kelvin
         PoundPerGallonUSPerPoundPerSquareInchKelvin,  // pound per gallon (US) per pound per square inch kelvin
         PoundPerCubicFootPerPoundPerSquareInchKelvin,  // pound per cubic foot per pound per square inch kelvin
         PoundPerCubicInchPerPoundPerSquareInchKelvin,  // pound per cubic inch per pound per square inch kelvin
         PoundPerCubicYardPerPoundPerSquareInchKelvin,  // pound per cubic yard per pound per square inch kelvin
         KilogramPerCubicMetrePerPascalCelsius,  // kilogram per cubic metre per pascal celsius
         SpecificGravityPerPascalCelsius,  // specific gravity per pascal celsius
         GramPerCubicCentimetrePerPascalCelsius,  // gram per cubic centimetre per pascal celsius
         PoundPerGallonUKPerPascalCelsius,  // pound per gallon (UK) per pascal celsius
         PoundPerGallonUSPerPascalCelsius,  // pound per gallon (US) per pascal celsius
         PoundPerCubicFootPerPascalCelsius,  // pound per cubic foot per pascal celsius
         PoundPerCubicInchPerPascalCelsius,  // pound per cubic inch per pascal celsius
         PoundPerCubicYardPerPascalCelsius,  // pound per cubic yard per pascal celsius
         KilogramPerCubicMetrePerBarCelsius,  // kilogram per cubic metre per bar celsius
         SpecificGravityPerBarCelsius,  // specific gravity per bar celsius
         GramPerCubicCentimetrePerBarCelsius,  // gram per cubic centimetre per bar celsius
         PoundPerGallonUKPerBarCelsius,  // pound per gallon (UK) per bar celsius
         PoundPerGallonUSPerBarCelsius,  // pound per gallon (US) per bar celsius
         PoundPerCubicFootPerBarCelsius,  // pound per cubic foot per bar celsius
         PoundPerCubicInchPerBarCelsius,  // pound per cubic inch per bar celsius
         PoundPerCubicYardPerBarCelsius,  // pound per cubic yard per bar celsius
         KilogramPerCubicMetrePerPoundPerSquareInchCelsius,  // kilogram per cubic metre per pound per square inch celsius
         SpecificGravityPerPoundPerSquareInchCelsius,  // specific gravity per pound per square inch celsius
         GramPerCubicCentimetrePerPoundPerSquareInchCelsius,  // gram per cubic centimetre per pound per square inch celsius
         PoundPerGallonUKPerPoundPerSquareInchCelsius,  // pound per gallon (UK) per pound per square inch celsius
         PoundPerGallonUSPerPoundPerSquareInchCelsius,  // pound per gallon (US) per pound per square inch celsius
         PoundPerCubicFootPerPoundPerSquareInchCelsius,  // pound per cubic foot per pound per square inch celsius
         PoundPerCubicInchPerPoundPerSquareInchCelsius,  // pound per cubic inch per pound per square inch celsius
         PoundPerCubicYardPerPoundPerSquareInchCelsius,  // pound per cubic yard per pound per square inch celsius
         KilogramPerCubicMetrePerPascalFahrenheit,  // kilogram per cubic metre per pascal fahrenheit
         SpecificGravityPerPascalFahrenheit,  // specific gravity per pascal fahrenheit
         GramPerCubicCentimetrePerPascalFahrenheit,  // gram per cubic centimetre per pascal fahrenheit
         PoundPerGallonUKPerPascalFahrenheit,  // pound per gallon (UK) per pascal fahrenheit
         PoundPerGallonUSPerPascalFahrenheit,  // pound per gallon (US) per pascal fahrenheit
         PoundPerCubicFootPerPascalFahrenheit,  // pound per cubic foot per pascal fahrenheit
         PoundPerCubicInchPerPascalFahrenheit,  // pound per cubic inch per pascal fahrenheit
         PoundPerCubicYardPerPascalFahrenheit,  // pound per cubic yard per pascal fahrenheit
         KilogramPerCubicMetrePerBarFahrenheit,  // kilogram per cubic metre per bar fahrenheit
         SpecificGravityPerBarFahrenheit,  // specific gravity per bar fahrenheit
         GramPerCubicCentimetrePerBarFahrenheit,  // gram per cubic centimetre per bar fahrenheit
         PoundPerGallonUKPerBarFahrenheit,  // pound per gallon (UK) per bar fahrenheit
         PoundPerGallonUSPerBarFahrenheit,  // pound per gallon (US) per bar fahrenheit
         PoundPerCubicFootPerBarFahrenheit,  // pound per cubic foot per bar fahrenheit
         PoundPerCubicInchPerBarFahrenheit,  // pound per cubic inch per bar fahrenheit
         PoundPerCubicYardPerBarFahrenheit,  // pound per cubic yard per bar fahrenheit
         KilogramPerCubicMetrePerPoundPerSquareInchFahrenheit,  // kilogram per cubic metre per pound per square inch fahrenheit
         SpecificGravityPerPoundPerSquareInchFahrenheit,  // specific gravity per pound per square inch fahrenheit
         GramPerCubicCentimetrePerPoundPerSquareInchFahrenheit,  // gram per cubic centimetre per pound per square inch fahrenheit
         PoundPerGallonUKPerPoundPerSquareInchFahrenheit,  // pound per gallon (UK) per pound per square inch fahrenheit
         PoundPerGallonUSPerPoundPerSquareInchFahrenheit,  // pound per gallon (US) per pound per square inch fahrenheit
         PoundPerCubicFootPerPoundPerSquareInchFahrenheit,  // pound per cubic foot per pound per square inch fahrenheit
         PoundPerCubicInchPerPoundPerSquareInchFahrenheit,  // pound per cubic inch per pound per square inch fahrenheit
         PoundPerCubicYardPerPoundPerSquareInchFahrenheit // pound per cubic yard per pound per square inch fahrenheit
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.KilogramPerCubicMetrePerPascalKelvin, new Guid("ab41e361-7ef5-488f-b121-e51afa56fcfa")},  // kilogram per cubic metre per pascal kelvin
         {UnitChoicesEnum.SpecificGravityPerPascalKelvin, new Guid("bd96fb03-de24-4171-abf8-eccfd6fcd2e8")},  // specific gravity per pascal kelvin
         {UnitChoicesEnum.GramPerCubicCentimetrePerPascalKelvin, new Guid("c2cae333-be7c-4567-a42e-00d2fa95d1aa")},  // gram per cubic centimetre per pascal kelvin
         {UnitChoicesEnum.PoundPerGallonUKPerPascalKelvin, new Guid("3ae6d54a-e365-4dc5-b5c8-083b88297719")},  // pound per gallon (UK) per pascal kelvin
         {UnitChoicesEnum.PoundPerGallonUSPerPascalKelvin, new Guid("8b5d3eec-f8ec-4233-8d99-6a99fee53ae8")},  // pound per gallon (US) per pascal kelvin
         {UnitChoicesEnum.PoundPerCubicFootPerPascalKelvin, new Guid("d6be025a-14b5-4c0e-8971-4fe5468237d5")},  // pound per cubic foot per pascal kelvin
         {UnitChoicesEnum.PoundPerCubicInchPerPascalKelvin, new Guid("e1dc7833-9369-4901-ae8c-c4c528f1d11d")},  // pound per cubic inch per pascal kelvin
         {UnitChoicesEnum.PoundPerCubicYardPerPascalKelvin, new Guid("fbb4e022-e102-436f-93c1-c9ea2c30f9e0")},  // pound per cubic yard per pascal kelvin
         {UnitChoicesEnum.KilogramPerCubicMetrePerBarKelvin, new Guid("8cbe6cb2-2a0f-400d-a659-0695eb16d508")},  // kilogram per cubic metre per bar kelvin
         {UnitChoicesEnum.SpecificGravityPerBarKelvin, new Guid("9aa8835d-e374-4527-bdd6-636ec7148e66")},  // specific gravity per bar kelvin
         {UnitChoicesEnum.GramPerCubicCentimetrePerBarKelvin, new Guid("bbb10e84-4dee-459d-b389-5127606db8cc")},  // gram per cubic centimetre per bar kelvin
         {UnitChoicesEnum.PoundPerGallonUKPerBarKelvin, new Guid("07e8b9de-be10-4750-836a-929a4b16588d")},  // pound per gallon (UK) per bar kelvin
         {UnitChoicesEnum.PoundPerGallonUSPerBarKelvin, new Guid("19804fa6-8dc6-4043-8b0a-d7cabaf98c13")},  // pound per gallon (US) per bar kelvin
         {UnitChoicesEnum.PoundPerCubicFootPerBarKelvin, new Guid("fb6f3791-8de0-42d7-a870-2b87b6a391bf")},  // pound per cubic foot per bar kelvin
         {UnitChoicesEnum.PoundPerCubicInchPerBarKelvin, new Guid("3390b64d-7959-4c92-9188-f842b1a7b4d6")},  // pound per cubic inch per bar kelvin
         {UnitChoicesEnum.PoundPerCubicYardPerBarKelvin, new Guid("2caa723f-4107-4632-ad12-3b72c3eb1174")},  // pound per cubic yard per bar kelvin
         {UnitChoicesEnum.KilogramPerCubicMetrePerMegapascalKelvin, new Guid("4fc27c0f-f9a6-47be-9f76-61f3a00a78f1")},  // kilogram per cubic metre per megapascal kelvin
         {UnitChoicesEnum.SpecificGravityPerMegapascalKelvin, new Guid("6eca416d-d590-475d-877a-3e5d9b74ecbc")},  // specific gravity per megapascal kelvin
         {UnitChoicesEnum.GramPerCubicCentimetrePerMegapascalKelvin, new Guid("6b3c00ae-499a-4a48-8400-b1b5704c9904")},  // gram per cubic centimetre per megapascal kelvin
         {UnitChoicesEnum.PoundPerGallonUKPerMegapascalKelvin, new Guid("35523ce7-c1a8-4742-9e80-a253d62d16dc")},  // pound per gallon (UK) per megapascal kelvin
         {UnitChoicesEnum.PoundPerGallonUSPerMegapascalKelvin, new Guid("3d5692e3-0baa-473a-abbd-b61fb3184c95")},  // pound per gallon (US) per megapascal kelvin
         {UnitChoicesEnum.PoundPerCubicFootPerMegapascalKelvin, new Guid("403a4c46-5990-47be-97d9-ef59505f6338")},  // pound per cubic foot per megapascal kelvin
         {UnitChoicesEnum.PoundPerCubicInchPerMegapascalKelvin, new Guid("b238105f-64e8-4d20-9cc4-09847041b60b")},  // pound per cubic inch per megapascal kelvin
         {UnitChoicesEnum.PoundPerCubicYardPerMegapascalKelvin, new Guid("b4959d44-0010-4853-9219-5696882ed448")},  // pound per cubic yard per megapascal kelvin
         {UnitChoicesEnum.KilogramPerCubicMetrePerGigapascalKelvin, new Guid("c53d4c83-4dc4-4084-9cbb-bb91ea9b704b")},  // kilogram per cubic metre per gigapascal kelvin
         {UnitChoicesEnum.SpecificGravityPerGigapascalKelvin, new Guid("23de1690-a6d7-438c-b96b-9dfc670b428d")},  // specific gravity per gigapascal kelvin
         {UnitChoicesEnum.GramPerCubicCentimetrePerGigapascalKelvin, new Guid("b262ca47-7eea-44fa-8e04-8a8d80fb9b34")},  // gram per cubic centimetre per gigapascal kelvin
         {UnitChoicesEnum.PoundPerGallonUKPerGigapascalKelvin, new Guid("67464a81-a4fb-4534-b44f-8f400528dd3e")},  // pound per gallon (UK) per gigapascal kelvin
         {UnitChoicesEnum.PoundPerGallonUSPerGigapascalKelvin, new Guid("028368a8-be80-4ce3-ad13-e6e4d2a17486")},  // pound per gallon (US) per gigapascal kelvin
         {UnitChoicesEnum.PoundPerCubicFootPerGigapascalKelvin, new Guid("612dc2f8-a5bb-44e9-a469-ba7bf15a2c79")},  // pound per cubic foot per gigapascal kelvin
         {UnitChoicesEnum.PoundPerCubicInchPerGigapascalKelvin, new Guid("2f5739b2-c7d4-406f-9596-508b3c2a1041")},  // pound per cubic inch per gigapascal kelvin
         {UnitChoicesEnum.PoundPerCubicYardPerGigapascalKelvin, new Guid("15be077d-791a-4b91-90c1-15373fc27017")},  // pound per cubic yard per gigapascal kelvin
         {UnitChoicesEnum.KilogramPerCubicMetrePerPoundPerSquareInchKelvin, new Guid("f0a415eb-f811-49bb-8a08-640a976fe1cf")},  // kilogram per cubic metre per pound per square inch kelvin
         {UnitChoicesEnum.SpecificGravityPerPoundPerSquareInchKelvin, new Guid("3a2b82e9-6f53-4149-9530-b202abee4a22")},  // specific gravity per pound per square inch kelvin
         {UnitChoicesEnum.GramPerCubicCentimetrePerPoundPerSquareInchKelvin, new Guid("4a4df929-c2df-4da5-aad4-601e82945f2c")},  // gram per cubic centimetre per pound per square inch kelvin
         {UnitChoicesEnum.PoundPerGallonUKPerPoundPerSquareInchKelvin, new Guid("dd330e7b-b47c-43c9-a4fe-4259a6a74b40")},  // pound per gallon (UK) per pound per square inch kelvin
         {UnitChoicesEnum.PoundPerGallonUSPerPoundPerSquareInchKelvin, new Guid("e351b849-3d09-44b7-ac67-4380d7a6908a")},  // pound per gallon (US) per pound per square inch kelvin
         {UnitChoicesEnum.PoundPerCubicFootPerPoundPerSquareInchKelvin, new Guid("11c5c48f-a880-479f-9f8a-952433aa1105")},  // pound per cubic foot per pound per square inch kelvin
         {UnitChoicesEnum.PoundPerCubicInchPerPoundPerSquareInchKelvin, new Guid("1b9b4c68-a0f7-43e3-bc6c-65a13672e340")},  // pound per cubic inch per pound per square inch kelvin
         {UnitChoicesEnum.PoundPerCubicYardPerPoundPerSquareInchKelvin, new Guid("5d80643e-871a-41d2-b215-6bf62585a408")},  // pound per cubic yard per pound per square inch kelvin
         {UnitChoicesEnum.KilogramPerCubicMetrePerPascalCelsius, new Guid("c0602be3-4094-40af-a85f-fab2378a5d83")},  // kilogram per cubic metre per pascal celsius
         {UnitChoicesEnum.SpecificGravityPerPascalCelsius, new Guid("03cc0ee6-3c31-417a-b8e6-c40cd5b377d3")},  // specific gravity per pascal celsius
         {UnitChoicesEnum.GramPerCubicCentimetrePerPascalCelsius, new Guid("ddcc5558-7bcc-424f-8a16-42a8e3cb4a52")},  // gram per cubic centimetre per pascal celsius
         {UnitChoicesEnum.PoundPerGallonUKPerPascalCelsius, new Guid("8022319e-61bc-42aa-b7d9-1b7e33771224")},  // pound per gallon (UK) per pascal celsius
         {UnitChoicesEnum.PoundPerGallonUSPerPascalCelsius, new Guid("31baed8a-9474-414b-adf7-74df7d244e40")},  // pound per gallon (US) per pascal celsius
         {UnitChoicesEnum.PoundPerCubicFootPerPascalCelsius, new Guid("684db3d1-6610-4774-8d80-ae212241991a")},  // pound per cubic foot per pascal celsius
         {UnitChoicesEnum.PoundPerCubicInchPerPascalCelsius, new Guid("85efde7e-0ab7-44c3-adeb-1934753aea1a")},  // pound per cubic inch per pascal celsius
         {UnitChoicesEnum.PoundPerCubicYardPerPascalCelsius, new Guid("dc662923-2863-494a-a1b0-90ad30b812e9")},  // pound per cubic yard per pascal celsius
         {UnitChoicesEnum.KilogramPerCubicMetrePerBarCelsius, new Guid("e70b203c-cae0-4494-bf5d-fac95d99cb8f")},  // kilogram per cubic metre per bar celsius
         {UnitChoicesEnum.SpecificGravityPerBarCelsius, new Guid("891b9f2e-94cc-41f2-8c36-d4315d923da2")},  // specific gravity per bar celsius
         {UnitChoicesEnum.GramPerCubicCentimetrePerBarCelsius, new Guid("d59d2a23-67f3-4224-8960-3732ca7e3c19")},  // gram per cubic centimetre per bar celsius
         {UnitChoicesEnum.PoundPerGallonUKPerBarCelsius, new Guid("55531b71-6a17-41bc-ac69-def74092a04b")},  // pound per gallon (UK) per bar celsius
         {UnitChoicesEnum.PoundPerGallonUSPerBarCelsius, new Guid("c04f57ed-37d8-4f32-bedf-48616e977cf3")},  // pound per gallon (US) per bar celsius
         {UnitChoicesEnum.PoundPerCubicFootPerBarCelsius, new Guid("cd1b76ec-a7e4-4963-8a3c-cbf8d7c1a858")},  // pound per cubic foot per bar celsius
         {UnitChoicesEnum.PoundPerCubicInchPerBarCelsius, new Guid("45c0e47f-4844-49a8-bb92-a564c99eb6df")},  // pound per cubic inch per bar celsius
         {UnitChoicesEnum.PoundPerCubicYardPerBarCelsius, new Guid("82092268-2d5f-4318-9812-556f63b68089")},  // pound per cubic yard per bar celsius
         {UnitChoicesEnum.KilogramPerCubicMetrePerPoundPerSquareInchCelsius, new Guid("fd02625d-d171-4ea2-8349-488ddbf9f6c8")},  // kilogram per cubic metre per pound per square inch celsius
         {UnitChoicesEnum.SpecificGravityPerPoundPerSquareInchCelsius, new Guid("d0e994dc-68fb-41e9-84fb-a62544d16674")},  // specific gravity per pound per square inch celsius
         {UnitChoicesEnum.GramPerCubicCentimetrePerPoundPerSquareInchCelsius, new Guid("261a18c2-5f0b-4e6f-9988-e1715de29c6f")},  // gram per cubic centimetre per pound per square inch celsius
         {UnitChoicesEnum.PoundPerGallonUKPerPoundPerSquareInchCelsius, new Guid("edf9f94f-2cd9-4b76-b0f9-38576be98feb")},  // pound per gallon (UK) per pound per square inch celsius
         {UnitChoicesEnum.PoundPerGallonUSPerPoundPerSquareInchCelsius, new Guid("a3c8fa89-a1fd-46a7-bef8-7a4cde7ad585")},  // pound per gallon (US) per pound per square inch celsius
         {UnitChoicesEnum.PoundPerCubicFootPerPoundPerSquareInchCelsius, new Guid("a0e2f8d9-cf7e-454e-8722-b4777f3ecd1b")},  // pound per cubic foot per pound per square inch celsius
         {UnitChoicesEnum.PoundPerCubicInchPerPoundPerSquareInchCelsius, new Guid("33eb59dd-07fe-4826-a1a3-108246bf98e8")},  // pound per cubic inch per pound per square inch celsius
         {UnitChoicesEnum.PoundPerCubicYardPerPoundPerSquareInchCelsius, new Guid("a10eaf12-acd4-42e6-8bff-f0fe223dfcc2")},  // pound per cubic yard per pound per square inch celsius
         {UnitChoicesEnum.KilogramPerCubicMetrePerPascalFahrenheit, new Guid("81baad5f-7be5-4db4-9e64-b7df06c3babd")},  // kilogram per cubic metre per pascal fahrenheit
         {UnitChoicesEnum.SpecificGravityPerPascalFahrenheit, new Guid("1c7199e1-89e6-479e-a048-179bfbce39d8")},  // specific gravity per pascal fahrenheit
         {UnitChoicesEnum.GramPerCubicCentimetrePerPascalFahrenheit, new Guid("1a1fc347-5829-4ed7-bb2c-cf2a0a08cd3b")},  // gram per cubic centimetre per pascal fahrenheit
         {UnitChoicesEnum.PoundPerGallonUKPerPascalFahrenheit, new Guid("6af15113-8196-4dc0-8688-543e4ab5c050")},  // pound per gallon (UK) per pascal fahrenheit
         {UnitChoicesEnum.PoundPerGallonUSPerPascalFahrenheit, new Guid("06c4a7f8-e43c-4a58-be79-c0fadc2416ec")},  // pound per gallon (US) per pascal fahrenheit
         {UnitChoicesEnum.PoundPerCubicFootPerPascalFahrenheit, new Guid("240f2cae-25fc-47cf-8757-fe51552189b7")},  // pound per cubic foot per pascal fahrenheit
         {UnitChoicesEnum.PoundPerCubicInchPerPascalFahrenheit, new Guid("3f059a64-a660-4c5f-a4e6-68844da8b1c4")},  // pound per cubic inch per pascal fahrenheit
         {UnitChoicesEnum.PoundPerCubicYardPerPascalFahrenheit, new Guid("19e48858-c3a7-44f7-b9e9-c326f0fafa33")},  // pound per cubic yard per pascal fahrenheit
         {UnitChoicesEnum.KilogramPerCubicMetrePerBarFahrenheit, new Guid("83606dd6-7f81-45ea-994c-743581cd62d3")},  // kilogram per cubic metre per bar fahrenheit
         {UnitChoicesEnum.SpecificGravityPerBarFahrenheit, new Guid("070c9187-3d1c-4c1d-a26c-8370d9ac6d8f")},  // specific gravity per bar fahrenheit
         {UnitChoicesEnum.GramPerCubicCentimetrePerBarFahrenheit, new Guid("7dd71c46-d10b-407f-abc9-6597b7dec4bf")},  // gram per cubic centimetre per bar fahrenheit
         {UnitChoicesEnum.PoundPerGallonUKPerBarFahrenheit, new Guid("349ffb7c-caa0-4ee1-b8a7-8a00ec028b6f")},  // pound per gallon (UK) per bar fahrenheit
         {UnitChoicesEnum.PoundPerGallonUSPerBarFahrenheit, new Guid("cbbc44bb-a924-4e10-8c7d-9388f248722d")},  // pound per gallon (US) per bar fahrenheit
         {UnitChoicesEnum.PoundPerCubicFootPerBarFahrenheit, new Guid("2de087ee-34df-4e54-b6d2-cfe5393cda8d")},  // pound per cubic foot per bar fahrenheit
         {UnitChoicesEnum.PoundPerCubicInchPerBarFahrenheit, new Guid("c2033261-a7fe-4fe4-a99e-641db13c276e")},  // pound per cubic inch per bar fahrenheit
         {UnitChoicesEnum.PoundPerCubicYardPerBarFahrenheit, new Guid("32a9db15-caf4-40f8-9a78-e4444d1cc687")},  // pound per cubic yard per bar fahrenheit
         {UnitChoicesEnum.KilogramPerCubicMetrePerPoundPerSquareInchFahrenheit, new Guid("8bac278b-fb5c-4ab2-b58e-bda28e6b9f65")},  // kilogram per cubic metre per pound per square inch fahrenheit
         {UnitChoicesEnum.SpecificGravityPerPoundPerSquareInchFahrenheit, new Guid("9d2e9844-cf55-43ca-806b-12147ca5d981")},  // specific gravity per pound per square inch fahrenheit
         {UnitChoicesEnum.GramPerCubicCentimetrePerPoundPerSquareInchFahrenheit, new Guid("49a3a66b-c08f-48bc-b2cd-207a8a31150d")},  // gram per cubic centimetre per pound per square inch fahrenheit
         {UnitChoicesEnum.PoundPerGallonUKPerPoundPerSquareInchFahrenheit, new Guid("65be0323-b39c-4406-9a1e-91eb36a7c963")},  // pound per gallon (UK) per pound per square inch fahrenheit
         {UnitChoicesEnum.PoundPerGallonUSPerPoundPerSquareInchFahrenheit, new Guid("8b1a8cf9-ce8c-40b2-872d-ae3112b69da1")},  // pound per gallon (US) per pound per square inch fahrenheit
         {UnitChoicesEnum.PoundPerCubicFootPerPoundPerSquareInchFahrenheit, new Guid("07ba1470-7fda-43a1-81bb-8e1e0075021a")},  // pound per cubic foot per pound per square inch fahrenheit
         {UnitChoicesEnum.PoundPerCubicInchPerPoundPerSquareInchFahrenheit, new Guid("af345846-e411-416e-b97c-0119f2cb4c3b")},  // pound per cubic inch per pound per square inch fahrenheit
         {UnitChoicesEnum.PoundPerCubicYardPerPoundPerSquareInchFahrenheit, new Guid("846a356a-3ce8-4f4d-b2c1-641d2236e151")} // pound per cubic yard per pound per square inch fahrenheit
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MassDensityGradientPerPressureSquaredTemperatureQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         KilogramPerCubicMetrePerPascalSquaredKelvin,  // kilogram per cubic metre per pascal squared kelvin
         SpecificGravityPerPascalSquaredKelvin,  // specific gravity per pascal squared kelvin
         GramPerCubicCentimetrePerPascalSquaredKelvin,  // gram per cubic centimetre per pascal squared kelvin
         PoundPerGallonUKPerPascalSquaredKelvin,  // pound per gallon (UK) per pascal squared kelvin
         PoundPerGallonUSPerPascalSquaredKelvin,  // pound per gallon (US) per pascal squared kelvin
         PoundPerCubicFootPerPascalSquaredKelvin,  // pound per cubic foot per pascal squared kelvin
         PoundPerCubicInchPerPascalSquaredKelvin,  // pound per cubic inch per pascal squared kelvin
         PoundPerCubicYardPerPascalSquaredKelvin,  // pound per cubic yard per pascal squared kelvin
         KilogramPerCubicMetrePerBarSquaredKelvin,  // kilogram per cubic metre per bar squared kelvin
         SpecificGravityPerBarSquaredKelvin,  // specific gravity per bar squared kelvin
         GramPerCubicCentimetrePerBarSquaredKelvin,  // gram per cubic centimetre per bar squared kelvin
         PoundPerGallonUKPerBarSquaredKelvin,  // pound per gallon (UK) per bar squared kelvin
         PoundPerGallonUSPerBarSquaredKelvin,  // pound per gallon (US) per bar squared kelvin
         PoundPerCubicFootPerBarSquaredKelvin,  // pound per cubic foot per bar squared kelvin
         PoundPerCubicInchPerBarSquaredKelvin,  // pound per cubic inch per bar squared kelvin
         PoundPerCubicYardPerBarSquaredKelvin,  // pound per cubic yard per bar squared kelvin
         KilogramPerCubicMetrePerMegapascalSquaredKelvin,  // kilogram per cubic metre per megapascal squared kelvin
         SpecificGravityPerMegapascalSquaredKelvin,  // specific gravity per megapascal squared kelvin
         GramPerCubicCentimetrePerMegapascalSquaredKelvin,  // gram per cubic centimetre per megapascal squared kelvin
         PoundPerGallonUKPerMegapascalSquaredKelvin,  // pound per gallon (UK) per megapascal squared kelvin
         PoundPerGallonUSPerMegapascalSquaredKelvin,  // pound per gallon (US) per megapascal squared kelvin
         PoundPerCubicFootPerMegapascalSquaredKelvin,  // pound per cubic foot per megapascal squared kelvin
         PoundPerCubicInchPerMegapascalSquaredKelvin,  // pound per cubic inch per megapascal squared kelvin
         PoundPerCubicYardPerMegapascalSquaredKelvin,  // pound per cubic yard per megapascal squared kelvin
         KilogramPerCubicMetrePerGigapascalSquaredKelvin,  // kilogram per cubic metre per gigapascal squared kelvin
         SpecificGravityPerGigapascalSquaredKelvin,  // specific gravity per gigapascal squared kelvin
         GramPerCubicCentimetrePerGigapascalSquaredKelvin,  // gram per cubic centimetre per gigapascal squared kelvin
         PoundPerGallonUKPerGigapascalSquaredKelvin,  // pound per gallon (UK) per gigapascal squared kelvin
         PoundPerGallonUSPerGigapascalSquaredKelvin,  // pound per gallon (US) per gigapascal squared kelvin
         PoundPerCubicFootPerGigapascalSquaredKelvin,  // pound per cubic foot per gigapascal squared kelvin
         PoundPerCubicInchPerGigapascalSquaredKelvin,  // pound per cubic inch per gigapascal squared kelvin
         PoundPerCubicYardPerGigapascalSquaredKelvin,  // pound per cubic yard per gigapascal squared kelvin
         KilogramPerCubicMetrePerPoundPerSquareInchSquaredKelvin,  // kilogram per cubic metre per pound per square inch squared kelvin
         SpecificGravityPerPoundPerSquareInchSquaredKelvin,  // specific gravity per pound per square inch squared kelvin
         GramPerCubicCentimetrePerPoundPerSquareInchSquaredKelvin,  // gram per cubic centimetre per pound per square inch squared kelvin
         PoundPerGallonUKPerPoundPerSquareInchSquaredKelvin,  // pound per gallon (UK) per pound per square inch squared kelvin
         PoundPerGallonUSPerPoundPerSquareInchSquaredKelvin,  // pound per gallon (US) per pound per square inch squared kelvin
         PoundPerCubicFootPerPoundPerSquareInchSquaredKelvin,  // pound per cubic foot per pound per square inch squared kelvin
         PoundPerCubicInchPerPoundPerSquareInchSquaredKelvin,  // pound per cubic inch per pound per square inch squared kelvin
         PoundPerCubicYardPerPoundPerSquareInchSquaredKelvin,  // pound per cubic yard per pound per square inch squared kelvin
         KilogramPerCubicMetrePerPascalSquaredCelsius,  // kilogram per cubic metre per pascal squared celsius
         SpecificGravityPerPascalSquaredCelsius,  // specific gravity per pascal squared celsius
         GramPerCubicCentimetrePerPascalSquaredCelsius,  // gram per cubic centimetre per pascal squared celsius
         PoundPerGallonUKPerPascalSquaredCelsius,  // pound per gallon (UK) per pascal squared celsius
         PoundPerGallonUSPerPascalSquaredCelsius,  // pound per gallon (US) per pascal squared celsius
         PoundPerCubicFootPerPascalSquaredCelsius,  // pound per cubic foot per pascal squared celsius
         PoundPerCubicInchPerPascalSquaredCelsius,  // pound per cubic inch per pascal squared celsius
         PoundPerCubicYardPerPascalSquaredCelsius,  // pound per cubic yard per pascal squared celsius
         KilogramPerCubicMetrePerBarSquaredCelsius,  // kilogram per cubic metre per bar squared celsius
         SpecificGravityPerBarSquaredCelsius,  // specific gravity per bar squared celsius
         GramPerCubicCentimetrePerBarSquaredCelsius,  // gram per cubic centimetre per bar squared celsius
         PoundPerGallonUKPerBarSquaredCelsius,  // pound per gallon (UK) per bar squared celsius
         PoundPerGallonUSPerBarSquaredCelsius,  // pound per gallon (US) per bar squared celsius
         PoundPerCubicFootPerBarSquaredCelsius,  // pound per cubic foot per bar squared celsius
         PoundPerCubicInchPerBarSquaredCelsius,  // pound per cubic inch per bar squared celsius
         PoundPerCubicYardPerBarSquaredCelsius,  // pound per cubic yard per bar squared celsius
         KilogramPerCubicMetrePerPoundPerSquareInchSquaredCelsius,  // kilogram per cubic metre per pound per square inch squared celsius
         SpecificGravityPerPoundPerSquareInchSquaredCelsius,  // specific gravity per pound per square inch squared celsius
         GramPerCubicCentimetrePerPoundPerSquareInchSquaredCelsius,  // gram per cubic centimetre per pound per square inch squared celsius
         PoundPerGallonUKPerPoundPerSquareInchSquaredCelsius,  // pound per gallon (UK) per pound per square inch squared celsius
         PoundPerGallonUSPerPoundPerSquareInchSquaredCelsius,  // pound per gallon (US) per pound per square inch squared celsius
         PoundPerCubicFootPerPoundPerSquareInchSquaredCelsius,  // pound per cubic foot per pound per square inch squared celsius
         PoundPerCubicInchPerPoundPerSquareInchSquaredCelsius,  // pound per cubic inch per pound per square inch squared celsius
         PoundPerCubicYardPerPoundPerSquareInchSquaredCelsius,  // pound per cubic yard per pound per square inch squared celsius
         KilogramPerCubicMetrePerPascalSquaredFahrenheit,  // kilogram per cubic metre per pascal squared fahrenheit
         SpecificGravityPerPascalSquaredFahrenheit,  // specific gravity per pascal squared fahrenheit
         GramPerCubicCentimetrePerPascalSquaredFahrenheit,  // gram per cubic centimetre per pascal squared fahrenheit
         PoundPerGallonUKPerPascalSquaredFahrenheit,  // pound per gallon (UK) per pascal squared fahrenheit
         PoundPerGallonUSPerPascalSquaredFahrenheit,  // pound per gallon (US) per pascal squared fahrenheit
         PoundPerCubicFootPerPascalSquaredFahrenheit,  // pound per cubic foot per pascal squared fahrenheit
         PoundPerCubicInchPerPascalSquaredFahrenheit,  // pound per cubic inch per pascal squared fahrenheit
         PoundPerCubicYardPerPascalSquaredFahrenheit,  // pound per cubic yard per pascal squared fahrenheit
         KilogramPerCubicMetrePerBarSquaredFahrenheit,  // kilogram per cubic metre per bar squared fahrenheit
         SpecificGravityPerBarSquaredFahrenheit,  // specific gravity per bar squared fahrenheit
         GramPerCubicCentimetrePerBarSquaredFahrenheit,  // gram per cubic centimetre per bar squared fahrenheit
         PoundPerGallonUKPerBarSquaredFahrenheit,  // pound per gallon (UK) per bar squared fahrenheit
         PoundPerGallonUSPerBarSquaredFahrenheit,  // pound per gallon (US) per bar squared fahrenheit
         PoundPerCubicFootPerBarSquaredFahrenheit,  // pound per cubic foot per bar squared fahrenheit
         PoundPerCubicInchPerBarSquaredFahrenheit,  // pound per cubic inch per bar squared fahrenheit
         PoundPerCubicYardPerBarSquaredFahrenheit,  // pound per cubic yard per bar squared fahrenheit
         KilogramPerCubicMetrePerPoundPerSquareInchSquaredFahrenheit,  // kilogram per cubic metre per pound per square inch squared fahrenheit
         SpecificGravityPerPoundPerSquareInchSquaredFahrenheit,  // specific gravity per pound per square inch squared fahrenheit
         GramPerCubicCentimetrePerPoundPerSquareInchSquaredFahrenheit,  // gram per cubic centimetre per pound per square inch squared fahrenheit
         PoundPerGallonUKPerPoundPerSquareInchSquaredFahrenheit,  // pound per gallon (UK) per pound per square inch squared fahrenheit
         PoundPerGallonUSPerPoundPerSquareInchSquaredFahrenheit,  // pound per gallon (US) per pound per square inch squared fahrenheit
         PoundPerCubicFootPerPoundPerSquareInchSquaredFahrenheit,  // pound per cubic foot per pound per square inch squared fahrenheit
         PoundPerCubicInchPerPoundPerSquareInchSquaredFahrenheit,  // pound per cubic inch per pound per square inch squared fahrenheit
         PoundPerCubicYardPerPoundPerSquareInchSquaredFahrenheit // pound per cubic yard per pound per square inch squared fahrenheit
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.KilogramPerCubicMetrePerPascalSquaredKelvin, new Guid("e00b25c4-cf11-43f8-b87f-e7e482729d18")},  // kilogram per cubic metre per pascal squared kelvin
         {UnitChoicesEnum.SpecificGravityPerPascalSquaredKelvin, new Guid("111b012c-3e70-4e3b-8102-5fb0ecd0b1cd")},  // specific gravity per pascal squared kelvin
         {UnitChoicesEnum.GramPerCubicCentimetrePerPascalSquaredKelvin, new Guid("4497271d-cfe7-402e-a20a-7d4aa644d569")},  // gram per cubic centimetre per pascal squared kelvin
         {UnitChoicesEnum.PoundPerGallonUKPerPascalSquaredKelvin, new Guid("2a2cc741-54d7-4b54-bee7-616e48d9bec6")},  // pound per gallon (UK) per pascal squared kelvin
         {UnitChoicesEnum.PoundPerGallonUSPerPascalSquaredKelvin, new Guid("f09f13b4-a260-4192-9ca6-3a3b1466f6bf")},  // pound per gallon (US) per pascal squared kelvin
         {UnitChoicesEnum.PoundPerCubicFootPerPascalSquaredKelvin, new Guid("2cbb0b80-789a-4b11-8fa4-62d8f5e7c92f")},  // pound per cubic foot per pascal squared kelvin
         {UnitChoicesEnum.PoundPerCubicInchPerPascalSquaredKelvin, new Guid("da10a140-ae39-4b34-83b1-223e456a3865")},  // pound per cubic inch per pascal squared kelvin
         {UnitChoicesEnum.PoundPerCubicYardPerPascalSquaredKelvin, new Guid("63e7f7d8-a38b-4a45-937c-8567e97d71a9")},  // pound per cubic yard per pascal squared kelvin
         {UnitChoicesEnum.KilogramPerCubicMetrePerBarSquaredKelvin, new Guid("cbc5f940-560b-44cf-b652-3f0b953abe5e")},  // kilogram per cubic metre per bar squared kelvin
         {UnitChoicesEnum.SpecificGravityPerBarSquaredKelvin, new Guid("3e25dd0e-2726-4c26-922b-81c7ec8b416d")},  // specific gravity per bar squared kelvin
         {UnitChoicesEnum.GramPerCubicCentimetrePerBarSquaredKelvin, new Guid("26b11134-95aa-49f4-8959-7475ca97626b")},  // gram per cubic centimetre per bar squared kelvin
         {UnitChoicesEnum.PoundPerGallonUKPerBarSquaredKelvin, new Guid("5c136f82-a39a-4973-bb95-ca19b256bfb8")},  // pound per gallon (UK) per bar squared kelvin
         {UnitChoicesEnum.PoundPerGallonUSPerBarSquaredKelvin, new Guid("14a06bea-ee90-4a76-92b4-7186c5b1b43f")},  // pound per gallon (US) per bar squared kelvin
         {UnitChoicesEnum.PoundPerCubicFootPerBarSquaredKelvin, new Guid("f96cf481-f2bc-4f18-9119-c2d88a705cf1")},  // pound per cubic foot per bar squared kelvin
         {UnitChoicesEnum.PoundPerCubicInchPerBarSquaredKelvin, new Guid("dc4112ed-2c78-4f94-9be2-880fb9271578")},  // pound per cubic inch per bar squared kelvin
         {UnitChoicesEnum.PoundPerCubicYardPerBarSquaredKelvin, new Guid("8623f335-7ff1-4bed-8ada-5635c6c32b18")},  // pound per cubic yard per bar squared kelvin
         {UnitChoicesEnum.KilogramPerCubicMetrePerMegapascalSquaredKelvin, new Guid("d45bc21a-7b78-4d93-969a-95ac6aa7a3b1")},  // kilogram per cubic metre per megapascal squared kelvin
         {UnitChoicesEnum.SpecificGravityPerMegapascalSquaredKelvin, new Guid("e2f290d5-1ead-46d5-9ae6-da068e022447")},  // specific gravity per megapascal squared kelvin
         {UnitChoicesEnum.GramPerCubicCentimetrePerMegapascalSquaredKelvin, new Guid("6dab06b6-83da-4283-9504-9e05cd881aa4")},  // gram per cubic centimetre per megapascal squared kelvin
         {UnitChoicesEnum.PoundPerGallonUKPerMegapascalSquaredKelvin, new Guid("56b48f9d-7725-465e-b8f7-fb60b56305a4")},  // pound per gallon (UK) per megapascal squared kelvin
         {UnitChoicesEnum.PoundPerGallonUSPerMegapascalSquaredKelvin, new Guid("28f33e02-ccf4-4b73-bfcb-f7bd567e1129")},  // pound per gallon (US) per megapascal squared kelvin
         {UnitChoicesEnum.PoundPerCubicFootPerMegapascalSquaredKelvin, new Guid("bb8afc52-e0d4-45e5-b300-7dc6140f72a1")},  // pound per cubic foot per megapascal squared kelvin
         {UnitChoicesEnum.PoundPerCubicInchPerMegapascalSquaredKelvin, new Guid("a89bf491-7a64-4637-8ae3-091e1d088bbd")},  // pound per cubic inch per megapascal squared kelvin
         {UnitChoicesEnum.PoundPerCubicYardPerMegapascalSquaredKelvin, new Guid("0f74bd5d-5f14-4b76-b2c1-827a85fb5670")},  // pound per cubic yard per megapascal squared kelvin
         {UnitChoicesEnum.KilogramPerCubicMetrePerGigapascalSquaredKelvin, new Guid("e7410220-85c2-4a2e-95de-7701cfc9fc44")},  // kilogram per cubic metre per gigapascal squared kelvin
         {UnitChoicesEnum.SpecificGravityPerGigapascalSquaredKelvin, new Guid("2a482430-a155-4c91-a981-633f12d115d0")},  // specific gravity per gigapascal squared kelvin
         {UnitChoicesEnum.GramPerCubicCentimetrePerGigapascalSquaredKelvin, new Guid("0046231f-e430-444f-bd2e-7fa79fe20aab")},  // gram per cubic centimetre per gigapascal squared kelvin
         {UnitChoicesEnum.PoundPerGallonUKPerGigapascalSquaredKelvin, new Guid("6e2bf814-a090-415b-9043-c43d02c469d7")},  // pound per gallon (UK) per gigapascal squared kelvin
         {UnitChoicesEnum.PoundPerGallonUSPerGigapascalSquaredKelvin, new Guid("efbd074e-1aeb-4941-8339-837f7c9e8abe")},  // pound per gallon (US) per gigapascal squared kelvin
         {UnitChoicesEnum.PoundPerCubicFootPerGigapascalSquaredKelvin, new Guid("cd9f0127-ad45-4842-8b64-fc5d2eb4ec15")},  // pound per cubic foot per gigapascal squared kelvin
         {UnitChoicesEnum.PoundPerCubicInchPerGigapascalSquaredKelvin, new Guid("f898dd9c-cbda-478d-bff7-b24c1887e9e1")},  // pound per cubic inch per gigapascal squared kelvin
         {UnitChoicesEnum.PoundPerCubicYardPerGigapascalSquaredKelvin, new Guid("dc9cf37d-6036-4151-8ea4-2a8c40efb3c8")},  // pound per cubic yard per gigapascal squared kelvin
         {UnitChoicesEnum.KilogramPerCubicMetrePerPoundPerSquareInchSquaredKelvin, new Guid("7179c84a-113f-44c1-97c2-ccda465876bd")},  // kilogram per cubic metre per pound per square inch squared kelvin
         {UnitChoicesEnum.SpecificGravityPerPoundPerSquareInchSquaredKelvin, new Guid("1700ead8-a6fa-455c-9389-6526cb099af1")},  // specific gravity per pound per square inch squared kelvin
         {UnitChoicesEnum.GramPerCubicCentimetrePerPoundPerSquareInchSquaredKelvin, new Guid("9c5f6e94-c1ca-4107-8656-29a00c575f34")},  // gram per cubic centimetre per pound per square inch squared kelvin
         {UnitChoicesEnum.PoundPerGallonUKPerPoundPerSquareInchSquaredKelvin, new Guid("5a8b846c-1fb3-4c53-b181-14d2b5806919")},  // pound per gallon (UK) per pound per square inch squared kelvin
         {UnitChoicesEnum.PoundPerGallonUSPerPoundPerSquareInchSquaredKelvin, new Guid("6f009bc6-d776-4bb5-9f34-1870f6492c45")},  // pound per gallon (US) per pound per square inch squared kelvin
         {UnitChoicesEnum.PoundPerCubicFootPerPoundPerSquareInchSquaredKelvin, new Guid("61df2832-d446-4875-b000-660e89ff1510")},  // pound per cubic foot per pound per square inch squared kelvin
         {UnitChoicesEnum.PoundPerCubicInchPerPoundPerSquareInchSquaredKelvin, new Guid("c68060f8-20b1-46ad-aed8-6612ebdbc7d2")},  // pound per cubic inch per pound per square inch squared kelvin
         {UnitChoicesEnum.PoundPerCubicYardPerPoundPerSquareInchSquaredKelvin, new Guid("2170eff8-21a6-4682-81ab-2d2834ba03d9")},  // pound per cubic yard per pound per square inch squared kelvin
         {UnitChoicesEnum.KilogramPerCubicMetrePerPascalSquaredCelsius, new Guid("0d5a8548-b83e-4a96-9550-57725a3f3e9c")},  // kilogram per cubic metre per pascal squared celsius
         {UnitChoicesEnum.SpecificGravityPerPascalSquaredCelsius, new Guid("667e0395-0678-458f-9a27-3ee726df2ea6")},  // specific gravity per pascal squared celsius
         {UnitChoicesEnum.GramPerCubicCentimetrePerPascalSquaredCelsius, new Guid("3454083c-dfe0-454d-a048-8ed9e6835fe9")},  // gram per cubic centimetre per pascal squared celsius
         {UnitChoicesEnum.PoundPerGallonUKPerPascalSquaredCelsius, new Guid("b0761965-0273-497b-843f-54faf816ad0f")},  // pound per gallon (UK) per pascal squared celsius
         {UnitChoicesEnum.PoundPerGallonUSPerPascalSquaredCelsius, new Guid("c28837e9-5bfb-4154-a33e-04f775cf71c3")},  // pound per gallon (US) per pascal squared celsius
         {UnitChoicesEnum.PoundPerCubicFootPerPascalSquaredCelsius, new Guid("7367c345-b965-43ea-94d0-7068d8eab3a3")},  // pound per cubic foot per pascal squared celsius
         {UnitChoicesEnum.PoundPerCubicInchPerPascalSquaredCelsius, new Guid("1c6f0c40-80ef-4fb3-a0b7-892ba89c35e7")},  // pound per cubic inch per pascal squared celsius
         {UnitChoicesEnum.PoundPerCubicYardPerPascalSquaredCelsius, new Guid("623d2ac0-cb0e-485e-8996-bed2b943c6fa")},  // pound per cubic yard per pascal squared celsius
         {UnitChoicesEnum.KilogramPerCubicMetrePerBarSquaredCelsius, new Guid("a7346d6e-bd4a-4f93-8c4d-39d2e70d70a1")},  // kilogram per cubic metre per bar squared celsius
         {UnitChoicesEnum.SpecificGravityPerBarSquaredCelsius, new Guid("04e257cd-c95b-46d3-ace6-3a7de12dba45")},  // specific gravity per bar squared celsius
         {UnitChoicesEnum.GramPerCubicCentimetrePerBarSquaredCelsius, new Guid("da92c1ef-6ce0-4ee8-bdf1-aa6111aac4c7")},  // gram per cubic centimetre per bar squared celsius
         {UnitChoicesEnum.PoundPerGallonUKPerBarSquaredCelsius, new Guid("bcd39cc2-603f-4663-a896-3a7d5556b51d")},  // pound per gallon (UK) per bar squared celsius
         {UnitChoicesEnum.PoundPerGallonUSPerBarSquaredCelsius, new Guid("80757949-116e-45ee-98ec-cee61c7e6175")},  // pound per gallon (US) per bar squared celsius
         {UnitChoicesEnum.PoundPerCubicFootPerBarSquaredCelsius, new Guid("731cb76b-17c1-4a7d-9a84-2bd3b90ec32a")},  // pound per cubic foot per bar squared celsius
         {UnitChoicesEnum.PoundPerCubicInchPerBarSquaredCelsius, new Guid("2d1eb56c-e8f5-46ce-bafd-0696f0b1956b")},  // pound per cubic inch per bar squared celsius
         {UnitChoicesEnum.PoundPerCubicYardPerBarSquaredCelsius, new Guid("f9ffcd7c-4479-4122-9906-4f115712a799")},  // pound per cubic yard per bar squared celsius
         {UnitChoicesEnum.KilogramPerCubicMetrePerPoundPerSquareInchSquaredCelsius, new Guid("1ce45b46-493e-4e43-9a01-da7f137822c7")},  // kilogram per cubic metre per pound per square inch squared celsius
         {UnitChoicesEnum.SpecificGravityPerPoundPerSquareInchSquaredCelsius, new Guid("b5642483-4968-41a8-addf-55f5844f24ec")},  // specific gravity per pound per square inch squared celsius
         {UnitChoicesEnum.GramPerCubicCentimetrePerPoundPerSquareInchSquaredCelsius, new Guid("bd600761-8fe6-4fc2-b3d8-f0b27e8a414c")},  // gram per cubic centimetre per pound per square inch squared celsius
         {UnitChoicesEnum.PoundPerGallonUKPerPoundPerSquareInchSquaredCelsius, new Guid("c7d1f1ba-6574-4271-927c-0983ece07f8b")},  // pound per gallon (UK) per pound per square inch squared celsius
         {UnitChoicesEnum.PoundPerGallonUSPerPoundPerSquareInchSquaredCelsius, new Guid("f3b574d7-be5d-4c41-ad56-188a175e823d")},  // pound per gallon (US) per pound per square inch squared celsius
         {UnitChoicesEnum.PoundPerCubicFootPerPoundPerSquareInchSquaredCelsius, new Guid("ca74b7e6-a4dc-419d-bdfd-75b927aaa380")},  // pound per cubic foot per pound per square inch squared celsius
         {UnitChoicesEnum.PoundPerCubicInchPerPoundPerSquareInchSquaredCelsius, new Guid("48ae377a-0eea-4b9c-b1a9-0156a2d0ff35")},  // pound per cubic inch per pound per square inch squared celsius
         {UnitChoicesEnum.PoundPerCubicYardPerPoundPerSquareInchSquaredCelsius, new Guid("eaaf6fee-8773-4ec3-a0ea-de1d8672066f")},  // pound per cubic yard per pound per square inch squared celsius
         {UnitChoicesEnum.KilogramPerCubicMetrePerPascalSquaredFahrenheit, new Guid("489deba9-0377-46ea-9577-68efb2aedbf6")},  // kilogram per cubic metre per pascal squared fahrenheit
         {UnitChoicesEnum.SpecificGravityPerPascalSquaredFahrenheit, new Guid("b57c2f62-a6b6-4a8d-a9f7-2edfb0b1651c")},  // specific gravity per pascal squared fahrenheit
         {UnitChoicesEnum.GramPerCubicCentimetrePerPascalSquaredFahrenheit, new Guid("546e2ac5-4937-433f-906f-077f010e7964")},  // gram per cubic centimetre per pascal squared fahrenheit
         {UnitChoicesEnum.PoundPerGallonUKPerPascalSquaredFahrenheit, new Guid("ec7d57e4-b2fa-40b6-9d81-743a0610baf9")},  // pound per gallon (UK) per pascal squared fahrenheit
         {UnitChoicesEnum.PoundPerGallonUSPerPascalSquaredFahrenheit, new Guid("4d8dc1f2-032f-4792-b59a-2bf25dd5990f")},  // pound per gallon (US) per pascal squared fahrenheit
         {UnitChoicesEnum.PoundPerCubicFootPerPascalSquaredFahrenheit, new Guid("a1517641-82b9-4270-aa74-b0b2ece8c693")},  // pound per cubic foot per pascal squared fahrenheit
         {UnitChoicesEnum.PoundPerCubicInchPerPascalSquaredFahrenheit, new Guid("8def68a4-6327-4457-9bd1-7703c405bd83")},  // pound per cubic inch per pascal squared fahrenheit
         {UnitChoicesEnum.PoundPerCubicYardPerPascalSquaredFahrenheit, new Guid("6eec2c4d-5b2f-4aa7-ac68-18d57c81928d")},  // pound per cubic yard per pascal squared fahrenheit
         {UnitChoicesEnum.KilogramPerCubicMetrePerBarSquaredFahrenheit, new Guid("9fcb07f9-bd0c-46da-bd97-8cfd7ddaa24f")},  // kilogram per cubic metre per bar squared fahrenheit
         {UnitChoicesEnum.SpecificGravityPerBarSquaredFahrenheit, new Guid("496be92b-9b7f-447e-8bd4-ccdbfb7e5d45")},  // specific gravity per bar squared fahrenheit
         {UnitChoicesEnum.GramPerCubicCentimetrePerBarSquaredFahrenheit, new Guid("a295e2a1-736b-455f-9a1a-26b93fc3ce37")},  // gram per cubic centimetre per bar squared fahrenheit
         {UnitChoicesEnum.PoundPerGallonUKPerBarSquaredFahrenheit, new Guid("a2eab10e-27ef-409e-85dc-41770fe3bcb3")},  // pound per gallon (UK) per bar squared fahrenheit
         {UnitChoicesEnum.PoundPerGallonUSPerBarSquaredFahrenheit, new Guid("f8a5b28b-a219-4045-9c72-a65086b7c459")},  // pound per gallon (US) per bar squared fahrenheit
         {UnitChoicesEnum.PoundPerCubicFootPerBarSquaredFahrenheit, new Guid("fac1aaa4-f129-4f0b-80d8-9706223f27af")},  // pound per cubic foot per bar squared fahrenheit
         {UnitChoicesEnum.PoundPerCubicInchPerBarSquaredFahrenheit, new Guid("512f5c6c-717b-4532-8294-47883c881377")},  // pound per cubic inch per bar squared fahrenheit
         {UnitChoicesEnum.PoundPerCubicYardPerBarSquaredFahrenheit, new Guid("af3daad4-a444-46a1-b046-4bb36141b5bd")},  // pound per cubic yard per bar squared fahrenheit
         {UnitChoicesEnum.KilogramPerCubicMetrePerPoundPerSquareInchSquaredFahrenheit, new Guid("7733b2c6-da6b-42d8-84d0-abf1c8dd8259")},  // kilogram per cubic metre per pound per square inch squared fahrenheit
         {UnitChoicesEnum.SpecificGravityPerPoundPerSquareInchSquaredFahrenheit, new Guid("0cb8518e-1e1b-4e3c-9f34-5e0042c9e6d7")},  // specific gravity per pound per square inch squared fahrenheit
         {UnitChoicesEnum.GramPerCubicCentimetrePerPoundPerSquareInchSquaredFahrenheit, new Guid("cd1cab93-09b5-4793-afd4-d04d01cbae69")},  // gram per cubic centimetre per pound per square inch squared fahrenheit
         {UnitChoicesEnum.PoundPerGallonUKPerPoundPerSquareInchSquaredFahrenheit, new Guid("c958922f-4a1c-4186-b08a-e642df71dde3")},  // pound per gallon (UK) per pound per square inch squared fahrenheit
         {UnitChoicesEnum.PoundPerGallonUSPerPoundPerSquareInchSquaredFahrenheit, new Guid("bcb65f47-4365-4143-a517-8db45650aa11")},  // pound per gallon (US) per pound per square inch squared fahrenheit
         {UnitChoicesEnum.PoundPerCubicFootPerPoundPerSquareInchSquaredFahrenheit, new Guid("f3548d52-8d7d-4f67-9029-118edb2c13b9")},  // pound per cubic foot per pound per square inch squared fahrenheit
         {UnitChoicesEnum.PoundPerCubicInchPerPoundPerSquareInchSquaredFahrenheit, new Guid("8d02dc32-3c91-4323-b840-ebea2a6cbf7b")},  // pound per cubic inch per pound per square inch squared fahrenheit
         {UnitChoicesEnum.PoundPerCubicYardPerPoundPerSquareInchSquaredFahrenheit, new Guid("5e07f626-35e9-494b-a4e1-ba7f746c627d")} // pound per cubic yard per pound per square inch squared fahrenheit
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class PowerRateOfChangeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         WattPerSecond,  // watt per second
         KilowattPerSecond,  // kilowatt per second
         MegawattPerSecond,  // megawatt per second
         WattPerMinute,  // watt per minute
         KilowattPerMinute // kilowatt per minute
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.WattPerSecond, new Guid("f97ebd0e-aa46-4d24-add4-d49380360e4e")},  // watt per second
         {UnitChoicesEnum.KilowattPerSecond, new Guid("b2cb43a2-dd5b-4682-99d5-4085579d852b")},  // kilowatt per second
         {UnitChoicesEnum.MegawattPerSecond, new Guid("2b37872e-a376-4ccd-aace-ca04b413954c")},  // megawatt per second
         {UnitChoicesEnum.WattPerMinute, new Guid("b311cfde-8c1e-4b68-a5d5-95aa0330bbb1")},  // watt per minute
         {UnitChoicesEnum.KilowattPerMinute, new Guid("45f3cda8-e81a-48fc-9ffd-0c3d6756bbcb")} // kilowatt per minute
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ProportionRateOfChangeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         ProportionPerSecond,  // proportion per second
         PercentPerSecond,  // percent per second
         PerThousandPerSecond,  // per thousand per second
         PartPerMillionPerSecond // part per million per second
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.ProportionPerSecond, new Guid("5f0cc602-309c-479d-9c9c-9488675dcba3")},  // proportion per second
         {UnitChoicesEnum.PercentPerSecond, new Guid("9dbadbb9-de09-4fdc-b383-ffbd01a28f1c")},  // percent per second
         {UnitChoicesEnum.PerThousandPerSecond, new Guid("fa64e5ee-e321-4b0f-91b7-ae04c704bbcf")},  // per thousand per second
         {UnitChoicesEnum.PartPerMillionPerSecond, new Guid("518c5571-4415-4687-99bf-fc2e2db1b924")} // part per million per second
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class DataStorageQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Bit,  // bit
         Byte,  // byte
         Kilobit,  // kilobit
         Kilobyte // kilobyte
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Bit, new Guid("ef371794-6550-40be-a7eb-fe4e7b6cadf4")},  // bit
         {UnitChoicesEnum.Byte, new Guid("ae06779c-0ffb-45f9-a4ea-3597677b1210")},  // byte
         {UnitChoicesEnum.Kilobit, new Guid("db6bfea5-1351-4bd8-a119-0639f8fc5e15")},  // kilobit
         {UnitChoicesEnum.Kilobyte, new Guid("5309aeca-c57a-412e-9080-57708fea1cd9")} // kilobyte
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class DataTransferRateQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         BitPerSecond,  // bit per second
         KilobitPerSecond,  // kilobit per second
         MegabitPerSecond,  // megabit per second
         MegabytePerSecond // megabyte per second
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.BitPerSecond, new Guid("ff3db29b-631c-4dd6-807b-fa1da189f88c")},  // bit per second
         {UnitChoicesEnum.KilobitPerSecond, new Guid("8602813f-8dd7-44d5-bfb8-a5500d61535b")},  // kilobit per second
         {UnitChoicesEnum.MegabitPerSecond, new Guid("cc4b2ee1-fa42-4d27-8a7d-0ce2cdd553dc")},  // megabit per second
         {UnitChoicesEnum.MegabytePerSecond, new Guid("773f4b75-1292-46ec-9c27-cafa7e00280a")} // megabyte per second
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class DiffusivityQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         SquareMetrePerSecond,  // square metre per second
         SquareMillimetrePerSecond,  // square millimetre per second
         SquareFootPerHour // square foot per hour
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.SquareMetrePerSecond, new Guid("52e4a981-1639-4f44-81b6-123ceb2ad229")},  // square metre per second
         {UnitChoicesEnum.SquareMillimetrePerSecond, new Guid("7f17e101-5abf-4d1a-abc8-0ade006a9e68")},  // square millimetre per second
         {UnitChoicesEnum.SquareFootPerHour, new Guid("e2fe8fb4-aff4-411a-8862-59d20290d6a5")} // square foot per hour
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class DigitalSymbolRateQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Baud,  // baud
         Kilobaud,  // kilobaud
         Megabaud // megabaud
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Baud, new Guid("386c7e7a-51c9-4169-886b-20f00d521da2")},  // baud
         {UnitChoicesEnum.Kilobaud, new Guid("d90a7813-f811-4392-bcf2-6bb1b16bcf56")},  // kilobaud
         {UnitChoicesEnum.Megabaud, new Guid("e351c060-9b56-45bf-a908-9e8d18a143a7")} // megabaud
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class DoseEquivalentQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Sievert,  // sievert
         Millisievert,  // millisievert
         Microsievert // microsievert
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Sievert, new Guid("d4bcea8d-2603-4574-9f54-a4ca8b4b7c3c")},  // sievert
         {UnitChoicesEnum.Millisievert, new Guid("b5d1391d-d278-4577-8420-8329e7a06ac6")},  // millisievert
         {UnitChoicesEnum.Microsievert, new Guid("7636abf6-7b68-4ae7-ad4a-a2550260f2c4")} // microsievert
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class DoseEquivalentRateQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         SievertPerSecond,  // sievert per second
         MillisievertPerHour,  // millisievert per hour
         MicrosievertPerHour // microsievert per hour
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.SievertPerSecond, new Guid("8e582959-40b2-4562-ae5f-37030a052f1c")},  // sievert per second
         {UnitChoicesEnum.MillisievertPerHour, new Guid("80178739-e7ff-41a6-ab3a-74dc5f8fd9b6")},  // millisievert per hour
         {UnitChoicesEnum.MicrosievertPerHour, new Guid("44fee091-53cd-4c8f-8335-7ce6c755a220")} // microsievert per hour
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ElectricChargePerAreaQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         CoulombPerSquareMetre,  // coulomb per square metre
         MicrocoulombPerSquareCentimetre // microcoulomb per square centimetre
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.CoulombPerSquareMetre, new Guid("7d5aa795-ed12-441d-9779-ad45a90a0158")},  // coulomb per square metre
         {UnitChoicesEnum.MicrocoulombPerSquareCentimetre, new Guid("4bc8c041-8c49-44d3-9b67-760d123442cb")} // microcoulomb per square centimetre
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ElectricChargePerMassQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         CoulombPerKilogram,  // coulomb per kilogram
         MillicoulombPerKilogram // millicoulomb per kilogram
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.CoulombPerKilogram, new Guid("9491def5-2e78-4c5b-957c-c62d5690b263")},  // coulomb per kilogram
         {UnitChoicesEnum.MillicoulombPerKilogram, new Guid("6c8eea48-773d-4847-bce2-d46e7c094694")} // millicoulomb per kilogram
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ElectricChargePerVolumeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         CoulombPerCubicMetre,  // coulomb per cubic metre
         MicrocoulombPerCubicMetre,  // microcoulomb per cubic metre
         CoulombPerLitre // coulomb per litre
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.CoulombPerCubicMetre, new Guid("eadd7d6b-71e9-4df2-8498-19ba03c2fd5b")},  // coulomb per cubic metre
         {UnitChoicesEnum.MicrocoulombPerCubicMetre, new Guid("0a112bca-5e8a-4e13-8bdc-63ee39085081")},  // microcoulomb per cubic metre
         {UnitChoicesEnum.CoulombPerLitre, new Guid("0cc2230a-9b24-471c-a0d8-21171aa78291")} // coulomb per litre
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ElectricChargeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Coulomb,  // coulomb
         AmpereHour,  // ampere hour
         ElementaryCharge // elementary charge
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Coulomb, new Guid("fc4741ce-7704-439c-95df-6c24df9551c5")},  // coulomb
         {UnitChoicesEnum.AmpereHour, new Guid("5087c5ba-6a37-4e5d-9b7e-f8dbcd2a4eda")},  // ampere hour
         {UnitChoicesEnum.ElementaryCharge, new Guid("be19407f-276d-457e-afe0-00e2bed34e2b")} // elementary charge
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ElectricConductanceQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Siemens,  // siemens
         Millisiemens,  // millisiemens
         Microsiemens // microsiemens
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Siemens, new Guid("fcf092e1-03ff-4d1d-8405-39a7b5d35135")},  // siemens
         {UnitChoicesEnum.Millisiemens, new Guid("b5dcf8d4-ca7b-4ec2-9529-8d2f092421b2")},  // millisiemens
         {UnitChoicesEnum.Microsiemens, new Guid("ff5b8ac2-e551-4fa9-8b2b-2488089623d5")} // microsiemens
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ElectricConductivityQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         SiemensPerMetre,  // siemens per metre
         MillisiemensPerMetre,  // millisiemens per metre
         MicrosiemensPerCentimetre // microsiemens per centimetre
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.SiemensPerMetre, new Guid("ff411d9f-fba7-408c-bc74-e4eeb62f2e55")},  // siemens per metre
         {UnitChoicesEnum.MillisiemensPerMetre, new Guid("97dc8b32-f052-4721-ada2-ae0ead9270c5")},  // millisiemens per metre
         {UnitChoicesEnum.MicrosiemensPerCentimetre, new Guid("1bbf59c3-3b41-4082-8cff-3c842653b5db")} // microsiemens per centimetre
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ElectricCurrentDensityQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         AmperePerSquareMetre,  // ampere per square metre
         MilliamperePerSquareCentimetre // milliampere per square centimetre
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.AmperePerSquareMetre, new Guid("52df7588-2f3d-4a96-b74a-cef1dac5b69b")},  // ampere per square metre
         {UnitChoicesEnum.MilliamperePerSquareCentimetre, new Guid("e571a9c5-e485-4b09-a00d-1642c10baff8")} // milliampere per square centimetre
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ElectricDipoleMomentQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         CoulombMetre,  // coulomb metre
         ElementaryChargeNanometre // elementary charge nanometre
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.CoulombMetre, new Guid("ebb5ca40-44ed-4a8d-a1f4-afe168ab17e5")},  // coulomb metre
         {UnitChoicesEnum.ElementaryChargeNanometre, new Guid("3b2c145b-7a0a-4377-9a72-5a33e25b5a9e")} // elementary charge nanometre
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ElectricFieldStrengthQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         VoltPerMetre,  // volt per metre
         KilovoltPerMetre,  // kilovolt per metre
         VoltPerCentimetre // volt per centimetre
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.VoltPerMetre, new Guid("37050dd4-52c5-401e-907a-25c7bf67bfde")},  // volt per metre
         {UnitChoicesEnum.KilovoltPerMetre, new Guid("6a59ee3c-d679-48cc-b6a0-eb198b749073")},  // kilovolt per metre
         {UnitChoicesEnum.VoltPerCentimetre, new Guid("b477165b-7535-4549-a064-c45ab6fcbb9b")} // volt per centimetre
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ElectricResistanceGradientPerLengthQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         OhmPerMetre,  // ohm per metre
         OhmPerKilometre,  // ohm per kilometre
         OhmPerFoot // ohm per foot
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.OhmPerMetre, new Guid("68043253-647a-478f-9265-e164ed2c9071")},  // ohm per metre
         {UnitChoicesEnum.OhmPerKilometre, new Guid("f819762d-1772-42d8-8a77-d8806653fa98")},  // ohm per kilometre
         {UnitChoicesEnum.OhmPerFoot, new Guid("13202b93-795b-4fd4-88f0-bbff7cdf57cd")} // ohm per foot
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ElectromagneticMomentQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         AmpereSquareMetre,  // ampere square metre
         AmpereSquareCentimetre // ampere square centimetre
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.AmpereSquareMetre, new Guid("2783341f-c473-4440-91fb-fb8192ebea70")},  // ampere square metre
         {UnitChoicesEnum.AmpereSquareCentimetre, new Guid("5f37f86b-d5c9-43bf-be31-7ff262071d97")} // ampere square centimetre
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ForceAreaQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         NewtonSquareMetre,  // newton square metre
         KilonewtonSquareMetre,  // kilonewton square metre
         PoundForceSquareFoot // pound force square foot
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.NewtonSquareMetre, new Guid("ca399da7-f37a-4845-a70b-734155df938f")},  // newton square metre
         {UnitChoicesEnum.KilonewtonSquareMetre, new Guid("c7a62913-295b-465b-8869-5c604b8da539")},  // kilonewton square metre
         {UnitChoicesEnum.PoundForceSquareFoot, new Guid("6966fa90-dbdb-451a-92cd-283db921d295")} // pound force square foot
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ForcePerVolumeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         NewtonPerCubicMetre,  // newton per cubic metre
         KilonewtonPerCubicMetre,  // kilonewton per cubic metre
         PoundForcePerCubicFoot // pound force per cubic foot
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.NewtonPerCubicMetre, new Guid("130f8eaf-2abd-4da7-8c47-aa34aa0f1948")},  // newton per cubic metre
         {UnitChoicesEnum.KilonewtonPerCubicMetre, new Guid("ee9d7c2e-f962-452c-ab1e-664c44cbf144")},  // kilonewton per cubic metre
         {UnitChoicesEnum.PoundForcePerCubicFoot, new Guid("ff272cc7-1157-440d-869b-c3183dbc2c90")} // pound force per cubic foot
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class HeatCapacityQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         JoulePerKelvin,  // joule per kelvin
         KilojoulePerKelvin,  // kilojoule per kelvin
         BritishThermalUnitPerDegreeFahrenheit // british thermal unit per degree fahrenheit
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.JoulePerKelvin, new Guid("387b4ab8-4e40-4872-82f9-5e3ef827c059")},  // joule per kelvin
         {UnitChoicesEnum.KilojoulePerKelvin, new Guid("b72d91a9-56e8-4dce-a01e-6c250ee8ca99")},  // kilojoule per kelvin
         {UnitChoicesEnum.BritishThermalUnitPerDegreeFahrenheit, new Guid("9ae9182e-5d9a-41a5-b857-33db3ad4e7b0")} // british thermal unit per degree fahrenheit
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class IlluminanceQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Lux,  // lux
         Kilolux,  // kilolux
         FootCandle // foot candle
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Lux, new Guid("4fd99f29-efe9-407b-9cf2-b3a03f878453")},  // lux
         {UnitChoicesEnum.Kilolux, new Guid("98e85663-a416-48f8-bb15-8d63541498a4")},  // kilolux
         {UnitChoicesEnum.FootCandle, new Guid("fdfd69d5-aa3f-45d6-ac90-0eec5ca15a7a")} // foot candle
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class InductanceQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Henry,  // henry
         Millihenry,  // millihenry
         Microhenry // microhenry
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Henry, new Guid("2ed09fce-b549-4e80-9340-3ff2b642f2d4")},  // henry
         {UnitChoicesEnum.Millihenry, new Guid("082a2617-a363-4931-8791-a05dbf560c23")},  // millihenry
         {UnitChoicesEnum.Microhenry, new Guid("05d37c8e-1b8b-47d1-aabb-2ea45a32499f")} // microhenry
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class KinematicViscosityQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         SquareMetrePerSecond,  // square metre per second
         Stokes,  // stokes
         Centistokes // centistokes
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.SquareMetrePerSecond, new Guid("3565c99f-6367-4c5b-abca-2f7c08533eba")},  // square metre per second
         {UnitChoicesEnum.Stokes, new Guid("cdec3bb5-8d0b-4ffc-910f-2cdc987f1b4c")},  // stokes
         {UnitChoicesEnum.Centistokes, new Guid("37a10a66-c11f-4254-b939-a770aca4ec47")} // centistokes
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class LengthPerAngleQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         MetrePerRadian,  // metre per radian
         MetrePerDegree,  // metre per degree
         FootPerDegree // foot per degree
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.MetrePerRadian, new Guid("b694a909-4627-4cbd-ae36-e7e29a239a9c")},  // metre per radian
         {UnitChoicesEnum.MetrePerDegree, new Guid("27ac407b-1b77-43da-8b46-d59e58c9ce60")},  // metre per degree
         {UnitChoicesEnum.FootPerDegree, new Guid("4c88f88a-103d-40ce-a2bb-4e99423f8b81")} // foot per degree
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class LengthPerMassQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         MetrePerKilogram,  // metre per kilogram
         MillimetrePerKilogram,  // millimetre per kilogram
         FootPerPound // foot per pound
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.MetrePerKilogram, new Guid("0e731160-fee5-409d-be70-423aa958cbb0")},  // metre per kilogram
         {UnitChoicesEnum.MillimetrePerKilogram, new Guid("eb7b01ca-b0f0-4850-9abe-0a3a42111a1f")},  // millimetre per kilogram
         {UnitChoicesEnum.FootPerPound, new Guid("5bbc1d13-0f4e-4f61-b9cc-b05e91f7ad5b")} // foot per pound
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class LengthPerPressureQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         MetrePerPascal,  // metre per pascal
         MillimetrePerBar,  // millimetre per bar
         InchPerPsi // inch per psi
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.MetrePerPascal, new Guid("22af19e8-1809-461b-a8a1-db4f803938d7")},  // metre per pascal
         {UnitChoicesEnum.MillimetrePerBar, new Guid("8bfdf072-db03-4604-a6d9-39f9b99eee9f")},  // millimetre per bar
         {UnitChoicesEnum.InchPerPsi, new Guid("8dcd3d94-db24-4573-8e67-bf8fbf3a4e13")} // inch per psi
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class LengthPerTemperatureQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         MetrePerKelvin,  // metre per kelvin
         MillimetrePerKelvin,  // millimetre per kelvin
         FootPerDegreeFahrenheit // foot per degree fahrenheit
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.MetrePerKelvin, new Guid("5eb0aaab-92e3-408c-a381-4eb929a6e2b2")},  // metre per kelvin
         {UnitChoicesEnum.MillimetrePerKelvin, new Guid("8bb8ac76-86b4-49bc-9794-a50c8a18c88b")},  // millimetre per kelvin
         {UnitChoicesEnum.FootPerDegreeFahrenheit, new Guid("899cedf7-5ddc-4f4c-a8eb-01f97a78b853")} // foot per degree fahrenheit
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class LengthPerVolumeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         MetrePerCubicMetre,  // metre per cubic metre
         FootPerCubicFoot // foot per cubic foot
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.MetrePerCubicMetre, new Guid("c64123b3-159a-494c-82d4-83c502d54742")},  // metre per cubic metre
         {UnitChoicesEnum.FootPerCubicFoot, new Guid("af270ecd-b5a6-4ef5-b2b9-073a951b15a0")} // foot per cubic foot
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class LightExposureQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         LuxSecond,  // lux second
         LuxHour,  // lux hour
         FootCandleHour // foot candle hour
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.LuxSecond, new Guid("0c43758f-fcae-4081-81d8-6cca69c752de")},  // lux second
         {UnitChoicesEnum.LuxHour, new Guid("e7284ffa-74aa-40ff-a1b8-9f8bb879698e")},  // lux hour
         {UnitChoicesEnum.FootCandleHour, new Guid("7067a899-4fb3-4d12-afa5-a0b217652659")} // foot candle hour
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class LinearThermalExpansionCoefficientQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         ReciprocalKelvin,  // reciprocal kelvin
         MicrometrePerMetreKelvin,  // micrometre per metre kelvin
         ReciprocalDegreeFahrenheit // reciprocal degree fahrenheit
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.ReciprocalKelvin, new Guid("6bb7d280-79f1-4082-931b-95745b88df0e")},  // reciprocal kelvin
         {UnitChoicesEnum.MicrometrePerMetreKelvin, new Guid("27d5ba90-d3de-44f8-bd80-554ecbd78d2d")},  // micrometre per metre kelvin
         {UnitChoicesEnum.ReciprocalDegreeFahrenheit, new Guid("be900f4d-4861-4942-892d-0864b3cbcb1a")} // reciprocal degree fahrenheit
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class LuminanceQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         CandelaPerSquareMetre,  // candela per square metre
         CandelaPerSquareCentimetre,  // candela per square centimetre
         CandelaPerSquareFoot // candela per square foot
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.CandelaPerSquareMetre, new Guid("8328ebbf-7267-444c-9bf8-c816ebd1b206")},  // candela per square metre
         {UnitChoicesEnum.CandelaPerSquareCentimetre, new Guid("559df3ee-9ee1-4c82-a35e-6355318f625a")},  // candela per square centimetre
         {UnitChoicesEnum.CandelaPerSquareFoot, new Guid("bace55d7-2c3b-4828-b466-5c246b41e648")} // candela per square foot
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class LuminousEfficacyQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         LumenPerWatt,  // lumen per watt
         LumenPerKilowatt // lumen per kilowatt
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.LumenPerWatt, new Guid("b886e335-2192-4f7b-b04d-0003f9b0b24b")},  // lumen per watt
         {UnitChoicesEnum.LumenPerKilowatt, new Guid("9ea10827-647e-4208-8495-93f794603212")} // lumen per kilowatt
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class LuminousEnergyQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         LumenSecond,  // lumen second
         LumenHour // lumen hour
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.LumenSecond, new Guid("d7c4ef1e-1180-4c56-a845-64e9e6dffd26")},  // lumen second
         {UnitChoicesEnum.LumenHour, new Guid("391bcaf6-3656-4821-bee2-27e92d7f7238")} // lumen hour
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class LuminousExitanceQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         LumenPerSquareMetre,  // lumen per square metre
         LumenPerSquareFoot // lumen per square foot
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.LumenPerSquareMetre, new Guid("eb2f91a6-23d6-41fc-939f-83612ba39adf")},  // lumen per square metre
         {UnitChoicesEnum.LumenPerSquareFoot, new Guid("95b07233-6478-4b6a-ba51-1ecbdba2a52d")} // lumen per square foot
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class LuminousFluxQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Lumen,  // lumen
         Kilolumen // kilolumen
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Lumen, new Guid("d8fbbfd8-cb93-4d36-bad7-3cb254f8ab48")},  // lumen
         {UnitChoicesEnum.Kilolumen, new Guid("2fc41212-2edb-492e-81ff-a0e04e196ecd")} // kilolumen
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MagneticFieldStrengthQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         AmperePerMetre,  // ampere per metre
         AmperePerCentimetre,  // ampere per centimetre
         KiloamperePerMetre // kiloampere per metre
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.AmperePerMetre, new Guid("9385dd68-2cd1-477c-b0d3-c1f7968d9f9b")},  // ampere per metre
         {UnitChoicesEnum.AmperePerCentimetre, new Guid("01270a8e-a2d3-47dc-aa9f-2d63f1fb34ef")},  // ampere per centimetre
         {UnitChoicesEnum.KiloamperePerMetre, new Guid("265bf325-609d-46ea-b4e6-7ad1bfa57f49")} // kiloampere per metre
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MagneticFluxDensityGradientPerLengthQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         TeslaPerMetre,  // tesla per metre
         MicroteslaPerMetre,  // microtesla per metre
         GaussPerCentimetre // gauss per centimetre
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.TeslaPerMetre, new Guid("69a7f57d-70ed-411d-bcd7-9fa6b18bffbe")},  // tesla per metre
         {UnitChoicesEnum.MicroteslaPerMetre, new Guid("5aa9532f-42a9-46e2-9fc5-43be50648ffb")},  // microtesla per metre
         {UnitChoicesEnum.GaussPerCentimetre, new Guid("5d3a9041-0a5d-473e-b26e-5c2a526bce84")} // gauss per centimetre
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MagneticPermeabilityQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         HenryPerMetre,  // henry per metre
         MicrohenryPerMetre // microhenry per metre
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.HenryPerMetre, new Guid("1fa6ee1b-6e92-4ee2-8cd8-224b2183246f")},  // henry per metre
         {UnitChoicesEnum.MicrohenryPerMetre, new Guid("fd5ed30d-289a-420f-83bb-732284846e91")} // microhenry per metre
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MagneticVectorPotentialQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         TeslaMetre,  // tesla metre
         WeberPerMetre,  // weber per metre
         GaussCentimetre // gauss centimetre
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.TeslaMetre, new Guid("bb7d89a9-8058-44b7-b8e6-85ee98ec071c")},  // tesla metre
         {UnitChoicesEnum.WeberPerMetre, new Guid("9e0dbfa9-bfb8-4f80-a42d-cf893d72f04d")},  // weber per metre
         {UnitChoicesEnum.GaussCentimetre, new Guid("4efec503-b750-47c0-8b39-e7dc000157f9")} // gauss centimetre
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MassLengthQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         KilogramMetre,  // kilogram metre
         GramCentimetre,  // gram centimetre
         PoundFoot // pound foot
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.KilogramMetre, new Guid("99577a2c-c4a7-4a9e-bea4-2f06687cdaf5")},  // kilogram metre
         {UnitChoicesEnum.GramCentimetre, new Guid("784940f3-a2f0-46a3-8023-db9e0ce0ff3b")},  // gram centimetre
         {UnitChoicesEnum.PoundFoot, new Guid("f85d0d50-d8f3-45b7-9ea2-d1170c4b4a28")} // pound foot
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MassPerAreaQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         KilogramPerSquareMetre,  // kilogram per square metre
         GramPerSquareCentimetre,  // gram per square centimetre
         PoundPerSquareFoot // pound per square foot
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.KilogramPerSquareMetre, new Guid("91ccf59f-2ea7-4e3d-9b20-09f365f40083")},  // kilogram per square metre
         {UnitChoicesEnum.GramPerSquareCentimetre, new Guid("d85eab07-847e-4dc5-a2f2-6d7e32cb35b5")},  // gram per square centimetre
         {UnitChoicesEnum.PoundPerSquareFoot, new Guid("f6de3be8-ae7b-4920-bd4a-b6ace09f02c7")} // pound per square foot
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MassRateGradientPerLengthQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         KilogramPerSecondMetre,  // kilogram per second metre
         KilogramPerMinuteMetre,  // kilogram per minute metre
         PoundPerMinuteFoot // pound per minute foot
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.KilogramPerSecondMetre, new Guid("dead3854-9f7f-488b-a6bf-6ce64fa54219")},  // kilogram per second metre
         {UnitChoicesEnum.KilogramPerMinuteMetre, new Guid("dc6e06ea-52ce-46f3-9733-6e0ac54a8706")},  // kilogram per minute metre
         {UnitChoicesEnum.PoundPerMinuteFoot, new Guid("05ee15e6-5ef3-403c-82f8-bae268834aa9")} // pound per minute foot
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MassRatePerAreaQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         KilogramPerSquareMetreSecond,  // kilogram per square metre second
         KilogramPerSquareMetreHour,  // kilogram per square metre hour
         PoundPerSquareFootSecond // pound per square foot second
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.KilogramPerSquareMetreSecond, new Guid("8a29a29b-8cb1-41a6-8649-2fc4997831d5")},  // kilogram per square metre second
         {UnitChoicesEnum.KilogramPerSquareMetreHour, new Guid("84cf7f23-5d7b-44c4-ba0c-638138a3e3a2")},  // kilogram per square metre hour
         {UnitChoicesEnum.PoundPerSquareFootSecond, new Guid("7b66712d-8c22-495b-8e89-eafbf83fb7e7")} // pound per square foot second
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MolarEnergyQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         JoulePerMole,  // joule per mole
         KilojoulePerMole,  // kilojoule per mole
         ElectronvoltPerParticle // electronvolt per particle
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.JoulePerMole, new Guid("641ee110-a322-4f46-be92-1a9bda7a294e")},  // joule per mole
         {UnitChoicesEnum.KilojoulePerMole, new Guid("b1feb189-8ed5-4002-9b3c-8dc1ea28198b")},  // kilojoule per mole
         {UnitChoicesEnum.ElectronvoltPerParticle, new Guid("de2026ff-c51d-4d01-a22f-1214f15adc2b")} // electronvolt per particle
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MolarHeatCapacityQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         JoulePerMoleKelvin,  // joule per mole kelvin
         KilojoulePerKilomoleKelvin,  // kilojoule per kilomole kelvin
         KilojoulePerMoleKelvin // kilojoule per mole kelvin
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.JoulePerMoleKelvin, new Guid("b66ae637-3fb8-4063-8e82-7e9001d2086f")},  // joule per mole kelvin
         {UnitChoicesEnum.KilojoulePerKilomoleKelvin, new Guid("6e9d07ed-9840-44ba-b1f0-7a6a4349185c")},  // kilojoule per kilomole kelvin
         {UnitChoicesEnum.KilojoulePerMoleKelvin, new Guid("43f78db6-d700-4830-b17c-a32f05c49f83")} // kilojoule per mole kelvin
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MolarMassQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         KilogramPerMole,  // kilogram per mole
         GramPerMole,  // gram per mole
         PoundPerPoundMole // pound per pound mole
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.KilogramPerMole, new Guid("915ae218-7794-4d09-acb7-3be121224712")},  // kilogram per mole
         {UnitChoicesEnum.GramPerMole, new Guid("11ef8810-213d-4767-9916-8dd4cc690e34")},  // gram per mole
         {UnitChoicesEnum.PoundPerPoundMole, new Guid("2dfc62db-f8b4-4f5c-89a1-61a0d67db36c")} // pound per pound mole
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MolarVolumeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         CubicMetrePerMole,  // cubic metre per mole
         LitrePerMole // litre per mole
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.CubicMetrePerMole, new Guid("c5983410-59e5-4bd5-825f-60b4b878a5ee")},  // cubic metre per mole
         {UnitChoicesEnum.LitrePerMole, new Guid("b42bb32a-e007-4103-aa46-ad6161242ee6")} // litre per mole
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MomentumQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         KilogramMetrePerSecond,  // kilogram metre per second
         NewtonSecond,  // newton second
         PoundFootPerSecond // pound foot per second
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.KilogramMetrePerSecond, new Guid("68a56004-c3ef-47ff-8cfb-ddf8a18c57de")},  // kilogram metre per second
         {UnitChoicesEnum.NewtonSecond, new Guid("7f7dced7-0b3a-4125-9920-a9ac4e3cf219")},  // newton second
         {UnitChoicesEnum.PoundFootPerSecond, new Guid("dd698492-45cc-45fc-9f9c-498daad59460")} // pound foot per second
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class PermeabilityLengthQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         SquareMetreMetre,  // square metre metre
         DarcyMetre,  // darcy metre
         MillidarcyFoot // millidarcy foot
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.SquareMetreMetre, new Guid("a4af5ef4-9b29-4ecc-8901-d056115a95f9")},  // square metre metre
         {UnitChoicesEnum.DarcyMetre, new Guid("62cbe1a7-4677-4453-a54c-76dea6ab2be1")},  // darcy metre
         {UnitChoicesEnum.MillidarcyFoot, new Guid("fa208496-9604-4190-9da5-6ca9d8742bbc")} // millidarcy foot
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class PermittivityQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         FaradPerMetre,  // farad per metre
         PicofaradPerMetre,  // picofarad per metre
         NanofaradPerMetre // nanofarad per metre
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.FaradPerMetre, new Guid("b0ac1f42-633d-4d26-8f74-4e9a274f39b9")},  // farad per metre
         {UnitChoicesEnum.PicofaradPerMetre, new Guid("d254d268-0c9d-420c-a2ce-da7ffadc20d6")},  // picofarad per metre
         {UnitChoicesEnum.NanofaradPerMetre, new Guid("2e1e5ad7-99b8-4b41-bf80-52bead473e8b")} // nanofarad per metre
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class PlaneAnglePerVolumeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         RadianPerCubicMetre,  // radian per cubic metre
         DegreePerLitre // degree per litre
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.RadianPerCubicMetre, new Guid("20daaf82-ae4d-4591-90d1-d6b199b802e6")},  // radian per cubic metre
         {UnitChoicesEnum.DegreePerLitre, new Guid("ff2a7ea1-102f-4197-b4fe-987dc7317abe")} // degree per litre
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class PowerPerAreaQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         WattPerSquareMetre,  // watt per square metre
         KilowattPerSquareMetre,  // kilowatt per square metre
         BritishThermalUnitPerHourSquareFoot // british thermal unit per hour square foot
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.WattPerSquareMetre, new Guid("1b1af17b-4896-4797-beb8-34d44fa22756")},  // watt per square metre
         {UnitChoicesEnum.KilowattPerSquareMetre, new Guid("7b3dd2fe-39d4-4cfd-9b87-246a179efbf6")},  // kilowatt per square metre
         {UnitChoicesEnum.BritishThermalUnitPerHourSquareFoot, new Guid("c2c4d8a2-fc19-4d18-91b6-ebdfc8009606")} // british thermal unit per hour square foot
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class PowerPerVolumeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         WattPerCubicMetre,  // watt per cubic metre
         KilowattPerCubicMetre,  // kilowatt per cubic metre
         WattPerLitre // watt per litre
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.WattPerCubicMetre, new Guid("cd52ade0-1620-4ddc-ba46-cc0a9ce612dc")},  // watt per cubic metre
         {UnitChoicesEnum.KilowattPerCubicMetre, new Guid("775e57a0-79e4-46f0-b770-573ae94297ef")},  // kilowatt per cubic metre
         {UnitChoicesEnum.WattPerLitre, new Guid("707f3c31-9e9d-49b5-9de6-7138bc8e199f")} // watt per litre
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ProductivityIndexQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         CubicMetrePerSecondPascal,  // cubic metre per second pascal
         CubicMetrePerDayBar,  // cubic metre per day bar
         BarrelPerDayPsi // barrel per day psi
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.CubicMetrePerSecondPascal, new Guid("36e1f025-d290-4244-8807-3662333cb0bf")},  // cubic metre per second pascal
         {UnitChoicesEnum.CubicMetrePerDayBar, new Guid("7b17b0c1-8a44-40c3-9ba3-13af3e0078dc")},  // cubic metre per day bar
         {UnitChoicesEnum.BarrelPerDayPsi, new Guid("57744986-a005-4e42-aa17-8d6a6de3653f")} // barrel per day psi
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class RadianceQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         WattPerSteradianSquareMetre,  // watt per steradian square metre
         KilowattPerSteradianSquareMetre // kilowatt per steradian square metre
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.WattPerSteradianSquareMetre, new Guid("9736fbeb-e61a-4e3a-adbd-9e0566c927a3")},  // watt per steradian square metre
         {UnitChoicesEnum.KilowattPerSteradianSquareMetre, new Guid("7eedcdcb-beb6-47e6-89ac-ab3bdf136abb")} // kilowatt per steradian square metre
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class RadiantIntensityQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         WattPerSteradian,  // watt per steradian
         KilowattPerSteradian // kilowatt per steradian
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.WattPerSteradian, new Guid("8b19fa13-bf01-4fbd-be34-5f8405f7b112")},  // watt per steradian
         {UnitChoicesEnum.KilowattPerSteradian, new Guid("02423578-1c58-4c6d-9132-ee42cb9a319b")} // kilowatt per steradian
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class RadioactivityQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         Becquerel,  // becquerel
         Kilobecquerel,  // kilobecquerel
         Megabecquerel // megabecquerel
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Becquerel, new Guid("9ac2ee01-e74f-440c-b927-f165b8eb7a76")},  // becquerel
         {UnitChoicesEnum.Kilobecquerel, new Guid("b0f25c9c-26fa-468c-8934-c389c00fc7a0")},  // kilobecquerel
         {UnitChoicesEnum.Megabecquerel, new Guid("7e873097-2723-44e7-a147-a5781121ed1b")} // megabecquerel
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ReciprocalAreaQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         ReciprocalSquareMetre,  // reciprocal square metre
         ReciprocalSquareFoot,  // reciprocal square foot
         ReciprocalSquareInch // reciprocal square inch
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.ReciprocalSquareMetre, new Guid("7d3ba499-70b9-43d0-b1eb-ddc2a52ca306")},  // reciprocal square metre
         {UnitChoicesEnum.ReciprocalSquareFoot, new Guid("05d07310-74a0-4c76-be4b-729b9e40b822")},  // reciprocal square foot
         {UnitChoicesEnum.ReciprocalSquareInch, new Guid("7207d426-917f-4c0a-8e97-bdb360392a62")} // reciprocal square inch
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ReciprocalElectricTensionQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         ReciprocalVolt,  // reciprocal volt
         ReciprocalMillivolt,  // reciprocal millivolt
         ReciprocalKilovolt // reciprocal kilovolt
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.ReciprocalVolt, new Guid("35bd64fa-bb15-4901-a93c-47491c421de7")},  // reciprocal volt
         {UnitChoicesEnum.ReciprocalMillivolt, new Guid("2471bdb0-a6af-49af-afbc-9fa958e8403b")},  // reciprocal millivolt
         {UnitChoicesEnum.ReciprocalKilovolt, new Guid("9adac12e-661e-4c25-9789-38ea2b7d5f84")} // reciprocal kilovolt
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ReciprocalForceQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         ReciprocalNewton,  // reciprocal newton
         ReciprocalKilonewton,  // reciprocal kilonewton
         ReciprocalPoundForce // reciprocal pound force
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.ReciprocalNewton, new Guid("32c97575-6497-4f87-819c-fb733de5de76")},  // reciprocal newton
         {UnitChoicesEnum.ReciprocalKilonewton, new Guid("eccd1de9-d0eb-4fac-beff-f08d53d5e8c6")},  // reciprocal kilonewton
         {UnitChoicesEnum.ReciprocalPoundForce, new Guid("f2b6f1b1-56ab-49a6-ae7a-1331c49e58ee")} // reciprocal pound force
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ReciprocalMassQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         ReciprocalKilogram,  // reciprocal kilogram
         ReciprocalGram,  // reciprocal gram
         ReciprocalPound // reciprocal pound
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.ReciprocalKilogram, new Guid("0a9c46ca-6fe0-40e0-97cb-e3ec61e1b4de")},  // reciprocal kilogram
         {UnitChoicesEnum.ReciprocalGram, new Guid("55fca621-68b2-448a-97eb-d8637bc4f34c")},  // reciprocal gram
         {UnitChoicesEnum.ReciprocalPound, new Guid("40672be3-695b-445e-b466-9b63feb8687c")} // reciprocal pound
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ReciprocalTimeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         ReciprocalSecond,  // reciprocal second
         ReciprocalMinute,  // reciprocal minute
         ReciprocalHour // reciprocal hour
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.ReciprocalSecond, new Guid("00da1610-4717-49e4-bc33-6547bf0654f5")},  // reciprocal second
         {UnitChoicesEnum.ReciprocalMinute, new Guid("9465e6c1-cbe9-413c-8702-9ccb64b9fee7")},  // reciprocal minute
         {UnitChoicesEnum.ReciprocalHour, new Guid("7fe15b16-6bc4-4865-a5df-1ff6ebcb7577")} // reciprocal hour
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ReciprocalVolumeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         ReciprocalCubicMetre,  // reciprocal cubic metre
         ReciprocalLitre,  // reciprocal litre
         ReciprocalCubicFoot // reciprocal cubic foot
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.ReciprocalCubicMetre, new Guid("f0af1387-39ca-4339-8b29-319b13e81fc3")},  // reciprocal cubic metre
         {UnitChoicesEnum.ReciprocalLitre, new Guid("32c42173-e7dc-43df-9725-703474c86c6e")},  // reciprocal litre
         {UnitChoicesEnum.ReciprocalCubicFoot, new Guid("1a5bf5ae-ea01-4702-92aa-edc1a645a2b9")} // reciprocal cubic foot
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ReluctanceQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         ReciprocalHenry,  // reciprocal henry
         ReciprocalMillihenry // reciprocal millihenry
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.ReciprocalHenry, new Guid("574c6277-85f5-49cd-bd12-1d2500cf236b")},  // reciprocal henry
         {UnitChoicesEnum.ReciprocalMillihenry, new Guid("5ed33523-5dd4-4427-874b-a29b593e237f")} // reciprocal millihenry
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class SectionModulusQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         CubicMetre,  // cubic metre
         CubicCentimetre,  // cubic centimetre
         CubicInch // cubic inch
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.CubicMetre, new Guid("cda6b200-50aa-4a4e-bde4-3de9d54d88f3")},  // cubic metre
         {UnitChoicesEnum.CubicCentimetre, new Guid("b3d3c73b-8250-4341-9d67-cbab9012e3a5")},  // cubic centimetre
         {UnitChoicesEnum.CubicInch, new Guid("11cdcc79-d2ac-4088-a5c9-e0a69ca4a2fe")} // cubic inch
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class SpecificActivityQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         BecquerelPerKilogram,  // becquerel per kilogram
         KilobecquerelPerKilogram,  // kilobecquerel per kilogram
         BecquerelPerGram // becquerel per gram
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.BecquerelPerKilogram, new Guid("cf522a5e-e624-4cb5-8333-218453cbc7ce")},  // becquerel per kilogram
         {UnitChoicesEnum.KilobecquerelPerKilogram, new Guid("5f3fd488-cf0d-4fd3-ab1d-32b9546c3e10")},  // kilobecquerel per kilogram
         {UnitChoicesEnum.BecquerelPerGram, new Guid("fdd53991-2c85-475c-a9b9-569307e258ac")} // becquerel per gram
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class SpecificEnergyQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         JoulePerKilogram,  // joule per kilogram
         KilojoulePerKilogram,  // kilojoule per kilogram
         BritishThermalUnitPerPound // british thermal unit per pound
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.JoulePerKilogram, new Guid("d000f7cb-1e7a-4d8b-8806-a97dfbbf7941")},  // joule per kilogram
         {UnitChoicesEnum.KilojoulePerKilogram, new Guid("34b7d64d-5ec5-4016-98fe-c96244ba531b")},  // kilojoule per kilogram
         {UnitChoicesEnum.BritishThermalUnitPerPound, new Guid("68b33f90-312d-4eb5-a094-5a5008d8ed81")} // british thermal unit per pound
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class SpecificProductivityIndexQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         CubicMetrePerSecondPascalMetre,  // cubic metre per second pascal metre
         CubicMetrePerDayBarMetre,  // cubic metre per day bar metre
         BarrelPerDayPsiFoot // barrel per day psi foot
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.CubicMetrePerSecondPascalMetre, new Guid("c83c9872-6216-4bdd-855e-fbe531413b42")},  // cubic metre per second pascal metre
         {UnitChoicesEnum.CubicMetrePerDayBarMetre, new Guid("a79418be-824b-4fe6-90c2-db429970e96e")},  // cubic metre per day bar metre
         {UnitChoicesEnum.BarrelPerDayPsiFoot, new Guid("2fd4ffeb-0fee-4f9e-8482-080b0402d7c8")} // barrel per day psi foot
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ThermalConductanceQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         WattPerKelvin,  // watt per kelvin
         KilowattPerKelvin,  // kilowatt per kelvin
         BritishThermalUnitPerHourDegreeFahrenheit // british thermal unit per hour degree fahrenheit
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.WattPerKelvin, new Guid("1d8f78ba-b042-4e8a-a2a1-cab7e29240f6")},  // watt per kelvin
         {UnitChoicesEnum.KilowattPerKelvin, new Guid("5591c455-0985-42a8-93c1-d43204867ef9")},  // kilowatt per kelvin
         {UnitChoicesEnum.BritishThermalUnitPerHourDegreeFahrenheit, new Guid("ebf17506-af29-4a0f-a876-0ffc44ed5314")} // british thermal unit per hour degree fahrenheit
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ThermalInsulanceQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         SquareMetreKelvinPerWatt,  // square metre kelvin per watt
         SquareFootHourDegreeFahrenheitPerBritishThermalUnit // square foot hour degree fahrenheit per british thermal unit
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.SquareMetreKelvinPerWatt, new Guid("4f7dd83c-dfbd-4dd4-ae9c-cf4f9e031c67")},  // square metre kelvin per watt
         {UnitChoicesEnum.SquareFootHourDegreeFahrenheitPerBritishThermalUnit, new Guid("d47cfe7d-beb9-4d2c-85ea-5ac42c1a482c")} // square foot hour degree fahrenheit per british thermal unit
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ThermalResistanceQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         KelvinPerWatt,  // kelvin per watt
         KelvinPerKilowatt,  // kelvin per kilowatt
         HourDegreeFahrenheitPerBritishThermalUnit // hour degree fahrenheit per british thermal unit
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.KelvinPerWatt, new Guid("33065071-5e8d-4b62-a3fd-66aaec43def6")},  // kelvin per watt
         {UnitChoicesEnum.KelvinPerKilowatt, new Guid("3fd80f75-5fa8-406b-ad2a-076c6576f892")},  // kelvin per kilowatt
         {UnitChoicesEnum.HourDegreeFahrenheitPerBritishThermalUnit, new Guid("d4a9d58d-d91d-4bbc-9516-8010d44ea282")} // hour degree fahrenheit per british thermal unit
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class TimePerLengthQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         SecondPerMetre,  // second per metre
         MinutePerMetre,  // minute per metre
         SecondPerFoot // second per foot
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.SecondPerMetre, new Guid("a70fe7ae-dd6f-4e44-ac3b-a7c53f287d26")},  // second per metre
         {UnitChoicesEnum.MinutePerMetre, new Guid("aad1e3d6-1dd6-4aed-8a3f-a96613d6c701")},  // minute per metre
         {UnitChoicesEnum.SecondPerFoot, new Guid("a660c9aa-3930-44bb-873a-1cc58b633f9c")} // second per foot
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class TimePerMassQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         SecondPerKilogram,  // second per kilogram
         MinutePerKilogram,  // minute per kilogram
         SecondPerPound // second per pound
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.SecondPerKilogram, new Guid("1b3f1ec3-bb78-4d71-961f-748133bd0f87")},  // second per kilogram
         {UnitChoicesEnum.MinutePerKilogram, new Guid("c26a00ce-2663-41ea-8996-c53acc7e6409")},  // minute per kilogram
         {UnitChoicesEnum.SecondPerPound, new Guid("3fec2d1b-1aba-4a2b-872d-a61f7f11dc43")} // second per pound
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class TimePerVolumeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         SecondPerCubicMetre,  // second per cubic metre
         MinutePerLitre,  // minute per litre
         SecondPerCubicFoot // second per cubic foot
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.SecondPerCubicMetre, new Guid("0639bb1c-8868-46ff-8854-d67c989ad0da")},  // second per cubic metre
         {UnitChoicesEnum.MinutePerLitre, new Guid("25053c0e-5373-46d1-a1de-c785480fdb94")},  // minute per litre
         {UnitChoicesEnum.SecondPerCubicFoot, new Guid("d84599be-f945-4440-b33a-6ce7b59bab96")} // second per cubic foot
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class VolumeGradientPerLengthQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         CubicMetrePerMetre,  // cubic metre per metre
         LitrePerMetre,  // litre per metre
         CubicFootPerFoot // cubic foot per foot
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.CubicMetrePerMetre, new Guid("64db95fe-8310-4a99-a668-d68bf90b14ed")},  // cubic metre per metre
         {UnitChoicesEnum.LitrePerMetre, new Guid("4bd8be1c-05ce-4d4a-b642-c429db1d11e5")},  // litre per metre
         {UnitChoicesEnum.CubicFootPerFoot, new Guid("11aedb9f-ac7a-4dbf-8668-f17f48725d1c")} // cubic foot per foot
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class VolumePerAreaQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         CubicMetrePerSquareMetre,  // cubic metre per square metre
         LitrePerSquareMetre,  // litre per square metre
         GallonUSPerSquareFoot,  // gallon US per square foot
         GallonUKPerSquareFoot // gallon UK per square foot
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.CubicMetrePerSquareMetre, new Guid("8bfc267b-0302-4811-bf00-9034372ae2e7")},  // cubic metre per square metre
         {UnitChoicesEnum.LitrePerSquareMetre, new Guid("e729798b-fa80-4004-a648-7376aaf64078")},  // litre per square metre
         {UnitChoicesEnum.GallonUSPerSquareFoot, new Guid("ef990147-20ec-4898-ab0e-77cd04eb7a1f")},  // gallon US per square foot
         {UnitChoicesEnum.GallonUKPerSquareFoot, new Guid("666401b1-68c4-4f43-b7b9-9ac067354a36")} // gallon UK per square foot
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class VolumePerAreaRateOfChangeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         CubicMetrePerSquareMetreSecond,  // cubic metre per square metre second
         LitrePerSquareMetreMinute // litre per square metre minute
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.CubicMetrePerSquareMetreSecond, new Guid("9b2b7fd7-cc5d-493d-9d84-d75c4a9dee2a")},  // cubic metre per square metre second
         {UnitChoicesEnum.LitrePerSquareMetreMinute, new Guid("b3ef43d1-af6a-4d50-b8bd-900fe293d045")} // litre per square metre minute
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class VolumePerLengthRateOfChangeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         CubicMetrePerMetreSecond,  // cubic metre per metre second
         LitrePerMetreMinute // litre per metre minute
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.CubicMetrePerMetreSecond, new Guid("1aef9de4-f583-4d25-b00c-0c7a06cd92d9")},  // cubic metre per metre second
         {UnitChoicesEnum.LitrePerMetreMinute, new Guid("8a2f6b3d-2613-4c4f-8ec6-c32b84f24033")} // litre per metre minute
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class VolumePerPressureQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         CubicMetrePerPascal,  // cubic metre per pascal
         LitrePerBar,  // litre per bar
         GallonUSPerPsi,  // gallon US per psi
         GallonUKPerPsi // gallon UK per psi
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.CubicMetrePerPascal, new Guid("8fa70f02-90dd-4add-a13f-89e99bae4310")},  // cubic metre per pascal
         {UnitChoicesEnum.LitrePerBar, new Guid("3c027d5f-dbd3-4ca3-94fe-0ad937cc3f17")},  // litre per bar
         {UnitChoicesEnum.GallonUSPerPsi, new Guid("99df103e-3761-4931-8436-1b3b7c5e9df7")},  // gallon US per psi
         {UnitChoicesEnum.GallonUKPerPsi, new Guid("7e526423-2bc0-4a90-b30a-86798f2bb4dc")} // gallon UK per psi
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class VolumePerPressureRateOfChangeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         CubicMetrePerPascalSecond,  // cubic metre per pascal second
         LitrePerBarMinute // litre per bar minute
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.CubicMetrePerPascalSecond, new Guid("7c3ec170-776e-4d3e-9c99-28e2ca6d10ba")},  // cubic metre per pascal second
         {UnitChoicesEnum.LitrePerBarMinute, new Guid("bca9770f-64db-490f-b9d1-f4caa16e7732")} // litre per bar minute
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class VolumePerVolumeRateOfChangeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         CubicMetrePerCubicMetreSecond,  // cubic metre per cubic metre second
         PercentPerSecond,  // percent per second
         PercentPerMinute // percent per minute
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.CubicMetrePerCubicMetreSecond, new Guid("ece407e6-967f-45be-b971-e63af2116db1")},  // cubic metre per cubic metre second
         {UnitChoicesEnum.PercentPerSecond, new Guid("8d8aa27b-7657-49d7-abd7-007057a1641f")},  // percent per second
         {UnitChoicesEnum.PercentPerMinute, new Guid("e1f92239-0b02-44dd-8cad-2d9df45e5854")} // percent per minute
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class VolumetricFlowRateGradientPerLengthQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         CubicMetrePerSecondMetre,  // cubic metre per second metre
         LitrePerMinuteMetre,  // litre per minute metre
         GallonUSPerMinuteFoot,  // gallon US per minute foot
         GallonUKPerMinuteFoot // gallon UK per minute foot
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.CubicMetrePerSecondMetre, new Guid("705d1e45-8f4f-4672-9696-cfb1b697282a")},  // cubic metre per second metre
         {UnitChoicesEnum.LitrePerMinuteMetre, new Guid("5404064e-c0f2-4eb4-b5cd-99119ba89b07")},  // litre per minute metre
         {UnitChoicesEnum.GallonUSPerMinuteFoot, new Guid("bf104618-b79a-49a1-82d9-ed6ee32d0aa4")},  // gallon US per minute foot
         {UnitChoicesEnum.GallonUKPerMinuteFoot, new Guid("179a2d8d-a6b3-4a01-87c9-95987cf5b25c")} // gallon UK per minute foot
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class VolumetricFlowRatePerAreaQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         CubicMetrePerSquareMetreSecond,  // cubic metre per square metre second
         LitrePerSquareMetreSecond,  // litre per square metre second
         GallonUSPerMinuteSquareFoot,  // gallon US per minute square foot
         GallonUKPerMinuteSquareFoot // gallon UK per minute square foot
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.CubicMetrePerSquareMetreSecond, new Guid("1c3d13e2-4ac3-4835-bf6b-be97a0141f7e")},  // cubic metre per square metre second
         {UnitChoicesEnum.LitrePerSquareMetreSecond, new Guid("10974ea8-0a8f-4a48-b828-9c9631af62e1")},  // litre per square metre second
         {UnitChoicesEnum.GallonUSPerMinuteSquareFoot, new Guid("e8feddf4-5fca-44e6-831c-931cedf6390e")},  // gallon US per minute square foot
         {UnitChoicesEnum.GallonUKPerMinuteSquareFoot, new Guid("7c5b7b15-9b5a-43be-bd6d-0b980a89d7e2")} // gallon UK per minute square foot
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class VolumetricHeatTransferCoefficientQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         WattPerCubicMetreKelvin,  // watt per cubic metre kelvin
         KilowattPerCubicMetreKelvin,  // kilowatt per cubic metre kelvin
         WattPerLitreKelvin // watt per litre kelvin
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.WattPerCubicMetreKelvin, new Guid("dfacd24d-56b2-4a48-8688-4bf6087d0886")},  // watt per cubic metre kelvin
         {UnitChoicesEnum.KilowattPerCubicMetreKelvin, new Guid("3fc64c67-82ce-4174-9d55-d9356893b508")},  // kilowatt per cubic metre kelvin
         {UnitChoicesEnum.WattPerLitreKelvin, new Guid("d6a9930d-b0e8-4b01-84e3-a5cb3f6bc271")} // watt per litre kelvin
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class VolumetricThermalExpansionCoefficientQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         ReciprocalKelvin,  // reciprocal kelvin
         PercentPerKelvin,  // percent per kelvin
         ReciprocalDegreeFahrenheit // reciprocal degree fahrenheit
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.ReciprocalKelvin, new Guid("db8a9f66-0d53-4e94-a600-724e963857fc")},  // reciprocal kelvin
         {UnitChoicesEnum.PercentPerKelvin, new Guid("611e3bae-862f-4dfc-93b7-7f62780b6eb1")},  // percent per kelvin
         {UnitChoicesEnum.ReciprocalDegreeFahrenheit, new Guid("90afffe4-4f86-4f0f-a01b-bbe4fe28a382")} // reciprocal degree fahrenheit
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ElectricalMobilityQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         SquareMetrePerVoltSecond,  // square metre per volt second
         SquareCentimetrePerVoltSecond // square centimetre per volt second
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.SquareMetrePerVoltSecond, new Guid("7788e7a0-8ff5-4252-bf3c-f81eada5f068")},  // square metre per volt second
         {UnitChoicesEnum.SquareCentimetrePerVoltSecond, new Guid("1ae50b3c-e251-4012-85e7-7dbbc3a46bec")} // square centimetre per volt second
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class VolumePerAngleQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         CubicMetrePerRadian,  // cubic metre per radian
         CubicMetrePerRevolution,  // cubic metre per revolution
         LitrePerRevolution,  // litre per revolution
         CubicInchPerRevolution // cubic inch per revolution
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.CubicMetrePerRadian, new Guid("e93fadc5-0e91-42b2-a7ec-faf8a7990c0e")},  // cubic metre per radian
         {UnitChoicesEnum.CubicMetrePerRevolution, new Guid("2cb98e09-e016-48de-a4e8-d778c0f03083")},  // cubic metre per revolution
         {UnitChoicesEnum.LitrePerRevolution, new Guid("fc1fc32a-3bc6-4d80-be6c-64f21328d428")},  // litre per revolution
         {UnitChoicesEnum.CubicInchPerRevolution, new Guid("c9690395-bcf5-4555-a5b3-d1f3c9adbad0")} // cubic inch per revolution
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MobilityQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         SquareMetrePerPascalSecond,  // square metre per pascal second
         DarcyPerPascalSecond,  // darcy per pascal second
         MillidarcyPerCentipoise,  // millidarcy per centipoise
         SquareMicrometrePerMillipascalSecond // square micrometre per millipascal second
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.SquareMetrePerPascalSecond, new Guid("b72eaf40-0787-4e94-a3d0-705e076af436")},  // square metre per pascal second
         {UnitChoicesEnum.DarcyPerPascalSecond, new Guid("130fc453-45ba-4c58-bb35-7b79aab66ce6")},  // darcy per pascal second
         {UnitChoicesEnum.MillidarcyPerCentipoise, new Guid("b271dc26-2870-4577-9198-8341a9aa7014")},  // millidarcy per centipoise
         {UnitChoicesEnum.SquareMicrometrePerMillipascalSecond, new Guid("37dff8de-a3c1-499f-bd7b-9b5a9f7482a5")} // square micrometre per millipascal second
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class EnergyPerAreaQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         JoulePerSquareMetre,  // joule per square metre
         KilojoulePerSquareMetre,  // kilojoule per square metre
         JoulePerSquareCentimetre,  // joule per square centimetre
         FootPoundForcePerSquareFoot // foot pound force per square foot
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.JoulePerSquareMetre, new Guid("79a7abbb-9c8c-499a-aab1-c2a9cde0f9a1")},  // joule per square metre
         {UnitChoicesEnum.KilojoulePerSquareMetre, new Guid("b01266c8-4711-4f15-9be6-36a07adbaffa")},  // kilojoule per square metre
         {UnitChoicesEnum.JoulePerSquareCentimetre, new Guid("ab9081db-72a5-4536-b133-cb64158e9fcf")},  // joule per square centimetre
         {UnitChoicesEnum.FootPoundForcePerSquareFoot, new Guid("4f0c36c7-f1d6-48ee-a097-591e606ef1f3")} // foot pound force per square foot
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class MassPerEnergyQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         KilogramPerJoule,  // kilogram per joule
         KilogramPerMegajoule,  // kilogram per megajoule
         GramPerKilojoule,  // gram per kilojoule
         PoundPerBritishThermalUnit // pound per british thermal unit
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.KilogramPerJoule, new Guid("8dd8f768-9029-47ae-b17e-bb469013456f")},  // kilogram per joule
         {UnitChoicesEnum.KilogramPerMegajoule, new Guid("428dfc44-76a3-4139-b091-9ffd573c2b8a")},  // kilogram per megajoule
         {UnitChoicesEnum.GramPerKilojoule, new Guid("4921b33b-348a-45fc-ac2d-e222839c17fa")},  // gram per kilojoule
         {UnitChoicesEnum.PoundPerBritishThermalUnit, new Guid("e5313428-2943-4ca7-9c93-4b4df0b9c45e")} // pound per british thermal unit
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class PressurePerVolumeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         PascalPerCubicMetre,  // pascal per cubic metre
         BarPerCubicMetre,  // bar per cubic metre
         BarPerLitre,  // bar per litre
         PsiPerBarrel,  // psi per barrel
         PsiPerUSGallon,  // psi per US gallon
         PsiPerUKGallon // psi per UK gallon
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.PascalPerCubicMetre, new Guid("628b9120-dab8-430c-bec0-9d9ad2aa55fb")},  // pascal per cubic metre
         {UnitChoicesEnum.BarPerCubicMetre, new Guid("0727709b-bbb7-440e-8020-131a342ea217")},  // bar per cubic metre
         {UnitChoicesEnum.BarPerLitre, new Guid("02f9fa89-9916-4fe6-9ba6-d51771e11f85")},  // bar per litre
         {UnitChoicesEnum.PsiPerBarrel, new Guid("8eb47e61-a01c-42d0-81df-e5adae17c0ed")},  // psi per barrel
         {UnitChoicesEnum.PsiPerUSGallon, new Guid("eeea969b-fb3e-4e41-8061-98a6acbdfc81")},  // psi per US gallon
         {UnitChoicesEnum.PsiPerUKGallon, new Guid("0aabb0c8-c1fe-4f51-8727-893170947d26")} // psi per UK gallon
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class PressureTimePerVolumeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         PascalSecondPerCubicMetre,  // pascal second per cubic metre
         BarSecondPerCubicMetre,  // bar second per cubic metre
         BarSecondPerLitre,  // bar second per litre
         PsiSecondPerBarrel,  // psi second per barrel
         PsiSecondPerUSGallon,  // psi second per US gallon
         PsiSecondPerUKGallon // psi second per UK gallon
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.PascalSecondPerCubicMetre, new Guid("8e991492-6e5d-4de6-8abb-19a8c3387a7b")},  // pascal second per cubic metre
         {UnitChoicesEnum.BarSecondPerCubicMetre, new Guid("9fe0b498-7b10-4180-8750-29e84b500c1f")},  // bar second per cubic metre
         {UnitChoicesEnum.BarSecondPerLitre, new Guid("b34a6394-c440-47bb-8be0-df723ab547a8")},  // bar second per litre
         {UnitChoicesEnum.PsiSecondPerBarrel, new Guid("4708a257-d19e-496a-aa82-4473f250f4b6")},  // psi second per barrel
         {UnitChoicesEnum.PsiSecondPerUSGallon, new Guid("bd57d35f-f3d9-4a00-b22a-82490af6f54a")},  // psi second per US gallon
         {UnitChoicesEnum.PsiSecondPerUKGallon, new Guid("2812ea08-5e50-46f5-b9a7-4b1f956c05ef")} // psi second per UK gallon
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class RelativeTemperaturePerPressureQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         KelvinPerPascal,  // kelvin per pascal
         KelvinPerBar,  // kelvin per bar
         RelativeCelsiusPerBar,  // relative celsius per bar
         RankinePerPsi // rankine per psi
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.KelvinPerPascal, new Guid("edc02e4f-5fa4-4498-b54d-7f820f6e6c3b")},  // kelvin per pascal
         {UnitChoicesEnum.KelvinPerBar, new Guid("c3964170-0f1b-4510-b771-ca13f636a972")},  // kelvin per bar
         {UnitChoicesEnum.RelativeCelsiusPerBar, new Guid("46f08355-221a-4cdc-bdc9-fcaa6169aa1c")},  // relative celsius per bar
         {UnitChoicesEnum.RankinePerPsi, new Guid("1c7f0459-a0e9-4a2d-9368-fa5629681d7c")} // rankine per psi
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class TemperatureRateOfChangeQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         KelvinPerSecond,  // kelvin per second
         KelvinPerMinute,  // kelvin per minute
         RelativeCelsiusPerMinute,  // relative celsius per minute
         RankinePerMinute,  // rankine per minute
         RankinePerHour // rankine per hour
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.KelvinPerSecond, new Guid("524c7149-c168-4103-a8d2-134686e3e267")},  // kelvin per second
         {UnitChoicesEnum.KelvinPerMinute, new Guid("a99064b2-d701-4099-947a-a6e22a14208a")},  // kelvin per minute
         {UnitChoicesEnum.RelativeCelsiusPerMinute, new Guid("3563299f-730e-4598-9c5c-a58bdce22894")},  // relative celsius per minute
         {UnitChoicesEnum.RankinePerMinute, new Guid("52491162-b0ec-429c-acdc-64ba1e584de8")},  // rankine per minute
         {UnitChoicesEnum.RankinePerHour, new Guid("c2c1a22c-ad7e-4c09-9c8e-a8cbed00771e")} // rankine per hour
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class VolumePerEnergyQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         CubicMetrePerJoule,  // cubic metre per joule
         LitrePerMegajoule,  // litre per megajoule
         USGallonPerBritishThermalUnit,  // US gallon per british thermal unit
         UKGallonPerBritishThermalUnit // UK gallon per british thermal unit
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.CubicMetrePerJoule, new Guid("a00aaeea-d574-458f-a411-0e9784c230db")},  // cubic metre per joule
         {UnitChoicesEnum.LitrePerMegajoule, new Guid("5d48e5b3-edf6-4e77-bace-3572c69765df")},  // litre per megajoule
         {UnitChoicesEnum.USGallonPerBritishThermalUnit, new Guid("b4efec52-58a4-4cb0-ba79-5d26ce82ebe6")},  // US gallon per british thermal unit
         {UnitChoicesEnum.UKGallonPerBritishThermalUnit, new Guid("0782d4b9-2588-47e3-a8b3-596135d549f0")} // UK gallon per british thermal unit
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class WorkGradientQuantity : DerivedBasePhysicalQuantity
  {
    public new enum UnitChoicesEnum
      {
         JoulePerMetre,  // joule per metre
         KilojoulePerMetre,  // kilojoule per metre
         JoulePerFoot,  // joule per foot
         FootPoundForcePerFoot // foot pound force per foot
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.JoulePerMetre, new Guid("b7488c2d-e76a-42ee-9da9-2729ad4fc5e7")},  // joule per metre
         {UnitChoicesEnum.KilojoulePerMetre, new Guid("8fd19e40-e368-469d-b779-8283d0af3312")},  // kilojoule per metre
         {UnitChoicesEnum.JoulePerFoot, new Guid("1d44f89c-7a70-4a84-82fd-cf7064c2e33f")},  // joule per foot
         {UnitChoicesEnum.FootPoundForcePerFoot, new Guid("e535e2e9-33d9-4f6c-b62c-fdd21ca855c0")} // foot pound force per foot
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class DiameterSmallQuantity : LengthQuantity
  {
    public new enum UnitChoicesEnum
      {
         Centimetre,  // centimetre
         Decimetre,  // decimetre
         Foot,  // foot
         Inch,  // inch
         Metre,  // metre
         Micrometre,  // micrometre
         Millimetre,  // millimetre
         Nanometre,  // nanometre
         Picometre,  // picometre
         Ångstrøm,  // ångstrøm
         InchPer32 // inch per 32
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Centimetre, new Guid("96a3d4b4-c321-4528-92c0-7a52646b6461")},  // centimetre
         {UnitChoicesEnum.Decimetre, new Guid("e84c1968-cc63-412e-82c1-93ed39a43c01")},  // decimetre
         {UnitChoicesEnum.Foot, new Guid("b4adebce-d0cd-417a-b38c-ab4a2e38233a")},  // foot
         {UnitChoicesEnum.Inch, new Guid("0a6e2349-6f90-4ac5-baed-ccdaf5e5b919")},  // inch
         {UnitChoicesEnum.Metre, new Guid("cc442e11-bb28-4e51-9074-87df66050d8a")},  // metre
         {UnitChoicesEnum.Micrometre, new Guid("60820c6d-d721-49b8-ba40-a75343aa0f2f")},  // micrometre
         {UnitChoicesEnum.Millimetre, new Guid("0b2094f1-ba22-4b7b-888a-7a6b5da2ba25")},  // millimetre
         {UnitChoicesEnum.Nanometre, new Guid("0d181caf-8121-46a8-bfa7-2cb7457d9db9")},  // nanometre
         {UnitChoicesEnum.Picometre, new Guid("f305ce05-7bd1-4d67-a834-e9b932ca586e")},  // picometre
         {UnitChoicesEnum.Ångstrøm, new Guid("0db6a73f-c58f-415c-ac0d-e56137b28d5a")},  // ångstrøm
         {UnitChoicesEnum.InchPer32, new Guid("758ae28a-7484-4e0c-91af-aa105bcd744f")} // inch per 32
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class DimensionLessStandardQuantity : DimensionlessQuantity
  {
    public new enum UnitChoicesEnum
      {
         Dimensionless // dimensionless
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Dimensionless, new Guid("8744b0f7-2866-42d8-bf6c-b619ac87b945")} // dimensionless
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class EarthMagneticFluxDensityQuantity : MagneticFluxDensityQuantity
  {
    public new enum UnitChoicesEnum
      {
         Tesla,  // tesla
         Gauss,  // gauss
         Milligauss,  // milligauss
         Microtesla,  // microtesla
         Nanotesla // nanotesla
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Tesla, new Guid("33c3b59d-9876-4918-9f31-f22de88d7bde")},  // tesla
         {UnitChoicesEnum.Gauss, new Guid("c09cd87d-8a84-45d0-88d3-20bb5cc48559")},  // gauss
         {UnitChoicesEnum.Milligauss, new Guid("41ace729-a2ff-4047-adc3-375829de64c6")},  // milligauss
         {UnitChoicesEnum.Microtesla, new Guid("c6b30197-be6b-41b7-803d-a8de61338612")},  // microtesla
         {UnitChoicesEnum.Nanotesla, new Guid("9bef9def-8cd3-4f7b-b991-290d3441b3d4")} // nanotesla
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ElasticModulusQuantity : PressureQuantity
  {
    public new enum UnitChoicesEnum
      {
         Pascal,  // pascal
         Megapascal,  // megapascal
         Gigapascal,  // gigapascal
         PoundPerSquareInch,  // pound per square inch
         MegapoundPerSquareInch // megapound per square inch
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Pascal, new Guid("4f8ebaf4-cd1b-4714-a609-0a9fbe44cafb")},  // pascal
         {UnitChoicesEnum.Megapascal, new Guid("4ef28797-f416-4d97-b36a-711ea848bcc0")},  // megapascal
         {UnitChoicesEnum.Gigapascal, new Guid("5c81fb9b-36ad-47b7-9a8e-c999f7fdbfe3")},  // gigapascal
         {UnitChoicesEnum.PoundPerSquareInch, new Guid("afce482e-a8cf-47f8-85c1-22595d5b5485")},  // pound per square inch
         {UnitChoicesEnum.MegapoundPerSquareInch, new Guid("bb49de0a-fbf3-4914-b6b9-fc60ab502522")} // megapound per square inch
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class LengthSmallQuantity : LengthQuantity
  {
    public new enum UnitChoicesEnum
      {
         Centimetre,  // centimetre
         Decimetre,  // decimetre
         Foot,  // foot
         Inch,  // inch
         Metre,  // metre
         Micrometre,  // micrometre
         Millimetre,  // millimetre
         Nanometre,  // nanometre
         Picometre,  // picometre
         Ångstrøm,  // ångstrøm
         InchPer32 // inch per 32
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Centimetre, new Guid("96a3d4b4-c321-4528-92c0-7a52646b6461")},  // centimetre
         {UnitChoicesEnum.Decimetre, new Guid("e84c1968-cc63-412e-82c1-93ed39a43c01")},  // decimetre
         {UnitChoicesEnum.Foot, new Guid("b4adebce-d0cd-417a-b38c-ab4a2e38233a")},  // foot
         {UnitChoicesEnum.Inch, new Guid("0a6e2349-6f90-4ac5-baed-ccdaf5e5b919")},  // inch
         {UnitChoicesEnum.Metre, new Guid("cc442e11-bb28-4e51-9074-87df66050d8a")},  // metre
         {UnitChoicesEnum.Micrometre, new Guid("60820c6d-d721-49b8-ba40-a75343aa0f2f")},  // micrometre
         {UnitChoicesEnum.Millimetre, new Guid("0b2094f1-ba22-4b7b-888a-7a6b5da2ba25")},  // millimetre
         {UnitChoicesEnum.Nanometre, new Guid("0d181caf-8121-46a8-bfa7-2cb7457d9db9")},  // nanometre
         {UnitChoicesEnum.Picometre, new Guid("f305ce05-7bd1-4d67-a834-e9b932ca586e")},  // picometre
         {UnitChoicesEnum.Ångstrøm, new Guid("0db6a73f-c58f-415c-ac0d-e56137b28d5a")},  // ångstrøm
         {UnitChoicesEnum.InchPer32, new Guid("758ae28a-7484-4e0c-91af-aa105bcd744f")} // inch per 32
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class RotationalFrequencyQuantity : FrequencyQuantity
  {
    public new enum UnitChoicesEnum
      {
         Hertz,  // hertz
         Rpm // rpm
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Hertz, new Guid("7c572c06-0699-4187-9d0c-397f479fe93d")},  // hertz
         {UnitChoicesEnum.Rpm, new Guid("30880b5f-803d-412e-9736-62dca3ba5bbd")} // rpm
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class HydraulicConductivityQuantity : VelocityQuantity
  {
    public new enum UnitChoicesEnum
      {
         MetrePerSecond,  // metre per second
         MetrePerMinute,  // metre per minute
         MetrePerHour,  // metre per hour
         MetrePerDay,  // metre per day
         FootPerSecond,  // foot per second
         FootPerMinute,  // foot per minute
         FootPerHour,  // foot per hour
         FootPerDay,  // foot per day
         CentimetrePerSecond,  // centimetre per second
         InchPerSecond // inch per second
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.MetrePerSecond, new Guid("919ac736-9a37-45d1-8c02-54bc453d65dc")},  // metre per second
         {UnitChoicesEnum.MetrePerMinute, new Guid("824d3b5b-1e51-446a-99a4-39c02377f303")},  // metre per minute
         {UnitChoicesEnum.MetrePerHour, new Guid("b4867c19-0668-4043-b3b9-f666f7552b02")},  // metre per hour
         {UnitChoicesEnum.MetrePerDay, new Guid("aa10055f-1ef6-460d-841c-b6dc69f6a7f2")},  // metre per day
         {UnitChoicesEnum.FootPerSecond, new Guid("6c9eef39-29f0-4d6d-ae7a-f9161d8fd4fa")},  // foot per second
         {UnitChoicesEnum.FootPerMinute, new Guid("2d139d2c-1063-4f8d-99ae-bf71a98a1076")},  // foot per minute
         {UnitChoicesEnum.FootPerHour, new Guid("adb3b459-aa7e-4639-ad07-6d19c80f8170")},  // foot per hour
         {UnitChoicesEnum.FootPerDay, new Guid("aa58ec87-0dbc-4ed4-b8aa-8553b02c7b14")},  // foot per day
         {UnitChoicesEnum.CentimetrePerSecond, new Guid("d32f84d7-f076-49ab-8bd5-1ccd27e0eba6")},  // centimetre per second
         {UnitChoicesEnum.InchPerSecond, new Guid("8cd16c97-5c7a-4ee9-b59b-cbe2decd8ff9")} // inch per second
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class VolumeLargeQuantity : VolumeQuantity
  {
    public new enum UnitChoicesEnum
      {
         CubicMetre,  // cubic metre
         Litre,  // litre
         USGallon,  // US gallon
         UKGallon,  // UK gallon
         Barrel,  // barrel
         MillionCubicMetre,  // million cubic metre
         MillionLitre,  // million litre
         MillionUKGallon,  // million UK gallon
         MillionBarrel,  // million barrel
         MillionStandardCubicFoot,  // million standard cubic foot
         ThousandStandardCubicFoot // thousand standard cubic foot
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.CubicMetre, new Guid("a465ba87-53d6-456c-8e74-315a1a212498")},  // cubic metre
         {UnitChoicesEnum.Litre, new Guid("3f713743-6248-4b0b-8f3b-c97b6fab76b1")},  // litre
         {UnitChoicesEnum.USGallon, new Guid("1377d8ac-d203-48ed-bad4-733b0dc9d496")},  // US gallon
         {UnitChoicesEnum.UKGallon, new Guid("78f1cef7-c489-498c-96fb-d37474e242a9")},  // UK gallon
         {UnitChoicesEnum.Barrel, new Guid("b0f03925-d158-48ee-8c33-bf72c08cae68")},  // barrel
         {UnitChoicesEnum.MillionCubicMetre, new Guid("4c021db5-327b-44d3-9c4f-5a9b84af602f")},  // million cubic metre
         {UnitChoicesEnum.MillionLitre, new Guid("4f3f67df-28af-4398-966f-b23de678f50c")},  // million litre
         {UnitChoicesEnum.MillionUKGallon, new Guid("ab9a2938-f519-47c0-bcaa-d61c8fa23c7b")},  // million UK gallon
         {UnitChoicesEnum.MillionBarrel, new Guid("9d03120c-2c74-4666-8e24-98e143ab88db")},  // million barrel
         {UnitChoicesEnum.MillionStandardCubicFoot, new Guid("387b78ff-d51b-4684-b059-4c813407d767")},  // million standard cubic foot
         {UnitChoicesEnum.ThousandStandardCubicFoot, new Guid("e50b6a47-cf4c-4a74-8c1c-758000df5d67")} // thousand standard cubic foot
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class RotationalFrequencyRateOfChangeQuantity : FrequencyRateOfChangeQuantity
  {
    public new enum UnitChoicesEnum
      {
         HertzPerSecond,  // hertz per second
         RpmPerSecond // rpm per second
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.HertzPerSecond, new Guid("4d7e4b49-df76-4259-a96c-8c1250d5ecdd")},  // hertz per second
         {UnitChoicesEnum.RpmPerSecond, new Guid("762b5d58-a1ba-40cb-8776-2004613d15fb")} // rpm per second
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class TensionQuantity : ForceQuantity
  {
    public new enum UnitChoicesEnum
      {
         Newton,  // newton
         Decanewton,  // decanewton
         Kilonewton,  // kilonewton
         Kilodecanewton,  // kilodecanewton
         KilogramForce,  // kilogram force
         PoundForce,  // pound force
         KilopoundForce // kilopound force
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Newton, new Guid("2e6b218c-0f85-4e8d-b9c5-73b78d207ef8")},  // newton
         {UnitChoicesEnum.Decanewton, new Guid("fc48e3a8-deb9-4cf6-aaad-5b18f7e37972")},  // decanewton
         {UnitChoicesEnum.Kilonewton, new Guid("e30d6f94-7887-4746-8d4f-eb720239b702")},  // kilonewton
         {UnitChoicesEnum.Kilodecanewton, new Guid("8ad7ae81-602b-4694-bc6d-c18f520f1266")},  // kilodecanewton
         {UnitChoicesEnum.KilogramForce, new Guid("ea771c51-4078-4aa6-b2df-db6f77a140ad")},  // kilogram force
         {UnitChoicesEnum.PoundForce, new Guid("c738ced5-1c99-42ec-9c47-59e7d6455ffa")},  // pound force
         {UnitChoicesEnum.KilopoundForce, new Guid("fa385f22-3ed9-4f34-ab0c-193e3ac79375")} // kilopound force
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ProportionStandardQuantity : ProportionQuantity
  {
    public new enum UnitChoicesEnum
      {
         Proportion,  // proportion
         Percent,  // percent
         PerThousand,  // per thousand
         PartPerMillion // part per million
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Proportion, new Guid("03eb339b-61aa-4b42-aa35-4a20c547fdb9")},  // proportion
         {UnitChoicesEnum.Percent, new Guid("1a825e84-bc53-4da8-a089-118fdf40b8f7")},  // percent
         {UnitChoicesEnum.PerThousand, new Guid("141465a2-9c3c-4dda-82ec-eb35e72250c2")},  // per thousand
         {UnitChoicesEnum.PartPerMillion, new Guid("af33bf27-c3b8-4746-8b08-826ed1d21792")} // part per million
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class LengthStandardQuantity : LengthQuantity
  {
    public new enum UnitChoicesEnum
      {
         Centimetre,  // centimetre
         Decimetre,  // decimetre
         Foot,  // foot
         Inch,  // inch
         Metre,  // metre
         Micrometre,  // micrometre
         Millimetre // millimetre
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Centimetre, new Guid("96a3d4b4-c321-4528-92c0-7a52646b6461")},  // centimetre
         {UnitChoicesEnum.Decimetre, new Guid("e84c1968-cc63-412e-82c1-93ed39a43c01")},  // decimetre
         {UnitChoicesEnum.Foot, new Guid("b4adebce-d0cd-417a-b38c-ab4a2e38233a")},  // foot
         {UnitChoicesEnum.Inch, new Guid("0a6e2349-6f90-4ac5-baed-ccdaf5e5b919")},  // inch
         {UnitChoicesEnum.Metre, new Guid("cc442e11-bb28-4e51-9074-87df66050d8a")},  // metre
         {UnitChoicesEnum.Micrometre, new Guid("60820c6d-d721-49b8-ba40-a75343aa0f2f")},  // micrometre
         {UnitChoicesEnum.Millimetre, new Guid("0b2094f1-ba22-4b7b-888a-7a6b5da2ba25")} // millimetre
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class FluidShearRateQuantity : FrequencyQuantity
  {
    public new enum UnitChoicesEnum
      {
         Hertz,  // hertz
         ReciprocalSecond // reciprocal second
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Hertz, new Guid("7c572c06-0699-4187-9d0c-397f479fe93d")},  // hertz
         {UnitChoicesEnum.ReciprocalSecond, new Guid("39240f8f-8c82-4026-9db7-f72ec60cb4c9")} // reciprocal second
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class FluidShearStressQuantity : PressureQuantity
  {
    public new enum UnitChoicesEnum
      {
         Pascal,  // pascal
         Kilopascal,  // kilopascal
         Bar,  // bar
         PoundPerSquareInch,  // pound per square inch
         PoundPerSquareFoot,  // pound per square foot
         PoundPer100SquareFoot,  // pound per 100 square foot
         DynePerSquareCentimetre // dyne per square centimetre
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Pascal, new Guid("4f8ebaf4-cd1b-4714-a609-0a9fbe44cafb")},  // pascal
         {UnitChoicesEnum.Kilopascal, new Guid("a41c04f5-198b-4a04-b90a-5700412a2a29")},  // kilopascal
         {UnitChoicesEnum.Bar, new Guid("0d182739-f8f6-47a6-afcb-71feac973307")},  // bar
         {UnitChoicesEnum.PoundPerSquareInch, new Guid("afce482e-a8cf-47f8-85c1-22595d5b5485")},  // pound per square inch
         {UnitChoicesEnum.PoundPerSquareFoot, new Guid("35b28889-c076-4274-b200-cf7732b17aa3")},  // pound per square foot
         {UnitChoicesEnum.PoundPer100SquareFoot, new Guid("e3b95821-d782-4f12-a492-489cbcd6d2a1")},  // pound per 100 square foot
         {UnitChoicesEnum.DynePerSquareCentimetre, new Guid("04ca59b8-90e1-4903-ac82-ee95cac0ca38")} // dyne per square centimetre
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class TorqueSmallQuantity : TorqueQuantity
  {
    public new enum UnitChoicesEnum
      {
         NewtonMetre,  // newton metre
         FootPound,  // foot pound
         NewtonDecimetre,  // newton decimetre
         NewtonCentimetre,  // newton centimetre
         NewtonMillimetre,  // newton millimetre
         InchPound // inch pound
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.NewtonMetre, new Guid("50b017fa-8d81-4076-a485-61de1d8301b5")},  // newton metre
         {UnitChoicesEnum.FootPound, new Guid("700d9fc7-17e1-4ad6-84f1-39cacbe5fe51")},  // foot pound
         {UnitChoicesEnum.NewtonDecimetre, new Guid("e70db590-c5fb-4ab9-a2c4-3ad611cb7f63")},  // newton decimetre
         {UnitChoicesEnum.NewtonCentimetre, new Guid("4acf4542-8df0-4f57-a852-7c0184dbeec9")},  // newton centimetre
         {UnitChoicesEnum.NewtonMillimetre, new Guid("a933225d-7c8e-4ce4-b0ea-4c1c6e9f7e34")},  // newton millimetre
         {UnitChoicesEnum.InchPound, new Guid("0d40553e-d8c4-4b75-ad05-61199b15a0a1")} // inch pound
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class RotationalFrequencySmallQuantity : RotationalFrequencyQuantity
  {
    public new enum UnitChoicesEnum
      {
         Hertz,  // hertz
         Rpm // rpm
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Hertz, new Guid("7c572c06-0699-4187-9d0c-397f479fe93d")},  // hertz
         {UnitChoicesEnum.Rpm, new Guid("30880b5f-803d-412e-9736-62dca3ba5bbd")} // rpm
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ProportionSmallQuantity : ProportionQuantity
  {
    public new enum UnitChoicesEnum
      {
         Proportion,  // proportion
         Percent,  // percent
         PerThousand,  // per thousand
         PartPerMillion // part per million
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Proportion, new Guid("03eb339b-61aa-4b42-aa35-4a20c547fdb9")},  // proportion
         {UnitChoicesEnum.Percent, new Guid("1a825e84-bc53-4da8-a089-118fdf40b8f7")},  // percent
         {UnitChoicesEnum.PerThousand, new Guid("141465a2-9c3c-4dda-82ec-eb35e72250c2")},  // per thousand
         {UnitChoicesEnum.PartPerMillion, new Guid("af33bf27-c3b8-4746-8b08-826ed1d21792")} // part per million
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class PorosityQuantity : ProportionQuantity
  {
    public new enum UnitChoicesEnum
      {
         Proportion,  // proportion
         Percent,  // percent
         PerThousand // per thousand
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Proportion, new Guid("03eb339b-61aa-4b42-aa35-4a20c547fdb9")},  // proportion
         {UnitChoicesEnum.Percent, new Guid("1a825e84-bc53-4da8-a089-118fdf40b8f7")},  // percent
         {UnitChoicesEnum.PerThousand, new Guid("141465a2-9c3c-4dda-82ec-eb35e72250c2")} // per thousand
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class StrokeFrequencyQuantity : FrequencyQuantity
  {
    public new enum UnitChoicesEnum
      {
         Hertz,  // hertz
         Spm // spm
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Hertz, new Guid("7c572c06-0699-4187-9d0c-397f479fe93d")},  // hertz
         {UnitChoicesEnum.Spm, new Guid("426b000b-235f-41d5-8806-b2e47fbfb53b")} // spm
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class ShockRateQuantity : FrequencyQuantity
  {
    public new enum UnitChoicesEnum
      {
         Hertz,  // hertz
         ShockPerMinute,  // shock per minute
         ShockPerHour // shock per hour
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Hertz, new Guid("7c572c06-0699-4187-9d0c-397f479fe93d")},  // hertz
         {UnitChoicesEnum.ShockPerMinute, new Guid("6ccbee46-cb8a-4777-b1d2-e88eedd24f73")},  // shock per minute
         {UnitChoicesEnum.ShockPerHour, new Guid("0c0d4ecb-ee11-4b57-9bc7-70860637232e")} // shock per hour
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class PlaneAngleGeodesicQuantity : PlaneAngleQuantity
  {
    public new enum UnitChoicesEnum
      {
         Radian,  // radian
         Degree // degree
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Radian, new Guid("a71fc712-342a-48c2-8e45-b56ee31c7ae0")},  // radian
         {UnitChoicesEnum.Degree, new Guid("023a3393-a01e-499f-967a-a76b1a78d586")} // degree
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
namespace OSDC.UnitConversion.Conversion
{
  public partial class PlaneAngleStandardQuantity : PlaneAngleQuantity
  {
    public new enum UnitChoicesEnum
      {
         Radian,  // radian
         Degree // degree
      }
    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()
    {
         {UnitChoicesEnum.Radian, new Guid("a71fc712-342a-48c2-8e45-b56ee31c7ae0")},  // radian
         {UnitChoicesEnum.Degree, new Guid("023a3393-a01e-499f-967a-a76b1a78d586")} // degree
    };
    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)
    {
       UnitChoice c = null;
       Guid guid;
       if (enumLookUp_.TryGetValue(choice, out guid))
       {
         c = GetUnitChoice(guid);
       }
       return c;
    }
  }
}
