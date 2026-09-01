using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class MagneticFluxUnitTest
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
            unitVal = MagneticFluxQuantity.Instance.FromSI(val, MagneticFluxQuantity.Instance.GetUnitChoice(MagneticFluxQuantity.UnitChoicesEnum.GaussSquareCentimetre).ID);
            Assert.AreEqual(100000000.0, unitVal, 1e-6);
            unitVal = MagneticFluxQuantity.Instance.FromSI(val, MagneticFluxQuantity.Instance.GetUnitChoice(MagneticFluxQuantity.UnitChoicesEnum.Kiloline).ID);
            Assert.AreEqual(100000.0, unitVal, 1e-6);
            unitVal = MagneticFluxQuantity.Instance.FromSI(val, MagneticFluxQuantity.Instance.GetUnitChoice(MagneticFluxQuantity.UnitChoicesEnum.Line).ID);
            Assert.AreEqual(100000000.0, unitVal, 1e-6);
            unitVal = MagneticFluxQuantity.Instance.FromSI(val, MagneticFluxQuantity.Instance.GetUnitChoice(MagneticFluxQuantity.UnitChoicesEnum.MagneticFluxQuantum).ID);
            Assert.AreEqual(483597670318520.0, unitVal, 1e9);
            unitVal = MagneticFluxQuantity.Instance.FromSI(val, MagneticFluxQuantity.Instance.GetUnitChoice(MagneticFluxQuantity.UnitChoicesEnum.Maxwell).ID);
            Assert.AreEqual(100000000.0, unitVal, 1e-6);
            unitVal = MagneticFluxQuantity.Instance.FromSI(val, MagneticFluxQuantity.Instance.GetUnitChoice(MagneticFluxQuantity.UnitChoicesEnum.Megaline).ID);
            Assert.AreEqual(100.0, unitVal, 1e-6);
            unitVal = MagneticFluxQuantity.Instance.FromSI(val, MagneticFluxQuantity.Instance.GetUnitChoice(MagneticFluxQuantity.UnitChoicesEnum.Microweber).ID);
            Assert.AreEqual(1000000.0, unitVal, 1e-6);
            unitVal = MagneticFluxQuantity.Instance.FromSI(val, MagneticFluxQuantity.Instance.GetUnitChoice(MagneticFluxQuantity.UnitChoicesEnum.Milliweber).ID);
            Assert.AreEqual(1000.0, unitVal, 1e-6);
            unitVal = MagneticFluxQuantity.Instance.FromSI(val, MagneticFluxQuantity.Instance.GetUnitChoice(MagneticFluxQuantity.UnitChoicesEnum.TeslaSquareCentimetre).ID);
            Assert.AreEqual(10000.0, unitVal, 1e-6);
            unitVal = MagneticFluxQuantity.Instance.FromSI(val, MagneticFluxQuantity.Instance.GetUnitChoice(MagneticFluxQuantity.UnitChoicesEnum.TeslaSquareMetre).ID);
            Assert.AreEqual(1.0, unitVal, 1e-6);
            unitVal = MagneticFluxQuantity.Instance.FromSI(val, MagneticFluxQuantity.Instance.GetUnitChoice(MagneticFluxQuantity.UnitChoicesEnum.UnitPole).ID);
            Assert.AreEqual(7.9577e14, unitVal, 1e10);
            unitVal = MagneticFluxQuantity.Instance.FromSI(val, MagneticFluxQuantity.Instance.GetUnitChoice(MagneticFluxQuantity.UnitChoicesEnum.VoltSecond).ID);
            Assert.AreEqual(1.0, unitVal, 1e-6);
            unitVal = MagneticFluxQuantity.Instance.FromSI(val, MagneticFluxQuantity.Instance.GetUnitChoice(MagneticFluxQuantity.UnitChoicesEnum.Weber).ID);
            Assert.AreEqual(1.0, unitVal, 1e-6);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, MagneticFluxQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-2, MagneticFluxQuantity.Instance.TimeDimension);
            Assert.AreEqual(1, MagneticFluxQuantity.Instance.MassDimension);
            Assert.AreEqual(2, MagneticFluxQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, MagneticFluxQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(-1, MagneticFluxQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, MagneticFluxQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, MagneticFluxQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, MagneticFluxQuantity.Instance.SolidAngleDimension);
        }
    }
}