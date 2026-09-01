using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class TimeUnitTest
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
            unitVal = TimeQuantity.Instance.FromSI(val, TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.Century).ID);
            Assert.AreEqual(1.0 / Factors.Century, unitVal);
            unitVal = TimeQuantity.Instance.FromSI(val, TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.Day).ID);
            Assert.AreEqual(1.0 / Factors.Day, unitVal);
            unitVal = TimeQuantity.Instance.FromSI(val, TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.Decade).ID);
            Assert.AreEqual(1.0 / Factors.Decade, unitVal);
            unitVal = TimeQuantity.Instance.FromSI(val, TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.Fortnight).ID);
            Assert.AreEqual(1.0 / Factors.Fortnight, unitVal);
            unitVal = TimeQuantity.Instance.FromSI(val, TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.Hour).ID);
            Assert.AreEqual(1.0 / Factors.Hour, unitVal);
            unitVal = TimeQuantity.Instance.FromSI(val, TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.Microsecond).ID);
            Assert.AreEqual(1.0 / Factors.Micro, unitVal);
            unitVal = TimeQuantity.Instance.FromSI(val, TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.Millenia).ID);
            Assert.AreEqual(1.0 / Factors.Millenia, unitVal);
            unitVal = TimeQuantity.Instance.FromSI(val, TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.MillionYear).ID);
            Assert.AreEqual(1.0 / Factors.MillionYear, unitVal);
            unitVal = TimeQuantity.Instance.FromSI(val, TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.Millisecond).ID);
            Assert.AreEqual(1.0 / Factors.Milli, unitVal);
            unitVal = TimeQuantity.Instance.FromSI(val, TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.Minute).ID);
            Assert.AreEqual(1.0 / Factors.Minute, unitVal);
            unitVal = TimeQuantity.Instance.FromSI(val, TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.MonthCommon).ID);
            Assert.AreEqual(1.0 / Factors.MonthCommon, unitVal);
            unitVal = TimeQuantity.Instance.FromSI(val, TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.MonthSynodic).ID);
            Assert.AreEqual(1.0 / Factors.MonthSynodic, unitVal);
            unitVal = TimeQuantity.Instance.FromSI(val, TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.Nanosecond).ID);
            Assert.AreEqual(1.0 / Factors.Nano, unitVal);
            unitVal = TimeQuantity.Instance.FromSI(val, TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.Picosecond).ID);
            Assert.AreEqual(1.0 / Factors.Pico, unitVal);
            unitVal = TimeQuantity.Instance.FromSI(val, TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.QuarterCommon).ID);
            Assert.AreEqual(1.0 / Factors.QuarterCommon, unitVal);
            unitVal = TimeQuantity.Instance.FromSI(val, TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.Second).ID);
            Assert.AreEqual(1.0 / Factors.Unit, unitVal);
            unitVal = TimeQuantity.Instance.FromSI(val, TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.Shake).ID);
            Assert.AreEqual(1.0 / (10.0*Factors.Nano), unitVal);
            unitVal = TimeQuantity.Instance.FromSI(val, TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.Week).ID);
            Assert.AreEqual(1.0 / Factors.Week, unitVal);
            unitVal = TimeQuantity.Instance.FromSI(val, TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.YearAverageGregorian).ID);
            Assert.AreEqual(1.0 / Factors.YearAverageGregorian, unitVal);
            unitVal = TimeQuantity.Instance.FromSI(val, TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.YearJulian).ID);
            Assert.AreEqual(1.0 / Factors.YearJulian, unitVal);
            unitVal = TimeQuantity.Instance.FromSI(val, TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.YearLeap).ID);
            Assert.AreEqual(1.0 / Factors.YearLeap, unitVal);
            unitVal = TimeQuantity.Instance.FromSI(val, TimeQuantity.Instance.GetUnitChoice(TimeQuantity.UnitChoicesEnum.YearTropical).ID);
            Assert.AreEqual(1.0 / Factors.YearTropical, unitVal);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, TimeQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(1, TimeQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, TimeQuantity.Instance.MassDimension);
            Assert.AreEqual(0, TimeQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, TimeQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, TimeQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, TimeQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, TimeQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, TimeQuantity.Instance.SolidAngleDimension);
        }
    }
}