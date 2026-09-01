using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class ImageScaleUnitTest
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
            unitVal = ImageScaleQuantity.Instance.FromSI(val, ImageScaleQuantity.Instance.GetUnitChoice(ImageScaleQuantity.UnitChoicesEnum.DotPerInch).ID);
            Assert.AreEqual(Factors.Inch, unitVal);
            unitVal = ImageScaleQuantity.Instance.FromSI(val, ImageScaleQuantity.Instance.GetUnitChoice(ImageScaleQuantity.UnitChoicesEnum.DotPerMetre).ID);
            Assert.AreEqual(Factors.Unit, unitVal);
            unitVal = ImageScaleQuantity.Instance.FromSI(val, ImageScaleQuantity.Instance.GetUnitChoice(ImageScaleQuantity.UnitChoicesEnum.DotPerMicrometre).ID);
            Assert.AreEqual(Factors.Micro, unitVal);
            unitVal = ImageScaleQuantity.Instance.FromSI(val, ImageScaleQuantity.Instance.GetUnitChoice(ImageScaleQuantity.UnitChoicesEnum.DotPerMillimetre).ID);
            Assert.AreEqual(Factors.Milli, unitVal);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, ImageScaleQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(0, ImageScaleQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, ImageScaleQuantity.Instance.MassDimension);
            Assert.AreEqual(-1, ImageScaleQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, ImageScaleQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, ImageScaleQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, ImageScaleQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, ImageScaleQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, ImageScaleQuantity.Instance.SolidAngleDimension);
        }
    }
}