using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class MassDensityGradientPerTemperatureUnitTest
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
            unitVal = MassDensityGradientPerTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerTemperatureQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerCelsius).ID);
            Assert.AreEqual(0.001, unitVal,1e-6);
            unitVal = MassDensityGradientPerTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerTemperatureQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerKelvin).ID);
            Assert.AreEqual(1.0, unitVal, 1e-6);
            unitVal = MassDensityGradientPerTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerTemperatureQuantity.UnitChoicesEnum.PoundPerCubicFootPerCelsius).ID);
            Assert.AreEqual(0.062427962, unitVal, 1e-6);
            unitVal = MassDensityGradientPerTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerTemperatureQuantity.UnitChoicesEnum.PoundPerCubicFootPerFahrenheit).ID);
            Assert.AreEqual(0.034685, unitVal, 1e-5);
            unitVal = MassDensityGradientPerTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerTemperatureQuantity.UnitChoicesEnum.PoundPerCubicInchPerCelsius).ID);
            Assert.AreEqual(3.6127292e-5, unitVal, 1e-8);
            unitVal = MassDensityGradientPerTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerTemperatureQuantity.UnitChoicesEnum.PoundPerCubicInchPerFahrenheit).ID);
            Assert.AreEqual(2.007e-5, unitVal, 1e-8);
            unitVal = MassDensityGradientPerTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerTemperatureQuantity.UnitChoicesEnum.PoundPerCubicYardPerCelsius).ID);
            Assert.AreEqual(1.685555, unitVal, 1e-6);
            unitVal = MassDensityGradientPerTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerTemperatureQuantity.UnitChoicesEnum.PoundPerCubicYeardPerFahrenheit).ID);
            Assert.AreEqual(0.936419, unitVal, 1e-6);
            unitVal = MassDensityGradientPerTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUKPerCelsius).ID);
            Assert.AreEqual(0.0100224129, unitVal, 1e-6);
            unitVal = MassDensityGradientPerTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUKPerFahrenheit).ID);
            Assert.AreEqual(0.005568, unitVal, 1e-6);
            unitVal = MassDensityGradientPerTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUSPerCelsius).ID);
            Assert.AreEqual(0.00834543421, unitVal, 1e-6);
            unitVal = MassDensityGradientPerTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUSPerFahrenheit).ID);
            Assert.AreEqual(0.00463635, unitVal, 1e-6);
            unitVal = MassDensityGradientPerTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerTemperatureQuantity.UnitChoicesEnum.SpecificGravityPerCelsius).ID);
            Assert.AreEqual(0.001, unitVal, 1e-6);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, MassDensityGradientPerTemperatureQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(0, MassDensityGradientPerTemperatureQuantity.Instance.TimeDimension);
            Assert.AreEqual(1, MassDensityGradientPerTemperatureQuantity.Instance.MassDimension);
            Assert.AreEqual(-3, MassDensityGradientPerTemperatureQuantity.Instance.LengthDimension);
            Assert.AreEqual(-1, MassDensityGradientPerTemperatureQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, MassDensityGradientPerTemperatureQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, MassDensityGradientPerTemperatureQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, MassDensityGradientPerTemperatureQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, MassDensityGradientPerTemperatureQuantity.Instance.SolidAngleDimension);
        }
    }
}