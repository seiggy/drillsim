using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class MassUnitTest
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
            unitVal = MassQuantity.Instance.FromSI(val, MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.AtomMassUnit).ID);
            Assert.AreEqual(6.0221366516752e+26, unitVal, 1e21);
            unitVal = MassQuantity.Instance.FromSI(val, MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.Centigram).ID);
            Assert.AreEqual(10000.0, unitVal);
            unitVal = MassQuantity.Instance.FromSI(val, MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.Decagram).ID);
            Assert.AreEqual(100.0, unitVal);
            unitVal = MassQuantity.Instance.FromSI(val, MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.EarthMass).ID);
            Assert.AreEqual(1.0/5.976e24, unitVal, 1e-27);
            unitVal = MassQuantity.Instance.FromSI(val, MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.Gigatonne).ID);
            Assert.AreEqual(1e-12, unitVal);
            unitVal = MassQuantity.Instance.FromSI(val, MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.Grain).ID);
            Assert.AreEqual(15432.35835294143, unitVal, 1e-6);
            unitVal = MassQuantity.Instance.FromSI(val, MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.Gram).ID);
            Assert.AreEqual(1000.0, unitVal);
            unitVal = MassQuantity.Instance.FromSI(val, MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.Hectogram).ID);
            Assert.AreEqual(10.0, unitVal);
            unitVal = MassQuantity.Instance.FromSI(val, MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.HundredWeights).ID);
            Assert.AreEqual(0.0220462262184878, unitVal, 1e-6);
            unitVal = MassQuantity.Instance.FromSI(val, MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.Kilogram).ID);
            Assert.AreEqual(1.0, unitVal);
            unitVal = MassQuantity.Instance.FromSI(val, MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.Kilopound).ID);
            Assert.AreEqual(0.002204622621849, unitVal, 1e-6);
            unitVal = MassQuantity.Instance.FromSI(val, MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.Kilotonne).ID);
            Assert.AreEqual(0.000001, unitVal);
            unitVal = MassQuantity.Instance.FromSI(val, MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.Megatonne).ID);
            Assert.AreEqual(1e-9, unitVal, 1e-12);
            unitVal = MassQuantity.Instance.FromSI(val, MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.Microgram).ID);
            Assert.AreEqual(1000000000, unitVal, 1e-6);
            unitVal = MassQuantity.Instance.FromSI(val, MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.Milligram).ID);
            Assert.AreEqual(1000000, unitVal, 1e-6);
            unitVal = MassQuantity.Instance.FromSI(val, MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.Nanogram).ID);
            Assert.AreEqual(1000000000000, unitVal, 1e-4);
            unitVal = MassQuantity.Instance.FromSI(val, MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.Ounce).ID);
            Assert.AreEqual(35.27396194958041, unitVal, 1e-6);
            unitVal = MassQuantity.Instance.FromSI(val, MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.Pound).ID);
            Assert.AreEqual(2.2046226218487757, unitVal, 1e-6);
            unitVal = MassQuantity.Instance.FromSI(val, MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.SolarMass).ID);
            Assert.AreEqual(5.02739933e-31, unitVal, 1e-32);
            unitVal = MassQuantity.Instance.FromSI(val, MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.Stone).ID);
            Assert.AreEqual(0.157473044, unitVal, 1e-6);
            unitVal = MassQuantity.Instance.FromSI(val, MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.TonneMetric).ID);
            Assert.AreEqual(0.001, unitVal, 1e-6);
            unitVal = MassQuantity.Instance.FromSI(val, MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.TonUK).ID);
            Assert.AreEqual(0.000984207, unitVal, 1e-6);
            unitVal = MassQuantity.Instance.FromSI(val, MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.TonUS).ID);
            Assert.AreEqual(0.00110231131, unitVal, 1e-6);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, MassQuantity.Instance.SolidAngleDimension);
            Assert.AreEqual(0, MassQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(0, MassQuantity.Instance.TimeDimension);
            Assert.AreEqual(1, MassQuantity.Instance.MassDimension);
            Assert.AreEqual(0, MassQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, MassQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, MassQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, MassQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, MassQuantity.Instance.LuminousIntensityDimension);
        }
    }
}