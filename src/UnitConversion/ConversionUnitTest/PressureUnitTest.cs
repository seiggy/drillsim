using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class PressureUnitTest
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
            unitVal = PressureQuantity.Instance.FromSI(val, PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.Bar).ID);
            Assert.AreEqual(1.0 / Factors.Bar, unitVal);
            unitVal = PressureQuantity.Instance.FromSI(val, PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.CentimetreMercuryAtZeroDegreeCelsius).ID);
            Assert.AreEqual(1.0 / (Factors.Deca * Factors.MillimetreMercury), unitVal);
            unitVal = PressureQuantity.Instance.FromSI(val, PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.CentimetreWaterAt4DegreeCelsius).ID);
            Assert.AreEqual(1.0 / (Factors.Deca*Factors.MillimetreWater4degC), unitVal);
            unitVal = PressureQuantity.Instance.FromSI(val, PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.DynePerSquareCentimetre).ID);
            Assert.AreEqual(Factors.Centi*Factors.Centi/ Factors.Dyne, unitVal);
            unitVal = PressureQuantity.Instance.FromSI(val, PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.FootWaterAt4DegreeCelsius).ID);
            Assert.AreEqual(1.0 / Factors.FootWater4degC, unitVal);
            unitVal = PressureQuantity.Instance.FromSI(val, PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.Gigapascal).ID);
            Assert.AreEqual(1.0 / Factors.Giga, unitVal);
            unitVal = PressureQuantity.Instance.FromSI(val, PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.InchMercuryAt32DegreeFahrenheit).ID);
            Assert.AreEqual(1.0 / Factors.InchMercury32degF, unitVal);
            unitVal = PressureQuantity.Instance.FromSI(val, PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.InchMercuryAt60DegreeFahrenheit).ID);
            Assert.AreEqual(1.0 / Factors.InchMercury60degF, unitVal);
            unitVal = PressureQuantity.Instance.FromSI(val, PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.InchWaterAt4DegreeCelsius).ID);
            Assert.AreEqual(1.0 / Factors.InchWater4degC, unitVal);
            unitVal = PressureQuantity.Instance.FromSI(val, PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.KilonewtonPerSquareMetre).ID);
            Assert.AreEqual(1.0 / Factors.Kilo, unitVal);
            unitVal = PressureQuantity.Instance.FromSI(val, PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.Kilopascal).ID);
            Assert.AreEqual(1.0 / Factors.Kilo, unitVal);
            unitVal = PressureQuantity.Instance.FromSI(val, PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.KilopoundPerSquareInch).ID);
            Assert.AreEqual(1.0 / (Factors.Kilo*Factors.PSI), unitVal);
            unitVal = PressureQuantity.Instance.FromSI(val, PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.Megapascal).ID);
            Assert.AreEqual(1.0 / Factors.Mega , unitVal);
            unitVal = PressureQuantity.Instance.FromSI(val, PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.MegapoundPerSquareInch).ID);
            Assert.AreEqual(1.0 / (Factors.Mega * Factors.PSI), unitVal);
            unitVal = PressureQuantity.Instance.FromSI(val, PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.Microbar).ID);
            Assert.AreEqual(1.0 / (Factors.Micro * Factors.Bar), unitVal);
            unitVal = PressureQuantity.Instance.FromSI(val, PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.Millibar).ID);
            Assert.AreEqual(1.0 / (Factors.Milli * Factors.Bar), unitVal);
            unitVal = PressureQuantity.Instance.FromSI(val, PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.MillimetreMercuryAtZeroDegreeCelsius).ID);
            Assert.AreEqual(1.0 / Factors.MillimetreMercury, unitVal);
            unitVal = PressureQuantity.Instance.FromSI(val, PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.MillimetreWaterAt4DegreeCelsius).ID);
            Assert.AreEqual(1.0 / Factors.MillimetreWater4degC, unitVal);
            unitVal = PressureQuantity.Instance.FromSI(val, PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.NewtonPerSquareCentimetre).ID);
            Assert.AreEqual(Factors.Centi*Factors.Centi / Factors.Unit, unitVal);
            unitVal = PressureQuantity.Instance.FromSI(val, PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.NewtonPerSquareMetre).ID);
            Assert.AreEqual(1.0 / Factors.Unit, unitVal);
            unitVal = PressureQuantity.Instance.FromSI(val, PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.NewtonPerSquareMillimetre).ID);
            Assert.AreEqual(Factors.Milli * Factors.Milli / Factors.Unit, unitVal);
            unitVal = PressureQuantity.Instance.FromSI(val, PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.Pascal).ID);
            Assert.AreEqual(1.0, unitVal);
            unitVal = PressureQuantity.Instance.FromSI(val, PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.PoundPer100SquareFoot).ID);
            Assert.AreEqual(100.0*Factors.Foot * Factors.Foot / Factors.PoundForce, unitVal);
            unitVal = PressureQuantity.Instance.FromSI(val, PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.PoundPerSquareFoot).ID);
            Assert.AreEqual(Factors.Foot * Factors.Foot / Factors.PoundForce, unitVal);
            unitVal = PressureQuantity.Instance.FromSI(val, PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.PoundPerSquareInch).ID);
            Assert.AreEqual(Factors.Inch * Factors.Inch / Factors.PoundForce, unitVal, 1e-6);
            unitVal = PressureQuantity.Instance.FromSI(val, PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.StandardAtmosphere).ID);
            Assert.AreEqual(1.0 / Factors.Atmosphere, unitVal);
            unitVal = PressureQuantity.Instance.FromSI(val, PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.Torr).ID);
            Assert.AreEqual(1.0 / Factors.Torr, unitVal);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, PressureQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-2, PressureQuantity.Instance.TimeDimension);
            Assert.AreEqual(1, PressureQuantity.Instance.MassDimension);
            Assert.AreEqual(-1, PressureQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, PressureQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, PressureQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, PressureQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, PressureQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, PressureQuantity.Instance.SolidAngleDimension);
        }
    }
}