using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class VelocityUnitTest
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
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.CentimetrePerSecond).ID);
            Assert.AreEqual(1.0 / Factors.Centi, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.CentimetrePerDay).ID);
            Assert.AreEqual(Factors.Day / Factors.Centi, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.CentimetrePerHour).ID);
            Assert.AreEqual(Factors.Hour / Factors.Centi, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.CentimetrePerMinute).ID);
            Assert.AreEqual(Factors.Minute / Factors.Centi, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.FootPerDay).ID);
            Assert.AreEqual(Factors.Day / Factors.Foot, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.FootPerHour).ID);
            Assert.AreEqual(Factors.Hour / Factors.Foot, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.FootPerMinute).ID);
            Assert.AreEqual(Factors.Minute / Factors.Foot, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.FootPerSecond).ID);
            Assert.AreEqual(Factors.Unit / Factors.Foot, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.InchPerSecond).ID);
            Assert.AreEqual(Factors.Unit / Factors.Inch, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.InchPerDay).ID);
            Assert.AreEqual(Factors.Day / Factors.Inch, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.InchPerHour).ID);
            Assert.AreEqual(Factors.Hour / Factors.Inch, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.InchPerMinute).ID);
            Assert.AreEqual(Factors.Minute / Factors.Inch, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.MetrePerDay).ID);
            Assert.AreEqual(Factors.Day / Factors.Unit, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.MetrePerHour).ID);
            Assert.AreEqual(Factors.Hour / Factors.Unit, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.MetrePerMinute).ID);
            Assert.AreEqual(Factors.Minute / Factors.Unit, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.MetrePerSecond).ID);
            Assert.AreEqual(Factors.Unit / Factors.Unit, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.MilePerDay).ID);
            Assert.AreEqual(Factors.Day / Factors.Mile, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.MilePerHour).ID);
            Assert.AreEqual(Factors.Hour / Factors.Mile, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.MilePerMinute).ID);
            Assert.AreEqual(Factors.Minute / Factors.Mile, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.MilePerSecond).ID);
            Assert.AreEqual(Factors.Unit / Factors.Mile, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.KilometrePerDay).ID);
            Assert.AreEqual(Factors.Day / Factors.Kilo, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.KilometrePerHour).ID);
            Assert.AreEqual(Factors.Hour / Factors.Kilo, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.KilometrePerMinute).ID);
            Assert.AreEqual(Factors.Minute / Factors.Kilo, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.KilometrePerSecond).ID);
            Assert.AreEqual(Factors.Unit / Factors.Kilo, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.DecimetrePerDay).ID);
            Assert.AreEqual(Factors.Day / Factors.Deci, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.DecimetrePerHour).ID);
            Assert.AreEqual(Factors.Hour / Factors.Deci, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.DecimetrePerMinute).ID);
            Assert.AreEqual(Factors.Minute / Factors.Deci, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.DecimetrePerSecond).ID);
            Assert.AreEqual(Factors.Unit / Factors.Deci, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.CentimetrePerDay).ID);
            Assert.AreEqual(Factors.Day / Factors.Centi, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.CentimetrePerHour).ID);
            Assert.AreEqual(Factors.Hour / Factors.Centi, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.CentimetrePerMinute).ID);
            Assert.AreEqual(Factors.Minute / Factors.Centi, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.CentimetrePerSecond).ID);
            Assert.AreEqual(Factors.Unit / Factors.Centi, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.MillimetrePerDay).ID);
            Assert.AreEqual(Factors.Day / Factors.Milli, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.MillimetrePerHour).ID);
            Assert.AreEqual(Factors.Hour / Factors.Milli, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.MillimetrePerMinute).ID);
            Assert.AreEqual(Factors.Minute / Factors.Milli, unitVal);
            unitVal = VelocityQuantity.Instance.FromSI(val, VelocityQuantity.Instance.GetUnitChoice(VelocityQuantity.UnitChoicesEnum.MillimetrePerSecond).ID);
            Assert.AreEqual(Factors.Unit / Factors.Milli, unitVal);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, VelocityQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-1, VelocityQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, VelocityQuantity.Instance.MassDimension);
            Assert.AreEqual(1, VelocityQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, VelocityQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, VelocityQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, VelocityQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, VelocityQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, VelocityQuantity.Instance.SolidAngleDimension);
        }
    }
}