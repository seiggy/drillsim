using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class PowerUnitTest
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
            unitVal = PowerQuantity.Instance.FromSI(val, PowerQuantity.Instance.GetUnitChoice(PowerQuantity.UnitChoicesEnum.Attowatt).ID);
            Assert.AreEqual(1.0 / Factors.Atto, unitVal);
            unitVal = PowerQuantity.Instance.FromSI(val, PowerQuantity.Instance.GetUnitChoice(PowerQuantity.UnitChoicesEnum.Centiwatt).ID);
            Assert.AreEqual(1.0 / Factors.Centi, unitVal);
            unitVal = PowerQuantity.Instance.FromSI(val, PowerQuantity.Instance.GetUnitChoice(PowerQuantity.UnitChoicesEnum.Decawatt).ID);
            Assert.AreEqual(1.0 / Factors.Deca, unitVal);
            unitVal = PowerQuantity.Instance.FromSI(val, PowerQuantity.Instance.GetUnitChoice(PowerQuantity.UnitChoicesEnum.Deciwatt).ID);
            Assert.AreEqual(1.0 / Factors.Deci, unitVal);
            unitVal = PowerQuantity.Instance.FromSI(val, PowerQuantity.Instance.GetUnitChoice(PowerQuantity.UnitChoicesEnum.Exawatt).ID);
            Assert.AreEqual(1.0 / Factors.Exa, unitVal);
            unitVal = PowerQuantity.Instance.FromSI(val, PowerQuantity.Instance.GetUnitChoice(PowerQuantity.UnitChoicesEnum.Femtowatt).ID);
            Assert.AreEqual(1.0 / Factors.Femto, unitVal);
            unitVal = PowerQuantity.Instance.FromSI(val, PowerQuantity.Instance.GetUnitChoice(PowerQuantity.UnitChoicesEnum.Gigawatt).ID);
            Assert.AreEqual(1.0 / Factors.Giga, unitVal);
            unitVal = PowerQuantity.Instance.FromSI(val, PowerQuantity.Instance.GetUnitChoice(PowerQuantity.UnitChoicesEnum.Hectowatt).ID);
            Assert.AreEqual(1.0 / Factors.Hecto, unitVal);
            unitVal = PowerQuantity.Instance.FromSI(val, PowerQuantity.Instance.GetUnitChoice(PowerQuantity.UnitChoicesEnum.Kilowatt).ID);
            Assert.AreEqual(1.0 / Factors.Kilo, unitVal);
            unitVal = PowerQuantity.Instance.FromSI(val, PowerQuantity.Instance.GetUnitChoice(PowerQuantity.UnitChoicesEnum.Megawatt).ID);
            Assert.AreEqual(1.0 / Factors.Mega, unitVal);
            unitVal = PowerQuantity.Instance.FromSI(val, PowerQuantity.Instance.GetUnitChoice(PowerQuantity.UnitChoicesEnum.Microwatt).ID);
            Assert.AreEqual(1.0 / Factors.Micro, unitVal);
            unitVal = PowerQuantity.Instance.FromSI(val, PowerQuantity.Instance.GetUnitChoice(PowerQuantity.UnitChoicesEnum.Milliwatt).ID);
            Assert.AreEqual(1.0 / Factors.Milli, unitVal);
            unitVal = PowerQuantity.Instance.FromSI(val, PowerQuantity.Instance.GetUnitChoice(PowerQuantity.UnitChoicesEnum.Nanowatt).ID);
            Assert.AreEqual(1.0 / Factors.Nano, unitVal);
            unitVal = PowerQuantity.Instance.FromSI(val, PowerQuantity.Instance.GetUnitChoice(PowerQuantity.UnitChoicesEnum.Petawatt).ID);
            Assert.AreEqual(1.0 / Factors.Peta, unitVal);
            unitVal = PowerQuantity.Instance.FromSI(val, PowerQuantity.Instance.GetUnitChoice(PowerQuantity.UnitChoicesEnum.Picowatt).ID);
            Assert.AreEqual(1.0 / Factors.Pico, unitVal);
            unitVal = PowerQuantity.Instance.FromSI(val, PowerQuantity.Instance.GetUnitChoice(PowerQuantity.UnitChoicesEnum.Terawatt).ID);
            Assert.AreEqual(1.0 / Factors.Tera, unitVal);
            unitVal = PowerQuantity.Instance.FromSI(val, PowerQuantity.Instance.GetUnitChoice(PowerQuantity.UnitChoicesEnum.Watt).ID);
            Assert.AreEqual(1.0 / Factors.Unit, unitVal);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, PowerQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-3, PowerQuantity.Instance.TimeDimension);
            Assert.AreEqual(1, PowerQuantity.Instance.MassDimension);
            Assert.AreEqual(2, PowerQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, PowerQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, PowerQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, PowerQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, PowerQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, PowerQuantity.Instance.SolidAngleDimension);
        }
    }
}