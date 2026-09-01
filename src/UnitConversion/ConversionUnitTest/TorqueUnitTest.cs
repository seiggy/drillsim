using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class TorqueUnitTest
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
            unitVal = TorqueQuantity.Instance.FromSI(val, TorqueQuantity.Instance.GetUnitChoice(TorqueQuantity.UnitChoicesEnum.DecanewtonMetre).ID);
            Assert.AreEqual(1.0 / Factors.Deca, unitVal);
            unitVal = TorqueQuantity.Instance.FromSI(val, TorqueQuantity.Instance.GetUnitChoice(TorqueQuantity.UnitChoicesEnum.FootPound).ID);
            Assert.AreEqual(1.0 / (Factors.Foot*Factors.PoundForce), unitVal);
            unitVal = TorqueQuantity.Instance.FromSI(val, TorqueQuantity.Instance.GetUnitChoice(TorqueQuantity.UnitChoicesEnum.InchPound).ID);
            Assert.AreEqual(1.0 / (Factors.Inch*Factors.PoundForce), unitVal);
            unitVal = TorqueQuantity.Instance.FromSI(val, TorqueQuantity.Instance.GetUnitChoice(TorqueQuantity.UnitChoicesEnum.KilofootPound).ID);
            Assert.AreEqual(1.0 / (Factors.Kilo*Factors.Foot * Factors.PoundForce), unitVal);
            unitVal = TorqueQuantity.Instance.FromSI(val, TorqueQuantity.Instance.GetUnitChoice(TorqueQuantity.UnitChoicesEnum.KilogramForceMetre).ID);
            Assert.AreEqual(1.0 / (Factors.KilogramForce), unitVal);
            unitVal = TorqueQuantity.Instance.FromSI(val, TorqueQuantity.Instance.GetUnitChoice(TorqueQuantity.UnitChoicesEnum.KilonewtonMetre).ID);
            Assert.AreEqual(1.0 / Factors.Kilo, unitVal);
            unitVal = TorqueQuantity.Instance.FromSI(val, TorqueQuantity.Instance.GetUnitChoice(TorqueQuantity.UnitChoicesEnum.NewtonCentimetre).ID);
            Assert.AreEqual(1.0 / Factors.Centi, unitVal);
            unitVal = TorqueQuantity.Instance.FromSI(val, TorqueQuantity.Instance.GetUnitChoice(TorqueQuantity.UnitChoicesEnum.NewtonDecimetre).ID);
            Assert.AreEqual(1.0 / Factors.Deci, unitVal);
            unitVal = TorqueQuantity.Instance.FromSI(val, TorqueQuantity.Instance.GetUnitChoice(TorqueQuantity.UnitChoicesEnum.NewtonMetre).ID);
            Assert.AreEqual(1.0 / Factors.Unit, unitVal);
            unitVal = TorqueQuantity.Instance.FromSI(val, TorqueQuantity.Instance.GetUnitChoice(TorqueQuantity.UnitChoicesEnum.NewtonMillimetre).ID);
            Assert.AreEqual(1.0 / Factors.Milli, unitVal);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, TorqueQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-2, TorqueQuantity.Instance.TimeDimension);
            Assert.AreEqual(1, TorqueQuantity.Instance.MassDimension);
            Assert.AreEqual(2, TorqueQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, TorqueQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, TorqueQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, TorqueQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, TorqueQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, TorqueQuantity.Instance.SolidAngleDimension);
        }
    }
}