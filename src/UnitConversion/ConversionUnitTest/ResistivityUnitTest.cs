using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class ResistivityUnitTest
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
            unitVal = ElectricResistivityQuantity.Instance.FromSI(val, ElectricResistivityQuantity.Instance.GetUnitChoice(ElectricResistivityQuantity.UnitChoicesEnum.OhmMetre).ID);
            Assert.AreEqual(1.0 / Factors.Unit, unitVal);
            unitVal = ElectricResistivityQuantity.Instance.FromSI(val, ElectricResistivityQuantity.Instance.GetUnitChoice(ElectricResistivityQuantity.UnitChoicesEnum.GigaOhmMetre).ID);
            Assert.AreEqual(1.0 / Factors.Giga, unitVal);
            unitVal = ElectricResistivityQuantity.Instance.FromSI(val, ElectricResistivityQuantity.Instance.GetUnitChoice(ElectricResistivityQuantity.UnitChoicesEnum.KiloOhmMetre).ID);
            Assert.AreEqual(1.0 / Factors.Kilo, unitVal);
            unitVal = ElectricResistivityQuantity.Instance.FromSI(val, ElectricResistivityQuantity.Instance.GetUnitChoice(ElectricResistivityQuantity.UnitChoicesEnum.MegaOhmMetre).ID);
            Assert.AreEqual(1.0 / Factors.Mega, unitVal);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, ElectricResistivityQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-3, ElectricResistivityQuantity.Instance.TimeDimension);
            Assert.AreEqual(1, ElectricResistivityQuantity.Instance.MassDimension);
            Assert.AreEqual(3, ElectricResistivityQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, ElectricResistivityQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(-2, ElectricResistivityQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, ElectricResistivityQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, ElectricResistivityQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, ElectricResistivityQuantity.Instance.SolidAngleDimension);
        }
    }
}