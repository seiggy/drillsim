using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class ForceGradientPerLengthUnitTest
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
            unitVal = ForceGradientPerLengthQuantity.Instance.FromSI(val, ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.DecanewtonPer10Metre).ID);
            Assert.AreEqual(10.0 / Factors.Deca, unitVal);
            unitVal = ForceGradientPerLengthQuantity.Instance.FromSI(val, ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.DecanewtonPer30Metre).ID);
            Assert.AreEqual(30.0 / Factors.Deca, unitVal);
            unitVal = ForceGradientPerLengthQuantity.Instance.FromSI(val, ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.DecanewtonPerCentimetre).ID);
            Assert.AreEqual(Factors.Centi / Factors.Deca, unitVal);
            unitVal = ForceGradientPerLengthQuantity.Instance.FromSI(val, ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.DecanewtonPerDecimetre).ID);
            Assert.AreEqual(Factors.Deci / Factors.Deca, unitVal);
            unitVal = ForceGradientPerLengthQuantity.Instance.FromSI(val, ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.DecanewtonPerMetre).ID);
            Assert.AreEqual(Factors.Unit / Factors.Deca, unitVal);
            unitVal = ForceGradientPerLengthQuantity.Instance.FromSI(val, ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.DecanewtonPerMillimetre).ID);
            Assert.AreEqual(Factors.Milli / Factors.Deca, unitVal);
            unitVal = ForceGradientPerLengthQuantity.Instance.FromSI(val, ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.KilonewtonPer10Metre).ID);
            Assert.AreEqual(Factors.Deca / Factors.Kilo, unitVal);
            unitVal = ForceGradientPerLengthQuantity.Instance.FromSI(val, ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.KilonewtonPer30Metre).ID);
            Assert.AreEqual(3 * Factors.Deca / Factors.Kilo, unitVal);
            unitVal = ForceGradientPerLengthQuantity.Instance.FromSI(val, ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.KilonewtonPerCentimetre).ID);
            Assert.AreEqual(Factors.Centi / Factors.Kilo, unitVal);
            unitVal = ForceGradientPerLengthQuantity.Instance.FromSI(val, ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.KilonewtonPerDecimetre).ID);
            Assert.AreEqual(Factors.Deci / Factors.Kilo, unitVal);
            unitVal = ForceGradientPerLengthQuantity.Instance.FromSI(val, ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.KilonewtonPerMetre).ID);
            Assert.AreEqual(Factors.Unit / Factors.Kilo, unitVal);
            unitVal = ForceGradientPerLengthQuantity.Instance.FromSI(val, ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.KilonewtonPerMillimetre).ID);
            Assert.AreEqual(Factors.Milli / Factors.Kilo, unitVal);
            unitVal = ForceGradientPerLengthQuantity.Instance.FromSI(val, ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.KilopoundPerFoot).ID);
            Assert.AreEqual(Factors.Foot / (Factors.Kilo * Factors.PoundForce), unitVal);
            unitVal = ForceGradientPerLengthQuantity.Instance.FromSI(val, ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.KilopoundPerInch).ID);
            Assert.AreEqual(Factors.Inch / (Factors.Kilo * Factors.PoundForce), unitVal);
            unitVal = ForceGradientPerLengthQuantity.Instance.FromSI(val, ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.NewtonPer10Metre).ID);
            Assert.AreEqual(Factors.Deca / Factors.Unit, unitVal);
            unitVal = ForceGradientPerLengthQuantity.Instance.FromSI(val, ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.NewtonPer30Metre).ID);
            Assert.AreEqual(3*Factors.Deca / Factors.Unit, unitVal);
            unitVal = ForceGradientPerLengthQuantity.Instance.FromSI(val, ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.NewtonPerCentimetre).ID);
            Assert.AreEqual(Factors.Centi / Factors.Unit, unitVal);
            unitVal = ForceGradientPerLengthQuantity.Instance.FromSI(val, ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.NewtonPerDecimetre).ID);
            Assert.AreEqual(Factors.Deci / Factors.Unit, unitVal);
            unitVal = ForceGradientPerLengthQuantity.Instance.FromSI(val, ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.NewtonPerMetre).ID);
            Assert.AreEqual(1, unitVal);
            unitVal = ForceGradientPerLengthQuantity.Instance.FromSI(val, ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.NewtonPerMillimetre).ID);
            Assert.AreEqual(Factors.Milli / Factors.Unit, unitVal);
            unitVal = ForceGradientPerLengthQuantity.Instance.FromSI(val, ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.PoundPerFoot).ID);
            Assert.AreEqual(Factors.Foot / (Factors.PoundForce), unitVal);
            unitVal = ForceGradientPerLengthQuantity.Instance.FromSI(val, ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.PoundPerInch).ID);
            Assert.AreEqual(Factors.Inch / (Factors.PoundForce), unitVal);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, ForceGradientPerLengthQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-2, ForceGradientPerLengthQuantity.Instance.TimeDimension);
            Assert.AreEqual(1, ForceGradientPerLengthQuantity.Instance.MassDimension);
            Assert.AreEqual(0, ForceGradientPerLengthQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, ForceGradientPerLengthQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, ForceGradientPerLengthQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, ForceGradientPerLengthQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, ForceGradientPerLengthQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, ForceGradientPerLengthQuantity.Instance.SolidAngleDimension);
        }
    }
}