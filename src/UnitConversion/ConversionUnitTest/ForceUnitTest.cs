using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class ForceUnitTest
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
            unitVal = ForceQuantity.Instance.FromSI(val, ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.Decanewton).ID);
            Assert.AreEqual(0.1, unitVal);
            unitVal = ForceQuantity.Instance.FromSI(val, ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.Kilodecanewton).ID);
            Assert.AreEqual(0.0001, unitVal);
            unitVal = ForceQuantity.Instance.FromSI(val, ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.KilogramForce).ID);
            Assert.AreEqual(0.10197162129779283, unitVal, 1e-6);
            unitVal = ForceQuantity.Instance.FromSI(val, ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.Kilonewton).ID);
            Assert.AreEqual(0.001, unitVal);
            unitVal = ForceQuantity.Instance.FromSI(val, ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.KilopoundForce).ID);
            Assert.AreEqual(0.00022480894387096184, unitVal, 1e-8);
            unitVal = ForceQuantity.Instance.FromSI(val, ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.Newton).ID);
            Assert.AreEqual(1, unitVal);
            unitVal = ForceQuantity.Instance.FromSI(val, ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.PoundForce).ID);
            Assert.AreEqual(0.2248089438709618, unitVal, 1e-6);
            unitVal = ForceQuantity.Instance.FromSI(val, ForceQuantity.Instance.GetUnitChoice(ForceQuantity.UnitChoicesEnum.TonneForce).ID);
            Assert.AreEqual(0.00010197162129779283, unitVal, 1e-6);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, ForceQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-2, ForceQuantity.Instance.TimeDimension);
            Assert.AreEqual(1, ForceQuantity.Instance.MassDimension);
            Assert.AreEqual(1, ForceQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, ForceQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, ForceQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, ForceQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, ForceQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, ForceQuantity.Instance.SolidAngleDimension);
        }
    }
}