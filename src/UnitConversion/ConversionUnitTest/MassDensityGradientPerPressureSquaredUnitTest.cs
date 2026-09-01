using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class MassDensityGradientPerPressureSquaredUnitTest
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
            unitVal = MassDensityGradientPerPressureSquaredQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerBarSquared).ID);
            Assert.AreEqual(100*1e5, unitVal,1e-2 * 1e5);
            unitVal = MassDensityGradientPerPressureSquaredQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerPascalSquared).ID);
            Assert.AreEqual(0.001, unitVal, 1e-6);
            unitVal = MassDensityGradientPerPressureSquaredQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerPoundPerSquareInchSquared).ID);
            Assert.AreEqual(6.895 * 6894.757293, unitVal, 2);
            unitVal = MassDensityGradientPerPressureSquaredQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerBarSquared).ID);
            Assert.AreEqual(100000.0 * 1e5, unitVal, 1e-1 * 1e5);
            unitVal = MassDensityGradientPerPressureSquaredQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerPascalSquared).ID);
            Assert.AreEqual(1, unitVal, 1e-6);
            unitVal = MassDensityGradientPerPressureSquaredQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerPoundPerSquareInchSquared).ID);
            Assert.AreEqual(6894.76 * 6894.757293, unitVal, 20);
            unitVal = MassDensityGradientPerPressureSquaredQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredQuantity.UnitChoicesEnum.PoundPerCubicFootPerBarSquared).ID);
            Assert.AreEqual(6243 * 1e5, unitVal, 1e0 * 1e5);
            unitVal = MassDensityGradientPerPressureSquaredQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredQuantity.UnitChoicesEnum.PoundPerCubicFootPerPascalSquared).ID);
            Assert.AreEqual(0.06243, unitVal, 1e-4);
            unitVal = MassDensityGradientPerPressureSquaredQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredQuantity.UnitChoicesEnum.PoundPerCubicFootPerPoundPerSquareInchSquared).ID);
            Assert.AreEqual(430.5 * 6894.757293, unitVal, 600);
            unitVal = MassDensityGradientPerPressureSquaredQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredQuantity.UnitChoicesEnum.PoundPerCubicInchPerBarSquared).ID);
            Assert.AreEqual(3.611 * 1e5, unitVal, 1e-2 * 1e5);
            unitVal = MassDensityGradientPerPressureSquaredQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredQuantity.UnitChoicesEnum.PoundPerCubicInchPerPascalSquared).ID);
            Assert.AreEqual(3.611e-5, unitVal, 1e-7);
            unitVal = MassDensityGradientPerPressureSquaredQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredQuantity.UnitChoicesEnum.PoundPerCubicInchPerPoundPerSquareInchSquared).ID);
            Assert.AreEqual(0.249 * 6894.757293, unitVal, 1);
            unitVal = MassDensityGradientPerPressureSquaredQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredQuantity.UnitChoicesEnum.PoundPerCubicYardPerBarSquared).ID);
            Assert.AreEqual(168500 * 1e5, unitVal, 1e2 * 1e5);
            unitVal = MassDensityGradientPerPressureSquaredQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredQuantity.UnitChoicesEnum.PoundPerCubicYardPerPascalSquared).ID);
            Assert.AreEqual(1.685, unitVal, 1e-2);
            unitVal = MassDensityGradientPerPressureSquaredQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredQuantity.UnitChoicesEnum.PoundPerCubicYardPerPoundPerSquareInchSquared).ID);
            Assert.AreEqual(11620 * 6894.757293, unitVal, 11000);
            unitVal = MassDensityGradientPerPressureSquaredQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredQuantity.UnitChoicesEnum.PoundPerGallonUKPerBarSquared).ID);
            Assert.AreEqual(1002 * 1e5, unitVal, 1 * 1e5);
            unitVal = MassDensityGradientPerPressureSquaredQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredQuantity.UnitChoicesEnum.PoundPerGallonUKPerPascalSquared).ID);
            Assert.AreEqual(0.01002, unitVal, 1e-4);
            unitVal = MassDensityGradientPerPressureSquaredQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredQuantity.UnitChoicesEnum.PoundPerGallonUKPerPoundPerSquareInchSquared).ID);
            Assert.AreEqual(69.1 * 6894.757293, unitVal, 15);
            unitVal = MassDensityGradientPerPressureSquaredQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredQuantity.UnitChoicesEnum.PoundPerGallonUSPerBarSquared).ID);
            Assert.AreEqual(834 * 1e5, unitVal, 1 * 1e5);
            unitVal = MassDensityGradientPerPressureSquaredQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredQuantity.UnitChoicesEnum.PoundPerGallonUSPerPascalSquared).ID);
            Assert.AreEqual(0.00834, unitVal, 1e-4);
            unitVal = MassDensityGradientPerPressureSquaredQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredQuantity.UnitChoicesEnum.PoundPerGallonUSPerPoundPerSquareInchSquared).ID);
            Assert.AreEqual(57.5 * 6894.757293, unitVal, 300);
            unitVal = MassDensityGradientPerPressureSquaredQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredQuantity.UnitChoicesEnum.SpecificGravityPerBarSquared).ID);
            Assert.AreEqual(100 * 1e5, unitVal, 300);
            unitVal = MassDensityGradientPerPressureSquaredQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredQuantity.UnitChoicesEnum.SpecificGravityPerPascalSquared).ID);
            Assert.AreEqual(0.001, unitVal, 1e-6);
            unitVal = MassDensityGradientPerPressureSquaredQuantity.Instance.FromSI(val, MassDensityGradientPerPressureSquaredQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureSquaredQuantity.UnitChoicesEnum.SpecificGravityPerPoundPerSquareInchSquared).ID);
            Assert.AreEqual(6.895 * 6894.757293, unitVal, 1);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, MassDensityGradientPerPressureSquaredQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(4, MassDensityGradientPerPressureSquaredQuantity.Instance.TimeDimension);
            Assert.AreEqual(-1, MassDensityGradientPerPressureSquaredQuantity.Instance.MassDimension);
            Assert.AreEqual(-1, MassDensityGradientPerPressureSquaredQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, MassDensityGradientPerPressureSquaredQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, MassDensityGradientPerPressureSquaredQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, MassDensityGradientPerPressureSquaredQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, MassDensityGradientPerPressureSquaredQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, MassDensityGradientPerPressureSquaredQuantity.Instance.SolidAngleDimension);
        }
    }
}