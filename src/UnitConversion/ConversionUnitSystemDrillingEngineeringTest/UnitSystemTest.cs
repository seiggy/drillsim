using OSDC.UnitConversion.Conversion.UnitSystem.DrillingEngineering;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using OSDC.UnitConversion.Conversion.UnitSystem;
using OSDC.UnitConversion.Conversion;

namespace ConversionUnitSystemDrillingEngineeringTest
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

            DrillingUnitSystem unitSystem = DrillingUnitSystem.SIUnitSystem;
            Assert.Pass();
        }

        [Test]
        public void Test2()
        {

            DrillingUnitSystem unitSystem = DrillingUnitSystem.MetricUnitSystem;
            Assert.Pass();
        }

        [Test]
        public void Test3()
        {

            DrillingUnitSystem unitSystem = DrillingUnitSystem.USUnitSystem;
            Assert.Pass();
        }

        [Test]
        public void Test4()
        {

            DrillingUnitSystem unitSystem = DrillingUnitSystem.ImperialUnitSystem;
            Assert.Pass();
        }

        [Test]
        public void DwisPowerAndChokeLimitsHaveDrillingDefaultsAndPrecision()
        {
            DrillingUnitSystem metric = DrillingUnitSystem.MetricUnitSystem;

            Assert.That(metric.GetChoice(PowerRateOfChangeDrillingQuantity.Instance.ID)?.UnitLabel, Is.EqualTo("W/s"));
            Assert.That(metric.GetChoice(ChokeOpeningRateDrillingQuantity.Instance.ID)?.UnitLabel, Is.EqualTo("%/s"));
            Assert.That(PowerRateOfChangeDrillingQuantity.Instance.MeaningfulPrecisionInSI, Is.EqualTo(1.0));
            Assert.That(ChokeOpeningRateDrillingQuantity.Instance.MeaningfulPrecisionInSI, Is.EqualTo(0.0001));
            Assert.That(PowerRateOfChangeDrillingQuantity.Instance.UsualNames, Does.Contain("PowerRate"));
            Assert.That(ChokeOpeningRateDrillingQuantity.Instance.UsualNames, Does.Contain("ChokeRate"));
        }

        [Test]
        public void EveryDrillingQuantityHasPrecisionAndAChoiceInEveryDefaultSystem()
        {
            BaseUnitSystem[] systems =
            {
                DrillingUnitSystem.SIUnitSystem,
                DrillingUnitSystem.MetricUnitSystem,
                DrillingUnitSystem.USUnitSystem,
                DrillingUnitSystem.ImperialUnitSystem
            };

            foreach (BasePhysicalQuantity quantity in DrillingPhysicalQuantity.AvailablePhysicalQuantities)
            {
                Assert.That(quantity.MeaningfulPrecisionInSI, Is.Not.Null, quantity.Name);
                Assert.That(quantity.MeaningfulPrecisionInSI, Is.GreaterThan(0.0), quantity.Name);
                foreach (BaseUnitSystem system in systems)
                {
                    Assert.That(system.GetChoice(quantity.ID), Is.Not.Null, $"{system.Name}: {quantity.Name}");
                }
            }
        }
    }
}
