using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class AngleGradientPerLengthUnitTest
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
            unitVal = AngleGradientPerLengthQuantity.Instance.FromSI(val, AngleGradientPerLengthQuantity.Instance.GetUnitChoice(AngleGradientPerLengthQuantity.UnitChoicesEnum.DegreePerCentimetre).ID);
            Assert.AreEqual(Factors.Degree * Factors.Centi, unitVal);
            unitVal = AngleGradientPerLengthQuantity.Instance.FromSI(val, AngleGradientPerLengthQuantity.Instance.GetUnitChoice(AngleGradientPerLengthQuantity.UnitChoicesEnum.DegreePerDecimetre).ID);
            Assert.AreEqual(Factors.Degree * Factors.Deci, unitVal);
            unitVal = AngleGradientPerLengthQuantity.Instance.FromSI(val, AngleGradientPerLengthQuantity.Instance.GetUnitChoice(AngleGradientPerLengthQuantity.UnitChoicesEnum.DegreePerFoot).ID);
            Assert.AreEqual(Factors.Degree * Factors.Foot, unitVal);
            unitVal = AngleGradientPerLengthQuantity.Instance.FromSI(val, AngleGradientPerLengthQuantity.Instance.GetUnitChoice(AngleGradientPerLengthQuantity.UnitChoicesEnum.DegreePerInch).ID);
            Assert.AreEqual(Factors.Degree * Factors.Inch, unitVal);
            unitVal = AngleGradientPerLengthQuantity.Instance.FromSI(val, AngleGradientPerLengthQuantity.Instance.GetUnitChoice(AngleGradientPerLengthQuantity.UnitChoicesEnum.DegreePerMetre).ID);
            Assert.AreEqual(Factors.Degree * Factors.Unit, unitVal);
            unitVal = AngleGradientPerLengthQuantity.Instance.FromSI(val, AngleGradientPerLengthQuantity.Instance.GetUnitChoice(AngleGradientPerLengthQuantity.UnitChoicesEnum.DegreePerMillimetre).ID);
            Assert.AreEqual(Factors.Degree * Factors.Milli, unitVal);
            unitVal = AngleGradientPerLengthQuantity.Instance.FromSI(val, AngleGradientPerLengthQuantity.Instance.GetUnitChoice(AngleGradientPerLengthQuantity.UnitChoicesEnum.RadianPerCentimetre).ID);
            Assert.AreEqual(Factors.Unit * Factors.Centi, unitVal);
            unitVal = AngleGradientPerLengthQuantity.Instance.FromSI(val, AngleGradientPerLengthQuantity.Instance.GetUnitChoice(AngleGradientPerLengthQuantity.UnitChoicesEnum.RadianPerDecimetre).ID);
            Assert.AreEqual(Factors.Unit * Factors.Deci, unitVal);
            unitVal = AngleGradientPerLengthQuantity.Instance.FromSI(val, AngleGradientPerLengthQuantity.Instance.GetUnitChoice(AngleGradientPerLengthQuantity.UnitChoicesEnum.RadianPerMetre).ID);
            Assert.AreEqual(Factors.Unit * Factors.Unit, unitVal);
            unitVal = AngleGradientPerLengthQuantity.Instance.FromSI(val, AngleGradientPerLengthQuantity.Instance.GetUnitChoice(AngleGradientPerLengthQuantity.UnitChoicesEnum.RadianPerMillimetre).ID);
            Assert.AreEqual(Factors.Unit * Factors.Milli, unitVal);
            unitVal = AngleGradientPerLengthQuantity.Instance.FromSI(val, AngleGradientPerLengthQuantity.Instance.GetUnitChoice(AngleGradientPerLengthQuantity.UnitChoicesEnum.RadianPerFoot).ID);
            Assert.AreEqual(Factors.Unit * Factors.Foot, unitVal);
            unitVal = AngleGradientPerLengthQuantity.Instance.FromSI(val, AngleGradientPerLengthQuantity.Instance.GetUnitChoice(AngleGradientPerLengthQuantity.UnitChoicesEnum.RadianPerInch).ID);
            Assert.AreEqual(Factors.Unit * Factors.Inch, unitVal);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(1, AngleGradientPerLengthQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(0, AngleGradientPerLengthQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, AngleGradientPerLengthQuantity.Instance.MassDimension);
            Assert.AreEqual(-1, AngleGradientPerLengthQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, AngleGradientPerLengthQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, AngleGradientPerLengthQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, AngleGradientPerLengthQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, AngleGradientPerLengthQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, AngleGradientPerLengthQuantity.Instance.SolidAngleDimension);
        }
    }
}