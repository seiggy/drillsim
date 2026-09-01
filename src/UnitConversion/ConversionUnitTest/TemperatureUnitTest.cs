using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class TemperatureUnitTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            double val = 1.0;
            double unitVal;
            unitVal = TemperatureQuantity.Instance.FromSI(val, TemperatureQuantity.Instance.GetUnitChoice(TemperatureQuantity.UnitChoicesEnum.Celsius).ID);
            Assert.AreEqual(1.0 - Factors.ZeroCelsius, unitVal);
            unitVal = TemperatureQuantity.Instance.FromSI(val, TemperatureQuantity.Instance.GetUnitChoice(TemperatureQuantity.UnitChoicesEnum.Fahrenheit).ID);
            Assert.AreEqual(-457.87, unitVal, 1e-6);
            unitVal = TemperatureQuantity.Instance.FromSI(val, TemperatureQuantity.Instance.GetUnitChoice(TemperatureQuantity.UnitChoicesEnum.Rankine).ID);
            Assert.AreEqual(1.8, unitVal, 1e-6);
            unitVal = TemperatureQuantity.Instance.FromSI(val, TemperatureQuantity.Instance.GetUnitChoice(TemperatureQuantity.UnitChoicesEnum.Réaumur).ID);
            Assert.AreEqual(-217.72, unitVal, 1e-6);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, TemperatureQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(0, TemperatureQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, TemperatureQuantity.Instance.MassDimension);
            Assert.AreEqual(0, TemperatureQuantity.Instance.LengthDimension);
            Assert.AreEqual(1, TemperatureQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, TemperatureQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, TemperatureQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, TemperatureQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, TemperatureQuantity.Instance.SolidAngleDimension);
        }
    }
}