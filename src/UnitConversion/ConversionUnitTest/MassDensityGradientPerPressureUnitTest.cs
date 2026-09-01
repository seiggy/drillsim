using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class MassDensityGradientPerPressureUnitTest
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
            unitVal = MassDensityGradientPerPressureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerBar).ID);
            Assert.AreEqual(100, unitVal,1e-2);
            unitVal = MassDensityGradientPerPressureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerPascal).ID);
            Assert.AreEqual(0.001, unitVal, 1e-6);
            unitVal = MassDensityGradientPerPressureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerPoundPerSquareInch).ID);
            Assert.AreEqual(6.895, unitVal, 1e-3);
            unitVal = MassDensityGradientPerPressureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerBar).ID);
            Assert.AreEqual(100000.0, unitVal, 1e-1);
            unitVal = MassDensityGradientPerPressureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerPascal).ID);
            Assert.AreEqual(1, unitVal, 1e-6);
            unitVal = MassDensityGradientPerPressureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerPoundPerSquareInch).ID);
            Assert.AreEqual(6894.76, unitVal, 1e-1);
            unitVal = MassDensityGradientPerPressureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureQuantity.UnitChoicesEnum.PoundPerCubicFootPerBar).ID);
            Assert.AreEqual(6243, unitVal, 1e0);
            unitVal = MassDensityGradientPerPressureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureQuantity.UnitChoicesEnum.PoundPerCubicFootPerPascal).ID);
            Assert.AreEqual(0.06243, unitVal, 1e-4);
            unitVal = MassDensityGradientPerPressureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureQuantity.UnitChoicesEnum.PoundPerCubicFootPerPoundPerSquareInch).ID);
            Assert.AreEqual(430.5, unitVal, 1e-1);
            unitVal = MassDensityGradientPerPressureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureQuantity.UnitChoicesEnum.PoundPerCubicInchPerBar).ID);
            Assert.AreEqual(3.611, unitVal, 1e-2);
            unitVal = MassDensityGradientPerPressureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureQuantity.UnitChoicesEnum.PoundPerCubicInchPerPascal).ID);
            Assert.AreEqual(3.611e-5, unitVal, 1e-7);
            unitVal = MassDensityGradientPerPressureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureQuantity.UnitChoicesEnum.PoundPerCubicInchPerPoundPerSquareInch).ID);
            Assert.AreEqual(0.249, unitVal, 1e-2);
            unitVal = MassDensityGradientPerPressureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureQuantity.UnitChoicesEnum.PoundPerCubicYardPerBar).ID);
            Assert.AreEqual(168500, unitVal, 1e2);
            unitVal = MassDensityGradientPerPressureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureQuantity.UnitChoicesEnum.PoundPerCubicYardPerPascal).ID);
            Assert.AreEqual(1.685, unitVal, 1e-2);
            unitVal = MassDensityGradientPerPressureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureQuantity.UnitChoicesEnum.PoundPerCubicYardPerPoundPerSquareInch).ID);
            Assert.AreEqual(11620, unitVal, 10);
            unitVal = MassDensityGradientPerPressureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureQuantity.UnitChoicesEnum.PoundPerGallonUKPerBar).ID);
            Assert.AreEqual(1002, unitVal, 1);
            unitVal = MassDensityGradientPerPressureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureQuantity.UnitChoicesEnum.PoundPerGallonUKPerPascal).ID);
            Assert.AreEqual(0.01002, unitVal, 1e-4);
            unitVal = MassDensityGradientPerPressureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureQuantity.UnitChoicesEnum.PoundPerGallonUKPerPoundPerSquareInch).ID);
            Assert.AreEqual(69.1, unitVal, 1e-1);
            unitVal = MassDensityGradientPerPressureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureQuantity.UnitChoicesEnum.PoundPerGallonUSPerBar).ID);
            Assert.AreEqual(834, unitVal, 1);
            unitVal = MassDensityGradientPerPressureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureQuantity.UnitChoicesEnum.PoundPerGallonUSPerPascal).ID);
            Assert.AreEqual(0.00834, unitVal, 1e-4);
            unitVal = MassDensityGradientPerPressureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureQuantity.UnitChoicesEnum.PoundPerGallonUSPerPoundPerSquareInch).ID);
            Assert.AreEqual(57.5, unitVal, 1e-1);
            unitVal = MassDensityGradientPerPressureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureQuantity.UnitChoicesEnum.SpecificGravityPerBar).ID);
            Assert.AreEqual(100, unitVal, 1e-2);
            unitVal = MassDensityGradientPerPressureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureQuantity.UnitChoicesEnum.SpecificGravityPerPascal).ID);
            Assert.AreEqual(0.001, unitVal, 1e-6);
            unitVal = MassDensityGradientPerPressureQuantity.Instance.FromSI(val, MassDensityGradientPerPressureQuantity.Instance.GetUnitChoice(MassDensityGradientPerPressureQuantity.UnitChoicesEnum.SpecificGravityPerPoundPerSquareInch).ID);
            Assert.AreEqual(6.895, unitVal, 1e-2);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, MassDensityGradientPerPressureQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(2, MassDensityGradientPerPressureQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, MassDensityGradientPerPressureQuantity.Instance.MassDimension);
            Assert.AreEqual(-2, MassDensityGradientPerPressureQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, MassDensityGradientPerPressureQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, MassDensityGradientPerPressureQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, MassDensityGradientPerPressureQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, MassDensityGradientPerPressureQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, MassDensityGradientPerPressureQuantity.Instance.SolidAngleDimension);
        }
    }
}