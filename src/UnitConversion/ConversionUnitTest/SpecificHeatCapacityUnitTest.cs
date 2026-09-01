using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class SpecificHeatCapacityUnitTest
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
            unitVal = IsobaricSpecificHeatCapacityQuantity.Instance.FromSI(val, IsobaricSpecificHeatCapacityQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityQuantity.UnitChoicesEnum.BritishThermalUnitPerPoundDegreeFahrenheit).ID);
            Assert.AreEqual(0.0002388459, unitVal, 1e-5);
            unitVal = IsobaricSpecificHeatCapacityQuantity.Instance.FromSI(val, IsobaricSpecificHeatCapacityQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityQuantity.UnitChoicesEnum.CaloriePerGramDegreeCelsius).ID);
            Assert.AreEqual(0.000239005736, unitVal, 1e-5);
            unitVal = IsobaricSpecificHeatCapacityQuantity.Instance.FromSI(val, IsobaricSpecificHeatCapacityQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityQuantity.UnitChoicesEnum.JoulePerGramDegreeCelsius).ID);
            Assert.AreEqual(0.001, unitVal, 1e-5);
            unitVal = IsobaricSpecificHeatCapacityQuantity.Instance.FromSI(val, IsobaricSpecificHeatCapacityQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityQuantity.UnitChoicesEnum.JoulePerGramKelvin).ID);
            Assert.AreEqual(0.001, unitVal, 1e-5);
            unitVal = IsobaricSpecificHeatCapacityQuantity.Instance.FromSI(val, IsobaricSpecificHeatCapacityQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityQuantity.UnitChoicesEnum.JoulePerKilogramKelvin).ID);
            Assert.AreEqual(1, unitVal, 1e-5);
            unitVal = IsobaricSpecificHeatCapacityQuantity.Instance.FromSI(val, IsobaricSpecificHeatCapacityQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityQuantity.UnitChoicesEnum.KilocaloriePerGramDegreeCelsius).ID);
            Assert.AreEqual(0.000000239005736, unitVal, 1e-10);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, IsobaricSpecificHeatCapacityQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-2, IsobaricSpecificHeatCapacityQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, IsobaricSpecificHeatCapacityQuantity.Instance.MassDimension);
            Assert.AreEqual(2, IsobaricSpecificHeatCapacityQuantity.Instance.LengthDimension);
            Assert.AreEqual(-1, IsobaricSpecificHeatCapacityQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, IsobaricSpecificHeatCapacityQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, IsobaricSpecificHeatCapacityQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, IsobaricSpecificHeatCapacityQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, IsobaricSpecificHeatCapacityQuantity.Instance.SolidAngleDimension);
        }
    }
}