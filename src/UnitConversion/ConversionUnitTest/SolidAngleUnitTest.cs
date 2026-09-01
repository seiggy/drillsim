using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class SolidAngleUnitTest
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
            unitVal = SolidAngleQuantity.Instance.FromSI(val, SolidAngleQuantity.Instance.GetUnitChoice(SolidAngleQuantity.UnitChoicesEnum.DegreeSquared).ID);
            Assert.AreEqual((180.0 * 180.0) / (Math.PI * Math.PI), unitVal);
            unitVal = SolidAngleQuantity.Instance.FromSI(val, SolidAngleQuantity.Instance.GetUnitChoice(SolidAngleQuantity.UnitChoicesEnum.Spat).ID);
            Assert.AreEqual(1 / (4.0 * Math.PI), unitVal);
            unitVal = SolidAngleQuantity.Instance.FromSI(val, SolidAngleQuantity.Instance.GetUnitChoice(SolidAngleQuantity.UnitChoicesEnum.Steradian).ID);
            Assert.AreEqual(1.0 , unitVal);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(1, SolidAngleQuantity.Instance.SolidAngleDimension);
            Assert.AreEqual(0, SolidAngleQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(0, SolidAngleQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, SolidAngleQuantity.Instance.MassDimension);
            Assert.AreEqual(0, SolidAngleQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, SolidAngleQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, SolidAngleQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, SolidAngleQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, SolidAngleQuantity.Instance.LuminousIntensityDimension);
        }
    }
}