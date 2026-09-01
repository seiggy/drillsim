using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class ElectricTensionUnitTest
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
            unitVal = ElectricTensionQuantity.Instance.FromSI(val, ElectricTensionQuantity.Instance.GetUnitChoice(ElectricTensionQuantity.UnitChoicesEnum.Centivolt).ID);
            Assert.AreEqual(1.0 / Factors.Centi, unitVal);
            unitVal = ElectricTensionQuantity.Instance.FromSI(val, ElectricTensionQuantity.Instance.GetUnitChoice(ElectricTensionQuantity.UnitChoicesEnum.Gigavolt).ID);
            Assert.AreEqual(1.0 / Factors.Giga, unitVal);
            unitVal = ElectricTensionQuantity.Instance.FromSI(val, ElectricTensionQuantity.Instance.GetUnitChoice(ElectricTensionQuantity.UnitChoicesEnum.Kilovolt).ID);
            Assert.AreEqual(1.0 / Factors.Kilo, unitVal);
            unitVal = ElectricTensionQuantity.Instance.FromSI(val, ElectricTensionQuantity.Instance.GetUnitChoice(ElectricTensionQuantity.UnitChoicesEnum.Megavolt).ID);
            Assert.AreEqual(1.0 / Factors.Mega, unitVal);
            unitVal = ElectricTensionQuantity.Instance.FromSI(val, ElectricTensionQuantity.Instance.GetUnitChoice(ElectricTensionQuantity.UnitChoicesEnum.Microvolt).ID);
            Assert.AreEqual(1.0 / Factors.Micro, unitVal);
            unitVal = ElectricTensionQuantity.Instance.FromSI(val, ElectricTensionQuantity.Instance.GetUnitChoice(ElectricTensionQuantity.UnitChoicesEnum.Millivolt).ID);
            Assert.AreEqual(1.0 / Factors.Milli, unitVal);
            unitVal = ElectricTensionQuantity.Instance.FromSI(val, ElectricTensionQuantity.Instance.GetUnitChoice(ElectricTensionQuantity.UnitChoicesEnum.Nanovolt).ID);
            Assert.AreEqual(1.0 / Factors.Nano, unitVal);
            unitVal = ElectricTensionQuantity.Instance.FromSI(val, ElectricTensionQuantity.Instance.GetUnitChoice(ElectricTensionQuantity.UnitChoicesEnum.Picovolt).ID);
            Assert.AreEqual(1.0 / Factors.Pico, unitVal);
            unitVal = ElectricTensionQuantity.Instance.FromSI(val, ElectricTensionQuantity.Instance.GetUnitChoice(ElectricTensionQuantity.UnitChoicesEnum.Volt).ID);
            Assert.AreEqual(1.0 / Factors.Unit, unitVal);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, ElectricTensionQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-3, ElectricTensionQuantity.Instance.TimeDimension);
            Assert.AreEqual(1, ElectricTensionQuantity.Instance.MassDimension);
            Assert.AreEqual(2, ElectricTensionQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, ElectricTensionQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(-1, ElectricTensionQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, ElectricTensionQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, ElectricTensionQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, ElectricTensionQuantity.Instance.SolidAngleDimension);
        }
    }
}