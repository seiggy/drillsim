using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class EnergyDensityUnitTest
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
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.BritishThermalUnitPerCubicFoot).ID);
            Assert.AreEqual(2.683e-5, unitVal, 1e-7);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.BritishThermalUnitPerCubicInch).ID);
            Assert.AreEqual(1.553e-8, unitVal, 1e-10);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.BritishThermalUnitPerCubicMetre).ID);
            Assert.AreEqual(0.000947817, unitVal, 1e-6);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.BritishThermalUnitPerGallonUK).ID);
            Assert.AreEqual(4.309e-6, unitVal, 1e-8);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.BritishThermalUnitPerGallonUS).ID);
            Assert.AreEqual(3.588e-6, unitVal, 1e-8);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.BritishThermalUnitPerLitre).ID);
            Assert.AreEqual(9.47817e-7, unitVal, 1e-9);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.CaloriePerCubicFoot).ID);
            Assert.AreEqual(0.006791, unitVal, 1e-4);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.CaloriePerCubicInch).ID);
            Assert.AreEqual(3.917e-6, unitVal, 1e-8);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.CaloriePerCubicMetre).ID);
            Assert.AreEqual(0.239005736, unitVal, 1e-6);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.CaloriePerGallonUK).ID);
            Assert.AreEqual(0.001087, unitVal, 1e-6);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.CaloriePerGallonUS).ID);
            Assert.AreEqual(0.000905, unitVal, 1e-6);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.GigajoulePerCubicFoot).ID);
            Assert.AreEqual(2.83168e-11, unitVal, 1e-14);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.GigajoulePerCubicInch).ID);
            Assert.AreEqual(1.63787e-14, unitVal, 1e-17);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.GigajoulePerLitre).ID);
            Assert.AreEqual(1e-12, unitVal);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.GigajoulePerCubicMetre).ID);
            Assert.AreEqual(1e-9, unitVal);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.GigajoulePerGallonUK).ID);
            Assert.AreEqual(4.548e-12, unitVal, 1e-14);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.GigajoulePerGallonUS).ID);
            Assert.AreEqual(3.782e-12, unitVal, 1e-14);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.JoulePerCubicFoot).ID);
            Assert.AreEqual(2.83168e-2, unitVal, 1e-5);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.JoulePerCubicInch).ID);
            Assert.AreEqual(1.63787e-5, unitVal, 1e-8);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.JoulePerLitre).ID);
            Assert.AreEqual(0.001, unitVal);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.JoulePerCubicMetre).ID);
            Assert.AreEqual(1.0, unitVal);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.JoulePerGallonUK).ID);
            Assert.AreEqual(4.548e-3, unitVal, 1e-5);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.JoulePerGallonUS).ID);
            Assert.AreEqual(3.782e-3, unitVal, 1e-5);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.KiloBritishThermalUnitPerCubicFoot).ID);
            Assert.AreEqual(2.683e-8, unitVal, 1e-10);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.KiloBritishThermalUnitPerCubicInch).ID);
            Assert.AreEqual(1.553e-11, unitVal, 1e-13);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.KiloBritishThermalUnitPerCubicMetre).ID);
            Assert.AreEqual(0.000947817e-3, unitVal, 1e-9);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.KiloBritishThermalUnitPerGallonUK).ID);
            Assert.AreEqual(4.309e-9, unitVal, 1e-11);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.KiloBritishThermalUnitPerGallonUS).ID);
            Assert.AreEqual(3.588e-9, unitVal, 1e-11);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.KiloBritishThermalUnitPerLitre).ID);
            Assert.AreEqual(9.47817e-10, unitVal, 1e-12);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.KilocaloriePerCubicFoot).ID);
            Assert.AreEqual(0.000006791, unitVal, 1e-7);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.KilocaloriePerCubicInch).ID);
            Assert.AreEqual(3.917e-9, unitVal, 1e-11);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.KilocaloriePerLitre).ID);
            Assert.AreEqual(2.39e-7, unitVal, 1e-9);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.KilocaloriePerCubicMetre).ID);
            Assert.AreEqual(0.000239005736, unitVal, 1e-6);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.KilocaloriePerGallonUK).ID);
            Assert.AreEqual(0.000001087, unitVal, 1e-8);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.KilocaloriePerGallonUS).ID);
            Assert.AreEqual(0.000000905, unitVal, 1e-8);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.KilojoulePerCubicFoot).ID);
            Assert.AreEqual(2.83168e-5, unitVal, 1e-7);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.KilojoulePerCubicInch).ID);
            Assert.AreEqual(1.63787e-8, unitVal,1e-10);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.KilojoulePerLitre).ID);
            Assert.AreEqual(1e-6, unitVal);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.KilojoulePerCubicMetre).ID);
            Assert.AreEqual(0.001, unitVal);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.KilojoulePerGallonUK).ID);
            Assert.AreEqual(4.548e-6, unitVal, 1e-8);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.KilojoulePerGallonUS).ID);
            Assert.AreEqual(3.782e-6, unitVal, 1e-8);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.MegajoulePerCubicFoot).ID);
            Assert.AreEqual(2.83168e-8, unitVal,1e-11);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.MegajoulePerCubicInch).ID);
            Assert.AreEqual(1.63787e-11, unitVal, 1e-14);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.MegajoulePerCubicMetre).ID);
            Assert.AreEqual(1e-6, unitVal);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.MegajoulePerGallonUK).ID);
            Assert.AreEqual(4.548e-9, unitVal, 1e-11);
            unitVal = EnergyDensityQuantity.Instance.FromSI(val, EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.MegajoulePerGallonUS).ID);
            Assert.AreEqual(3.782e-9, unitVal, 1e-11);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, EnergyDensityQuantity.Instance.SolidAngleDimension);
            Assert.AreEqual(0, EnergyDensityQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-2, EnergyDensityQuantity.Instance.TimeDimension);
            Assert.AreEqual(1, EnergyDensityQuantity.Instance.MassDimension);
            Assert.AreEqual(-1, EnergyDensityQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, EnergyDensityQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, EnergyDensityQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, EnergyDensityQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, EnergyDensityQuantity.Instance.LuminousIntensityDimension);
        }
    }
}