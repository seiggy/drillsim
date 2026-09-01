using System.Reflection;
using OSDC.UnitConversion.Conversion.UnitSystem;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;

namespace OSDC.UnitConversion.Conversion.UnitSystem.DrillingEngineering
{
    public class DrillingUnitSystem : BaseUnitSystem
    {
        private static Dictionary<Guid, DrillingUnitSystem> unitSystemDict_ = null;

        private static List<BasePhysicalQuantity> availablePhysicalQuantities_ = null;

        private static DrillingUnitSystem SIUnitSystem_ = null;
        private static DrillingUnitSystem MetricUnitSystem_ = null;
        private static DrillingUnitSystem ImperialUnitSystem_ = null;
        private static DrillingUnitSystem USUnitSystem_ = null;

        /// <summary>
        /// default constructor
        /// </summary>
        public DrillingUnitSystem() : base()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="defaultUnitChoice"></param>
        public DrillingUnitSystem(BaseUnitSystem.DefaultUnitSystemEnum defaultUnitChoice) : base(defaultUnitChoice)
        {
            List<BasePhysicalQuantity> quantities = DrillingUnitSystem.AvailableQuantities;
            if (quantities != null)
            {
                /*
                // generate
                foreach(PhysicalQuantity quantity in quantities)
                {
                    Console.WriteLine("Choices.Add(" + quantity.GetType().Name + ".Instance.ID, " + quantity.GetType().Name + ".Instance.GetUnitChoice(" + quantity.GetType().Name + ".UnitChoicesEnum.).ID);");
                }
                */

                if (defaultUnitChoice == BaseUnitSystem.DefaultUnitSystemEnum.SI)
                {
                    IsSI = true;
                    IsDefault = true;
                    ID = new Guid("f8338e35-c548-4284-a2e7-61b94a7b4769");
                    Name = "SI";
                    Description = "International System of Units";

                    foreach (BasePhysicalQuantity quantity in quantities)
                    {
                        if (quantity.ID != Guid.Empty)
                        {
                            Guid unitChoiceID = Guid.Empty;
                            foreach (UnitChoice unitChoice in quantity.UnitChoices)
                            {
                                if (unitChoice.IsSI)
                                {
                                    unitChoiceID = unitChoice.ID;
                                    break;
                                }
                            }
                            if (unitChoiceID != Guid.Empty)
                            {
                                if (!Choices.ContainsKey(quantity.ID.ToString()))
                                {
                                    Choices.Add(quantity.ID.ToString(), unitChoiceID.ToString());
                                }
                                else
                                {
                                    throw new Exception("duplicate unit choice guid (" + quantity.ID.ToString() + ", " + quantity.Name + ") for drilling quantity: " + quantity.Name);
                                }
                            }
                            else
                            {
                                throw new Exception("missing default unit choice for this physical quantity");
                            }
                        }
                    }
                }
                else if (defaultUnitChoice == BaseUnitSystem.DefaultUnitSystemEnum.Metric)
                {
                    IsDefault = true;
                    ID = new Guid("0e595036-8f8b-4b70-9d81-3b45f351f55c");
                    Name = "Metric";
                    Description = "Metric System of Units";

                    Choices.Add(AccelerationDrillingQuantity.Instance.ID.ToString(), AccelerationDrillingQuantity.Instance.GetUnitChoice(AccelerationDrillingQuantity.UnitChoicesEnum.MetrePerSecondSquared).ID.ToString());
                    Choices.Add(AngleGradientPerLengthDrillingQuantity.Instance.ID.ToString(), AngleGradientPerLengthDrillingQuantity.Instance.GetUnitChoice(AngleGradientPerLengthDrillingQuantity.UnitChoicesEnum.DegreePerMetre).ID.ToString());
                    Choices.Add(AngularAccelerationDrillingQuantity.Instance.ID.ToString(), AngularAccelerationDrillingQuantity.Instance.GetUnitChoice(AngularAccelerationDrillingQuantity.UnitChoicesEnum.DegreePerSecondSquared).ID.ToString());
                    Choices.Add(AngularVelocityDrillingQuantity.Instance.ID.ToString(), AngularVelocityDrillingQuantity.Instance.GetUnitChoice(AngularVelocityDrillingQuantity.UnitChoicesEnum.RevolutionPerMinute).ID.ToString());
                    Choices.Add(AreaDrillingQuantity.Instance.ID.ToString(), AreaDrillingQuantity.Instance.GetUnitChoice(AreaDrillingQuantity.UnitChoicesEnum.SquareMetre).ID.ToString());
                    Choices.Add(BendingMomentDrillingQuantity.Instance.ID.ToString(), BendingMomentDrillingQuantity.Instance.GetUnitChoice(BendingMomentDrillingQuantity.UnitChoicesEnum.DecanewtonMetre).ID.ToString());
                    Choices.Add(TotalFlowAreaDrillingQuantity.Instance.ID.ToString(), TotalFlowAreaDrillingQuantity.Instance.GetUnitChoice(TotalFlowAreaDrillingQuantity.UnitChoicesEnum.SquareCentimetre).ID.ToString());
                    Choices.Add(AxialVelocityDrillingQuantity.Instance.ID.ToString(), AxialVelocityDrillingQuantity.Instance.GetUnitChoice(AxialVelocityDrillingQuantity.UnitChoicesEnum.MetrePerSecond).ID.ToString());
                    Choices.Add(BlockVelocityDrillingQuantity.Instance.ID.ToString(), BlockVelocityDrillingQuantity.Instance.GetUnitChoice(BlockVelocityDrillingQuantity.UnitChoicesEnum.MetrePerSecond).ID.ToString());
                    Choices.Add(CableDiameterDrillingQuantity.Instance.ID.ToString(), CableDiameterDrillingQuantity.Instance.GetUnitChoice(CableDiameterDrillingQuantity.UnitChoicesEnum.Inch).ID.ToString());
                    Choices.Add(CapillaryPressureDrillingQuantity.Instance.ID.ToString(), CapillaryPressureDrillingQuantity.Instance.GetUnitChoice(CapillaryPressureDrillingQuantity.UnitChoicesEnum.Bar).ID.ToString());
                    Choices.Add(CompressibilityDrillingQuantity.Instance.ID.ToString(), CompressibilityDrillingQuantity.Instance.GetUnitChoice(CompressibilityDrillingQuantity.UnitChoicesEnum.InverseBar).ID.ToString());
                    Choices.Add(CurvatureDrillingQuantity.Instance.ID.ToString(), CurvatureDrillingQuantity.Instance.GetUnitChoice(CurvatureDrillingQuantity.UnitChoicesEnum.DegreePer30m).ID.ToString());
                    Choices.Add(SpecificVolumeDrillingQuantity.Instance.ID.ToString(), SpecificVolumeDrillingQuantity.Instance.GetUnitChoice(SpecificVolumeDrillingQuantity.UnitChoicesEnum.CubicCentimetrePerGram).ID.ToString());
                    Choices.Add(SpecificVolumeSquaredDrillingQuantity.Instance.ID.ToString(), SpecificVolumeSquaredDrillingQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredDrillingQuantity.UnitChoicesEnum.CubicCentimetreSquaredPerGramSquared).ID.ToString());
                    Choices.Add(MassDensityGradientPerPressureDrillingQuantity.Instance.ID.ToString(), MassDensityGradientPerPressureDrillingQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureDrillingQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerGigapascal).ID.ToString());
                    Choices.Add(MassDensityGradientPerPressureSquaredDrillingQuantity.Instance.ID.ToString(), MassDensityGradientPerPressureSquaredDrillingQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredDrillingQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerGigapascalSquared).ID.ToString());
                    Choices.Add(MassDensityGradientPerPressureSquaredTemperatureDrillingQuantity.Instance.ID.ToString(), MassDensityGradientPerPressureSquaredTemperatureDrillingQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureDrillingQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerGigapascalSquaredKelvin).ID.ToString());
                    Choices.Add(MassDensityGradientPerPressureTemperatureDrillingQuantity.Instance.ID.ToString(), MassDensityGradientPerPressureTemperatureDrillingQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureDrillingQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerGigapascalKelvin).ID.ToString());
                    Choices.Add(MassDensityDrillingQuantity.Instance.ID.ToString(), MassDensityDrillingQuantity.Instance.GetUnitChoice(MassDensityDrillingQuantity.UnitChoicesEnum.SpecificGravity).ID.ToString());
                    Choices.Add(MassDensityGradientPerLengthDrillingQuantity.Instance.ID.ToString(), MassDensityGradientPerLengthDrillingQuantity.Instance.GetUnitChoice(MassDensityGradientPerLengthDrillingQuantity.UnitChoicesEnum.SpecificGravityPerMetre).ID.ToString());
                    Choices.Add(MassDensityGradientPerTemperatureDrillingQuantity.Instance.ID.ToString(), MassDensityGradientPerTemperatureDrillingQuantity.Instance.GetUnitChoice(MassDensityGradientPerTemperatureDrillingQuantity.UnitChoicesEnum.SpecificGravityPerCelsius).ID.ToString());
                    Choices.Add(MassDensityRateOfChangeDrillingQuantity.Instance.ID.ToString(), MassDensityRateOfChangeDrillingQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeDrillingQuantity.UnitChoicesEnum.SpecificGravityPerSecond).ID.ToString());
                    Choices.Add(DepthDrillingQuantity.Instance.ID.ToString(), DepthDrillingQuantity.Instance.GetUnitChoice(DepthDrillingQuantity.UnitChoicesEnum.Metre).ID.ToString());
                    Choices.Add(DrillStringMagneticFluxDrillingQuantity.Instance.ID.ToString(), DrillStringMagneticFluxDrillingQuantity.Instance.GetUnitChoice(DrillStringMagneticFluxDrillingQuantity.UnitChoicesEnum.Microweber).ID.ToString());
                    Choices.Add(DrillStemMaterialStrengthDrillingQuantity.Instance.ID.ToString(), DrillStemMaterialStrengthDrillingQuantity.Instance.GetUnitChoice(DrillStemMaterialStrengthDrillingQuantity.UnitChoicesEnum.Megapascal).ID.ToString());
                    Choices.Add(DurationDrillingQuantity.Instance.ID.ToString(), DurationDrillingQuantity.Instance.GetUnitChoice(DurationDrillingQuantity.UnitChoicesEnum.Second).ID.ToString());
                    Choices.Add(DynamicViscosityDrillingQuantity.Instance.ID.ToString(), DynamicViscosityDrillingQuantity.Instance.GetUnitChoice(DynamicViscosityDrillingQuantity.UnitChoicesEnum.PascalSecond).ID.ToString());
                    Choices.Add(ElongationGradientPerLengthDrillingQuantity.Instance.ID.ToString(), ElongationGradientPerLengthDrillingQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthDrillingQuantity.UnitChoicesEnum.MillimetrePerKilometre).ID.ToString()); ;
                    Choices.Add(EnergyDensityDrillingQuantity.Instance.ID.ToString(), EnergyDensityDrillingQuantity.Instance.GetUnitChoice(EnergyDensityDrillingQuantity.UnitChoicesEnum.JoulePerCubicMetre).ID.ToString()); ;
                    Choices.Add(FluidVelocityDrillingQuantity.Instance.ID.ToString(), FluidVelocityDrillingQuantity.Instance.GetUnitChoice(FluidVelocityDrillingQuantity.UnitChoicesEnum.FootPerSecond).ID.ToString());
                    Choices.Add(ForceDrillingQuantity.Instance.ID.ToString(), ForceDrillingQuantity.Instance.GetUnitChoice(ForceDrillingQuantity.UnitChoicesEnum.Decanewton).ID.ToString());
                    Choices.Add(ForceGradientPerLengthDrillingQuantity.Instance.ID.ToString(), ForceGradientPerLengthDrillingQuantity.Instance.GetUnitChoice(ForceGradientPerLengthDrillingQuantity.UnitChoicesEnum.DecanewtonPer30Metre).ID.ToString());
                    Choices.Add(ForceRateOfChangeDrillingQuantity.Instance.ID.ToString(), ForceRateOfChangeDrillingQuantity.Instance.GetUnitChoice(ForceRateOfChangeDrillingQuantity.UnitChoicesEnum.DecanewtonPerSecond).ID.ToString());
                    Choices.Add(FormationResistivityDrillingQuantity.Instance.ID.ToString(), FormationResistivityDrillingQuantity.Instance.GetUnitChoice(FormationResistivityDrillingQuantity.UnitChoicesEnum.OhmMetre).ID.ToString());
                    Choices.Add(FormationStrengthDrillingQuantity.Instance.ID.ToString(), FormationStrengthDrillingQuantity.Instance.GetUnitChoice(FormationStrengthDrillingQuantity.UnitChoicesEnum.Megapascal).ID.ToString());
                    Choices.Add(GammaRayIndexDrillingQuantity.Instance.ID.ToString(), GammaRayIndexDrillingQuantity.Instance.GetUnitChoice(GammaRayIndexDrillingQuantity.UnitChoicesEnum.Dimensionless).ID.ToString());
                    Choices.Add(GasShowDrillingQuantity.Instance.ID.ToString(), GasShowDrillingQuantity.Instance.GetUnitChoice(GasShowDrillingQuantity.UnitChoicesEnum.Percent).ID.ToString());
                    Choices.Add(GasVolumetricFlowRateDrillingQuantity.Instance.ID.ToString(), GasVolumetricFlowRateDrillingQuantity.Instance.GetUnitChoice(GasVolumetricFlowRateDrillingQuantity.UnitChoicesEnum.LitrePerMinute).ID.ToString());
                    Choices.Add(HeatTransferCoefficientDrillingQuantity.Instance.ID.ToString(), HeatTransferCoefficientDrillingQuantity.Instance.GetUnitChoice(HeatTransferCoefficientDrillingQuantity.UnitChoicesEnum.WattPerSquareMetrePerKelvin).ID.ToString());
                    Choices.Add(HeightDrillingQuantity.Instance.ID.ToString(), HeightDrillingQuantity.Instance.GetUnitChoice(HeightDrillingQuantity.UnitChoicesEnum.Metre).ID.ToString());
                    Choices.Add(HookLoadDrillingQuantity.Instance.ID.ToString(), HookLoadDrillingQuantity.Instance.GetUnitChoice(HookLoadDrillingQuantity.UnitChoicesEnum.TonneForce).ID.ToString());
                    Choices.Add(HydraulicConductivityDrillingQuantity.Instance.ID.ToString(), HydraulicConductivityDrillingQuantity.Instance.GetUnitChoice(HydraulicConductivityDrillingQuantity.UnitChoicesEnum.CentimetrePerSecond).ID.ToString());
                    Choices.Add(InterfacialTensionDrillingQuantity.Instance.ID.ToString(), InterfacialTensionDrillingQuantity.Instance.GetUnitChoice(InterfacialTensionDrillingQuantity.UnitChoicesEnum.NewtonPerMetre).ID.ToString());
                    Choices.Add(MassDrillingQuantity.Instance.ID.ToString(), MassDrillingQuantity.Instance.GetUnitChoice(MassDrillingQuantity.UnitChoicesEnum.Kilogram).ID.ToString());
                    Choices.Add(MassGradientPerLengthDrillingQuantity.Instance.ID.ToString(), MassGradientPerLengthDrillingQuantity.Instance.GetUnitChoice(MassGradientPerLengthDrillingQuantity.UnitChoicesEnum.KilogramPerMetre).ID.ToString());
                    Choices.Add(MassRateDrillingQuantity.Instance.ID.ToString(), MassRateDrillingQuantity.Instance.GetUnitChoice(MassRateDrillingQuantity.UnitChoicesEnum.KilogramPerSecond).ID.ToString());
                    Choices.Add(MomentOfAreaDrillingQuantity.Instance.ID.ToString(), MomentOfAreaDrillingQuantity.Instance.GetUnitChoice(MomentOfAreaDrillingQuantity.UnitChoicesEnum.CentimetresToTheFourthPower).ID.ToString());
                    Choices.Add(MomentOfInertiaDrillingQuantity.Instance.ID.ToString(), MomentOfInertiaDrillingQuantity.Instance.GetUnitChoice(MomentOfInertiaDrillingQuantity.UnitChoicesEnum.KilogramMetreSquared).ID.ToString());
                    Choices.Add(NozzleDiameterDrillingQuantity.Instance.ID.ToString(), NozzleDiameterDrillingQuantity.Instance.GetUnitChoice(NozzleDiameterDrillingQuantity.UnitChoicesEnum.InchPer32).ID.ToString());
                    Choices.Add(PorousMediumPermeabilityDrillingQuantity.Instance.ID.ToString(), PorousMediumPermeabilityDrillingQuantity.Instance.GetUnitChoice(PorousMediumPermeabilityDrillingQuantity.UnitChoicesEnum.Millidarcy).ID.ToString());
                    Choices.Add(DiameterPipeDrillingQuantity.Instance.ID.ToString(), DiameterPipeDrillingQuantity.Instance.GetUnitChoice(DiameterPipeDrillingQuantity.UnitChoicesEnum.Inch).ID.ToString());
                    Choices.Add(PlaneAngleDrillingQuantity.Instance.ID.ToString(), PlaneAngleDrillingQuantity.Instance.GetUnitChoice(PlaneAngleDrillingQuantity.UnitChoicesEnum.Degree).ID.ToString());
                    Choices.Add(DiameterPoreDrillingQuantity.Instance.ID.ToString(), DiameterPoreDrillingQuantity.Instance.GetUnitChoice(DiameterPoreDrillingQuantity.UnitChoicesEnum.Micrometre).ID.ToString());
                    Choices.Add(SurfacePoreDrillingQuantity.Instance.ID.ToString(), SurfacePoreDrillingQuantity.Instance.GetUnitChoice(SurfacePoreDrillingQuantity.UnitChoicesEnum.SquareMicrometre).ID.ToString());
                    Choices.Add(PositionDrillingQuantity.Instance.ID.ToString(), PositionDrillingQuantity.Instance.GetUnitChoice(PositionDrillingQuantity.UnitChoicesEnum.Metre).ID.ToString());
                    Choices.Add(PowerDrillingQuantity.Instance.ID.ToString(), PowerDrillingQuantity.Instance.GetUnitChoice(PowerDrillingQuantity.UnitChoicesEnum.Watt).ID.ToString());
                    Choices.Add(PowerRateOfChangeDrillingQuantity.Instance.ID.ToString(), PowerRateOfChangeDrillingQuantity.Instance.GetUnitChoice(PowerRateOfChangeDrillingQuantity.UnitChoicesEnum.WattPerSecond).ID.ToString());
                    Choices.Add(ChokeOpeningRateDrillingQuantity.Instance.ID.ToString(), ChokeOpeningRateDrillingQuantity.Instance.GetUnitChoice(ChokeOpeningRateDrillingQuantity.UnitChoicesEnum.PercentPerSecond).ID.ToString());
                    Choices.Add(PressureDrillingQuantity.Instance.ID.ToString(), PressureDrillingQuantity.Instance.GetUnitChoice(PressureDrillingQuantity.UnitChoicesEnum.Bar).ID.ToString());
                    Choices.Add(PressureGradientPerLengthDrillingQuantity.Instance.ID.ToString(), PressureGradientPerLengthDrillingQuantity.Instance.GetUnitChoice(PressureGradientPerLengthDrillingQuantity.UnitChoicesEnum.BarPerMetre).ID.ToString());
                    Choices.Add(PressureLossConstantDrillingQuantity.Instance.ID.ToString(), PressureLossConstantDrillingQuantity.Instance.GetUnitChoice(PressureLossConstantDrillingQuantity.UnitChoicesEnum.PressureLossConstantMetric).ID.ToString());
                    Choices.Add(PressureRateOfChangeDrillingQuantity.Instance.ID.ToString(), PressureRateOfChangeDrillingQuantity.Instance.GetUnitChoice(PressureRateOfChangeDrillingQuantity.UnitChoicesEnum.BarPerSecond).ID.ToString());
                    Choices.Add(RandomWalkDrillingQuantity.Instance.ID.ToString(), RandomWalkDrillingQuantity.Instance.GetUnitChoice(RandomWalkDrillingQuantity.UnitChoicesEnum.DegreePerSquareRootHour).ID.ToString());
                    Choices.Add(RateOfPenetrationDrillingQuantity.Instance.ID.ToString(), RateOfPenetrationDrillingQuantity.Instance.GetUnitChoice(RateOfPenetrationDrillingQuantity.UnitChoicesEnum.MetrePerHour).ID.ToString());
                    Choices.Add(RotationalFrequencyRateOfChangeDrillingQuantity.Instance.ID.ToString(), RotationalFrequencyRateOfChangeDrillingQuantity.Instance.GetUnitChoice(RotationalFrequencyRateOfChangeDrillingQuantity.UnitChoicesEnum.RpmPerSecond).ID.ToString());
                    Choices.Add(IsobaricSpecificHeatCapacityDrillingQuantity.Instance.ID.ToString(), IsobaricSpecificHeatCapacityDrillingQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityDrillingQuantity.UnitChoicesEnum.JoulePerKilogramKelvin).ID.ToString());
                    Choices.Add(IsobaricSpecificHeatCapacityGradientPerTemperatureDrillingQuantity.Instance.ID.ToString(), IsobaricSpecificHeatCapacityGradientPerTemperatureDrillingQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityGradientPerTemperatureDrillingQuantity.UnitChoicesEnum.JoulePerKilogramSquaredKelvin).ID.ToString());
                    Choices.Add(StickDurationDrillingQuantity.Instance.ID.ToString(), StickDurationDrillingQuantity.Instance.GetUnitChoice(StickDurationDrillingQuantity.UnitChoicesEnum.Second).ID.ToString());
                    Choices.Add(AngleMagneticFluxDensitySurveyInstrumentDrillingQuantity.Instance.ID.ToString(), AngleMagneticFluxDensitySurveyInstrumentDrillingQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensitySurveyInstrumentDrillingQuantity.UnitChoicesEnum.DegreeNanotesla).ID.ToString());
                    Choices.Add(AngularVelocitySurveyInstrumentDrillingQuantity.Instance.ID.ToString(), AngularVelocitySurveyInstrumentDrillingQuantity.Instance.GetUnitChoice(AngularVelocitySurveyInstrumentDrillingQuantity.UnitChoicesEnum.DegreePerHour).ID.ToString());
                    Choices.Add(ReciprocalLengthSurveyInstrumentDrillingQuantity.Instance.ID.ToString(), ReciprocalLengthSurveyInstrumentDrillingQuantity.Instance.GetUnitChoice(ReciprocalLengthSurveyInstrumentDrillingQuantity.UnitChoicesEnum.ReciprocalKilometre).ID.ToString());
                    Choices.Add(TemperatureDrillingQuantity.Instance.ID.ToString(), TemperatureDrillingQuantity.Instance.GetUnitChoice(TemperatureDrillingQuantity.UnitChoicesEnum.Celsius).ID.ToString());
                    Choices.Add(TemperatureGradientPerLengthDrillingQuantity.Instance.ID.ToString(), TemperatureGradientPerLengthDrillingQuantity.Instance.GetUnitChoice(TemperatureGradientPerLengthDrillingQuantity.UnitChoicesEnum.CelsiusPer100Metre).ID.ToString());
                    Choices.Add(TensionDrillingQuantity.Instance.ID.ToString(), TensionDrillingQuantity.Instance.GetUnitChoice(TensionDrillingQuantity.UnitChoicesEnum.Kilodecanewton).ID.ToString());
                    Choices.Add(ThermalConductivityDrillingQuantity.Instance.ID.ToString(), ThermalConductivityDrillingQuantity.Instance.GetUnitChoice(ThermalConductivityDrillingQuantity.UnitChoicesEnum.WattPerMetreKelvin).ID.ToString());
                    Choices.Add(ThermalConductivityGradientPerTemperatureDrillingQuantity.Instance.ID.ToString(), ThermalConductivityGradientPerTemperatureDrillingQuantity.Instance.GetUnitChoice(ThermalConductivityGradientPerTemperatureDrillingQuantity.UnitChoicesEnum.WattPerMetreKelvinPerKelvin).ID.ToString());
                    Choices.Add(TorqueDrillingQuantity.Instance.ID.ToString(), TorqueDrillingQuantity.Instance.GetUnitChoice(TorqueDrillingQuantity.UnitChoicesEnum.DecanewtonMetre).ID.ToString());
                    Choices.Add(TorqueGradientPerLengthDrillingQuantity.Instance.ID.ToString(), TorqueGradientPerLengthDrillingQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthDrillingQuantity.UnitChoicesEnum.NewtonMetrePerMetre).ID.ToString());
                    Choices.Add(TorqueRateOfChangeDrillingQuantity.Instance.ID.ToString(), TorqueRateOfChangeDrillingQuantity.Instance.GetUnitChoice(TorqueRateOfChangeDrillingQuantity.UnitChoicesEnum.DecanewtonMetrePerSecond).ID.ToString());
                    Choices.Add(VolumeDrillingQuantity.Instance.ID.ToString(), VolumeDrillingQuantity.Instance.GetUnitChoice(VolumeDrillingQuantity.UnitChoicesEnum.Litre).ID.ToString());
                    Choices.Add(VolumetricFlowrateDrillingQuantity.Instance.ID.ToString(), VolumetricFlowrateDrillingQuantity.Instance.GetUnitChoice(VolumetricFlowrateDrillingQuantity.UnitChoicesEnum.LitrePerMinute).ID.ToString());
                    Choices.Add(VolumetricFlowRateOfChangeDrillingQuantity.Instance.ID.ToString(), VolumetricFlowRateOfChangeDrillingQuantity.Instance.GetUnitChoice(VolumetricFlowRateOfChangeDrillingQuantity.UnitChoicesEnum.LitrePerMinutePerSecond).ID.ToString());
                    Choices.Add(WeightOnBitDrillingQuantity.Instance.ID.ToString(), WeightOnBitDrillingQuantity.Instance.GetUnitChoice(WeightOnBitDrillingQuantity.UnitChoicesEnum.TonneForce).ID.ToString());
                }
                else if (defaultUnitChoice == BaseUnitSystem.DefaultUnitSystemEnum.US)
                {
                    IsDefault = true;
                    ID = new Guid("3693c680-8c7e-4977-874e-109be3600c64");
                    Name = "US";
                    Description = "United States of America System of Units";

                    Choices.Add(AccelerationDrillingQuantity.Instance.ID.ToString(), AccelerationDrillingQuantity.Instance.GetUnitChoice(AccelerationDrillingQuantity.UnitChoicesEnum.FootPerSecondSquared).ID.ToString());
                    Choices.Add(AngleGradientPerLengthDrillingQuantity.Instance.ID.ToString(), AngleGradientPerLengthDrillingQuantity.Instance.GetUnitChoice(AngleGradientPerLengthDrillingQuantity.UnitChoicesEnum.DegreePerFoot).ID.ToString());
                    Choices.Add(AngularAccelerationDrillingQuantity.Instance.ID.ToString(), AngularAccelerationDrillingQuantity.Instance.GetUnitChoice(AngularAccelerationDrillingQuantity.UnitChoicesEnum.DegreePerSecondSquared).ID.ToString());
                    Choices.Add(AngularVelocityDrillingQuantity.Instance.ID.ToString(), AngularVelocityDrillingQuantity.Instance.GetUnitChoice(AngularVelocityDrillingQuantity.UnitChoicesEnum.RevolutionPerMinute).ID.ToString());
                    Choices.Add(AreaDrillingQuantity.Instance.ID.ToString(), AreaDrillingQuantity.Instance.GetUnitChoice(AreaDrillingQuantity.UnitChoicesEnum.SquareFoot).ID.ToString());
                    Choices.Add(BendingMomentDrillingQuantity.Instance.ID.ToString(), BendingMomentDrillingQuantity.Instance.GetUnitChoice(BendingMomentDrillingQuantity.UnitChoicesEnum.FootPound).ID.ToString());
                    Choices.Add(TotalFlowAreaDrillingQuantity.Instance.ID.ToString(), TotalFlowAreaDrillingQuantity.Instance.GetUnitChoice(TotalFlowAreaDrillingQuantity.UnitChoicesEnum.SquareInch).ID.ToString());
                    Choices.Add(AxialVelocityDrillingQuantity.Instance.ID.ToString(), AxialVelocityDrillingQuantity.Instance.GetUnitChoice(AxialVelocityDrillingQuantity.UnitChoicesEnum.FootPerSecond).ID.ToString());
                    Choices.Add(BlockVelocityDrillingQuantity.Instance.ID.ToString(), BlockVelocityDrillingQuantity.Instance.GetUnitChoice(BlockVelocityDrillingQuantity.UnitChoicesEnum.FootPerSecond).ID.ToString());
                    Choices.Add(CableDiameterDrillingQuantity.Instance.ID.ToString(), CableDiameterDrillingQuantity.Instance.GetUnitChoice(CableDiameterDrillingQuantity.UnitChoicesEnum.Inch).ID.ToString());
                    Choices.Add(CapillaryPressureDrillingQuantity.Instance.ID.ToString(), CapillaryPressureDrillingQuantity.Instance.GetUnitChoice(CapillaryPressureDrillingQuantity.UnitChoicesEnum.PoundPerSquareInch).ID.ToString());
                    Choices.Add(CompressibilityDrillingQuantity.Instance.ID.ToString(), CompressibilityDrillingQuantity.Instance.GetUnitChoice(CompressibilityDrillingQuantity.UnitChoicesEnum.InversePoundPerSquareInch).ID.ToString());
                    Choices.Add(CurvatureDrillingQuantity.Instance.ID.ToString(), CurvatureDrillingQuantity.Instance.GetUnitChoice(CurvatureDrillingQuantity.UnitChoicesEnum.DegreePer100ft).ID.ToString());
                    Choices.Add(SpecificVolumeDrillingQuantity.Instance.ID.ToString(), SpecificVolumeDrillingQuantity.Instance.GetUnitChoice(SpecificVolumeDrillingQuantity.UnitChoicesEnum.GallonUSPerPound).ID.ToString());
                    Choices.Add(SpecificVolumeSquaredDrillingQuantity.Instance.ID.ToString(), SpecificVolumeSquaredDrillingQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredDrillingQuantity.UnitChoicesEnum.GallonUSSquaredPerPoundSquared).ID.ToString());
                    Choices.Add(MassDensityGradientPerPressureDrillingQuantity.Instance.ID.ToString(), MassDensityGradientPerPressureDrillingQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureDrillingQuantity.UnitChoicesEnum.PoundPerGallonUSPerPoundPerSquareInch).ID.ToString());
                    Choices.Add(MassDensityGradientPerPressureSquaredDrillingQuantity.Instance.ID.ToString(), MassDensityGradientPerPressureSquaredDrillingQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredDrillingQuantity.UnitChoicesEnum.PoundPerGallonUSPerPoundPerSquareInchSquared).ID.ToString());
                    Choices.Add(MassDensityGradientPerPressureSquaredTemperatureDrillingQuantity.Instance.ID.ToString(), MassDensityGradientPerPressureSquaredTemperatureDrillingQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureDrillingQuantity.UnitChoicesEnum.PoundPerGallonUSPerPoundPerSquareInchSquaredFahrenheit).ID.ToString());
                    Choices.Add(MassDensityGradientPerPressureTemperatureDrillingQuantity.Instance.ID.ToString(), MassDensityGradientPerPressureTemperatureDrillingQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureDrillingQuantity.UnitChoicesEnum.PoundPerGallonUSPerPoundPerSquareInchFahrenheit).ID.ToString());
                    Choices.Add(MassDensityDrillingQuantity.Instance.ID.ToString(), MassDensityDrillingQuantity.Instance.GetUnitChoice(MassDensityDrillingQuantity.UnitChoicesEnum.PoundPerGallonUS).ID.ToString());
                    Choices.Add(MassDensityGradientPerLengthDrillingQuantity.Instance.ID.ToString(), MassDensityGradientPerLengthDrillingQuantity.Instance.GetUnitChoice(MassDensityGradientPerLengthDrillingQuantity.UnitChoicesEnum.PoundPerGallonUSPer100Foot).ID.ToString());
                    Choices.Add(MassDensityGradientPerTemperatureDrillingQuantity.Instance.ID.ToString(), MassDensityGradientPerTemperatureDrillingQuantity.Instance.GetUnitChoice(MassDensityGradientPerTemperatureDrillingQuantity.UnitChoicesEnum.PoundPerGallonUSPerFahrenheit).ID.ToString());
                    Choices.Add(MassDensityRateOfChangeDrillingQuantity.Instance.ID.ToString(), MassDensityRateOfChangeDrillingQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeDrillingQuantity.UnitChoicesEnum.PoundPerGallonUSPerHour).ID.ToString());
                    Choices.Add(DepthDrillingQuantity.Instance.ID.ToString(), DepthDrillingQuantity.Instance.GetUnitChoice(DepthDrillingQuantity.UnitChoicesEnum.Foot).ID.ToString());
                    Choices.Add(DrillStringMagneticFluxDrillingQuantity.Instance.ID.ToString(), DrillStringMagneticFluxDrillingQuantity.Instance.GetUnitChoice(DrillStringMagneticFluxDrillingQuantity.UnitChoicesEnum.Microweber).ID.ToString());
                    Choices.Add(DrillStemMaterialStrengthDrillingQuantity.Instance.ID.ToString(), DrillStemMaterialStrengthDrillingQuantity.Instance.GetUnitChoice(DrillStemMaterialStrengthDrillingQuantity.UnitChoicesEnum.Psi).ID.ToString());
                    Choices.Add(DurationDrillingQuantity.Instance.ID.ToString(), DurationDrillingQuantity.Instance.GetUnitChoice(DurationDrillingQuantity.UnitChoicesEnum.Second).ID.ToString());
                    Choices.Add(DynamicViscosityDrillingQuantity.Instance.ID.ToString(), DynamicViscosityDrillingQuantity.Instance.GetUnitChoice(DynamicViscosityDrillingQuantity.UnitChoicesEnum.PascalSecond).ID.ToString());
                    Choices.Add(ElongationGradientPerLengthDrillingQuantity.Instance.ID.ToString(), ElongationGradientPerLengthDrillingQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthDrillingQuantity.UnitChoicesEnum.InchPerMile).ID.ToString()); ;
                    Choices.Add(EnergyDensityDrillingQuantity.Instance.ID.ToString(), EnergyDensityDrillingQuantity.Instance.GetUnitChoice(EnergyDensityDrillingQuantity.UnitChoicesEnum.JoulePerCubicFoot).ID.ToString()); ;
                    Choices.Add(FluidVelocityDrillingQuantity.Instance.ID.ToString(), FluidVelocityDrillingQuantity.Instance.GetUnitChoice(FluidVelocityDrillingQuantity.UnitChoicesEnum.FootPerSecond).ID.ToString());
                    Choices.Add(ForceDrillingQuantity.Instance.ID.ToString(), ForceDrillingQuantity.Instance.GetUnitChoice(ForceDrillingQuantity.UnitChoicesEnum.PoundForce).ID.ToString());
                    Choices.Add(ForceGradientPerLengthDrillingQuantity.Instance.ID.ToString(), ForceGradientPerLengthDrillingQuantity.Instance.GetUnitChoice(ForceGradientPerLengthDrillingQuantity.UnitChoicesEnum.PoundPer100Foot).ID.ToString());
                    Choices.Add(ForceRateOfChangeDrillingQuantity.Instance.ID.ToString(), ForceRateOfChangeDrillingQuantity.Instance.GetUnitChoice(ForceRateOfChangeDrillingQuantity.UnitChoicesEnum.PoundForcePerSecond).ID.ToString());
                    Choices.Add(FormationResistivityDrillingQuantity.Instance.ID.ToString(), FormationResistivityDrillingQuantity.Instance.GetUnitChoice(FormationResistivityDrillingQuantity.UnitChoicesEnum.OhmMetre).ID.ToString());
                    Choices.Add(FormationStrengthDrillingQuantity.Instance.ID.ToString(), FormationStrengthDrillingQuantity.Instance.GetUnitChoice(FormationStrengthDrillingQuantity.UnitChoicesEnum.Psi).ID.ToString());
                    Choices.Add(GammaRayIndexDrillingQuantity.Instance.ID.ToString(), GammaRayIndexDrillingQuantity.Instance.GetUnitChoice(GammaRayIndexDrillingQuantity.UnitChoicesEnum.Dimensionless).ID.ToString());
                    Choices.Add(GasShowDrillingQuantity.Instance.ID.ToString(), GasShowDrillingQuantity.Instance.GetUnitChoice(GasShowDrillingQuantity.UnitChoicesEnum.Percent).ID.ToString());
                    Choices.Add(GasVolumetricFlowRateDrillingQuantity.Instance.ID.ToString(), GasVolumetricFlowRateDrillingQuantity.Instance.GetUnitChoice(GasVolumetricFlowRateDrillingQuantity.UnitChoicesEnum.USGallonPerMinute).ID.ToString());
                    Choices.Add(HeatTransferCoefficientDrillingQuantity.Instance.ID.ToString(), HeatTransferCoefficientDrillingQuantity.Instance.GetUnitChoice(HeatTransferCoefficientDrillingQuantity.UnitChoicesEnum.BritishThermalUnitPerHourPerSquareFootPerDegreeFahrenheit).ID.ToString());
                    Choices.Add(HeightDrillingQuantity.Instance.ID.ToString(), HeightDrillingQuantity.Instance.GetUnitChoice(HeightDrillingQuantity.UnitChoicesEnum.Foot).ID.ToString());
                    Choices.Add(HookLoadDrillingQuantity.Instance.ID.ToString(), HookLoadDrillingQuantity.Instance.GetUnitChoice(HookLoadDrillingQuantity.UnitChoicesEnum.KilopoundForce).ID.ToString());
                    Choices.Add(HydraulicConductivityDrillingQuantity.Instance.ID.ToString(), HydraulicConductivityDrillingQuantity.Instance.GetUnitChoice(HydraulicConductivityDrillingQuantity.UnitChoicesEnum.InchPerSecond).ID.ToString());
                    Choices.Add(InterfacialTensionDrillingQuantity.Instance.ID.ToString(), InterfacialTensionDrillingQuantity.Instance.GetUnitChoice(InterfacialTensionDrillingQuantity.UnitChoicesEnum.PoundPerSecondSquared).ID.ToString());
                    Choices.Add(MassDrillingQuantity.Instance.ID.ToString(), MassDrillingQuantity.Instance.GetUnitChoice(MassDrillingQuantity.UnitChoicesEnum.Pound).ID.ToString());
                    Choices.Add(MassGradientPerLengthDrillingQuantity.Instance.ID.ToString(), MassGradientPerLengthDrillingQuantity.Instance.GetUnitChoice(MassGradientPerLengthDrillingQuantity.UnitChoicesEnum.PoundPerFoot).ID.ToString());
                    Choices.Add(MassRateDrillingQuantity.Instance.ID.ToString(), MassRateDrillingQuantity.Instance.GetUnitChoice(MassRateDrillingQuantity.UnitChoicesEnum.KilogramPerSecond).ID.ToString());
                    Choices.Add(MomentOfAreaDrillingQuantity.Instance.ID.ToString(), MomentOfAreaDrillingQuantity.Instance.GetUnitChoice(MomentOfAreaDrillingQuantity.UnitChoicesEnum.InchesToTheFourthPower).ID.ToString());
                    Choices.Add(MomentOfInertiaDrillingQuantity.Instance.ID.ToString(), MomentOfInertiaDrillingQuantity.Instance.GetUnitChoice(MomentOfInertiaDrillingQuantity.UnitChoicesEnum.PoundInchSquared).ID.ToString());
                    Choices.Add(NozzleDiameterDrillingQuantity.Instance.ID.ToString(), NozzleDiameterDrillingQuantity.Instance.GetUnitChoice(NozzleDiameterDrillingQuantity.UnitChoicesEnum.InchPer32).ID.ToString());
                    Choices.Add(PorousMediumPermeabilityDrillingQuantity.Instance.ID.ToString(), PorousMediumPermeabilityDrillingQuantity.Instance.GetUnitChoice(PorousMediumPermeabilityDrillingQuantity.UnitChoicesEnum.Millidarcy).ID.ToString());
                    Choices.Add(DiameterPipeDrillingQuantity.Instance.ID.ToString(), DiameterPipeDrillingQuantity.Instance.GetUnitChoice(DiameterPipeDrillingQuantity.UnitChoicesEnum.Inch).ID.ToString());
                    Choices.Add(PlaneAngleDrillingQuantity.Instance.ID.ToString(), PlaneAngleDrillingQuantity.Instance.GetUnitChoice(PlaneAngleDrillingQuantity.UnitChoicesEnum.Degree).ID.ToString());
                    Choices.Add(DiameterPoreDrillingQuantity.Instance.ID.ToString(), DiameterPoreDrillingQuantity.Instance.GetUnitChoice(DiameterPoreDrillingQuantity.UnitChoicesEnum.InchPer32).ID.ToString());
                    Choices.Add(SurfacePoreDrillingQuantity.Instance.ID.ToString(), SurfacePoreDrillingQuantity.Instance.GetUnitChoice(SurfacePoreDrillingQuantity.UnitChoicesEnum.SquareMicrometre).ID.ToString());
                    Choices.Add(PositionDrillingQuantity.Instance.ID.ToString(), PositionDrillingQuantity.Instance.GetUnitChoice(PositionDrillingQuantity.UnitChoicesEnum.Foot).ID.ToString());
                    Choices.Add(PowerDrillingQuantity.Instance.ID.ToString(), PowerDrillingQuantity.Instance.GetUnitChoice(PowerDrillingQuantity.UnitChoicesEnum.Watt).ID.ToString());
                    Choices.Add(PowerRateOfChangeDrillingQuantity.Instance.ID.ToString(), PowerRateOfChangeDrillingQuantity.Instance.GetUnitChoice(PowerRateOfChangeDrillingQuantity.UnitChoicesEnum.WattPerSecond).ID.ToString());
                    Choices.Add(ChokeOpeningRateDrillingQuantity.Instance.ID.ToString(), ChokeOpeningRateDrillingQuantity.Instance.GetUnitChoice(ChokeOpeningRateDrillingQuantity.UnitChoicesEnum.PercentPerSecond).ID.ToString());
                    Choices.Add(PressureDrillingQuantity.Instance.ID.ToString(), PressureDrillingQuantity.Instance.GetUnitChoice(PressureDrillingQuantity.UnitChoicesEnum.PoundPerSquareInch).ID.ToString());
                    Choices.Add(PressureGradientPerLengthDrillingQuantity.Instance.ID.ToString(), PressureGradientPerLengthDrillingQuantity.Instance.GetUnitChoice(PressureGradientPerLengthDrillingQuantity.UnitChoicesEnum.PsiPerFoot).ID.ToString());
                    Choices.Add(PressureLossConstantDrillingQuantity.Instance.ID.ToString(), PressureLossConstantDrillingQuantity.Instance.GetUnitChoice(PressureLossConstantDrillingQuantity.UnitChoicesEnum.PressureLossConstantUS).ID.ToString());
                    Choices.Add(PressureRateOfChangeDrillingQuantity.Instance.ID.ToString(), PressureRateOfChangeDrillingQuantity.Instance.GetUnitChoice(PressureRateOfChangeDrillingQuantity.UnitChoicesEnum.PoundPerSquareInchPerSecond).ID.ToString());
                    Choices.Add(RandomWalkDrillingQuantity.Instance.ID.ToString(), RandomWalkDrillingQuantity.Instance.GetUnitChoice(RandomWalkDrillingQuantity.UnitChoicesEnum.DegreePerSquareRootHour).ID.ToString());
                    Choices.Add(RateOfPenetrationDrillingQuantity.Instance.ID.ToString(), RateOfPenetrationDrillingQuantity.Instance.GetUnitChoice(RateOfPenetrationDrillingQuantity.UnitChoicesEnum.FootPerHour).ID.ToString());
                    Choices.Add(RotationalFrequencyRateOfChangeDrillingQuantity.Instance.ID.ToString(), RotationalFrequencyRateOfChangeDrillingQuantity.Instance.GetUnitChoice(RotationalFrequencyRateOfChangeDrillingQuantity.UnitChoicesEnum.RpmPerSecond).ID.ToString());
                    Choices.Add(IsobaricSpecificHeatCapacityDrillingQuantity.Instance.ID.ToString(), IsobaricSpecificHeatCapacityDrillingQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityDrillingQuantity.UnitChoicesEnum.BritishThermalUnitPerPoundDegreeFahrenheit).ID.ToString());
                    Choices.Add(IsobaricSpecificHeatCapacityGradientPerTemperatureDrillingQuantity.Instance.ID.ToString(), IsobaricSpecificHeatCapacityGradientPerTemperatureDrillingQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityGradientPerTemperatureDrillingQuantity.UnitChoicesEnum.BritishThermalUnitPerPoundSquaredDegreeFahrenheit).ID.ToString());
                    Choices.Add(StickDurationDrillingQuantity.Instance.ID.ToString(), StickDurationDrillingQuantity.Instance.GetUnitChoice(StickDurationDrillingQuantity.UnitChoicesEnum.Second).ID.ToString());
                    Choices.Add(AngularVelocitySurveyInstrumentDrillingQuantity.Instance.ID.ToString(), AngularVelocitySurveyInstrumentDrillingQuantity.Instance.GetUnitChoice(AngularVelocitySurveyInstrumentDrillingQuantity.UnitChoicesEnum.DegreePerHour).ID.ToString());
                    Choices.Add(AngleMagneticFluxDensitySurveyInstrumentDrillingQuantity.Instance.ID.ToString(), AngleMagneticFluxDensitySurveyInstrumentDrillingQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensitySurveyInstrumentDrillingQuantity.UnitChoicesEnum.DegreeNanotesla).ID.ToString());
                    Choices.Add(ReciprocalLengthSurveyInstrumentDrillingQuantity.Instance.ID.ToString(), ReciprocalLengthSurveyInstrumentDrillingQuantity.Instance.GetUnitChoice(ReciprocalLengthSurveyInstrumentDrillingQuantity.UnitChoicesEnum.ReciprocalKilometre).ID.ToString());
                    Choices.Add(TemperatureDrillingQuantity.Instance.ID.ToString(), TemperatureDrillingQuantity.Instance.GetUnitChoice(TemperatureDrillingQuantity.UnitChoicesEnum.Fahrenheit).ID.ToString());
                    Choices.Add(TemperatureGradientPerLengthDrillingQuantity.Instance.ID.ToString(), TemperatureGradientPerLengthDrillingQuantity.Instance.GetUnitChoice(TemperatureGradientPerLengthDrillingQuantity.UnitChoicesEnum.FahrenheitPer100Foot).ID.ToString());
                    Choices.Add(TensionDrillingQuantity.Instance.ID.ToString(), TensionDrillingQuantity.Instance.GetUnitChoice(TensionDrillingQuantity.UnitChoicesEnum.KilopoundForce).ID.ToString());
                    Choices.Add(ThermalConductivityDrillingQuantity.Instance.ID.ToString(), ThermalConductivityDrillingQuantity.Instance.GetUnitChoice(ThermalConductivityDrillingQuantity.UnitChoicesEnum.BritishThermalUnitPerHourFootDegreeFahrenheit).ID.ToString());
                    Choices.Add(ThermalConductivityGradientPerTemperatureDrillingQuantity.Instance.ID.ToString(), ThermalConductivityGradientPerTemperatureDrillingQuantity.Instance.GetUnitChoice(ThermalConductivityGradientPerTemperatureDrillingQuantity.UnitChoicesEnum.BritishThermalUnitPerHourFootDegreeFahrenheitSquared).ID.ToString());
                    Choices.Add(TorqueDrillingQuantity.Instance.ID.ToString(), TorqueDrillingQuantity.Instance.GetUnitChoice(TorqueDrillingQuantity.UnitChoicesEnum.FootPound).ID.ToString());
                    Choices.Add(TorqueGradientPerLengthDrillingQuantity.Instance.ID.ToString(), TorqueGradientPerLengthDrillingQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthDrillingQuantity.UnitChoicesEnum.FootPoundPerFoot).ID.ToString());
                    Choices.Add(TorqueRateOfChangeDrillingQuantity.Instance.ID.ToString(), TorqueRateOfChangeDrillingQuantity.Instance.GetUnitChoice(TorqueRateOfChangeDrillingQuantity.UnitChoicesEnum.FootPoundPerSecond).ID.ToString());
                    Choices.Add(VolumeDrillingQuantity.Instance.ID.ToString(), VolumeDrillingQuantity.Instance.GetUnitChoice(VolumeDrillingQuantity.UnitChoicesEnum.USGallon).ID.ToString());
                    Choices.Add(VolumetricFlowrateDrillingQuantity.Instance.ID.ToString(), VolumetricFlowrateDrillingQuantity.Instance.GetUnitChoice(VolumetricFlowrateDrillingQuantity.UnitChoicesEnum.USGallonPerMinute).ID.ToString());
                    Choices.Add(VolumetricFlowRateOfChangeDrillingQuantity.Instance.ID.ToString(), VolumetricFlowRateOfChangeDrillingQuantity.Instance.GetUnitChoice(VolumetricFlowRateOfChangeDrillingQuantity.UnitChoicesEnum.USGallonPerMinutePerSecond).ID.ToString());
                    Choices.Add(WeightOnBitDrillingQuantity.Instance.ID.ToString(), WeightOnBitDrillingQuantity.Instance.GetUnitChoice(WeightOnBitDrillingQuantity.UnitChoicesEnum.KilopoundForce).ID.ToString());
                }
                else if (defaultUnitChoice == BaseUnitSystem.DefaultUnitSystemEnum.Imperial)
                {
                    IsDefault = true;
                    ID = new Guid("67e6faf9-8d2f-4071-badb-f8d1355017a4");
                    Name = "Imperial";
                    Description = "United Kingdom System of Units";

                    Choices.Add(AccelerationDrillingQuantity.Instance.ID.ToString(), AccelerationDrillingQuantity.Instance.GetUnitChoice(AccelerationDrillingQuantity.UnitChoicesEnum.FootPerSecondSquared).ID.ToString());
                    Choices.Add(AngleGradientPerLengthDrillingQuantity.Instance.ID.ToString(), AngleGradientPerLengthDrillingQuantity.Instance.GetUnitChoice(AngleGradientPerLengthDrillingQuantity.UnitChoicesEnum.DegreePerFoot).ID.ToString());
                    Choices.Add(AngularAccelerationDrillingQuantity.Instance.ID.ToString(), AngularAccelerationDrillingQuantity.Instance.GetUnitChoice(AngularAccelerationDrillingQuantity.UnitChoicesEnum.DegreePerSecondSquared).ID.ToString());
                    Choices.Add(AngularVelocityDrillingQuantity.Instance.ID.ToString(), AngularVelocityDrillingQuantity.Instance.GetUnitChoice(AngularVelocityDrillingQuantity.UnitChoicesEnum.RevolutionPerMinute).ID.ToString());
                    Choices.Add(AreaDrillingQuantity.Instance.ID.ToString(), AreaDrillingQuantity.Instance.GetUnitChoice(AreaDrillingQuantity.UnitChoicesEnum.SquareFoot).ID.ToString());
                    Choices.Add(BendingMomentDrillingQuantity.Instance.ID.ToString(), BendingMomentDrillingQuantity.Instance.GetUnitChoice(BendingMomentDrillingQuantity.UnitChoicesEnum.FootPound).ID.ToString());
                    Choices.Add(TotalFlowAreaDrillingQuantity.Instance.ID.ToString(), TotalFlowAreaDrillingQuantity.Instance.GetUnitChoice(TotalFlowAreaDrillingQuantity.UnitChoicesEnum.SquareInch).ID.ToString());
                    Choices.Add(AxialVelocityDrillingQuantity.Instance.ID.ToString(), AxialVelocityDrillingQuantity.Instance.GetUnitChoice(AxialVelocityDrillingQuantity.UnitChoicesEnum.FootPerSecond).ID.ToString());
                    Choices.Add(BlockVelocityDrillingQuantity.Instance.ID.ToString(), BlockVelocityDrillingQuantity.Instance.GetUnitChoice(BlockVelocityDrillingQuantity.UnitChoicesEnum.FootPerSecond).ID.ToString());
                    Choices.Add(CableDiameterDrillingQuantity.Instance.ID.ToString(), CableDiameterDrillingQuantity.Instance.GetUnitChoice(CableDiameterDrillingQuantity.UnitChoicesEnum.Inch).ID.ToString());
                    Choices.Add(CapillaryPressureDrillingQuantity.Instance.ID.ToString(), CapillaryPressureDrillingQuantity.Instance.GetUnitChoice(CapillaryPressureDrillingQuantity.UnitChoicesEnum.PoundPerSquareInch).ID.ToString());
                    Choices.Add(CompressibilityDrillingQuantity.Instance.ID.ToString(), CompressibilityDrillingQuantity.Instance.GetUnitChoice(CompressibilityDrillingQuantity.UnitChoicesEnum.InversePoundPerSquareInch).ID.ToString());
                    Choices.Add(CurvatureDrillingQuantity.Instance.ID.ToString(), CurvatureDrillingQuantity.Instance.GetUnitChoice(CurvatureDrillingQuantity.UnitChoicesEnum.DegreePer100ft).ID.ToString());
                    Choices.Add(SpecificVolumeDrillingQuantity.Instance.ID.ToString(), SpecificVolumeDrillingQuantity.Instance.GetUnitChoice(SpecificVolumeDrillingQuantity.UnitChoicesEnum.GallonUKPerPound).ID.ToString());
                    Choices.Add(SpecificVolumeSquaredDrillingQuantity.Instance.ID.ToString(), SpecificVolumeSquaredDrillingQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredDrillingQuantity.UnitChoicesEnum.GallonUKSquaredPerPoundSquared).ID.ToString());
                    Choices.Add(MassDensityGradientPerPressureDrillingQuantity.Instance.ID.ToString(), MassDensityGradientPerPressureDrillingQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureDrillingQuantity.UnitChoicesEnum.PoundPerGallonUKPerPoundPerSquareInch).ID.ToString());
                    Choices.Add(MassDensityGradientPerPressureSquaredDrillingQuantity.Instance.ID.ToString(), MassDensityGradientPerPressureSquaredDrillingQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredDrillingQuantity.UnitChoicesEnum.PoundPerGallonUKPerPoundPerSquareInchSquared).ID.ToString());
                    Choices.Add(MassDensityGradientPerPressureSquaredTemperatureDrillingQuantity.Instance.ID.ToString(), MassDensityGradientPerPressureSquaredTemperatureDrillingQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureDrillingQuantity.UnitChoicesEnum.PoundPerGallonUKPerPoundPerSquareInchSquaredCelsius).ID.ToString());
                    Choices.Add(MassDensityGradientPerPressureTemperatureDrillingQuantity.Instance.ID.ToString(), MassDensityGradientPerPressureTemperatureDrillingQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureDrillingQuantity.UnitChoicesEnum.PoundPerGallonUKPerPoundPerSquareInchCelsius).ID.ToString());
                    Choices.Add(MassDensityDrillingQuantity.Instance.ID.ToString(), MassDensityDrillingQuantity.Instance.GetUnitChoice(MassDensityDrillingQuantity.UnitChoicesEnum.PoundPerGallonUK).ID.ToString());
                    Choices.Add(MassDensityGradientPerLengthDrillingQuantity.Instance.ID.ToString(), MassDensityGradientPerLengthDrillingQuantity.Instance.GetUnitChoice(MassDensityGradientPerLengthDrillingQuantity.UnitChoicesEnum.PoundPerGallonUKPer100Foot).ID.ToString());
                    Choices.Add(MassDensityGradientPerTemperatureDrillingQuantity.Instance.ID.ToString(), MassDensityGradientPerTemperatureDrillingQuantity.Instance.GetUnitChoice(MassDensityGradientPerTemperatureDrillingQuantity.UnitChoicesEnum.PoundPerGallonUKPerCelsius).ID.ToString());
                    Choices.Add(MassDensityRateOfChangeDrillingQuantity.Instance.ID.ToString(), MassDensityRateOfChangeDrillingQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeDrillingQuantity.UnitChoicesEnum.PoundPerGallonUKPerHour).ID.ToString());
                    Choices.Add(DepthDrillingQuantity.Instance.ID.ToString(), DepthDrillingQuantity.Instance.GetUnitChoice(DepthDrillingQuantity.UnitChoicesEnum.Foot).ID.ToString());
                    Choices.Add(DrillStringMagneticFluxDrillingQuantity.Instance.ID.ToString(), DrillStringMagneticFluxDrillingQuantity.Instance.GetUnitChoice(DrillStringMagneticFluxDrillingQuantity.UnitChoicesEnum.Microweber).ID.ToString());
                    Choices.Add(DrillStemMaterialStrengthDrillingQuantity.Instance.ID.ToString(), DrillStemMaterialStrengthDrillingQuantity.Instance.GetUnitChoice(DrillStemMaterialStrengthDrillingQuantity.UnitChoicesEnum.Psi).ID.ToString());
                    Choices.Add(DurationDrillingQuantity.Instance.ID.ToString(), DurationDrillingQuantity.Instance.GetUnitChoice(DurationDrillingQuantity.UnitChoicesEnum.Second).ID.ToString());
                    Choices.Add(DynamicViscosityDrillingQuantity.Instance.ID.ToString(), DynamicViscosityDrillingQuantity.Instance.GetUnitChoice(DynamicViscosityDrillingQuantity.UnitChoicesEnum.PascalSecond).ID.ToString());
                    Choices.Add(ElongationGradientPerLengthDrillingQuantity.Instance.ID.ToString(), ElongationGradientPerLengthDrillingQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthDrillingQuantity.UnitChoicesEnum.InchPerMile).ID.ToString()); ;
                    Choices.Add(EnergyDensityDrillingQuantity.Instance.ID.ToString(), EnergyDensityDrillingQuantity.Instance.GetUnitChoice(EnergyDensityDrillingQuantity.UnitChoicesEnum.JoulePerCubicFoot).ID.ToString()); ;
                    Choices.Add(FluidVelocityDrillingQuantity.Instance.ID.ToString(), FluidVelocityDrillingQuantity.Instance.GetUnitChoice(FluidVelocityDrillingQuantity.UnitChoicesEnum.FootPerSecond).ID.ToString());
                    Choices.Add(ForceDrillingQuantity.Instance.ID.ToString(), ForceDrillingQuantity.Instance.GetUnitChoice(ForceDrillingQuantity.UnitChoicesEnum.PoundForce).ID.ToString());
                    Choices.Add(ForceGradientPerLengthDrillingQuantity.Instance.ID.ToString(), ForceGradientPerLengthDrillingQuantity.Instance.GetUnitChoice(ForceGradientPerLengthDrillingQuantity.UnitChoicesEnum.PoundPer100Foot).ID.ToString());
                    Choices.Add(ForceRateOfChangeDrillingQuantity.Instance.ID.ToString(), ForceRateOfChangeDrillingQuantity.Instance.GetUnitChoice(ForceRateOfChangeDrillingQuantity.UnitChoicesEnum.PoundForcePerSecond).ID.ToString());
                    Choices.Add(FormationResistivityDrillingQuantity.Instance.ID.ToString(), FormationResistivityDrillingQuantity.Instance.GetUnitChoice(FormationResistivityDrillingQuantity.UnitChoicesEnum.OhmMetre).ID.ToString());
                    Choices.Add(FormationStrengthDrillingQuantity.Instance.ID.ToString(), FormationStrengthDrillingQuantity.Instance.GetUnitChoice(FormationStrengthDrillingQuantity.UnitChoicesEnum.Psi).ID.ToString());
                    Choices.Add(GammaRayIndexDrillingQuantity.Instance.ID.ToString(), GammaRayIndexDrillingQuantity.Instance.GetUnitChoice(GammaRayIndexDrillingQuantity.UnitChoicesEnum.Dimensionless).ID.ToString());
                    Choices.Add(GasShowDrillingQuantity.Instance.ID.ToString(), GasShowDrillingQuantity.Instance.GetUnitChoice(GasShowDrillingQuantity.UnitChoicesEnum.Percent).ID.ToString());
                    Choices.Add(GasVolumetricFlowRateDrillingQuantity.Instance.ID.ToString(), GasVolumetricFlowRateDrillingQuantity.Instance.GetUnitChoice(GasVolumetricFlowRateDrillingQuantity.UnitChoicesEnum.UKGallonPerMinute).ID.ToString());
                    Choices.Add(HeatTransferCoefficientDrillingQuantity.Instance.ID.ToString(), HeatTransferCoefficientDrillingQuantity.Instance.GetUnitChoice(HeatTransferCoefficientDrillingQuantity.UnitChoicesEnum.BritishThermalUnitPerHourPerSquareFootPerDegreeFahrenheit).ID.ToString());
                    Choices.Add(HeightDrillingQuantity.Instance.ID.ToString(), HeightDrillingQuantity.Instance.GetUnitChoice(HeightDrillingQuantity.UnitChoicesEnum.Foot).ID.ToString());
                    Choices.Add(HookLoadDrillingQuantity.Instance.ID.ToString(), HookLoadDrillingQuantity.Instance.GetUnitChoice(HookLoadDrillingQuantity.UnitChoicesEnum.KilopoundForce).ID.ToString());
                    Choices.Add(HydraulicConductivityDrillingQuantity.Instance.ID.ToString(), HydraulicConductivityDrillingQuantity.Instance.GetUnitChoice(HydraulicConductivityDrillingQuantity.UnitChoicesEnum.InchPerSecond).ID.ToString());
                    Choices.Add(InterfacialTensionDrillingQuantity.Instance.ID.ToString(), InterfacialTensionDrillingQuantity.Instance.GetUnitChoice(InterfacialTensionDrillingQuantity.UnitChoicesEnum.PoundPerSecondSquared).ID.ToString());
                    Choices.Add(MassDrillingQuantity.Instance.ID.ToString(), MassDrillingQuantity.Instance.GetUnitChoice(MassDrillingQuantity.UnitChoicesEnum.Pound).ID.ToString());
                    Choices.Add(MassGradientPerLengthDrillingQuantity.Instance.ID.ToString(), MassGradientPerLengthDrillingQuantity.Instance.GetUnitChoice(MassGradientPerLengthDrillingQuantity.UnitChoicesEnum.PoundPerFoot).ID.ToString());
                    Choices.Add(MassRateDrillingQuantity.Instance.ID.ToString(), MassRateDrillingQuantity.Instance.GetUnitChoice(MassRateDrillingQuantity.UnitChoicesEnum.KilogramPerSecond).ID.ToString());
                    Choices.Add(MomentOfAreaDrillingQuantity.Instance.ID.ToString(), MomentOfAreaDrillingQuantity.Instance.GetUnitChoice(MomentOfAreaDrillingQuantity.UnitChoicesEnum.InchesToTheFourthPower).ID.ToString());
                    Choices.Add(MomentOfInertiaDrillingQuantity.Instance.ID.ToString(), MomentOfInertiaDrillingQuantity.Instance.GetUnitChoice(MomentOfInertiaDrillingQuantity.UnitChoicesEnum.PoundInchSquared).ID.ToString());
                    Choices.Add(NozzleDiameterDrillingQuantity.Instance.ID.ToString(), NozzleDiameterDrillingQuantity.Instance.GetUnitChoice(NozzleDiameterDrillingQuantity.UnitChoicesEnum.InchPer32).ID.ToString());
                    Choices.Add(PorousMediumPermeabilityDrillingQuantity.Instance.ID.ToString(), PorousMediumPermeabilityDrillingQuantity.Instance.GetUnitChoice(PorousMediumPermeabilityDrillingQuantity.UnitChoicesEnum.Millidarcy).ID.ToString());
                    Choices.Add(DiameterPipeDrillingQuantity.Instance.ID.ToString(), DiameterPipeDrillingQuantity.Instance.GetUnitChoice(DiameterPipeDrillingQuantity.UnitChoicesEnum.Inch).ID.ToString());
                    Choices.Add(PlaneAngleDrillingQuantity.Instance.ID.ToString(), PlaneAngleDrillingQuantity.Instance.GetUnitChoice(PlaneAngleDrillingQuantity.UnitChoicesEnum.Degree).ID.ToString());
                    Choices.Add(DiameterPoreDrillingQuantity.Instance.ID.ToString(), DiameterPoreDrillingQuantity.Instance.GetUnitChoice(DiameterPoreDrillingQuantity.UnitChoicesEnum.InchPer32).ID.ToString());
                    Choices.Add(SurfacePoreDrillingQuantity.Instance.ID.ToString(), SurfacePoreDrillingQuantity.Instance.GetUnitChoice(SurfacePoreDrillingQuantity.UnitChoicesEnum.SquareMicrometre).ID.ToString());
                    Choices.Add(PositionDrillingQuantity.Instance.ID.ToString(), PositionDrillingQuantity.Instance.GetUnitChoice(PositionDrillingQuantity.UnitChoicesEnum.Foot).ID.ToString());
                    Choices.Add(PowerDrillingQuantity.Instance.ID.ToString(), PowerDrillingQuantity.Instance.GetUnitChoice(PowerDrillingQuantity.UnitChoicesEnum.Watt).ID.ToString());
                    Choices.Add(PowerRateOfChangeDrillingQuantity.Instance.ID.ToString(), PowerRateOfChangeDrillingQuantity.Instance.GetUnitChoice(PowerRateOfChangeDrillingQuantity.UnitChoicesEnum.WattPerSecond).ID.ToString());
                    Choices.Add(ChokeOpeningRateDrillingQuantity.Instance.ID.ToString(), ChokeOpeningRateDrillingQuantity.Instance.GetUnitChoice(ChokeOpeningRateDrillingQuantity.UnitChoicesEnum.PercentPerSecond).ID.ToString());
                    Choices.Add(PressureDrillingQuantity.Instance.ID.ToString(), PressureDrillingQuantity.Instance.GetUnitChoice(PressureDrillingQuantity.UnitChoicesEnum.PoundPerSquareInch).ID.ToString());
                    Choices.Add(PressureGradientPerLengthDrillingQuantity.Instance.ID.ToString(), PressureGradientPerLengthDrillingQuantity.Instance.GetUnitChoice(PressureGradientPerLengthDrillingQuantity.UnitChoicesEnum.PsiPerFoot).ID.ToString());
                    Choices.Add(PressureLossConstantDrillingQuantity.Instance.ID.ToString(), PressureLossConstantDrillingQuantity.Instance.GetUnitChoice(PressureLossConstantDrillingQuantity.UnitChoicesEnum.PressureLossConstantUK).ID.ToString());
                    Choices.Add(PressureRateOfChangeDrillingQuantity.Instance.ID.ToString(), PressureRateOfChangeDrillingQuantity.Instance.GetUnitChoice(PressureRateOfChangeDrillingQuantity.UnitChoicesEnum.PoundPerSquareInchPerSecond).ID.ToString());
                    Choices.Add(RandomWalkDrillingQuantity.Instance.ID.ToString(), RandomWalkDrillingQuantity.Instance.GetUnitChoice(RandomWalkDrillingQuantity.UnitChoicesEnum.DegreePerSquareRootHour).ID.ToString());
                    Choices.Add(RateOfPenetrationDrillingQuantity.Instance.ID.ToString(), RateOfPenetrationDrillingQuantity.Instance.GetUnitChoice(RateOfPenetrationDrillingQuantity.UnitChoicesEnum.FootPerHour).ID.ToString());
                    Choices.Add(RotationalFrequencyRateOfChangeDrillingQuantity.Instance.ID.ToString(), RotationalFrequencyRateOfChangeDrillingQuantity.Instance.GetUnitChoice(RotationalFrequencyRateOfChangeDrillingQuantity.UnitChoicesEnum.RpmPerSecond).ID.ToString());
                    Choices.Add(IsobaricSpecificHeatCapacityDrillingQuantity.Instance.ID.ToString(), IsobaricSpecificHeatCapacityDrillingQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityDrillingQuantity.UnitChoicesEnum.BritishThermalUnitPerPoundDegreeFahrenheit).ID.ToString());
                    Choices.Add(IsobaricSpecificHeatCapacityGradientPerTemperatureDrillingQuantity.Instance.ID.ToString(), IsobaricSpecificHeatCapacityGradientPerTemperatureDrillingQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityGradientPerTemperatureDrillingQuantity.UnitChoicesEnum.BritishThermalUnitPerPoundSquaredDegreeFahrenheit).ID.ToString());
                    Choices.Add(StickDurationDrillingQuantity.Instance.ID.ToString(), StickDurationDrillingQuantity.Instance.GetUnitChoice(StickDurationDrillingQuantity.UnitChoicesEnum.Second).ID.ToString());
                    Choices.Add(AngularVelocitySurveyInstrumentDrillingQuantity.Instance.ID.ToString(), AngularVelocitySurveyInstrumentDrillingQuantity.Instance.GetUnitChoice(AngularVelocitySurveyInstrumentDrillingQuantity.UnitChoicesEnum.DegreePerHour).ID.ToString());
                    Choices.Add(AngleMagneticFluxDensitySurveyInstrumentDrillingQuantity.Instance.ID.ToString(), AngleMagneticFluxDensitySurveyInstrumentDrillingQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensitySurveyInstrumentDrillingQuantity.UnitChoicesEnum.DegreeNanotesla).ID.ToString());
                    Choices.Add(ReciprocalLengthSurveyInstrumentDrillingQuantity.Instance.ID.ToString(), ReciprocalLengthSurveyInstrumentDrillingQuantity.Instance.GetUnitChoice(ReciprocalLengthSurveyInstrumentDrillingQuantity.UnitChoicesEnum.ReciprocalKilometre).ID.ToString());
                    Choices.Add(TemperatureDrillingQuantity.Instance.ID.ToString(), TemperatureDrillingQuantity.Instance.GetUnitChoice(TemperatureDrillingQuantity.UnitChoicesEnum.Fahrenheit).ID.ToString());
                    Choices.Add(TemperatureGradientPerLengthDrillingQuantity.Instance.ID.ToString(), TemperatureGradientPerLengthDrillingQuantity.Instance.GetUnitChoice(TemperatureGradientPerLengthDrillingQuantity.UnitChoicesEnum.CelsiusPer100Foot).ID.ToString());
                    Choices.Add(TensionDrillingQuantity.Instance.ID.ToString(), TensionDrillingQuantity.Instance.GetUnitChoice(TensionDrillingQuantity.UnitChoicesEnum.KilopoundForce).ID.ToString());
                    Choices.Add(ThermalConductivityDrillingQuantity.Instance.ID.ToString(), ThermalConductivityDrillingQuantity.Instance.GetUnitChoice(ThermalConductivityDrillingQuantity.UnitChoicesEnum.BritishThermalUnitPerHourFootDegreeFahrenheit).ID.ToString());
                    Choices.Add(ThermalConductivityGradientPerTemperatureDrillingQuantity.Instance.ID.ToString(), ThermalConductivityGradientPerTemperatureDrillingQuantity.Instance.GetUnitChoice(ThermalConductivityGradientPerTemperatureDrillingQuantity.UnitChoicesEnum.BritishThermalUnitPerHourFootDegreeFahrenheitSquared).ID.ToString());
                    Choices.Add(TorqueDrillingQuantity.Instance.ID.ToString(), TorqueDrillingQuantity.Instance.GetUnitChoice(TorqueDrillingQuantity.UnitChoicesEnum.FootPound).ID.ToString());
                    Choices.Add(TorqueGradientPerLengthDrillingQuantity.Instance.ID.ToString(), TorqueGradientPerLengthDrillingQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthDrillingQuantity.UnitChoicesEnum.FootPoundPerFoot).ID.ToString());
                    Choices.Add(TorqueRateOfChangeDrillingQuantity.Instance.ID.ToString(), TorqueRateOfChangeDrillingQuantity.Instance.GetUnitChoice(TorqueRateOfChangeDrillingQuantity.UnitChoicesEnum.FootPoundPerSecond).ID.ToString());
                    Choices.Add(VolumeDrillingQuantity.Instance.ID.ToString(), VolumeDrillingQuantity.Instance.GetUnitChoice(VolumeDrillingQuantity.UnitChoicesEnum.UKGallon).ID.ToString());
                    Choices.Add(VolumetricFlowrateDrillingQuantity.Instance.ID.ToString(), VolumetricFlowrateDrillingQuantity.Instance.GetUnitChoice(VolumetricFlowrateDrillingQuantity.UnitChoicesEnum.UKGallonPerMinute).ID.ToString());
                    Choices.Add(VolumetricFlowRateOfChangeDrillingQuantity.Instance.ID.ToString(), VolumetricFlowRateOfChangeDrillingQuantity.Instance.GetUnitChoice(VolumetricFlowRateOfChangeDrillingQuantity.UnitChoicesEnum.UKGallonPerMinutePerSecond).ID.ToString());
                    Choices.Add(WeightOnBitDrillingQuantity.Instance.ID.ToString(), WeightOnBitDrillingQuantity.Instance.GetUnitChoice(WeightOnBitDrillingQuantity.UnitChoicesEnum.KilopoundForce).ID.ToString());
                }
                AddInheritedChoices(quantities);
                CheckMissing(quantities);
            }
        }

        /// <summary>
        /// A drilling specialization normally reuses the unit choices of its base
        /// physical quantity and only adds drilling semantics and precision. Reuse
        /// the selected base-system choice when no deliberate drilling override was
        /// specified above.
        /// </summary>
        private void AddInheritedChoices(IEnumerable<BasePhysicalQuantity> quantities)
        {
            foreach (BasePhysicalQuantity quantity in quantities)
            {
                if (Choices.ContainsKey(quantity.ID.ToString()))
                {
                    continue;
                }

                Type baseType = quantity.GetType().BaseType;
                while (baseType != null && typeof(BasePhysicalQuantity).IsAssignableFrom(baseType))
                {
                    PropertyInfo instanceProperty = baseType.GetProperty(
                        "Instance",
                        BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
                    if (instanceProperty?.GetValue(null) is BasePhysicalQuantity baseQuantity)
                    {
                        UnitChoice inheritedChoice = GetChoice(baseQuantity.ID);
                        if (inheritedChoice != null && quantity.GetUnitChoice(inheritedChoice.ID) != null)
                        {
                            Choices.Add(quantity.ID.ToString(), inheritedChoice.ID.ToString());
                            break;
                        }
                    }
                    baseType = baseType.BaseType;
                }
            }
        }


        public static new BaseUnitSystem Get(Guid ID)
        {
            if (unitSystemDict_ == null)
            {
                Initialize();
            }
            DrillingUnitSystem result;
            unitSystemDict_.TryGetValue(ID, out result);
            if (result == null)
            {
                return BaseUnitSystem.Get(ID);
            }
            else
            {
                return result;
            }
        }

        public static new DrillingUnitSystem CreateNew()
        {
            if (unitSystemDict_ == null)
            {
                Initialize();
            }
            DrillingUnitSystem unitSystem = new DrillingUnitSystem();
            unitSystem.ID = Guid.NewGuid();
            unitSystemDict_.Add(unitSystem.ID, unitSystem);
            return unitSystem;
        }

        public static new DrillingUnitSystem CreateNew(Guid guid)
        {
            if (unitSystemDict_ == null)
            {
                Initialize();
            }
            if (unitSystemDict_.ContainsKey(guid))
            {
                DrillingUnitSystem result;
                unitSystemDict_.TryGetValue(guid, out result);
                return result;
            }
            else
            {
                DrillingUnitSystem unitSystem = new DrillingUnitSystem();
                unitSystem.ID = guid;
                unitSystemDict_.Add(unitSystem.ID, unitSystem);
                return unitSystem;
            }
        }

        /// <summary>
        /// Adds the given UnitSystem to the static Dictionary of available UnitSystems
        /// </summary>
        /// <param name="unitSystem"></param>
        /// <returns>Returns true is effectively added to the static Dictionary, false otherwise</returns>
        public static bool Add(DrillingUnitSystem unitSystem)
        {
            if (unitSystemDict_ == null)
            {
                Initialize();
            }
            if (!unitSystemDict_.ContainsKey(unitSystem.ID))
            {
                unitSystemDict_.Add(unitSystem.ID, unitSystem);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Updates the given UnitSystem in the static Dictionary of available UnitSystems
        /// </summary>
        /// <param name="unitSystem"></param>
        /// <returns>Returns true is effectively updated in the static Dictionary, false otherwise</returns>
        public static bool Update(DrillingUnitSystem unitSystem)
        {
            if (unitSystemDict_ == null)
            {
                Initialize();
            }
            if (unitSystemDict_.ContainsKey(unitSystem.ID))
            {
                unitSystemDict_[unitSystem.ID] = unitSystem;
                return true;
            }
            return false;
        }

        private static void Initialize()
        {
            if (unitSystemDict_ == null)
            {
                unitSystemDict_ = new Dictionary<Guid, DrillingUnitSystem>();
                DrillingUnitSystem SI = SIUnitSystem;
                unitSystemDict_.Add(SI.ID, SI);
                DrillingUnitSystem metric = MetricUnitSystem;
                unitSystemDict_.Add(metric.ID, metric);
                DrillingUnitSystem US = USUnitSystem;
                unitSystemDict_.Add(US.ID, US);
                DrillingUnitSystem imperial = ImperialUnitSystem;
                unitSystemDict_.Add(imperial.ID, imperial);
            }
        }

        public static DrillingUnitSystem SIUnitSystem
        {
            get
            {
                if (SIUnitSystem_ == null)
                {
                    SIUnitSystem_ = new DrillingUnitSystem(BaseUnitSystem.DefaultUnitSystemEnum.SI);
                }
                return SIUnitSystem_;
            }
        }
        public static DrillingUnitSystem MetricUnitSystem
        {
            get
            {
                if (MetricUnitSystem_ == null)
                {
                    MetricUnitSystem_ = new DrillingUnitSystem(BaseUnitSystem.DefaultUnitSystemEnum.Metric);
                }
                return MetricUnitSystem_;
            }
        }
        public static DrillingUnitSystem ImperialUnitSystem
        {
            get
            {
                if (ImperialUnitSystem_ == null)
                {
                    ImperialUnitSystem_ = new DrillingUnitSystem(BaseUnitSystem.DefaultUnitSystemEnum.Imperial);
                }
                return ImperialUnitSystem_;
            }
        }
        public static DrillingUnitSystem USUnitSystem
        {
            get
            {
                if (USUnitSystem_ == null)
                {
                    USUnitSystem_ = new DrillingUnitSystem(BaseUnitSystem.DefaultUnitSystemEnum.US);
                }
                return USUnitSystem_;
            }
        }

        public static List<BasePhysicalQuantity>? AvailableQuantities
        {
            get
            {
                if (availablePhysicalQuantities_ == null)
                {
                    Assembly? assembly = Assembly.GetAssembly(typeof(DrillingPhysicalQuantity));
                    if (assembly != null)
                    {
                        foreach (Type typ in assembly.GetTypes())
                        {
                            if (typ.IsSubclassOf(typeof(BasePhysicalQuantity)))
                            {
                                MethodInfo? method = null;
                                foreach (MethodInfo meth in typ.GetMethods())
                                {
                                    if (meth.IsStatic &&
                                        meth.Name.EndsWith("Instance") &&
                                        meth.ReturnType.IsSubclassOf(typeof(BasePhysicalQuantity)))
                                    {
                                        method = meth;
                                        break;
                                    }
                                }
                                // call the method
                                if (method != null)
                                {
                                    object? obj = null;
                                    try
                                    {
                                        obj = method.Invoke(null, null);
                                    } catch (Exception e)
                                    {

                                    }
                                    if (obj != null)
                                    {
                                        var res = (BasePhysicalQuantity)obj;
                                        if (availablePhysicalQuantities_ == null)
                                        {
                                            availablePhysicalQuantities_ = new List<BasePhysicalQuantity>();
                                        }
                                        availablePhysicalQuantities_.Add(res);
                                    }
                                }
                            }
                        }
                    }
                }
                return availablePhysicalQuantities_;
            }
        }

        public double? FromSI(DrillingPhysicalQuantity.QuantityEnum physicalQuantityID, double val)
        {
            UnitChoice choice = GetChoice(physicalQuantityID);
            if (choice != null)
            {
                return choice.FromSI(val);
            }
            else
            {
                return null;
            }
        }
        public string FromSIString(DrillingPhysicalQuantity.QuantityEnum physicalQuantityID, double val)
        {
            double? meaningfulPrecision = null;
            BasePhysicalQuantity quantity = DrillingPhysicalQuantity.GetQuantity(physicalQuantityID);
            meaningfulPrecision = quantity.MeaningfulPrecisionInSI;
            UnitChoice choice = GetChoice(physicalQuantityID);
            if (choice != null)
            {
                return choice.FromSI(val, meaningfulPrecision);
            }
            else
            {
                return null;
            }
        }
        public double? ToSI(DrillingPhysicalQuantity.QuantityEnum physicalQuantityID, double val)
        {
            UnitChoice choice = GetChoice(physicalQuantityID);
            if (choice != null)
            {
                return choice.ToSI(val);
            }
            else
            {
                return null;
            }
        }

        public UnitChoice GetChoice(DrillingPhysicalQuantity.QuantityEnum physicalQuantityID)
        {
            UnitChoice choice = null;
            string unitChoiceID;
            BasePhysicalQuantity quantity = GetQuantity(physicalQuantityID);
            if (quantity != null && Choices.TryGetValue(quantity.ID.ToString(), out unitChoiceID))
            {
                Guid ID = StringToGUID(unitChoiceID);
                if (ID != Guid.Empty)
                {
                    choice = quantity.GetUnitChoice(ID);
                }
            }
            return choice;
        }
        public string GetUnitLabel(DrillingPhysicalQuantity.QuantityEnum physicalQuantityID)
        {
            UnitChoice choice = GetChoice(physicalQuantityID);
            if (choice != null)
            {
                return choice.UnitLabel;
            }
            else
            {
                return null;
            }
        }

        protected BasePhysicalQuantity GetQuantity(DrillingPhysicalQuantity.QuantityEnum quantityChoice)
        {
            return DrillingPhysicalQuantity.GetQuantity(quantityChoice);
        }
        protected override BasePhysicalQuantity GetQuantity(Guid quantityID)
        {
            BasePhysicalQuantity quantity = DrillingPhysicalQuantity.GetQuantity(quantityID);
            if (quantity == null)
            {
                quantity = base.GetQuantity(quantityID);
            }
            return quantity;
        }
        protected override BasePhysicalQuantity GetQuantity(string quantityName)
        {
            BasePhysicalQuantity quantity = DrillingPhysicalQuantity.GetQuantity(quantityName);
            if (quantity == null)
            {
                quantity = base.GetQuantity(quantityName);
            }
            return quantity;
        }

    }
}
