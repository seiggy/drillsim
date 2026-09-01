using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class FrequencyRateOfChangeUnitTest
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
            unitVal = FrequencyRateOfChangeQuantity.Instance.FromSI(val, FrequencyRateOfChangeQuantity.Instance.GetUnitChoice(FrequencyRateOfChangeQuantity.UnitChoicesEnum.GigaHertzPerDay).ID);
            Assert.AreEqual(Factors.Day / Factors.Giga, unitVal);
            unitVal = FrequencyRateOfChangeQuantity.Instance.FromSI(val, FrequencyRateOfChangeQuantity.Instance.GetUnitChoice(FrequencyRateOfChangeQuantity.UnitChoicesEnum.GigaHertzPerHour).ID);
            Assert.AreEqual(Factors.Hour / Factors.Giga, unitVal);
            unitVal = FrequencyRateOfChangeQuantity.Instance.FromSI(val, FrequencyRateOfChangeQuantity.Instance.GetUnitChoice(FrequencyRateOfChangeQuantity.UnitChoicesEnum.GigaHertzPerMinute).ID);
            Assert.AreEqual(Factors.Minute / Factors.Giga, unitVal);
            unitVal = FrequencyRateOfChangeQuantity.Instance.FromSI(val, FrequencyRateOfChangeQuantity.Instance.GetUnitChoice(FrequencyRateOfChangeQuantity.UnitChoicesEnum.GigaHertzPerSecond).ID);
            Assert.AreEqual(Factors.Unit / Factors.Giga, unitVal);
            unitVal = FrequencyRateOfChangeQuantity.Instance.FromSI(val, FrequencyRateOfChangeQuantity.Instance.GetUnitChoice(FrequencyRateOfChangeQuantity.UnitChoicesEnum.GigaHertzPerYear).ID);
            Assert.AreEqual(Factors.YearJulian / Factors.Giga, unitVal);
            unitVal = FrequencyRateOfChangeQuantity.Instance.FromSI(val, FrequencyRateOfChangeQuantity.Instance.GetUnitChoice(FrequencyRateOfChangeQuantity.UnitChoicesEnum.HertzPerDay).ID);
            Assert.AreEqual(Factors.Day / Factors.Unit, unitVal);
            unitVal = FrequencyRateOfChangeQuantity.Instance.FromSI(val, FrequencyRateOfChangeQuantity.Instance.GetUnitChoice(FrequencyRateOfChangeQuantity.UnitChoicesEnum.HertzPerHour).ID);
            Assert.AreEqual(Factors.Hour / Factors.Unit, unitVal);
            unitVal = FrequencyRateOfChangeQuantity.Instance.FromSI(val, FrequencyRateOfChangeQuantity.Instance.GetUnitChoice(FrequencyRateOfChangeQuantity.UnitChoicesEnum.HertzPerMinute).ID);
            Assert.AreEqual(Factors.Minute / Factors.Unit, unitVal);
            unitVal = FrequencyRateOfChangeQuantity.Instance.FromSI(val, FrequencyRateOfChangeQuantity.Instance.GetUnitChoice(FrequencyRateOfChangeQuantity.UnitChoicesEnum.HertzPerSecond).ID);
            Assert.AreEqual(Factors.Unit / Factors.Unit, unitVal);
            unitVal = FrequencyRateOfChangeQuantity.Instance.FromSI(val, FrequencyRateOfChangeQuantity.Instance.GetUnitChoice(FrequencyRateOfChangeQuantity.UnitChoicesEnum.HertzPerYear).ID);
            Assert.AreEqual(Factors.YearJulian / Factors.Unit, unitVal);
            unitVal = FrequencyRateOfChangeQuantity.Instance.FromSI(val, FrequencyRateOfChangeQuantity.Instance.GetUnitChoice(FrequencyRateOfChangeQuantity.UnitChoicesEnum.KiloHertzPerDay).ID);
            Assert.AreEqual(Factors.Day / Factors.Kilo, unitVal);
            unitVal = FrequencyRateOfChangeQuantity.Instance.FromSI(val, FrequencyRateOfChangeQuantity.Instance.GetUnitChoice(FrequencyRateOfChangeQuantity.UnitChoicesEnum.KiloHertzPerHour).ID);
            Assert.AreEqual(Factors.Hour / Factors.Kilo, unitVal);
            unitVal = FrequencyRateOfChangeQuantity.Instance.FromSI(val, FrequencyRateOfChangeQuantity.Instance.GetUnitChoice(FrequencyRateOfChangeQuantity.UnitChoicesEnum.KiloHertzPerMinute).ID);
            Assert.AreEqual(Factors.Minute / Factors.Kilo, unitVal);
            unitVal = FrequencyRateOfChangeQuantity.Instance.FromSI(val, FrequencyRateOfChangeQuantity.Instance.GetUnitChoice(FrequencyRateOfChangeQuantity.UnitChoicesEnum.KiloHertzPerSecond).ID);
            Assert.AreEqual(Factors.Unit / Factors.Kilo, unitVal);
            unitVal = FrequencyRateOfChangeQuantity.Instance.FromSI(val, FrequencyRateOfChangeQuantity.Instance.GetUnitChoice(FrequencyRateOfChangeQuantity.UnitChoicesEnum.KiloHertzPerYear).ID);
            Assert.AreEqual(Factors.YearJulian / Factors.Kilo, unitVal);
            unitVal = FrequencyRateOfChangeQuantity.Instance.FromSI(val, FrequencyRateOfChangeQuantity.Instance.GetUnitChoice(FrequencyRateOfChangeQuantity.UnitChoicesEnum.MegaHertzPerDay).ID);
            Assert.AreEqual(Factors.Day / Factors.Mega, unitVal);
            unitVal = FrequencyRateOfChangeQuantity.Instance.FromSI(val, FrequencyRateOfChangeQuantity.Instance.GetUnitChoice(FrequencyRateOfChangeQuantity.UnitChoicesEnum.MegaHertzPerHour).ID);
            Assert.AreEqual(Factors.Hour / Factors.Mega, unitVal);
            unitVal = FrequencyRateOfChangeQuantity.Instance.FromSI(val, FrequencyRateOfChangeQuantity.Instance.GetUnitChoice(FrequencyRateOfChangeQuantity.UnitChoicesEnum.MegaHertzPerMinute).ID);
            Assert.AreEqual(Factors.Minute / Factors.Mega, unitVal);
            unitVal = FrequencyRateOfChangeQuantity.Instance.FromSI(val, FrequencyRateOfChangeQuantity.Instance.GetUnitChoice(FrequencyRateOfChangeQuantity.UnitChoicesEnum.MegaHertzPerSecond).ID);
            Assert.AreEqual(Factors.Unit / Factors.Mega, unitVal);
            unitVal = FrequencyRateOfChangeQuantity.Instance.FromSI(val, FrequencyRateOfChangeQuantity.Instance.GetUnitChoice(FrequencyRateOfChangeQuantity.UnitChoicesEnum.MegaHertzPerYear).ID);
            Assert.AreEqual(Factors.YearJulian / Factors.Mega, unitVal);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, FrequencyRateOfChangeQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-2, FrequencyRateOfChangeQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, FrequencyRateOfChangeQuantity.Instance.MassDimension);
            Assert.AreEqual(0, FrequencyRateOfChangeQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, FrequencyRateOfChangeQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, FrequencyRateOfChangeQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, FrequencyRateOfChangeQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, FrequencyRateOfChangeQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, FrequencyRateOfChangeQuantity.Instance.SolidAngleDimension);
        }
    }
}