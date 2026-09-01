using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class MassRateUnitTest
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
            unitVal = MassRateQuantity.Instance.FromSI(val, MassRateQuantity.Instance.GetUnitChoice(MassRateQuantity.UnitChoicesEnum.KilogramPerHour).ID);
            Assert.AreEqual(Factors.Hour / Factors.Unit, unitVal);
            unitVal = MassRateQuantity.Instance.FromSI(val, MassRateQuantity.Instance.GetUnitChoice(MassRateQuantity.UnitChoicesEnum.KilogramPerMinute).ID);
            Assert.AreEqual(Factors.Minute / Factors.Unit, unitVal);
            unitVal = MassRateQuantity.Instance.FromSI(val, MassRateQuantity.Instance.GetUnitChoice(MassRateQuantity.UnitChoicesEnum.KilogramPerSecond).ID);
            Assert.AreEqual(Factors.Unit / Factors.Unit, unitVal);
            unitVal = MassRateQuantity.Instance.FromSI(val, MassRateQuantity.Instance.GetUnitChoice(MassRateQuantity.UnitChoicesEnum.KilogramPerYear).ID);
            Assert.AreEqual(Factors.YearJulian / Factors.Unit, unitVal);
            unitVal = MassRateQuantity.Instance.FromSI(val, MassRateQuantity.Instance.GetUnitChoice(MassRateQuantity.UnitChoicesEnum.PoundPerHour).ID);
            Assert.AreEqual(Factors.Hour / Factors.Pound, unitVal);
            unitVal = MassRateQuantity.Instance.FromSI(val, MassRateQuantity.Instance.GetUnitChoice(MassRateQuantity.UnitChoicesEnum.PoundPerMinute).ID);
            Assert.AreEqual(Factors.Minute / Factors.Pound, unitVal);
            unitVal = MassRateQuantity.Instance.FromSI(val, MassRateQuantity.Instance.GetUnitChoice(MassRateQuantity.UnitChoicesEnum.PoundPerSecond).ID);
            Assert.AreEqual(Factors.Unit / Factors.Pound, unitVal);
            unitVal = MassRateQuantity.Instance.FromSI(val, MassRateQuantity.Instance.GetUnitChoice(MassRateQuantity.UnitChoicesEnum.PoundPerYear).ID);
            Assert.AreEqual(Factors.YearJulian / Factors.Pound, unitVal);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, MassRateQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-1, MassRateQuantity.Instance.TimeDimension);
            Assert.AreEqual(1, MassRateQuantity.Instance.MassDimension);
            Assert.AreEqual(0, MassRateQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, MassRateQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, MassRateQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, MassRateQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, MassRateQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, MassRateQuantity.Instance.SolidAngleDimension);
        }
    }
}