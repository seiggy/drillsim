using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using System.Globalization;

namespace OSDC.UnitConversion.ModelTest
{
    public class Tests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
        }

        [Test]
        public void Test_General_BasePhysicalQuantity()
        {
            #region availability of BasePhysicalQuantities
            List<BasePhysicalQuantity> quantityList = BasePhysicalQuantity.AvailableBasePhysicalQuantities;
            Assert.That(quantityList, Is.Not.Null);
            #endregion

            #region uniqueness of BasePhysicalQuantities
            Assert.That(quantityList.Count, Is.EqualTo(Enum.GetValues<BasePhysicalQuantity.QuantityEnum>().Length));
            Assert.That(quantityList, Does.Contain(AccelerationQuantity.Instance));
            Assert.That(quantityList, Does.Contain(AmountSubstanceQuantity.Instance));
            Assert.That(quantityList, Does.Contain(AngleMagneticFluxDensityQuantity.Instance));
            Assert.That(quantityList, Does.Contain(AngleGradientPerLengthQuantity.Instance));
            Assert.That(quantityList, Does.Contain(AngularVelocityQuantity.Instance));
            Assert.That(quantityList, Does.Contain(AreaQuantity.Instance));
            Assert.That(quantityList, Does.Contain(CompressibilityQuantity.Instance));
            Assert.That(quantityList, Does.Contain(CurvatureQuantity.Instance));
            Assert.That(quantityList, Does.Contain(MassDensityGradientPerLengthQuantity.Instance));
            Assert.That(quantityList, Does.Contain(MassDensityGradientPerTemperatureQuantity.Instance));
            Assert.That(quantityList, Does.Contain(MassDensityQuantity.Instance));
            Assert.That(quantityList, Does.Contain(MassDensityRateOfChangeQuantity.Instance));
            Assert.That(quantityList, Does.Contain(DimensionlessQuantity.Instance));
            Assert.That(quantityList, Does.Contain(DynamicViscosityQuantity.Instance));
            Assert.That(quantityList, Does.Contain(EarthMagneticFluxDensityQuantity.Instance));
            Assert.That(quantityList, Does.Contain(ElectricCapacitanceQuantity.Instance));
            Assert.That(quantityList, Does.Contain(ElectricCurrentQuantity.Instance));
            Assert.That(quantityList, Does.Contain(ElectricTensionQuantity.Instance));
            Assert.That(quantityList, Does.Contain(ElongationGradientPerLengthQuantity.Instance));
            Assert.That(quantityList, Does.Contain(EnergyQuantity.Instance));
            Assert.That(quantityList, Does.Contain(FluidShearRateQuantity.Instance));
            Assert.That(quantityList, Does.Contain(FluidShearStressQuantity.Instance));
            Assert.That(quantityList, Does.Contain(ForceGradientPerLengthQuantity.Instance));
            Assert.That(quantityList, Does.Contain(ForceQuantity.Instance));
            Assert.That(quantityList, Does.Contain(FrequencyQuantity.Instance));
            Assert.That(quantityList, Does.Contain(FrequencyRateOfChangeQuantity.Instance));
            Assert.That(quantityList, Does.Contain(HeatTransferCoefficientQuantity.Instance));
            Assert.That(quantityList, Does.Contain(HydraulicConductivityQuantity.Instance));
            Assert.That(quantityList, Does.Contain(ImageScaleQuantity.Instance));
            Assert.That(quantityList, Does.Contain(InterfacialTensionQuantity.Instance));
            Assert.That(quantityList, Does.Contain(VolumeLargeQuantity.Instance));
            Assert.That(quantityList, Does.Contain(LengthQuantity.Instance));
            Assert.That(quantityList, Does.Contain(LuminousIntensityQuantity.Instance));
            Assert.That(quantityList, Does.Contain(MagneticFluxDensityQuantity.Instance));
            Assert.That(quantityList, Does.Contain(MagneticFluxQuantity.Instance));
            Assert.That(quantityList, Does.Contain(MassGradientPerLengthQuantity.Instance));
            Assert.That(quantityList, Does.Contain(MassQuantity.Instance));
            Assert.That(quantityList, Does.Contain(MassRateQuantity.Instance));
            Assert.That(quantityList, Does.Contain(MaterialStrengthQuantity.Instance));
            Assert.That(quantityList, Does.Contain(MomentOfAreaQuantity.Instance));
            Assert.That(quantityList, Does.Contain(MomentOfInertiaQuantity.Instance));
            Assert.That(quantityList, Does.Contain(PorousMediumPermeabilityQuantity.Instance));
            Assert.That(quantityList, Does.Contain(PlaneAngleQuantity.Instance));
            Assert.That(quantityList, Does.Contain(PorosityQuantity.Instance));
            Assert.That(quantityList, Does.Contain(PowerQuantity.Instance));
            Assert.That(quantityList, Does.Contain(PressureGradientPerLengthQuantity.Instance));
            Assert.That(quantityList, Does.Contain(PressureLossConstantQuantity.Instance));
            Assert.That(quantityList, Does.Contain(PressureQuantity.Instance));
            Assert.That(quantityList, Does.Contain(ProportionQuantity.Instance));
            Assert.That(quantityList, Does.Contain(RandomWalkQuantity.Instance));
            Assert.That(quantityList, Does.Contain(RelativeTemperatureQuantity.Instance));
            Assert.That(quantityList, Does.Contain(ElectricResistivityQuantity.Instance));
            Assert.That(quantityList, Does.Contain(ConsistencyIndexRheologyQuantity.Instance));
            Assert.That(quantityList, Does.Contain(RotationalFrequencyQuantity.Instance));
            Assert.That(quantityList, Does.Contain(RotationalFrequencyRateOfChangeQuantity.Instance));
            Assert.That(quantityList, Does.Contain(ShockRateQuantity.Instance));
            Assert.That(quantityList, Does.Contain(DiameterSmallQuantity.Instance));
            Assert.That(quantityList, Does.Contain(LengthSmallQuantity.Instance));
            Assert.That(quantityList, Does.Contain(ProportionSmallQuantity.Instance));
            Assert.That(quantityList, Does.Contain(RotationalFrequencySmallQuantity.Instance));
            Assert.That(quantityList, Does.Contain(TorqueSmallQuantity.Instance));
            Assert.That(quantityList, Does.Contain(SolidAngleQuantity.Instance));
            Assert.That(quantityList, Does.Contain(IsobaricSpecificHeatCapacityQuantity.Instance));
            Assert.That(quantityList, Does.Contain(IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance));
            Assert.That(quantityList, Does.Contain(DimensionLessStandardQuantity.Instance));
            Assert.That(quantityList, Does.Contain(LengthStandardQuantity.Instance));
            Assert.That(quantityList, Does.Contain(ProportionStandardQuantity.Instance));
            Assert.That(quantityList, Does.Contain(TemperatureGradientPerLengthQuantity.Instance));
            Assert.That(quantityList, Does.Contain(TemperatureQuantity.Instance));
            Assert.That(quantityList, Does.Contain(TensionQuantity.Instance));
            Assert.That(quantityList, Does.Contain(ThermalConductivityQuantity.Instance));
            Assert.That(quantityList, Does.Contain(ThermalConductivityGradientPerTemperatureQuantity.Instance));
            Assert.That(quantityList, Does.Contain(TimeQuantity.Instance));
            Assert.That(quantityList, Does.Contain(TorqueQuantity.Instance));
            Assert.That(quantityList, Does.Contain(VelocityQuantity.Instance));
            Assert.That(quantityList, Does.Contain(VolumeQuantity.Instance));
            Assert.That(quantityList, Does.Contain(VolumetricFlowRateOfChangeQuantity.Instance));
            Assert.That(quantityList, Does.Contain(VolumetricFlowRateQuantity.Instance));
            Assert.That(quantityList, Does.Contain(WaveNumberQuantity.Instance));
            Assert.That(quantityList, Does.Contain(ElasticModulusQuantity.Instance));
            #endregion
        }

        [Test]
        public void Test_General_PhysicalQuantity()
        {
            #region availability of PhysicalQuantities
            List<BasePhysicalQuantity> quantityList = DrillingPhysicalQuantity.AvailablePhysicalQuantities;
            Assert.That(quantityList, Is.Not.Null);
            #endregion

            #region uniqueness of PhysicalQuantities
            Assert.That(quantityList.Count, Is.EqualTo(Enum.GetValues<DrillingPhysicalQuantity.QuantityEnum>().Length));
            Assert.That(quantityList, Does.Contain(AccelerationDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(AngleGradientPerLengthDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(AngularVelocityDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(AreaDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(AxialVelocityDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(BlockVelocityDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(CableDiameterDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(CapillaryPressureDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(CompressibilityDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(CurvatureDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(MassDensityDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(MassDensityGradientPerLengthDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(MassDensityGradientPerTemperatureDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(MassDensityRateOfChangeDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(MomentOfAreaDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(MomentOfInertiaDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(DepthDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(DrillStemMaterialStrengthDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(DrillStringMagneticFluxDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(DurationDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(DynamicViscosityDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(ElongationGradientPerLengthDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(FluidVelocityDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(ForceDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(ForceGradientPerLengthDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(FormationResistivityDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(FormationStrengthDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(GammaRayIndexDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(GasShowDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(GasVolumetricFlowRateDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(HeatTransferCoefficientDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(HeightDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(HookLoadDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(HydraulicConductivityDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(InterfacialTensionDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(MassDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(MassGradientPerLengthDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(MassRateDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(NozzleDiameterDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(DiameterPipeDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(PlaneAngleDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(DiameterPoreDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(SurfacePoreDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(PositionDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(PowerDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(PressureDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(PressureGradientPerLengthDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(PressureLossConstantDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(RandomWalkDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(RateOfPenetrationDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(RotationalFrequencyRateOfChangeDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(IsobaricSpecificHeatCapacityDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(IsobaricSpecificHeatCapacityGradientPerTemperatureDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(StickDurationDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(AngleMagneticFluxDensitySurveyInstrumentDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(AngularVelocitySurveyInstrumentDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(ReciprocalLengthSurveyInstrumentDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(TemperatureDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(TemperatureGradientPerLengthDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(TensionDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(ThermalConductivityDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(ThermalConductivityGradientPerTemperatureDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(TorqueDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(VolumeDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(VolumetricFlowrateDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(VolumetricFlowRateOfChangeDrillingQuantity.Instance));
            Assert.That(quantityList, Does.Contain(WeightOnBitDrillingQuantity.Instance));

            #endregion
        }

        [Test]
        public void Test_Calculus_LengthQuantity()
        {
            List<string> choices = LengthQuantity.Instance.GetUnitChoiceNames();
            double actual = LengthQuantity.Instance.ToSI(1.0, "foot");
            Assert.That(actual, Is.EqualTo(0.3048).Within(1e-6));
            // note that it is not possible to test the string formatting
            // for PhysicalQuantities that do not define a MeaningfullPrecision
        }

        [Test]
        public void Test_Calculus_DepthQuantity()
        {
            List<string> choices = DepthDrillingQuantity.Instance.GetUnitChoiceNames();
            string actualStr = DepthDrillingQuantity.Instance.ToSIString(1.0, "foot");
            string expected = 0.305.ToString(CultureInfo.InvariantCulture.NumberFormat);
            Assert.That(actualStr, Is.EqualTo(expected));
        }
        [Test]
        public void Test_Calculus_PipeDiameterQuantity()
        {
            List<string> choices = DiameterPipeDrillingQuantity.Instance.GetUnitChoiceNames();
            string actualStr = DiameterPipeDrillingQuantity.Instance.ToSIString(1.0, "inch");
            string expected = 0.0254.ToString(CultureInfo.InvariantCulture.NumberFormat);
            Assert.That(actualStr, Is.EqualTo(expected));
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
        }
    }
}
