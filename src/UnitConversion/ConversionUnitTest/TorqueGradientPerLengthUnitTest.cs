using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class TorqueGradientPerLengthUnitTest
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
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.DecanewtonMetrePerCentimetre).ID);
            Assert.AreEqual(Factors.Centi/ (Factors.Deca), unitVal);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.DecanewtonMetrePerDecimetre).ID);
            Assert.AreEqual(Factors.Deci / (Factors.Deca), unitVal);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.DecanewtonMetrePerFoot).ID);
            Assert.AreEqual(Factors.Foot / (Factors.Deca), unitVal);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.DecanewtonMetrePerInch).ID);
            Assert.AreEqual(Factors.Inch / (Factors.Deca), unitVal);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.DecanewtonMetrePerMetre).ID);
            Assert.AreEqual(Factors.Unit / (Factors.Deca), unitVal);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.DecanewtonMetrePerMillimetre).ID);
            Assert.AreEqual(Factors.Milli / (Factors.Deca), unitVal);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.FootPoundPerCentimetre).ID);
            Assert.AreEqual(Factors.Centi / (Factors.Foot*Factors.PoundForce), unitVal);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.FootPoundPerDecimetre).ID);
            Assert.AreEqual(Factors.Deci / (Factors.Foot * Factors.PoundForce), unitVal);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.FootPoundPerFoot).ID);
            Assert.AreEqual(Factors.Foot / (Factors.Foot * Factors.PoundForce), unitVal);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.FootPoundPerInch).ID);
            Assert.AreEqual(Factors.Inch / (Factors.Foot * Factors.PoundForce), unitVal);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.FootPoundPerMetre).ID);
            Assert.AreEqual(Factors.Unit / (Factors.Foot * Factors.PoundForce), unitVal);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.FootPoundPerMillimetre).ID);
            Assert.AreEqual(Factors.Milli / (Factors.Foot * Factors.PoundForce), unitVal);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.InchPoundPerCentimetre).ID);
            Assert.AreEqual(Factors.Centi / (Factors.Inch * Factors.PoundForce), unitVal);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.InchPoundPerDecimetre).ID);
            Assert.AreEqual(Factors.Deci / (Factors.Inch * Factors.PoundForce), unitVal);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.InchPoundPerFoot).ID);
            Assert.AreEqual(Factors.Foot / (Factors.Inch * Factors.PoundForce), unitVal);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.InchPoundPerInch).ID);
            Assert.AreEqual(Factors.Inch / (Factors.Inch * Factors.PoundForce), unitVal);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.InchPoundPerMetre).ID);
            Assert.AreEqual(Factors.Unit / (Factors.Inch * Factors.PoundForce), unitVal);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.InchPoundPerMillimetre).ID);
            Assert.AreEqual(Factors.Milli / (Factors.Inch * Factors.PoundForce), unitVal);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.KilofootPoundPerCentimetre).ID);
            Assert.AreEqual(Factors.Centi / (Factors.Kilo*Factors.Foot * Factors.PoundForce), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.KilofootPoundPerDecimetre).ID);
            Assert.AreEqual(Factors.Deci / (Factors.Kilo * Factors.Foot * Factors.PoundForce), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.KilofootPoundPerFoot).ID);
            Assert.AreEqual(Factors.Foot / (Factors.Kilo * Factors.Foot * Factors.PoundForce), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.KilofootPoundPerInch).ID);
            Assert.AreEqual(Factors.Inch / (Factors.Kilo * Factors.Foot * Factors.PoundForce), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.KilofootPoundPerMetre).ID);
            Assert.AreEqual(Factors.Unit / (Factors.Kilo * Factors.Foot * Factors.PoundForce), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.KilofootPoundPerMillimetre).ID);
            Assert.AreEqual(Factors.Milli / (Factors.Kilo * Factors.Foot * Factors.PoundForce), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.KilogramForceMetrePerCentimetre).ID);
            Assert.AreEqual(Factors.Centi / (Factors.Unit * Factors.KilogramForce), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.KilogramForceMetrePerDecimetre).ID);
            Assert.AreEqual(Factors.Deci / (Factors.Unit * Factors.KilogramForce), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.KilogramForceMetrePerFoot).ID);
            Assert.AreEqual(Factors.Foot / (Factors.Unit * Factors.KilogramForce), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.KilogramForceMetrePerInch).ID);
            Assert.AreEqual(Factors.Inch / (Factors.Unit * Factors.KilogramForce), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.KilogramForceMetrePerMetre).ID);
            Assert.AreEqual(Factors.Unit / (Factors.Unit * Factors.KilogramForce), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.KilogramForceMetrePerMillimetre).ID);
            Assert.AreEqual(Factors.Milli / (Factors.Unit * Factors.KilogramForce), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.KilonewtonMetrePerCentimetre).ID);
            Assert.AreEqual(Factors.Centi / (Factors.Kilo * Factors.Unit), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.KilonewtonMetrePerDecimetre).ID);
            Assert.AreEqual(Factors.Deci / (Factors.Kilo * Factors.Unit), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.KilonewtonMetrePerFoot).ID);
            Assert.AreEqual(Factors.Foot / (Factors.Kilo * Factors.Unit), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.KilonewtonMetrePerInch).ID);
            Assert.AreEqual(Factors.Inch / (Factors.Kilo * Factors.Unit), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.KilonewtonMetrePerMetre).ID);
            Assert.AreEqual(Factors.Unit / (Factors.Kilo * Factors.Unit), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.KilonewtonMetrePerMillimetre).ID);
            Assert.AreEqual(Factors.Milli / (Factors.Kilo * Factors.Unit), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.NewtonCentimetrePerCentimetre).ID);
            Assert.AreEqual(Factors.Centi / (Factors.Unit * Factors.Centi), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.NewtonCentimetrePerDecimetre).ID);
            Assert.AreEqual(Factors.Deci / (Factors.Unit * Factors.Centi), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.NewtonCentimetrePerFoot).ID);
            Assert.AreEqual(Factors.Foot / (Factors.Unit * Factors.Centi), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.NewtonCentimetrePerInch).ID);
            Assert.AreEqual(Factors.Inch / (Factors.Unit * Factors.Centi), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.NewtonCentimetrePerMetre).ID);
            Assert.AreEqual(Factors.Unit / (Factors.Unit * Factors.Centi), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.NewtonCentimetrePerMillimetre).ID);
            Assert.AreEqual(Factors.Milli / (Factors.Unit * Factors.Centi), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.NewtonDecimetrePerCentimetre).ID);
            Assert.AreEqual(Factors.Centi / (Factors.Unit * Factors.Deci), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.NewtonDecimetrePerDecimetre).ID);
            Assert.AreEqual(Factors.Deci / (Factors.Unit * Factors.Deci), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.NewtonDecimetrePerFoot).ID);
            Assert.AreEqual(Factors.Foot / (Factors.Unit * Factors.Deci), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.NewtonDecimetrePerInch).ID);
            Assert.AreEqual(Factors.Inch / (Factors.Unit * Factors.Deci), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.NewtonDecimetrePerMetre).ID);
            Assert.AreEqual(Factors.Unit / (Factors.Unit * Factors.Deci), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.NewtonDecimetrePerMillimetre).ID);
            Assert.AreEqual(Factors.Milli / (Factors.Unit * Factors.Deci), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.NewtonMetrePerCentimetre).ID);
            Assert.AreEqual(Factors.Centi / (Factors.Unit * Factors.Unit), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.NewtonMetrePerDecimetre).ID);
            Assert.AreEqual(Factors.Deci / (Factors.Unit * Factors.Unit), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.NewtonMetrePerFoot).ID);
            Assert.AreEqual(Factors.Foot / (Factors.Unit * Factors.Unit), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.NewtonMetrePerInch).ID);
            Assert.AreEqual(Factors.Inch / (Factors.Unit * Factors.Unit), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.NewtonMetrePerMetre).ID);
            Assert.AreEqual(Factors.Unit / (Factors.Unit * Factors.Unit), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.NewtonMetrePerMillimetre).ID);
            Assert.AreEqual(Factors.Milli / (Factors.Unit * Factors.Unit), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.NewtonMillimetrePerCentimetre).ID);
            Assert.AreEqual(Factors.Centi / (Factors.Unit * Factors.Milli), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.NewtonMillimetrePerDecimetre).ID);
            Assert.AreEqual(Factors.Deci / (Factors.Unit * Factors.Milli), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.NewtonMillimetrePerFoot).ID);
            Assert.AreEqual(Factors.Foot / (Factors.Unit * Factors.Milli), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.NewtonMillimetrePerInch).ID);
            Assert.AreEqual(Factors.Inch / (Factors.Unit * Factors.Milli), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.NewtonMillimetrePerMetre).ID);
            Assert.AreEqual(Factors.Unit / (Factors.Unit * Factors.Milli), unitVal, 1e-6);
            unitVal = TorqueGradientPerLengthQuantity.Instance.FromSI(val, TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.NewtonMillimetrePerMillimetre).ID);
            Assert.AreEqual(Factors.Milli / (Factors.Unit * Factors.Milli), unitVal, 1e-6);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, TorqueGradientPerLengthQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-2, TorqueGradientPerLengthQuantity.Instance.TimeDimension);
            Assert.AreEqual(1, TorqueGradientPerLengthQuantity.Instance.MassDimension);
            Assert.AreEqual(1, TorqueGradientPerLengthQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, TorqueGradientPerLengthQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, TorqueGradientPerLengthQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, TorqueGradientPerLengthQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, TorqueGradientPerLengthQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, TorqueGradientPerLengthQuantity.Instance.SolidAngleDimension);
        }
    }
}