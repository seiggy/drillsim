using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class PressureLossConstantUnitTest
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
            unitVal = PressureLossConstantQuantity.Instance.FromSI(val, PressureLossConstantQuantity.Instance.GetUnitChoice(PressureLossConstantQuantity.UnitChoicesEnum.PressureLossConstantMetric).ID);
            Assert.AreEqual(360000000000, unitVal, 1e8);
            unitVal = PressureLossConstantQuantity.Instance.FromSI(val, PressureLossConstantQuantity.Instance.GetUnitChoice(PressureLossConstantQuantity.UnitChoicesEnum.PressureLossConstantSI).ID);
            Assert.AreEqual(1, unitVal);
            unitVal = PressureLossConstantQuantity.Instance.FromSI(val, PressureLossConstantQuantity.Instance.GetUnitChoice(PressureLossConstantQuantity.UnitChoicesEnum.PressureLossConstantUK).ID);
            Assert.AreEqual(12036978643.119, unitVal, 1e4);
            unitVal = PressureLossConstantQuantity.Instance.FromSI(val, PressureLossConstantQuantity.Instance.GetUnitChoice(PressureLossConstantQuantity.UnitChoicesEnum.PressureLossConstantUS).ID);
            Assert.AreEqual(14455817187.722, unitVal, 1e4);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, PressureLossConstantQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(0, PressureLossConstantQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, PressureLossConstantQuantity.Instance.MassDimension);
            Assert.AreEqual(4, PressureLossConstantQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, PressureLossConstantQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, PressureLossConstantQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, PressureLossConstantQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, PressureLossConstantQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, PressureLossConstantQuantity.Instance.SolidAngleDimension);
        }
    }
}