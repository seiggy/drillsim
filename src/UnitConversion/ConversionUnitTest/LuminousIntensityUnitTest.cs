using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class LuminousIntensityUnitTest
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
            unitVal = LuminousIntensityQuantity.Instance.FromSI(val, LuminousIntensityQuantity.Instance.GetUnitChoice(LuminousIntensityQuantity.UnitChoicesEnum.BerlinerLichteinheit).ID);
            Assert.AreEqual(1.086956, unitVal, 1e-6);
            unitVal = LuminousIntensityQuantity.Instance.FromSI(val, LuminousIntensityQuantity.Instance.GetUnitChoice(LuminousIntensityQuantity.UnitChoicesEnum.Candela).ID);
            Assert.AreEqual(1.0, unitVal, 1e-6);
            unitVal = LuminousIntensityQuantity.Instance.FromSI(val, LuminousIntensityQuantity.Instance.GetUnitChoice(LuminousIntensityQuantity.UnitChoicesEnum.Carcel).ID);
            Assert.AreEqual(0.102669, unitVal, 1e-6);
            unitVal = LuminousIntensityQuantity.Instance.FromSI(val, LuminousIntensityQuantity.Instance.GetUnitChoice(LuminousIntensityQuantity.UnitChoicesEnum.DecimalCandle).ID);
            Assert.AreEqual(1.0, unitVal, 1e-6);
            unitVal = LuminousIntensityQuantity.Instance.FromSI(val, LuminousIntensityQuantity.Instance.GetUnitChoice(LuminousIntensityQuantity.UnitChoicesEnum.DVWGCandle).ID);
            Assert.AreEqual(0.935418, unitVal, 1e-6);
            unitVal = LuminousIntensityQuantity.Instance.FromSI(val, LuminousIntensityQuantity.Instance.GetUnitChoice(LuminousIntensityQuantity.UnitChoicesEnum.Hefnerkerze).ID);
            Assert.AreEqual(1.086956, unitVal, 1e-6);
            unitVal = LuminousIntensityQuantity.Instance.FromSI(val, LuminousIntensityQuantity.Instance.GetUnitChoice(LuminousIntensityQuantity.UnitChoicesEnum.InternationalCandle).ID);
            Assert.AreEqual(0.9803921, unitVal, 1e-6);
            unitVal = LuminousIntensityQuantity.Instance.FromSI(val, LuminousIntensityQuantity.Instance.GetUnitChoice(LuminousIntensityQuantity.UnitChoicesEnum.Kilocandela).ID);
            Assert.AreEqual(0.001, unitVal, 1e-6);
            unitVal = LuminousIntensityQuantity.Instance.FromSI(val, LuminousIntensityQuantity.Instance.GetUnitChoice(LuminousIntensityQuantity.UnitChoicesEnum.LumenPerSteradian).ID);
            Assert.AreEqual(1.0 , unitVal, 1e-6);
            unitVal = LuminousIntensityQuantity.Instance.FromSI(val, LuminousIntensityQuantity.Instance.GetUnitChoice(LuminousIntensityQuantity.UnitChoicesEnum.Millicandela).ID);
            Assert.AreEqual(1000.0, unitVal, 1e-6);
            unitVal = LuminousIntensityQuantity.Instance.FromSI(val, LuminousIntensityQuantity.Instance.GetUnitChoice(LuminousIntensityQuantity.UnitChoicesEnum.Violle).ID);
            Assert.AreEqual(0.0166667, unitVal, 1e-6);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, LuminousIntensityQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(0, LuminousIntensityQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, LuminousIntensityQuantity.Instance.MassDimension);
            Assert.AreEqual(0, LuminousIntensityQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, LuminousIntensityQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, LuminousIntensityQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, LuminousIntensityQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(1, LuminousIntensityQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, LuminousIntensityQuantity.Instance.SolidAngleDimension);
        }
    }
}