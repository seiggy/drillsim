using OSDC.UnitConversion.Conversion.UnitSystem;
using OSDC.UnitConversion.Conversion;

namespace ConversionUnitSystemTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            
            BaseUnitSystem unitSystem = BaseUnitSystem.SIBaseUnitSystem;
            Assert.Pass();    
        }

        [Test]
        public void Test2()
        {

            BaseUnitSystem unitSystem = BaseUnitSystem.MetricBaseUnitSystem;
            Assert.Pass();
        }

        [Test]
        public void Test3()
        {

            BaseUnitSystem unitSystem = BaseUnitSystem.USBaseUnitSystem;
            Assert.Pass();
        }

        [Test]
        public void Test4()
        {

            BaseUnitSystem unitSystem = BaseUnitSystem.ImperialBaseUnitSystem;
            Assert.Pass();
        }

        [TestCase(BaseUnitSystem.DefaultUnitSystemEnum.SI)]
        [TestCase(BaseUnitSystem.DefaultUnitSystemEnum.Metric)]
        [TestCase(BaseUnitSystem.DefaultUnitSystemEnum.US)]
        [TestCase(BaseUnitSystem.DefaultUnitSystemEnum.Imperial)]
        public void NewAffineCatalogHasCompleteDefaults(BaseUnitSystem.DefaultUnitSystemEnum systemChoice)
        {
            BaseUnitSystem unitSystem = systemChoice switch
            {
                BaseUnitSystem.DefaultUnitSystemEnum.SI => BaseUnitSystem.SIBaseUnitSystem,
                BaseUnitSystem.DefaultUnitSystemEnum.Metric => BaseUnitSystem.MetricBaseUnitSystem,
                BaseUnitSystem.DefaultUnitSystemEnum.US => BaseUnitSystem.USBaseUnitSystem,
                _ => BaseUnitSystem.ImperialBaseUnitSystem
            };

            Assert.That(unitSystem.GetChoice(MomentumQuantity.Instance.ID), Is.Not.Null);
            Assert.That(unitSystem.GetChoice(ElectricConductivityQuantity.Instance.ID), Is.Not.Null);
            Assert.That(unitSystem.GetChoice(DoseEquivalentRateQuantity.Instance.ID), Is.Not.Null);
            Assert.That(unitSystem.GetChoice(DataTransferRateQuantity.Instance.ID), Is.Not.Null);
        }

        [Test]
        public void NewSemanticQuantitiesHaveDeliberateSystemDefaults()
        {
            Assert.That(BaseUnitSystem.MetricBaseUnitSystem.GetChoice(MobilityQuantity.Instance.ID)?.UnitLabel,
                Is.EqualTo("mD/cP"));
            Assert.That(BaseUnitSystem.MetricBaseUnitSystem.GetChoice(ElectricalMobilityQuantity.Instance.ID)?.UnitLabel,
                Is.EqualTo("cm^2/(V*s)"));
            Assert.That(BaseUnitSystem.MetricBaseUnitSystem.GetChoice(VolumePerAngleQuantity.Instance.ID)?.UnitLabel,
                Is.EqualTo("L/rev"));

            Assert.That(BaseUnitSystem.USBaseUnitSystem.GetChoice(VolumePerAngleQuantity.Instance.ID)?.UnitLabel,
                Is.EqualTo("in^3/rev"));
            Assert.That(BaseUnitSystem.ImperialBaseUnitSystem.GetChoice(VolumePerAngleQuantity.Instance.ID)?.UnitLabel,
                Is.EqualTo("in^3/rev"));
            Assert.That(BaseUnitSystem.USBaseUnitSystem.GetChoice(VolumePerAreaQuantity.Instance.ID)?.UnitLabel,
                Is.EqualTo("galUS/ft^2"));
            Assert.That(BaseUnitSystem.ImperialBaseUnitSystem.GetChoice(VolumePerAreaQuantity.Instance.ID)?.UnitLabel,
                Is.EqualTo("galUK/ft^2"));
        }
    }
}
