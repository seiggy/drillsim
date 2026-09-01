using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class EnergyUnitTest
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
            unitVal = EnergyQuantity.Instance.FromSI(val, EnergyQuantity.Instance.GetUnitChoice(EnergyQuantity.UnitChoicesEnum.BritishThermalUnit).ID);
            Assert.AreEqual(1.0 / Factors.BTU, unitVal);
            unitVal = EnergyQuantity.Instance.FromSI(val, EnergyQuantity.Instance.GetUnitChoice(EnergyQuantity.UnitChoicesEnum.Calorie).ID);
            Assert.AreEqual(1.0 / Factors.Calorie, unitVal);
            unitVal = EnergyQuantity.Instance.FromSI(val, EnergyQuantity.Instance.GetUnitChoice(EnergyQuantity.UnitChoicesEnum.Gigajoule).ID);
            Assert.AreEqual(1.0 / Factors.Giga, unitVal);
            unitVal = EnergyQuantity.Instance.FromSI(val, EnergyQuantity.Instance.GetUnitChoice(EnergyQuantity.UnitChoicesEnum.Joule).ID);
            Assert.AreEqual(1.0 / Factors.Unit, unitVal);
            unitVal = EnergyQuantity.Instance.FromSI(val, EnergyQuantity.Instance.GetUnitChoice(EnergyQuantity.UnitChoicesEnum.KiloBritishThermalUnit).ID);
            Assert.AreEqual(1.0 / (Factors.Kilo * Factors.BTU), unitVal);
            unitVal = EnergyQuantity.Instance.FromSI(val, EnergyQuantity.Instance.GetUnitChoice(EnergyQuantity.UnitChoicesEnum.Kilocalorie).ID);
            Assert.AreEqual(1.0 / (Factors.Kilo * Factors.Calorie), unitVal);
            unitVal = EnergyQuantity.Instance.FromSI(val, EnergyQuantity.Instance.GetUnitChoice(EnergyQuantity.UnitChoicesEnum.Kilojoule).ID);
            Assert.AreEqual(1.0 / (Factors.Kilo ), unitVal);
            unitVal = EnergyQuantity.Instance.FromSI(val, EnergyQuantity.Instance.GetUnitChoice(EnergyQuantity.UnitChoicesEnum.MegaBritishThermalUnit).ID);
            Assert.AreEqual(1.0 / (Factors.Mega * Factors.BTU), unitVal);
            unitVal = EnergyQuantity.Instance.FromSI(val, EnergyQuantity.Instance.GetUnitChoice(EnergyQuantity.UnitChoicesEnum.Megajoule).ID);
            Assert.AreEqual(1.0 / (Factors.Mega * Factors.Unit), unitVal);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, EnergyQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-2, EnergyQuantity.Instance.TimeDimension);
            Assert.AreEqual(1, EnergyQuantity.Instance.MassDimension);
            Assert.AreEqual(2, EnergyQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, EnergyQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, EnergyQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, EnergyQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, EnergyQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, EnergyQuantity.Instance.SolidAngleDimension);
        }
    }
}