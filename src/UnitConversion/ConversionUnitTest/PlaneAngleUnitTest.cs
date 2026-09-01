using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class PlaneAngleUnitTest
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
            unitVal = PlaneAngleQuantity.Instance.FromSI(val, PlaneAngleQuantity.Instance.GetUnitChoice(PlaneAngleQuantity.UnitChoicesEnum.ArcMinute).ID);
            Assert.AreEqual(3437.75, unitVal, 1e-2);
            unitVal = PlaneAngleQuantity.Instance.FromSI(val, PlaneAngleQuantity.Instance.GetUnitChoice(PlaneAngleQuantity.UnitChoicesEnum.ArcSecond).ID);
            Assert.AreEqual(180.0*3600.0/Math.PI, unitVal);
            unitVal = PlaneAngleQuantity.Instance.FromSI(val, PlaneAngleQuantity.Instance.GetUnitChoice(PlaneAngleQuantity.UnitChoicesEnum.Circle).ID);
            Assert.AreEqual(1.0/(2.0 * Math.PI), unitVal);
            unitVal = PlaneAngleQuantity.Instance.FromSI(val, PlaneAngleQuantity.Instance.GetUnitChoice(PlaneAngleQuantity.UnitChoicesEnum.Degree).ID);
            Assert.AreEqual(180.0/Math.PI, unitVal, 1e-6);
            unitVal = PlaneAngleQuantity.Instance.FromSI(val, PlaneAngleQuantity.Instance.GetUnitChoice(PlaneAngleQuantity.UnitChoicesEnum.Gon).ID);
            Assert.AreEqual(200.0/Math.PI, unitVal, 1e-6);
            unitVal = PlaneAngleQuantity.Instance.FromSI(val, PlaneAngleQuantity.Instance.GetUnitChoice(PlaneAngleQuantity.UnitChoicesEnum.Grad).ID);
            Assert.AreEqual(200.0/Math.PI, unitVal);
            unitVal = PlaneAngleQuantity.Instance.FromSI(val, PlaneAngleQuantity.Instance.GetUnitChoice(PlaneAngleQuantity.UnitChoicesEnum.Milliradian).ID);
            Assert.AreEqual(1000.0, unitVal);
            unitVal = PlaneAngleQuantity.Instance.FromSI(val, PlaneAngleQuantity.Instance.GetUnitChoice(PlaneAngleQuantity.UnitChoicesEnum.Octant).ID);
            Assert.AreEqual(4.0/Math.PI, unitVal);
            unitVal = PlaneAngleQuantity.Instance.FromSI(val, PlaneAngleQuantity.Instance.GetUnitChoice(PlaneAngleQuantity.UnitChoicesEnum.Quadrant).ID);
            Assert.AreEqual(2.0/Math.PI, unitVal);
            unitVal = PlaneAngleQuantity.Instance.FromSI(val, PlaneAngleQuantity.Instance.GetUnitChoice(PlaneAngleQuantity.UnitChoicesEnum.Revolution).ID);
            Assert.AreEqual(1.0 / (2.0 * Math.PI), unitVal);
            unitVal = PlaneAngleQuantity.Instance.FromSI(val, PlaneAngleQuantity.Instance.GetUnitChoice(PlaneAngleQuantity.UnitChoicesEnum.Sextant).ID);
            Assert.AreEqual(3.0/Math.PI, unitVal);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(1, PlaneAngleQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(0, PlaneAngleQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, PlaneAngleQuantity.Instance.MassDimension);
            Assert.AreEqual(0, PlaneAngleQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, PlaneAngleQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, PlaneAngleQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, PlaneAngleQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, PlaneAngleQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, PlaneAngleQuantity.Instance.SolidAngleDimension);
        }
    }
}