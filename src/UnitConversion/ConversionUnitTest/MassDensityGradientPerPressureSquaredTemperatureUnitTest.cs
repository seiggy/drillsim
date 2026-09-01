using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class MassDensityGradientPerPressureSquaredTemperatureUnitTest
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
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerBarSquaredKelvin).ID);
            Assert.AreEqual(100*1e5, unitVal,1e-2 * 1e5);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerPascalSquaredKelvin).ID);
            Assert.AreEqual(0.001, unitVal, 1e-6);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerPoundPerSquareInchSquaredKelvin).ID);
            Assert.AreEqual(6.895 * 6894.757293, unitVal, 2);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerBarSquaredKelvin).ID);
            Assert.AreEqual(100000.0 * 1e5, unitVal, 1e-1 * 1e5);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerPascalSquaredKelvin).ID);
            Assert.AreEqual(1, unitVal, 1e-6);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerPoundPerSquareInchSquaredKelvin).ID);
            Assert.AreEqual(6894.76 * 6894.757293, unitVal, 20);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerCubicFootPerBarSquaredKelvin).ID);
            Assert.AreEqual(6243 * 1e5, unitVal, 1e0 * 1e5);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerCubicFootPerPascalSquaredKelvin).ID);
            Assert.AreEqual(0.06243, unitVal, 1e-4);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerCubicFootPerPoundPerSquareInchSquaredKelvin).ID);
            Assert.AreEqual(430.5 * 6894.757293, unitVal, 600);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerCubicInchPerBarSquaredKelvin).ID);
            Assert.AreEqual(3.611 * 1e5, unitVal, 1e-2 * 1e5);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerCubicInchPerPascalSquaredKelvin).ID);
            Assert.AreEqual(3.611e-5, unitVal, 1e-7);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerCubicInchPerPoundPerSquareInchSquaredKelvin).ID);
            Assert.AreEqual(0.249 * 6894.757293, unitVal, 1);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerCubicYardPerBarSquaredKelvin).ID);
            Assert.AreEqual(168500 * 1e5, unitVal, 1e2 * 1e5);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerCubicYardPerPascalSquaredKelvin).ID);
            Assert.AreEqual(1.685, unitVal, 1e-2);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerCubicYardPerPoundPerSquareInchSquaredKelvin).ID);
            Assert.AreEqual(11620 * 6894.757293, unitVal, 11000);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUKPerBarSquaredKelvin).ID);
            Assert.AreEqual(1002 * 1e5, unitVal, 1 * 1e5);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUKPerPascalSquaredKelvin).ID);
            Assert.AreEqual(0.01002, unitVal, 1e-4);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUKPerPoundPerSquareInchSquaredKelvin).ID);
            Assert.AreEqual(69.1 * 6894.757293, unitVal, 15);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUSPerBarSquaredKelvin).ID);
            Assert.AreEqual(834 * 1e5, unitVal, 1 * 1e5);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUSPerPascalSquaredKelvin).ID);
            Assert.AreEqual(0.00834, unitVal, 1e-4);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUSPerPoundPerSquareInchSquaredKelvin).ID);
            Assert.AreEqual(57.5 * 6894.757293, unitVal, 300);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.SpecificGravityPerBarSquaredKelvin).ID);
            Assert.AreEqual(100 * 1e5, unitVal, 300);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.SpecificGravityPerPascalSquaredKelvin).ID);
            Assert.AreEqual(0.001, unitVal, 1e-6);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.SpecificGravityPerPoundPerSquareInchSquaredKelvin).ID);
            Assert.AreEqual(6.895 * 6894.757293, unitVal, 1);

            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerBarSquaredCelsius).ID);
            Assert.AreEqual(100 * 1e5, unitVal, 1e-2 * 1e5);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerPascalSquaredCelsius).ID);
            Assert.AreEqual(0.001, unitVal, 1e-6);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerPoundPerSquareInchSquaredCelsius).ID);
            Assert.AreEqual(6.895 * 6894.757293, unitVal, 2);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerBarSquaredCelsius).ID);
            Assert.AreEqual(100000.0 * 1e5, unitVal, 1e-1 * 1e5);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerPascalSquaredCelsius).ID);
            Assert.AreEqual(1, unitVal, 1e-6);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerPoundPerSquareInchSquaredCelsius).ID);
            Assert.AreEqual(6894.76 * 6894.757293, unitVal, 20);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerCubicFootPerBarSquaredCelsius).ID);
            Assert.AreEqual(6243 * 1e5, unitVal, 1e0 * 1e5);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerCubicFootPerPascalSquaredCelsius).ID);
            Assert.AreEqual(0.06243, unitVal, 1e-4);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerCubicFootPerPoundPerSquareInchSquaredCelsius).ID);
            Assert.AreEqual(430.5 * 6894.757293, unitVal, 600);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerCubicInchPerBarSquaredCelsius).ID);
            Assert.AreEqual(3.611 * 1e5, unitVal, 1e-2 * 1e5);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerCubicInchPerPascalSquaredCelsius).ID);
            Assert.AreEqual(3.611e-5, unitVal, 1e-7);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerCubicInchPerPoundPerSquareInchSquaredCelsius).ID);
            Assert.AreEqual(0.249 * 6894.757293, unitVal, 1);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerCubicYardPerBarSquaredCelsius).ID);
            Assert.AreEqual(168500 * 1e5, unitVal, 1e2 * 1e5);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerCubicYardPerPascalSquaredCelsius).ID);
            Assert.AreEqual(1.685, unitVal, 1e-2);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerCubicYardPerPoundPerSquareInchSquaredCelsius).ID);
            Assert.AreEqual(11620 * 6894.757293, unitVal, 11000);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUKPerBarSquaredCelsius).ID);
            Assert.AreEqual(1002 * 1e5, unitVal, 1 * 1e5);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUKPerPascalSquaredCelsius).ID);
            Assert.AreEqual(0.01002, unitVal, 1e-4);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUKPerPoundPerSquareInchSquaredCelsius).ID);
            Assert.AreEqual(69.1 * 6894.757293, unitVal, 15);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUSPerBarSquaredCelsius).ID);
            Assert.AreEqual(834 * 1e5, unitVal, 1 * 1e5);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUSPerPascalSquaredCelsius).ID);
            Assert.AreEqual(0.00834, unitVal, 1e-4);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUSPerPoundPerSquareInchSquaredCelsius).ID);
            Assert.AreEqual(57.5 * 6894.757293, unitVal, 300);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.SpecificGravityPerBarSquaredCelsius).ID);
            Assert.AreEqual(100 * 1e5, unitVal, 300);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.SpecificGravityPerPascalSquaredCelsius).ID);
            Assert.AreEqual(0.001, unitVal, 1e-6);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.SpecificGravityPerPoundPerSquareInchSquaredCelsius).ID);
            Assert.AreEqual(6.895 * 6894.757293, unitVal, 1);

            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerBarSquaredFahrenheit).ID);
            Assert.AreEqual(100 * 1e5 * 5.0 / 9.0, unitVal, 1e-2 * 1e5);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerPascalSquaredFahrenheit).ID);
            Assert.AreEqual(0.001 * 5.0 / 9.0, unitVal, 1e-6);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerPoundPerSquareInchSquaredFahrenheit).ID);
            Assert.AreEqual(6.895 * 6894.757293 * 5.0 / 9.0, unitVal, 2);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerBarSquaredFahrenheit).ID);
            Assert.AreEqual(100000.0 * 1e5 * 5.0 / 9.0, unitVal, 1e-1 * 1e5);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerPascalSquaredFahrenheit).ID);
            Assert.AreEqual(1 * 5.0 / 9.0, unitVal, 1e-6);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerPoundPerSquareInchSquaredFahrenheit).ID);
            Assert.AreEqual(6894.76 * 6894.757293 * 5.0 / 9.0, unitVal, 20);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerCubicFootPerBarSquaredFahrenheit).ID);
            Assert.AreEqual(6243 * 1e5 * 5.0 / 9.0, unitVal, 1e0 * 1e5);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerCubicFootPerPascalSquaredFahrenheit).ID);
            Assert.AreEqual(0.06243 * 5.0 / 9.0, unitVal, 1e-4);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerCubicFootPerPoundPerSquareInchSquaredFahrenheit).ID);
            Assert.AreEqual(430.5 * 6894.757293 * 5.0 / 9.0, unitVal, 600);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerCubicInchPerBarSquaredFahrenheit).ID);
            Assert.AreEqual(3.611 * 1e5 * 5.0 / 9.0, unitVal, 1e-2 * 1e5);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerCubicInchPerPascalSquaredFahrenheit).ID);
            Assert.AreEqual(3.611e-5 * 5.0 / 9.0, unitVal, 1e-7);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerCubicInchPerPoundPerSquareInchSquaredFahrenheit).ID);
            Assert.AreEqual(0.249 * 6894.757293 * 5.0 / 9.0, unitVal, 1);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerCubicYardPerBarSquaredFahrenheit).ID);
            Assert.AreEqual(168500 * 1e5 * 5.0 / 9.0, unitVal, 1e2 * 1e5);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerCubicYardPerPascalSquaredFahrenheit).ID);
            Assert.AreEqual(1.685 * 5.0 / 9.0, unitVal, 1e-2);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerCubicYardPerPoundPerSquareInchSquaredFahrenheit).ID);
            Assert.AreEqual(11620 * 6894.757293 * 5.0 / 9.0, unitVal, 11000);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUKPerBarSquaredFahrenheit).ID);
            Assert.AreEqual(1002 * 1e5 * 5.0 / 9.0, unitVal, 1 * 1e5);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUKPerPascalSquaredFahrenheit).ID);
            Assert.AreEqual(0.01002 * 5.0 / 9.0, unitVal, 1e-4);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUKPerPoundPerSquareInchSquaredFahrenheit).ID);
            Assert.AreEqual(69.1 * 6894.757293 * 5.0 / 9.0, unitVal, 15);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUSPerBarSquaredFahrenheit).ID);
            Assert.AreEqual(834 * 1e5 * 5.0 / 9.0, unitVal, 1 * 1e5);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUSPerPascalSquaredFahrenheit).ID);
            Assert.AreEqual(0.00834 * 5.0 / 9.0, unitVal, 1e-4);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.PoundPerGallonUSPerPoundPerSquareInchSquaredFahrenheit).ID);
            Assert.AreEqual(57.5 * 6894.757293 * 5.0 / 9.0, unitVal, 300);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.SpecificGravityPerBarSquaredFahrenheit).ID);
            Assert.AreEqual(100 * 1e5 * 5.0 / 9.0, unitVal, 300);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.SpecificGravityPerPascalSquaredFahrenheit).ID);
            Assert.AreEqual(0.001 * 5.0 / 9.0, unitVal, 1e-6);
            unitVal = MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredTemperatureQuantity.UnitChoicesEnum.SpecificGravityPerPoundPerSquareInchSquaredFahrenheit).ID);
            Assert.AreEqual(6.895 * 6894.757293 * 5.0 / 9.0, unitVal, 1);


        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(4, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.TimeDimension);
            Assert.AreEqual(-1, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.MassDimension);
            Assert.AreEqual(-1, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.LengthDimension);
            Assert.AreEqual(-1, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, MassDensityGradientPerPressureSquaredTemperatureQuantity.Instance.SolidAngleDimension);
        }
    }
}