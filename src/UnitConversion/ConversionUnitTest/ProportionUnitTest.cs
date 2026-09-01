using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class ProportionUnitTest
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
            unitVal = ProportionQuantity.Instance.FromSI(val, ProportionQuantity.Instance.GetUnitChoice(ProportionQuantity.UnitChoicesEnum.PartPerMillion).ID);
            Assert.AreEqual(1000000, unitVal);
            unitVal = ProportionQuantity.Instance.FromSI(val, ProportionQuantity.Instance.GetUnitChoice(ProportionQuantity.UnitChoicesEnum.Percent).ID);
            Assert.AreEqual(100, unitVal);
            unitVal = ProportionQuantity.Instance.FromSI(val, ProportionQuantity.Instance.GetUnitChoice(ProportionQuantity.UnitChoicesEnum.PerThousand).ID);
            Assert.AreEqual(1000, unitVal);
            unitVal = ProportionQuantity.Instance.FromSI(val, ProportionQuantity.Instance.GetUnitChoice(ProportionQuantity.UnitChoicesEnum.Proportion).ID);
            Assert.AreEqual(1, unitVal);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, ProportionQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(0, ProportionQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, ProportionQuantity.Instance.MassDimension);
            Assert.AreEqual(0, ProportionQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, ProportionQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, ProportionQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, ProportionQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, ProportionQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, ProportionQuantity.Instance.SolidAngleDimension);
        }
    }
}