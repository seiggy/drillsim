using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class MagneticFluxDensityUnitTest
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
            unitVal = MagneticFluxDensityQuantity.Instance.FromSI(val, MagneticFluxDensityQuantity.Instance.GetUnitChoice(MagneticFluxDensityQuantity.UnitChoicesEnum.Gauss).ID);
            Assert.AreEqual(1.0 / Factors.Gauss, unitVal);
            unitVal = MagneticFluxDensityQuantity.Instance.FromSI(val, MagneticFluxDensityQuantity.Instance.GetUnitChoice(MagneticFluxDensityQuantity.UnitChoicesEnum.MaxwellPerSquareCentimetre).ID);
            Assert.AreEqual(Factors.Centi*Factors.Centi/ Factors.Unit, unitVal);
            unitVal = MagneticFluxDensityQuantity.Instance.FromSI(val, MagneticFluxDensityQuantity.Instance.GetUnitChoice(MagneticFluxDensityQuantity.UnitChoicesEnum.Microtesla).ID);
            Assert.AreEqual(1.0 / Factors.Micro, unitVal);
            unitVal = MagneticFluxDensityQuantity.Instance.FromSI(val, MagneticFluxDensityQuantity.Instance.GetUnitChoice(MagneticFluxDensityQuantity.UnitChoicesEnum.Milligauss).ID);
            Assert.AreEqual(1.0 / (Factors.Milli * Factors.Gauss), unitVal);
            unitVal = MagneticFluxDensityQuantity.Instance.FromSI(val, MagneticFluxDensityQuantity.Instance.GetUnitChoice(MagneticFluxDensityQuantity.UnitChoicesEnum.Millitesla).ID);
            Assert.AreEqual(1.0 / Factors.Milli, unitVal);
            unitVal = MagneticFluxDensityQuantity.Instance.FromSI(val, MagneticFluxDensityQuantity.Instance.GetUnitChoice(MagneticFluxDensityQuantity.UnitChoicesEnum.Nanotesla).ID);
            Assert.AreEqual(1.0 / Factors.Nano, unitVal);
            unitVal = MagneticFluxDensityQuantity.Instance.FromSI(val, MagneticFluxDensityQuantity.Instance.GetUnitChoice(MagneticFluxDensityQuantity.UnitChoicesEnum.WeberPerSquareMetre).ID);
            Assert.AreEqual(1.0 , unitVal);
            unitVal = MagneticFluxDensityQuantity.Instance.FromSI(val, MagneticFluxDensityQuantity.Instance.GetUnitChoice(MagneticFluxDensityQuantity.UnitChoicesEnum.Tesla).ID);
            Assert.AreEqual(1.0 , unitVal);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, MagneticFluxDensityQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-2, MagneticFluxDensityQuantity.Instance.TimeDimension);
            Assert.AreEqual(1, MagneticFluxDensityQuantity.Instance.MassDimension);
            Assert.AreEqual(0, MagneticFluxDensityQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, MagneticFluxDensityQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(-1, MagneticFluxDensityQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, MagneticFluxDensityQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, MagneticFluxDensityQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, MagneticFluxDensityQuantity.Instance.SolidAngleDimension);
        }
    }
}