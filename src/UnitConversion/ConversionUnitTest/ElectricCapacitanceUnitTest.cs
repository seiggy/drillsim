using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class ElectricCapacitanceUnitTest
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
            unitVal = ElectricCapacitanceQuantity.Instance.FromSI(val, ElectricCapacitanceQuantity.Instance.GetUnitChoice(ElectricCapacitanceQuantity.UnitChoicesEnum.CoulombPerVolt).ID);
            Assert.AreEqual(1.0 / Factors.Unit, unitVal, 1e-6);
            unitVal = ElectricCapacitanceQuantity.Instance.FromSI(val, ElectricCapacitanceQuantity.Instance.GetUnitChoice(ElectricCapacitanceQuantity.UnitChoicesEnum.Farad).ID);
            Assert.AreEqual(1.0 / Factors.Unit, unitVal, 1e-6);
            unitVal = ElectricCapacitanceQuantity.Instance.FromSI(val, ElectricCapacitanceQuantity.Instance.GetUnitChoice(ElectricCapacitanceQuantity.UnitChoicesEnum.Microfarad).ID);
            Assert.AreEqual(1.0 / Factors.Micro, unitVal, 1e-6);
            unitVal = ElectricCapacitanceQuantity.Instance.FromSI(val, ElectricCapacitanceQuantity.Instance.GetUnitChoice(ElectricCapacitanceQuantity.UnitChoicesEnum.Millifarad).ID);
            Assert.AreEqual(1.0 / Factors.Milli, unitVal, 1e-6);
            unitVal = ElectricCapacitanceQuantity.Instance.FromSI(val, ElectricCapacitanceQuantity.Instance.GetUnitChoice(ElectricCapacitanceQuantity.UnitChoicesEnum.Nanofarad).ID);
            Assert.AreEqual(1.0 / Factors.Nano, unitVal, 1e-6);
            unitVal = ElectricCapacitanceQuantity.Instance.FromSI(val, ElectricCapacitanceQuantity.Instance.GetUnitChoice(ElectricCapacitanceQuantity.UnitChoicesEnum.Picofarad).ID);
            Assert.AreEqual(1.0 / Factors.Pico, unitVal, 1e-6);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, ElectricCapacitanceQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(4, ElectricCapacitanceQuantity.Instance.TimeDimension);
            Assert.AreEqual(-1, ElectricCapacitanceQuantity.Instance.MassDimension);
            Assert.AreEqual(-2, ElectricCapacitanceQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, ElectricCapacitanceQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(2, ElectricCapacitanceQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, ElectricCapacitanceQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, ElectricCapacitanceQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, ElectricCapacitanceQuantity.Instance.SolidAngleDimension);
        }
    }
}