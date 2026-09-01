using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class SpecificHeatCapacityTemperatureGradientUnitTest
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
            unitVal = IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.FromSI(val, IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.UnitChoicesEnum.BritishThermalUnitPerPoundSquaredDegreeFahrenheit).ID);
            Assert.AreEqual(0.00013269216666666667, unitVal,1e-6);
            unitVal = IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.FromSI(val, IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.UnitChoicesEnum.CaloriePerGramDegreeSquaredCelsius).ID);
            Assert.AreEqual(0.0002388459, unitVal, 1e-6);
            unitVal = IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.FromSI(val, IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.UnitChoicesEnum.JoulePerGramDegreeSquaredCelsius).ID);
            Assert.AreEqual(0.001, unitVal, 1e-6);
            unitVal = IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.FromSI(val, IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.UnitChoicesEnum.JoulePerGramSquaredKelvin).ID);
            Assert.AreEqual(0.001, unitVal, 1e-6);
            unitVal = IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.FromSI(val, IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.GetUnitChoice(IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.UnitChoicesEnum.JoulePerKilogramSquaredKelvin).ID);
            Assert.AreEqual(1, unitVal, 1e-6);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-2, IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.MassDimension);
            Assert.AreEqual(2, IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.LengthDimension);
            Assert.AreEqual(-2, IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, IsobaricSpecificHeatCapacityGradientPerTemperatureQuantity.Instance.SolidAngleDimension);
        }
    }
}