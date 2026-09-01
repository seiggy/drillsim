using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class AngularAccelerationUnitTest
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
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.RadianPerDayPerHour).ID);
            Assert.AreEqual(24.0*3600.0*3600.0, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.RadianPerDayPerMinute).ID);
            Assert.AreEqual(24.0 * 3600.0 * 60.0, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.RadianPerDayPerSecond).ID);
            Assert.AreEqual(24.0 * 3600.0 *1.0, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.RadianPerDaySquared).ID);
            Assert.AreEqual(24.0 * 3600.0 * 24.0 * 3600.0, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.RadianPerHourPerDay).ID);
            Assert.AreEqual(3600.0*24.0*3600.0, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.RadianPerHourPerMinute).ID);
            Assert.AreEqual(3600.0*60.0, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.RadianPerHourPerSecond).ID);
            Assert.AreEqual(3600.0*1.0, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.RadianPerHourSquared).ID);
            Assert.AreEqual(3600.0*3600.0, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.RadianPerMinutePerDay).ID);
            Assert.AreEqual(60.0*24.0*3600.0, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.RadianPerMinutePerHour).ID);
            Assert.AreEqual(60.0*3600.0, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.RadianPerMinutePerSecond).ID);
            Assert.AreEqual(60.0*1.0, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.RadianPerMinuteSquared).ID);
            Assert.AreEqual(60.0*60.0, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.RadianPerSecondPerDay).ID);
            Assert.AreEqual(1.0*24.0*3600.0, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.RadianPerSecondPerHour).ID);
            Assert.AreEqual(1.0*3600.0, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.RadianPerSecondPerMinute).ID);
            Assert.AreEqual(1.0*60.0, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.RadianPerSecondSquared).ID);
            Assert.AreEqual(1.0*1.0, unitVal, 1e-6);

            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.DegreePerDayPerHour).ID);
            Assert.AreEqual(180.0*24.0 * 3600.0 * 3600.0 / Math.PI, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.DegreePerDayPerMinute).ID);
            Assert.AreEqual(180.0 * 24.0 * 3600.0 * 60.0 / Math.PI, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.DegreePerDayPerSecond).ID);
            Assert.AreEqual(180.0 * 24.0 * 3600.0 * 1.0 / Math.PI, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.DegreePerDaySquared).ID);
            Assert.AreEqual(180.0 * 24.0 * 3600.0 * 24.0 * 3600.0 / Math.PI, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.DegreePerHourPerDay).ID);
            Assert.AreEqual(180.0 * 3600.0 * 24.0 * 3600.0 / Math.PI, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.DegreePerHourPerMinute).ID);
            Assert.AreEqual(180.0 * 3600.0 * 60.0 / Math.PI, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.DegreePerHourPerSecond).ID);
            Assert.AreEqual(180.0 * 3600.0 * 1.0 / Math.PI, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.DegreePerHourSquared).ID);
            Assert.AreEqual(180.0 * 3600.0 * 3600.0 / Math.PI, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.DegreePerMinutePerDay).ID);
            Assert.AreEqual(180.0 * 60.0 * 24.0 * 3600.0 / Math.PI, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.DegreePerMinutePerHour).ID);
            Assert.AreEqual(180.0 * 60.0 * 3600.0 / Math.PI, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.DegreePerMinutePerSecond).ID);
            Assert.AreEqual(180.0 * 60.0 * 1.0 / Math.PI, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.DegreePerMinuteSquared).ID);
            Assert.AreEqual(180.0 * 60.0 * 60.0 / Math.PI, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.DegreePerSecondPerDay).ID);
            Assert.AreEqual(180.0 * 1.0 * 24.0*3600.0 / Math.PI, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.DegreePerSecondPerHour).ID);
            Assert.AreEqual(180.0 * 1.0 * 3600.0 / Math.PI, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.DegreePerSecondPerMinute).ID);
            Assert.AreEqual(180.0 * 1.0 * 60.0 / Math.PI, unitVal, 1e-6);
            unitVal = AngularAccelerationQuantity.Instance.FromSI(val, AngularAccelerationQuantity.Instance.GetUnitChoice(AngularAccelerationQuantity.UnitChoicesEnum.DegreePerSecondSquared).ID);
            Assert.AreEqual(180.0 * 1.0 * 1.0 / Math.PI, unitVal, 1e-6);
        }
        [Test]
        public void Test2()
        {
            Assert.AreEqual(1, AngularAccelerationQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-2, AngularAccelerationQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, AngularAccelerationQuantity.Instance.MassDimension);
            Assert.AreEqual(0, AngularAccelerationQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, AngularAccelerationQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, AngularAccelerationQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, AngularAccelerationQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, AngularAccelerationQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, AngularAccelerationQuantity.Instance.SolidAngleDimension);
        }

    }
}