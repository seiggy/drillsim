using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class AffineCatalogUnitTest
    {
        [TestCase(typeof(MomentumQuantity))]
        [TestCase(typeof(ProductivityIndexQuantity))]
        [TestCase(typeof(MolarHeatCapacityQuantity))]
        [TestCase(typeof(ElectricConductivityQuantity))]
        [TestCase(typeof(AbsorbedDoseQuantity))]
        [TestCase(typeof(LuminousFluxQuantity))]
        [TestCase(typeof(RadioactivityQuantity))]
        [TestCase(typeof(DataTransferRateQuantity))]
        public void RepresentativeQuantitiesHaveGeneratedUsableChoices(Type quantityType)
        {
            var instanceProperty = quantityType.GetProperty("Instance");
            var quantity = (BasePhysicalQuantity?)instanceProperty?.GetValue(null);

            Assert.That(quantity, Is.Not.Null);
            Assert.That(quantity!.UnitChoices, Is.Not.Empty);
            Assert.That(quantity.UnitChoices.Count(choice => choice.IsSI), Is.EqualTo(1));

            foreach (UnitChoice choice in quantity.UnitChoices)
            {
                Assert.That(choice.ConversionFactorFromSIFormula, Is.Not.Empty);
                double converted = choice.FromSI(1.23456789);
                Assert.That(choice.ToSI(converted), Is.EqualTo(1.23456789).Within(1e-12));
            }
        }

        [Test]
        public void SemanticQuantitiesRemainDistinctWhenDimensionsMatch()
        {
            Assert.That(RadioactivityQuantity.Instance.ID, Is.Not.EqualTo(ReciprocalTimeQuantity.Instance.ID));
            Assert.That(AbsorbedDoseQuantity.Instance.ID, Is.Not.EqualTo(SpecificEnergyQuantity.Instance.ID));
            Assert.That(IlluminanceQuantity.Instance.ID, Is.Not.EqualTo(LuminousExitanceQuantity.Instance.ID));

            Assert.That(RadioactivityQuantity.Instance.TimeDimension, Is.EqualTo(-1));
            Assert.That(ReciprocalTimeQuantity.Instance.TimeDimension, Is.EqualTo(-1));
        }

        [Test]
        public void ElectronVoltPerParticleUsesExactDefiningConstants()
        {
            UnitChoice choice = MolarEnergyQuantity.Instance.GetUnitChoice(MolarEnergyQuantity.UnitChoicesEnum.ElectronvoltPerParticle);

            Assert.That(choice.ConversionFactorFromSIFormula,
                Is.EqualTo("1.0/(Factors.ElectronCharge*Factors.AvogadroConstant)"));
            Assert.That(choice.FromSI(Factors.ElectronCharge * Factors.AvogadroConstant), Is.EqualTo(1.0));
        }

        [Test]
        public void RemainingDwisAffineQuantitiesPreserveTheirSemanticDimensionsAndConversions()
        {
            Assert.Multiple(() =>
            {
                Assert.That(PressurePerVolumeQuantity.Instance.LengthDimension, Is.EqualTo(-4));
                Assert.That(PressureTimePerVolumeQuantity.Instance.TimeDimension, Is.EqualTo(-1));
                Assert.That(RelativeTemperaturePerPressureQuantity.Instance.TemperatureDimension, Is.EqualTo(1));
                Assert.That(TemperatureRateOfChangeQuantity.Instance.TimeDimension, Is.EqualTo(-1));
                Assert.That(EnergyPerAreaQuantity.Instance.LengthDimension, Is.EqualTo(0));
                Assert.That(MassPerEnergyQuantity.Instance.MassDimension, Is.EqualTo(0));
                Assert.That(VolumePerEnergyQuantity.Instance.LengthDimension, Is.EqualTo(1));
                Assert.That(WorkGradientQuantity.Instance.SIUnitName, Is.EqualTo("joule per metre"));
                Assert.That(BendingMomentGradientQuantity.Instance.SIUnitName, Is.EqualTo("newton metre per metre"));
            });

            UnitChoice pressurePerLitre = PressurePerVolumeQuantity.Instance.UnitChoices.Single(c => c.UnitName == "bar per litre");
            Assert.That(pressurePerLitre.FromSI(Factors.Bar / Factors.Litre), Is.EqualTo(1.0).Within(1e-12));

            UnitChoice rankinePerPsi = RelativeTemperaturePerPressureQuantity.Instance.UnitChoices.Single(c => c.UnitName == "rankine per psi");
            Assert.That(rankinePerPsi.FromSI(Factors.FahrenheitSlope / Factors.PSI), Is.EqualTo(1.0).Within(1e-12));

            UnitChoice massFuel = MassPerEnergyQuantity.Instance.UnitChoices.Single(c => c.UnitName == "pound per british thermal unit");
            Assert.That(massFuel.FromSI(Factors.Pound / Factors.BTU), Is.EqualTo(1.0).Within(1e-12));

            UnitChoice volumeFuel = VolumePerEnergyQuantity.Instance.UnitChoices.Single(c => c.UnitName == "US gallon per british thermal unit");
            Assert.That(volumeFuel.FromSI(Factors.GallonUS / Factors.BTU), Is.EqualTo(1.0).Within(1e-12));

            Assert.That(EnergyPerAreaQuantity.Instance.ID, Is.Not.EqualTo(InterfacialTensionQuantity.Instance.ID));
            Assert.That(WorkGradientQuantity.Instance.ID, Is.Not.EqualTo(ForceQuantity.Instance.ID));
            Assert.That(BendingMomentGradientQuantity.Instance.ID, Is.Not.EqualTo(ForceQuantity.Instance.ID));
        }
    }
}
