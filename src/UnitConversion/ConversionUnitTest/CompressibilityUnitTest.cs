using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class CompressibilityUnitTest
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
            unitVal = CompressibilityQuantity.Instance.FromSI(val, CompressibilityQuantity.Instance.GetUnitChoice(CompressibilityQuantity.UnitChoicesEnum.InverseAtmosphere).ID);
            Assert.AreEqual(Factors.Atmosphere, unitVal, 1e-6);
            unitVal = CompressibilityQuantity.Instance.FromSI(val, CompressibilityQuantity.Instance.GetUnitChoice(CompressibilityQuantity.UnitChoicesEnum.InverseBar).ID);
            Assert.AreEqual(Factors.Bar, unitVal, 1e-6);
            unitVal = CompressibilityQuantity.Instance.FromSI(val, CompressibilityQuantity.Instance.GetUnitChoice(CompressibilityQuantity.UnitChoicesEnum.InversePascal).ID);
            Assert.AreEqual(Factors.Unit, unitVal, 1e-6);
            unitVal = CompressibilityQuantity.Instance.FromSI(val, CompressibilityQuantity.Instance.GetUnitChoice(CompressibilityQuantity.UnitChoicesEnum.InversePoundPerSquareInch).ID);
            Assert.AreEqual(Factors.PSI, unitVal, 1e-6);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, CompressibilityQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(2, CompressibilityQuantity.Instance.TimeDimension);
            Assert.AreEqual(-1, CompressibilityQuantity.Instance.MassDimension);
            Assert.AreEqual(1, CompressibilityQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, CompressibilityQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, CompressibilityQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, CompressibilityQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, CompressibilityQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, CompressibilityQuantity.Instance.SolidAngleDimension);
        }
    }
}