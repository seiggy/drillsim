using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class VolumeUnitTest
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
            unitVal = VolumeQuantity.Instance.FromSI(val, VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.Barrel).ID);
            Assert.AreEqual(1.0 / Factors.Barrel, unitVal);
            unitVal = VolumeQuantity.Instance.FromSI(val, VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.Centilitre).ID);
            Assert.AreEqual(1.0 / (Factors.Centi*Factors.Milli), unitVal);
            unitVal = VolumeQuantity.Instance.FromSI(val, VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.CubicFoot).ID);
            Assert.AreEqual(1.0 / (Factors.Foot*Factors.Foot*Factors.Foot), unitVal);
            unitVal = VolumeQuantity.Instance.FromSI(val, VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.CubicInch).ID);
            Assert.AreEqual(1.0 / (Factors.Inch * Factors.Inch * Factors.Inch), unitVal);
            unitVal = VolumeQuantity.Instance.FromSI(val, VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.CubicMetre).ID);
            Assert.AreEqual(1.0 / Factors.Unit, unitVal);
            unitVal = VolumeQuantity.Instance.FromSI(val, VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.Decilitre).ID);
            Assert.AreEqual(1.0 / (Factors.Deci*Factors.Milli), unitVal);
            unitVal = VolumeQuantity.Instance.FromSI(val, VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.Litre).ID);
            Assert.AreEqual(1.0 / Factors.Milli, unitVal);
            unitVal = VolumeQuantity.Instance.FromSI(val, VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.Millilitre).ID);
            Assert.AreEqual(1.0 / (Factors.Milli*Factors.Milli), unitVal);
            unitVal = VolumeQuantity.Instance.FromSI(val, VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.MillionBarrel).ID);
            Assert.AreEqual(1.0 / (Factors.Mega*Factors.Barrel), unitVal);
            unitVal = VolumeQuantity.Instance.FromSI(val, VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.MillionCubicMetre).ID);
            Assert.AreEqual(1.0 / Factors.Mega, unitVal);
            unitVal = VolumeQuantity.Instance.FromSI(val, VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.MillionLitre).ID);
            Assert.AreEqual(1.0 / (Factors.Mega*Factors.Milli), unitVal);
            unitVal = VolumeQuantity.Instance.FromSI(val, VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.MillionStandardCubicFoot).ID);
            Assert.AreEqual(1.0 / (Factors.Mega*Factors.Foot*Factors.Foot*Factors.Foot), unitVal);
            unitVal = VolumeQuantity.Instance.FromSI(val, VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.MillionUKGallon).ID);
            Assert.AreEqual(1.0 / (Factors.Mega*Factors.GallonUK), unitVal);
            unitVal = VolumeQuantity.Instance.FromSI(val, VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.MillionUSGallon).ID);
            Assert.AreEqual(1.0 / (Factors.Mega * Factors.GallonUS), unitVal);
            unitVal = VolumeQuantity.Instance.FromSI(val, VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.ThousandStandardCubicFoot).ID);
            Assert.AreEqual(1.0 / (Factors.Kilo*Factors.Foot*Factors.Foot*Factors.Foot), unitVal);
            unitVal = VolumeQuantity.Instance.FromSI(val, VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.UKGallon).ID);
            Assert.AreEqual(1.0 / Factors.GallonUK, unitVal);
            unitVal = VolumeQuantity.Instance.FromSI(val, VolumeQuantity.Instance.GetUnitChoice(VolumeQuantity.UnitChoicesEnum.USGallon).ID);
            Assert.AreEqual(1.0 / Factors.GallonUS, unitVal);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, VolumeQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(0, VolumeQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, VolumeQuantity.Instance.MassDimension);
            Assert.AreEqual(3, VolumeQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, VolumeQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, VolumeQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, VolumeQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, VolumeQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, VolumeQuantity.Instance.SolidAngleDimension);
        }
    }
}