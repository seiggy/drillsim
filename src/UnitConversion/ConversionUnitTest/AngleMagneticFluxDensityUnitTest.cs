using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class AngleMagneticFluxDensityUnitTest
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
            unitVal = AngleMagneticFluxDensityQuantity.Instance.FromSI(val, AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.DegreeGauss).ID);
            Assert.AreEqual(Factors.Degree / Factors.Gauss, unitVal);
            unitVal = AngleMagneticFluxDensityQuantity.Instance.FromSI(val, AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.DegreeMaxwellPerSquareCentimetre).ID);
            Assert.AreEqual(Factors.Degree / (Factors.Centi*Factors.Centi), unitVal);
            unitVal = AngleMagneticFluxDensityQuantity.Instance.FromSI(val, AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.DegreeMicrotesla).ID);
            Assert.AreEqual(Factors.Degree / Factors.Micro, unitVal);
            unitVal = AngleMagneticFluxDensityQuantity.Instance.FromSI(val, AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.DegreeMilligauss).ID);
            Assert.AreEqual(Factors.Degree / (Factors.Gauss*Factors.Milli), unitVal);
            unitVal = AngleMagneticFluxDensityQuantity.Instance.FromSI(val, AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.DegreeMillitesla).ID);
            Assert.AreEqual(Factors.Degree / Factors.Milli, unitVal);
            unitVal = AngleMagneticFluxDensityQuantity.Instance.FromSI(val, AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.DegreeNanotesla).ID);
            Assert.AreEqual(Factors.Degree / Factors.Nano, unitVal);
            unitVal = AngleMagneticFluxDensityQuantity.Instance.FromSI(val, AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.DegreeTesla).ID);
            Assert.AreEqual(Factors.Degree / Factors.Unit, unitVal);
            unitVal = AngleMagneticFluxDensityQuantity.Instance.FromSI(val, AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.DegreeWeberPerSquareMetre).ID);
            Assert.AreEqual(Factors.Degree / Factors.Unit, unitVal);
            unitVal = AngleMagneticFluxDensityQuantity.Instance.FromSI(val, AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.RadianGauss).ID);
            Assert.AreEqual(Factors.Unit / Factors.Gauss, unitVal);
            unitVal = AngleMagneticFluxDensityQuantity.Instance.FromSI(val, AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.RadianMaxwellPerSquareCentimetre).ID);
            Assert.AreEqual(Factors.Unit / (Factors.Centi * Factors.Centi), unitVal);
            unitVal = AngleMagneticFluxDensityQuantity.Instance.FromSI(val, AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.RadianMicrotesla).ID);
            Assert.AreEqual(Factors.Unit / Factors.Micro, unitVal);
            unitVal = AngleMagneticFluxDensityQuantity.Instance.FromSI(val, AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.RadianMilligauss).ID);
            Assert.AreEqual(Factors.Unit / (Factors.Milli*Factors.Gauss), unitVal);
            unitVal = AngleMagneticFluxDensityQuantity.Instance.FromSI(val, AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.RadianNanotesla).ID);
            Assert.AreEqual(Factors.Unit / Factors.Nano, unitVal);
            unitVal = AngleMagneticFluxDensityQuantity.Instance.FromSI(val, AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.RadianTesla).ID);
            Assert.AreEqual(Factors.Unit / Factors.Unit, unitVal);
            unitVal = AngleMagneticFluxDensityQuantity.Instance.FromSI(val, AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.RadianWeberPerSquareMetre).ID);
            Assert.AreEqual(Factors.Unit / Factors.Unit, unitVal);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(1, AngleMagneticFluxDensityQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-2, AngleMagneticFluxDensityQuantity.Instance.TimeDimension);
            Assert.AreEqual(1, AngleMagneticFluxDensityQuantity.Instance.MassDimension);
            Assert.AreEqual(0, AngleMagneticFluxDensityQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, AngleMagneticFluxDensityQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(-1, AngleMagneticFluxDensityQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, AngleMagneticFluxDensityQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, AngleMagneticFluxDensityQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, AngleMagneticFluxDensityQuantity.Instance.SolidAngleDimension);
        }
    }
}