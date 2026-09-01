using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class MassDensityGradientPerPressureTemperatureUnitTest
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
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerBarKelvin).ID);
            Assert.AreEqual(100, unitVal,1e-2);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerPascalKelvin).ID);
            Assert.AreEqual(0.001, unitVal, 1e-6);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerPoundPerSquareInchKelvin).ID);
            Assert.AreEqual(6.895, unitVal, 1e-3);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerBarKelvin).ID);
            Assert.AreEqual(100000.0, unitVal, 1e-1);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerPascalKelvin).ID);
            Assert.AreEqual(1, unitVal, 1e-6);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerPoundPerSquareInchKelvin).ID);
            Assert.AreEqual(6894.76, unitVal, 1e-1);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerCubicFootPerBarKelvin).ID);
            Assert.AreEqual(6243, unitVal, 1e0);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerCubicFootPerPascalKelvin).ID);
            Assert.AreEqual(0.06243, unitVal, 1e-4);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerCubicFootPerPoundPerSquareInchKelvin).ID);
            Assert.AreEqual(430.5, unitVal, 1e-1);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerCubicInchPerBarKelvin).ID);
            Assert.AreEqual(3.611, unitVal, 1e-2);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerCubicInchPerPascalKelvin).ID);
            Assert.AreEqual(3.611e-5, unitVal, 1e-7);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerCubicInchPerPoundPerSquareInchKelvin).ID);
            Assert.AreEqual(0.249, unitVal, 1e-2);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerCubicYardPerBarKelvin).ID);
            Assert.AreEqual(168500, unitVal, 1e2);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerCubicYardPerPascalKelvin).ID);
            Assert.AreEqual(1.685, unitVal, 1e-2);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerCubicYardPerPoundPerSquareInchKelvin).ID);
            Assert.AreEqual(11620, unitVal, 10);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUKPerBarKelvin).ID);
            Assert.AreEqual(1002, unitVal, 1);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUKPerPascalKelvin).ID);
            Assert.AreEqual(0.01002, unitVal, 1e-4);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUKPerPoundPerSquareInchKelvin).ID);
            Assert.AreEqual(69.1, unitVal, 1e-1);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUSPerBarKelvin).ID);
            Assert.AreEqual(834, unitVal, 1);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUSPerPascalKelvin).ID);
            Assert.AreEqual(0.00834, unitVal, 1e-4);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUSPerPoundPerSquareInchKelvin).ID);
            Assert.AreEqual(57.5, unitVal, 1e-1);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.SpecificGravityPerBarKelvin).ID);
            Assert.AreEqual(100, unitVal, 1e-2);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.SpecificGravityPerPascalKelvin).ID);
            Assert.AreEqual(0.001, unitVal, 1e-6);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.SpecificGravityPerPoundPerSquareInchKelvin).ID);
            Assert.AreEqual(6.895, unitVal, 1e-2);

            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerBarCelsius).ID);
            Assert.AreEqual(100, unitVal, 1e-2);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerPascalCelsius).ID);
            Assert.AreEqual(0.001, unitVal, 1e-6);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerPoundPerSquareInchCelsius).ID);
            Assert.AreEqual(6.895, unitVal, 1e-3);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerBarCelsius).ID);
            Assert.AreEqual(100000.0, unitVal, 1e-1);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerPascalCelsius).ID);
            Assert.AreEqual(1, unitVal, 1e-6);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerPoundPerSquareInchCelsius).ID);
            Assert.AreEqual(6894.76, unitVal, 1e-1);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerCubicFootPerBarCelsius).ID);
            Assert.AreEqual(6243, unitVal, 1e0);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerCubicFootPerPascalCelsius).ID);
            Assert.AreEqual(0.06243, unitVal, 1e-4);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerCubicFootPerPoundPerSquareInchCelsius).ID);
            Assert.AreEqual(430.5, unitVal, 1e-1);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerCubicInchPerBarCelsius).ID);
            Assert.AreEqual(3.611, unitVal, 1e-2);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerCubicInchPerPascalCelsius).ID);
            Assert.AreEqual(3.611e-5, unitVal, 1e-7);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerCubicInchPerPoundPerSquareInchCelsius).ID);
            Assert.AreEqual(0.249, unitVal, 1e-2);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerCubicYardPerBarCelsius).ID);
            Assert.AreEqual(168500, unitVal, 1e2);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerCubicYardPerPascalCelsius).ID);
            Assert.AreEqual(1.685, unitVal, 1e-2);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerCubicYardPerPoundPerSquareInchCelsius).ID);
            Assert.AreEqual(11620, unitVal, 10);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUKPerBarCelsius).ID);
            Assert.AreEqual(1002, unitVal, 1);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUKPerPascalCelsius).ID);
            Assert.AreEqual(0.01002, unitVal, 1e-4);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUKPerPoundPerSquareInchCelsius).ID);
            Assert.AreEqual(69.1, unitVal, 1e-1);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUSPerBarCelsius).ID);
            Assert.AreEqual(834, unitVal, 1);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUSPerPascalCelsius).ID);
            Assert.AreEqual(0.00834, unitVal, 1e-4);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUSPerPoundPerSquareInchCelsius).ID);
            Assert.AreEqual(57.5, unitVal, 1e-1);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.SpecificGravityPerBarCelsius).ID);
            Assert.AreEqual(100, unitVal, 1e-2);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.SpecificGravityPerPascalCelsius).ID);
            Assert.AreEqual(0.001, unitVal, 1e-6);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.SpecificGravityPerPoundPerSquareInchCelsius).ID);
            Assert.AreEqual(6.895, unitVal, 1e-2);

            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerBarFahrenheit).ID);
            Assert.AreEqual(100 * 5.0/9.0, unitVal, 1e-2);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerPascalFahrenheit).ID);
            Assert.AreEqual(0.001 * 5.0 / 9.0, unitVal, 1e-6);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerPoundPerSquareInchFahrenheit).ID);
            Assert.AreEqual(6.895 * 5.0 / 9.0, unitVal, 1e-3);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerBarFahrenheit).ID);
            Assert.AreEqual(100000.0 * 5.0 / 9.0, unitVal, 1e-1);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerPascalFahrenheit).ID);
            Assert.AreEqual(1 * 5.0 / 9.0, unitVal, 1e-6);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerPoundPerSquareInchFahrenheit).ID);
            Assert.AreEqual(6894.76 * 5.0 / 9.0, unitVal, 1e-1);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerCubicFootPerBarFahrenheit).ID);
            Assert.AreEqual(6243 * 5.0 / 9.0, unitVal, 1e0);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerCubicFootPerPascalFahrenheit).ID);
            Assert.AreEqual(0.06243 * 5.0 / 9.0, unitVal, 1e-4);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerCubicFootPerPoundPerSquareInchFahrenheit).ID);
            Assert.AreEqual(430.5 * 5.0 / 9.0, unitVal, 1e-1);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerCubicInchPerBarFahrenheit).ID);
            Assert.AreEqual(3.611 * 5.0 / 9.0, unitVal, 1e-2);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerCubicInchPerPascalFahrenheit).ID);
            Assert.AreEqual(3.611e-5 * 5.0 / 9.0, unitVal, 1e-7);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerCubicInchPerPoundPerSquareInchFahrenheit).ID);
            Assert.AreEqual(0.249 * 5.0 / 9.0, unitVal, 1e-2);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerCubicYardPerBarFahrenheit).ID);
            Assert.AreEqual(168500 * 5.0 / 9.0, unitVal, 1e2);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerCubicYardPerPascalFahrenheit).ID);
            Assert.AreEqual(1.685 * 5.0 / 9.0, unitVal, 1e-2);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerCubicYardPerPoundPerSquareInchFahrenheit).ID);
            Assert.AreEqual(11620 * 5.0 / 9.0, unitVal, 10);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUKPerBarFahrenheit).ID);
            Assert.AreEqual(1002 * 5.0 / 9.0, unitVal, 1);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUKPerPascalFahrenheit).ID);
            Assert.AreEqual(0.01002 * 5.0 / 9.0, unitVal, 1e-4);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUKPerPoundPerSquareInchFahrenheit).ID);
            Assert.AreEqual(69.1 * 5.0 / 9.0, unitVal, 1e-1);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUSPerBarFahrenheit).ID);
            Assert.AreEqual(834 * 5.0 / 9.0, unitVal, 1);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUSPerPascalFahrenheit).ID);
            Assert.AreEqual(0.00834 * 5.0 / 9.0, unitVal, 1e-4);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUSPerPoundPerSquareInchFahrenheit).ID);
            Assert.AreEqual(57.5 * 5.0 / 9.0, unitVal, 1e-1);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.SpecificGravityPerBarFahrenheit).ID);
            Assert.AreEqual(100 * 5.0 / 9.0, unitVal, 1e-2);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.SpecificGravityPerPascalFahrenheit).ID);
            Assert.AreEqual(0.001 * 5.0 / 9.0, unitVal, 1e-6);
            unitVal = MassDensityGradientPerPressureTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureTemperatureQuantity.UnitChoicesEnum.SpecificGravityPerPoundPerSquareInchFahrenheit).ID);
            Assert.AreEqual(6.895 * 5.0 / 9.0, unitVal, 1e-2);

        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, MassDensityGradientPerPressureTemperatureQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(2, MassDensityGradientPerPressureTemperatureQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, MassDensityGradientPerPressureTemperatureQuantity.Instance.MassDimension);
            Assert.AreEqual(-2, MassDensityGradientPerPressureTemperatureQuantity.Instance.LengthDimension);
            Assert.AreEqual(-1, MassDensityGradientPerPressureTemperatureQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, MassDensityGradientPerPressureTemperatureQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, MassDensityGradientPerPressureTemperatureQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, MassDensityGradientPerPressureTemperatureQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, MassDensityGradientPerPressureTemperatureQuantity.Instance.SolidAngleDimension);
        }
    }
}