using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class ThermalConductivityTemperatureGradientPerLengthUnitTest
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
            unitVal = ThermalConductivityGradientPerTemperatureQuantity.Instance.FromSI(val, ThermalConductivityGradientPerTemperatureQuantity.Instance.GetUnitChoice(ThermalConductivityGradientPerTemperatureQuantity.UnitChoicesEnum.BritishThermalUnitInchPerHourSquareFootDegreeFahrenheitSquared).ID);
            Assert.AreEqual(3.8519288888888887, unitVal, 1e-2);
            unitVal = ThermalConductivityGradientPerTemperatureQuantity.Instance.FromSI(val, ThermalConductivityGradientPerTemperatureQuantity.Instance.GetUnitChoice(ThermalConductivityGradientPerTemperatureQuantity.UnitChoicesEnum.BritishThermalUnitPerHourFootDegreeFahrenheitSquared).ID);
            Assert.AreEqual(0.32099405555555555, unitVal, 1e-3);
            unitVal = ThermalConductivityGradientPerTemperatureQuantity.Instance.FromSI(val, ThermalConductivityGradientPerTemperatureQuantity.Instance.GetUnitChoice(ThermalConductivityGradientPerTemperatureQuantity.UnitChoicesEnum.CaloriePerCentimetreSecondDegreeCelsiusSquared).ID);
            Assert.AreEqual(0.00238846, unitVal, 1e-5);
            unitVal = ThermalConductivityGradientPerTemperatureQuantity.Instance.FromSI(val, ThermalConductivityGradientPerTemperatureQuantity.Instance.GetUnitChoice(ThermalConductivityGradientPerTemperatureQuantity.UnitChoicesEnum.CaloriePerMetreSecondDegreeCelsiusSquared).ID);
            Assert.AreEqual(0.238846, unitVal, 1e-3);
            unitVal = ThermalConductivityGradientPerTemperatureQuantity.Instance.FromSI(val, ThermalConductivityGradientPerTemperatureQuantity.Instance.GetUnitChoice(ThermalConductivityGradientPerTemperatureQuantity.UnitChoicesEnum.WattPerMetreKelvinPerKelvin).ID);
            Assert.AreEqual(1, unitVal, 1e-6);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, ThermalConductivityGradientPerTemperatureQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-3, ThermalConductivityGradientPerTemperatureQuantity.Instance.TimeDimension);
            Assert.AreEqual(1, ThermalConductivityGradientPerTemperatureQuantity.Instance.MassDimension);
            Assert.AreEqual(1, ThermalConductivityGradientPerTemperatureQuantity.Instance.LengthDimension);
            Assert.AreEqual(-2, ThermalConductivityGradientPerTemperatureQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, ThermalConductivityGradientPerTemperatureQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, ThermalConductivityGradientPerTemperatureQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, ThermalConductivityGradientPerTemperatureQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, ThermalConductivityGradientPerTemperatureQuantity.Instance.SolidAngleDimension);
        }
    }
}