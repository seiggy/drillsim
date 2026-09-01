using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class TemperatureGradientPerLengthUnitTest
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
            unitVal = TemperatureGradientPerLengthQuantity.Instance.FromSI(val, TemperatureGradientPerLengthQuantity.Instance.GetUnitChoice(TemperatureGradientPerLengthQuantity.UnitChoicesEnum.CelsiusPer100Foot).ID);
            Assert.AreEqual(100.0*Factors.Foot / Factors.Unit, unitVal);
            unitVal = TemperatureGradientPerLengthQuantity.Instance.FromSI(val, TemperatureGradientPerLengthQuantity.Instance.GetUnitChoice(TemperatureGradientPerLengthQuantity.UnitChoicesEnum.CelsiusPer100Metre).ID);
            Assert.AreEqual(100.0 / Factors.Unit, unitVal);
            unitVal = TemperatureGradientPerLengthQuantity.Instance.FromSI(val, TemperatureGradientPerLengthQuantity.Instance.GetUnitChoice(TemperatureGradientPerLengthQuantity.UnitChoicesEnum.CelsiusPer10Metre).ID);
            Assert.AreEqual(10.0 / Factors.Unit, unitVal);
            unitVal = TemperatureGradientPerLengthQuantity.Instance.FromSI(val, TemperatureGradientPerLengthQuantity.Instance.GetUnitChoice(TemperatureGradientPerLengthQuantity.UnitChoicesEnum.CelsiusPer30Foot).ID);
            Assert.AreEqual(30.0 * Factors.Foot / Factors.Unit, unitVal);
            unitVal = TemperatureGradientPerLengthQuantity.Instance.FromSI(val, TemperatureGradientPerLengthQuantity.Instance.GetUnitChoice(TemperatureGradientPerLengthQuantity.UnitChoicesEnum.CelsiusPerFoot).ID);
            Assert.AreEqual(Factors.Foot / Factors.Unit, unitVal);
            unitVal = TemperatureGradientPerLengthQuantity.Instance.FromSI(val, TemperatureGradientPerLengthQuantity.Instance.GetUnitChoice(TemperatureGradientPerLengthQuantity.UnitChoicesEnum.CelsiusPerMetre).ID);
            Assert.AreEqual(1 / Factors.Unit, unitVal);
            unitVal = TemperatureGradientPerLengthQuantity.Instance.FromSI(val, TemperatureGradientPerLengthQuantity.Instance.GetUnitChoice(TemperatureGradientPerLengthQuantity.UnitChoicesEnum.FahrenheitPer100Foot).ID);
            Assert.AreEqual(100.0 * Factors.Foot / Factors.FahrenheitSlope, unitVal);
            unitVal = TemperatureGradientPerLengthQuantity.Instance.FromSI(val, TemperatureGradientPerLengthQuantity.Instance.GetUnitChoice(TemperatureGradientPerLengthQuantity.UnitChoicesEnum.FahrenheitPer30Foot).ID);
            Assert.AreEqual(30.0 * Factors.Foot / Factors.FahrenheitSlope, unitVal);
            unitVal = TemperatureGradientPerLengthQuantity.Instance.FromSI(val, TemperatureGradientPerLengthQuantity.Instance.GetUnitChoice(TemperatureGradientPerLengthQuantity.UnitChoicesEnum.FahrenheitPerFoot).ID);
            Assert.AreEqual(Factors.Foot / Factors.FahrenheitSlope, unitVal);
            unitVal = TemperatureGradientPerLengthQuantity.Instance.FromSI(val, TemperatureGradientPerLengthQuantity.Instance.GetUnitChoice(TemperatureGradientPerLengthQuantity.UnitChoicesEnum.KelvinPerMetre).ID);
            Assert.AreEqual(1 / Factors.Unit, unitVal);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, TemperatureGradientPerLengthQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(0, TemperatureGradientPerLengthQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, TemperatureGradientPerLengthQuantity.Instance.MassDimension);
            Assert.AreEqual(-1, TemperatureGradientPerLengthQuantity.Instance.LengthDimension);
            Assert.AreEqual(1, TemperatureGradientPerLengthQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, TemperatureGradientPerLengthQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, TemperatureGradientPerLengthQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, TemperatureGradientPerLengthQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, TemperatureGradientPerLengthQuantity.Instance.SolidAngleDimension);
        }
    }
}