using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class LengthUnitTest
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
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.AstronomicalUnit).ID);
            Assert.AreEqual(1.0/Factors.AstronomicalUnit, unitVal);
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Centimetre).ID);
            Assert.AreEqual(1.0 / Factors.Centi, unitVal);
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Decametre).ID);
            Assert.AreEqual(1.0 / Factors.Deca, unitVal);
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Decimetre).ID);
            Assert.AreEqual(1.0 / Factors.Deci, unitVal);
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Fathom).ID);
            Assert.AreEqual(1.0 / Factors.Fathom, unitVal);
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Foot).ID);
            Assert.AreEqual(1.0 / Factors.Foot, unitVal);
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Furlong).ID);
            Assert.AreEqual(1.0 / Factors.Furlong, unitVal);
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Hand).ID);
            Assert.AreEqual(1.0 / Factors.Hand, unitVal);
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Hectometre).ID);
            Assert.AreEqual(1.0 / Factors.Hecto, unitVal);
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Inch).ID);
            Assert.AreEqual(1.0 / Factors.Inch, unitVal);
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.InchPer32).ID);
            Assert.AreEqual(1.0 / Factors.InchPer32, unitVal);
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.InternationalNauticalMile).ID);
            Assert.AreEqual(1.0 / Factors.InternationalNauticalMile, unitVal);
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Kilometre).ID);
            Assert.AreEqual(1.0 / Factors.Kilo, unitVal);
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.LightYear).ID);
            Assert.AreEqual(1.0 / Factors.LightYear, unitVal);
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Metre).ID);
            Assert.AreEqual(1.0 / Factors.Unit, unitVal);
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Micrometre).ID);
            Assert.AreEqual(1.0 / Factors.Micro, unitVal);
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Mil).ID);
            Assert.AreEqual(1.0 / Factors.Mil, unitVal);
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Mile).ID);
            Assert.AreEqual(1.0 / Factors.Mile, unitVal);
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Millimetre).ID);
            Assert.AreEqual(1.0 / Factors.Milli, unitVal);
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Nanometre).ID);
            Assert.AreEqual(1.0 / Factors.Nano, unitVal);
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Parsec).ID);
            Assert.AreEqual(1.0 / Factors.Parsec, unitVal);
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Picometre).ID);
            Assert.AreEqual(1.0 / Factors.Pico, unitVal);
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.ScandinavianMile).ID);
            Assert.AreEqual(1.0 / Factors.ScandinavianMile, unitVal);
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Surveyor_sChain).ID);
            Assert.AreEqual(1.0 / Factors.SurveyorChain, unitVal);
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Thou).ID);
            Assert.AreEqual(1.0 / Factors.Thou, unitVal);
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.UKNauticalMile).ID);
            Assert.AreEqual(1.0 / Factors.UKNauticalMile, unitVal);
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.USSurveyFoot).ID);
            Assert.AreEqual(1.0 / Factors.USSurveyFoot, unitVal);
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Yard).ID);
            Assert.AreEqual(1.0 / Factors.Yard, unitVal);
            unitVal = LengthQuantity.Instance.FromSI(val, LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Ångstrøm).ID);
            Assert.AreEqual(1.0 / Factors.Angstrom, unitVal);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, LengthQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(0, LengthQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, LengthQuantity.Instance.MassDimension);
            Assert.AreEqual(1, LengthQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, LengthQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, LengthQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, LengthQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, LengthQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, LengthQuantity.Instance.SolidAngleDimension);
        }
    }
}