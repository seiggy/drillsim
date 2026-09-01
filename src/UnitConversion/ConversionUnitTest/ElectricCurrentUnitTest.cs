using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class ElectricCurrentUnitTest
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
            unitVal = ElectricCurrentQuantity.Instance.FromSI(val, ElectricCurrentQuantity.Instance.GetUnitChoice(ElectricCurrentQuantity.UnitChoicesEnum.Abampere).ID);
            Assert.AreEqual(0.1, unitVal);
            unitVal = ElectricCurrentQuantity.Instance.FromSI(val, ElectricCurrentQuantity.Instance.GetUnitChoice(ElectricCurrentQuantity.UnitChoicesEnum.Ampere).ID);
            Assert.AreEqual(1.0, unitVal);
            unitVal = ElectricCurrentQuantity.Instance.FromSI(val, ElectricCurrentQuantity.Instance.GetUnitChoice(ElectricCurrentQuantity.UnitChoicesEnum.Biot).ID);
            Assert.AreEqual(0.1, unitVal);
            unitVal = ElectricCurrentQuantity.Instance.FromSI(val, ElectricCurrentQuantity.Instance.GetUnitChoice(ElectricCurrentQuantity.UnitChoicesEnum.Centiampere).ID);
            Assert.AreEqual(100.0, unitVal);
            unitVal = ElectricCurrentQuantity.Instance.FromSI(val, ElectricCurrentQuantity.Instance.GetUnitChoice(ElectricCurrentQuantity.UnitChoicesEnum.CoulombPerSecond).ID);
            Assert.AreEqual(1.0, unitVal);
            unitVal = ElectricCurrentQuantity.Instance.FromSI(val, ElectricCurrentQuantity.Instance.GetUnitChoice(ElectricCurrentQuantity.UnitChoicesEnum.Deciampere).ID);
            Assert.AreEqual(10.0, unitVal);
            unitVal = ElectricCurrentQuantity.Instance.FromSI(val, ElectricCurrentQuantity.Instance.GetUnitChoice(ElectricCurrentQuantity.UnitChoicesEnum.Gigaampere).ID);
            Assert.AreEqual(1.0/Factors.Giga, unitVal);
            unitVal = ElectricCurrentQuantity.Instance.FromSI(val, ElectricCurrentQuantity.Instance.GetUnitChoice(ElectricCurrentQuantity.UnitChoicesEnum.Kiloampere).ID);
            Assert.AreEqual(1.0 / Factors.Kilo, unitVal);
            unitVal = ElectricCurrentQuantity.Instance.FromSI(val, ElectricCurrentQuantity.Instance.GetUnitChoice(ElectricCurrentQuantity.UnitChoicesEnum.Megaampere).ID);
            Assert.AreEqual(1.0 / Factors.Mega, unitVal);
            unitVal = ElectricCurrentQuantity.Instance.FromSI(val, ElectricCurrentQuantity.Instance.GetUnitChoice(ElectricCurrentQuantity.UnitChoicesEnum.Microampere).ID);
            Assert.AreEqual(1.0 / Factors.Micro, unitVal);
            unitVal = ElectricCurrentQuantity.Instance.FromSI(val, ElectricCurrentQuantity.Instance.GetUnitChoice(ElectricCurrentQuantity.UnitChoicesEnum.Milliampere).ID);
            Assert.AreEqual(1.0 / Factors.Milli, unitVal);
            unitVal = ElectricCurrentQuantity.Instance.FromSI(val, ElectricCurrentQuantity.Instance.GetUnitChoice(ElectricCurrentQuantity.UnitChoicesEnum.Nanoampere).ID);
            Assert.AreEqual(1.0 / Factors.Nano, unitVal);
            unitVal = ElectricCurrentQuantity.Instance.FromSI(val, ElectricCurrentQuantity.Instance.GetUnitChoice(ElectricCurrentQuantity.UnitChoicesEnum.Picoampere).ID);
            Assert.AreEqual(1.0 / Factors.Pico, unitVal);
            unitVal = ElectricCurrentQuantity.Instance.FromSI(val, ElectricCurrentQuantity.Instance.GetUnitChoice(ElectricCurrentQuantity.UnitChoicesEnum.SiemensVolt).ID);
            Assert.AreEqual(1.0 / Factors.Unit, unitVal);
            unitVal = ElectricCurrentQuantity.Instance.FromSI(val, ElectricCurrentQuantity.Instance.GetUnitChoice(ElectricCurrentQuantity.UnitChoicesEnum.Statampere).ID);
            Assert.AreEqual(2997924580, unitVal, 1e-4);
            unitVal = ElectricCurrentQuantity.Instance.FromSI(val, ElectricCurrentQuantity.Instance.GetUnitChoice(ElectricCurrentQuantity.UnitChoicesEnum.Teraampere).ID);
            Assert.AreEqual(1.0 / Factors.Tera, unitVal, 1e-4);
            unitVal = ElectricCurrentQuantity.Instance.FromSI(val, ElectricCurrentQuantity.Instance.GetUnitChoice(ElectricCurrentQuantity.UnitChoicesEnum.VoltPerOhm).ID);
            Assert.AreEqual(1.0 / Factors.Unit, unitVal, 1e-4);
            unitVal = ElectricCurrentQuantity.Instance.FromSI(val, ElectricCurrentQuantity.Instance.GetUnitChoice(ElectricCurrentQuantity.UnitChoicesEnum.WattPerVolt).ID);
            Assert.AreEqual(1.0 / Factors.Unit, unitVal, 1e-4);
            unitVal = ElectricCurrentQuantity.Instance.FromSI(val, ElectricCurrentQuantity.Instance.GetUnitChoice(ElectricCurrentQuantity.UnitChoicesEnum.WeberPerHenry).ID);
            Assert.AreEqual(1.0 / Factors.Unit, unitVal, 1e-4);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, ElectricCurrentQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(0, ElectricCurrentQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, ElectricCurrentQuantity.Instance.MassDimension);
            Assert.AreEqual(0, ElectricCurrentQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, ElectricCurrentQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(1, ElectricCurrentQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, ElectricCurrentQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, ElectricCurrentQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, ElectricCurrentQuantity.Instance.SolidAngleDimension);
        }
    }
}