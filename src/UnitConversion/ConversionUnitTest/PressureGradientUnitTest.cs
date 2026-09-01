using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class PressureGradientUnitTest
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
            unitVal = PressureGradientPerLengthQuantity.Instance.FromSI(val, PressureGradientPerLengthQuantity.Instance.GetUnitChoice(PressureGradientPerLengthQuantity.UnitChoicesEnum.BarPerMetre).ID);
            Assert.AreEqual(1E-05, unitVal);
            unitVal = PressureGradientPerLengthQuantity.Instance.FromSI(val, PressureGradientPerLengthQuantity.Instance.GetUnitChoice(PressureGradientPerLengthQuantity.UnitChoicesEnum.PascalPerMetre).ID);
            Assert.AreEqual(1, unitVal);
            unitVal = PressureGradientPerLengthQuantity.Instance.FromSI(val, PressureGradientPerLengthQuantity.Instance.GetUnitChoice(PressureGradientPerLengthQuantity.UnitChoicesEnum.PsiPerFoot).ID);
            Assert.AreEqual(4.4207468542442094E-05, unitVal, 1e-8);
            unitVal = PressureGradientPerLengthQuantity.Instance.FromSI(val, PressureGradientPerLengthQuantity.Instance.GetUnitChoice(PressureGradientPerLengthQuantity.UnitChoicesEnum.PsiPerMetre).ID);
            Assert.AreEqual(0.00014503762645158165, unitVal, 1e-8);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, PressureGradientPerLengthQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-2, PressureGradientPerLengthQuantity.Instance.TimeDimension);
            Assert.AreEqual(1, PressureGradientPerLengthQuantity.Instance.MassDimension);
            Assert.AreEqual(-2, PressureGradientPerLengthQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, PressureGradientPerLengthQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, PressureGradientPerLengthQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, PressureGradientPerLengthQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, PressureGradientPerLengthQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, PressureGradientPerLengthQuantity.Instance.SolidAngleDimension);
        }
    }
}