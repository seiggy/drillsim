using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class AccelerationUnitTest
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
            unitVal = AccelerationQuantity.Instance.FromSI(val, AccelerationQuantity.Instance.GetUnitChoice(AccelerationQuantity.UnitChoicesEnum.CentimetrePerHourPerSecond).ID);
            Assert.AreEqual(Factors.Hour / Factors.Centi, unitVal, 1e-6);
            unitVal = AccelerationQuantity.Instance.FromSI(val, AccelerationQuantity.Instance.GetUnitChoice(AccelerationQuantity.UnitChoicesEnum.CentimetrePerMinutePerSecond).ID);
            Assert.AreEqual(Factors.Minute / Factors.Centi, unitVal, 1e-6);
            unitVal = AccelerationQuantity.Instance.FromSI(val, AccelerationQuantity.Instance.GetUnitChoice(AccelerationQuantity.UnitChoicesEnum.CentimetrePerSecondSquared).ID);
            Assert.AreEqual(Factors.Unit / Factors.Centi, unitVal, 1e-6);
            unitVal = AccelerationQuantity.Instance.FromSI(val, AccelerationQuantity.Instance.GetUnitChoice(AccelerationQuantity.UnitChoicesEnum.CentimetrePerHourPerSecond).ID);
            Assert.AreEqual(Factors.Hour / Factors.Centi, unitVal, 1e-6);
            unitVal = AccelerationQuantity.Instance.FromSI(val, AccelerationQuantity.Instance.GetUnitChoice(AccelerationQuantity.UnitChoicesEnum.FootPerHourPerSecond).ID);
            Assert.AreEqual(Factors.Hour / Factors.Foot, unitVal, 1e-6);
            unitVal = AccelerationQuantity.Instance.FromSI(val, AccelerationQuantity.Instance.GetUnitChoice(AccelerationQuantity.UnitChoicesEnum.FootPerMinutePerSecond).ID);
            Assert.AreEqual(Factors.Minute / Factors.Foot, unitVal, 1e-6);
            unitVal = AccelerationQuantity.Instance.FromSI(val, AccelerationQuantity.Instance.GetUnitChoice(AccelerationQuantity.UnitChoicesEnum.FootPerSecondSquared).ID);
            Assert.AreEqual(Factors.Unit / Factors.Foot, unitVal, 1e-6);
            unitVal = AccelerationQuantity.Instance.FromSI(val, AccelerationQuantity.Instance.GetUnitChoice(AccelerationQuantity.UnitChoicesEnum.Galileo).ID);
            Assert.AreEqual(Factors.Unit / Factors.Centi, unitVal, 1e-6);
            unitVal = AccelerationQuantity.Instance.FromSI(val, AccelerationQuantity.Instance.GetUnitChoice(AccelerationQuantity.UnitChoicesEnum.GravityStandard).ID);
            Assert.AreEqual(Factors.Unit / Factors.G, unitVal, 1e-6);
            unitVal = AccelerationQuantity.Instance.FromSI(val, AccelerationQuantity.Instance.GetUnitChoice(AccelerationQuantity.UnitChoicesEnum.InchPerHourPerSecond).ID);
            Assert.AreEqual(Factors.Hour / Factors.Inch, unitVal, 1e-6);
            unitVal = AccelerationQuantity.Instance.FromSI(val, AccelerationQuantity.Instance.GetUnitChoice(AccelerationQuantity.UnitChoicesEnum.InchPerMinutePerSecond).ID);
            Assert.AreEqual(Factors.Minute / Factors.Inch, unitVal, 1e-6);
            unitVal = AccelerationQuantity.Instance.FromSI(val, AccelerationQuantity.Instance.GetUnitChoice(AccelerationQuantity.UnitChoicesEnum.InchPerSecondSquared).ID);
            Assert.AreEqual(Factors.Unit / Factors.Inch, unitVal, 1e-6);
            unitVal = AccelerationQuantity.Instance.FromSI(val, AccelerationQuantity.Instance.GetUnitChoice(AccelerationQuantity.UnitChoicesEnum.KilometrePerHourPerSecond).ID);
            Assert.AreEqual(Factors.Hour / Factors.Kilo, unitVal, 1e-6);
            unitVal = AccelerationQuantity.Instance.FromSI(val, AccelerationQuantity.Instance.GetUnitChoice(AccelerationQuantity.UnitChoicesEnum.KilometrePerMinutePerSecond).ID);
            Assert.AreEqual(Factors.Minute / Factors.Kilo, unitVal, 1e-6);
            unitVal = AccelerationQuantity.Instance.FromSI(val, AccelerationQuantity.Instance.GetUnitChoice(AccelerationQuantity.UnitChoicesEnum.KilometrePerSecondSquared).ID);
            Assert.AreEqual(Factors.Unit / Factors.Kilo, unitVal, 1e-6);
            unitVal = AccelerationQuantity.Instance.FromSI(val, AccelerationQuantity.Instance.GetUnitChoice(AccelerationQuantity.UnitChoicesEnum.KnotPerSecond).ID);
            Assert.AreEqual(Factors.Unit / Factors.Knot, unitVal, 1e-6);
            unitVal = AccelerationQuantity.Instance.FromSI(val, AccelerationQuantity.Instance.GetUnitChoice(AccelerationQuantity.UnitChoicesEnum.MetrePerSecondPerMillisecond).ID);
            Assert.AreEqual(Factors.Milli / Factors.Unit, unitVal, 1e-6);
            unitVal = AccelerationQuantity.Instance.FromSI(val, AccelerationQuantity.Instance.GetUnitChoice(AccelerationQuantity.UnitChoicesEnum.MetrePerSecondSquared).ID);
            Assert.AreEqual(Factors.Unit / Factors.Unit, unitVal, 1e-6);
            unitVal = AccelerationQuantity.Instance.FromSI(val, AccelerationQuantity.Instance.GetUnitChoice(AccelerationQuantity.UnitChoicesEnum.MilePerHourPerSecond).ID);
            Assert.AreEqual(Factors.Hour / Factors.Mile, unitVal, 1e-6);
            unitVal = AccelerationQuantity.Instance.FromSI(val, AccelerationQuantity.Instance.GetUnitChoice(AccelerationQuantity.UnitChoicesEnum.MilePerMinutePerSecond).ID);
            Assert.AreEqual(Factors.Minute / Factors.Mile, unitVal, 1e-6);
            unitVal = AccelerationQuantity.Instance.FromSI(val, AccelerationQuantity.Instance.GetUnitChoice(AccelerationQuantity.UnitChoicesEnum.MilePerSecondSquared).ID);
            Assert.AreEqual(Factors.Unit / Factors.Mile, unitVal, 1e-6);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, AccelerationQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-2, AccelerationQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, AccelerationQuantity.Instance.MassDimension);
            Assert.AreEqual(1, AccelerationQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, AccelerationQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, AccelerationQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, AccelerationQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, AccelerationQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, AccelerationQuantity.Instance.SolidAngleDimension);
        }
    }
}