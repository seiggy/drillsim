using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class RelativeTemperatureUnitTest
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
            unitVal = RelativeTemperatureQuantity.Instance.FromSI(val, RelativeTemperatureQuantity.Instance.GetUnitChoice(RelativeTemperatureQuantity.UnitChoicesEnum.Kelvin).ID);
            Assert.AreEqual(1.0 , unitVal);
            unitVal = RelativeTemperatureQuantity.Instance.FromSI(val, RelativeTemperatureQuantity.Instance.GetUnitChoice(RelativeTemperatureQuantity.UnitChoicesEnum.Rankine).ID);
            Assert.AreEqual(9.0/5.0, unitVal, 1e-6);
            unitVal = RelativeTemperatureQuantity.Instance.FromSI(val, RelativeTemperatureQuantity.Instance.GetUnitChoice(RelativeTemperatureQuantity.UnitChoicesEnum.RelativeCelsius).ID);
            Assert.AreEqual(1, unitVal);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, RelativeTemperatureQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(0, RelativeTemperatureQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, RelativeTemperatureQuantity.Instance.MassDimension);
            Assert.AreEqual(0, RelativeTemperatureQuantity.Instance.LengthDimension);
            Assert.AreEqual(1, RelativeTemperatureQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, RelativeTemperatureQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, RelativeTemperatureQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, RelativeTemperatureQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, RelativeTemperatureQuantity.Instance.SolidAngleDimension);
        }
    }
}