using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class MassDensityRateOfChangeUnitTest
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
            unitVal = MassDensityRateOfChangeQuantity.Instance.FromSI(val, MassDensityRateOfChangeQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerHour).ID);
            Assert.AreEqual(3.6, unitVal);
            unitVal = MassDensityRateOfChangeQuantity.Instance.FromSI(val, MassDensityRateOfChangeQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerMinute).ID);
            Assert.AreEqual(0.06, unitVal);
            unitVal = MassDensityRateOfChangeQuantity.Instance.FromSI(val, MassDensityRateOfChangeQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerSecond).ID);
            Assert.AreEqual(0.001, unitVal);
            unitVal = MassDensityRateOfChangeQuantity.Instance.FromSI(val, MassDensityRateOfChangeQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerSecond).ID);
            Assert.AreEqual(1.0, unitVal);
            unitVal = MassDensityRateOfChangeQuantity.Instance.FromSI(val, MassDensityRateOfChangeQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeQuantity.UnitChoicesEnum.PoundPerGallonUKPerHour).ID);
            Assert.AreEqual(36.1, unitVal, 1e-1);
            unitVal = MassDensityRateOfChangeQuantity.Instance.FromSI(val, MassDensityRateOfChangeQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeQuantity.UnitChoicesEnum.PoundPerGallonUKPerMinute).ID);
            Assert.AreEqual(0.60, unitVal, 1e-2);
            unitVal = MassDensityRateOfChangeQuantity.Instance.FromSI(val, MassDensityRateOfChangeQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeQuantity.UnitChoicesEnum.PoundPerGallonUKPerSecond).ID);
            Assert.AreEqual(0.01, unitVal, 1e-3);
            unitVal = MassDensityRateOfChangeQuantity.Instance.FromSI(val, MassDensityRateOfChangeQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeQuantity.UnitChoicesEnum.PoundPerGallonUSPerHour).ID);
            Assert.AreEqual(30.0, unitVal, 1e-1);
            unitVal = MassDensityRateOfChangeQuantity.Instance.FromSI(val, MassDensityRateOfChangeQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeQuantity.UnitChoicesEnum.PoundPerGallonUSPerMinute).ID);
            Assert.AreEqual(0.50, unitVal, 1e-2);
            unitVal = MassDensityRateOfChangeQuantity.Instance.FromSI(val, MassDensityRateOfChangeQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeQuantity.UnitChoicesEnum.PoundPerGallonUSPerSecond).ID);
            Assert.AreEqual(0.008344, unitVal, 1e-3);
            unitVal = MassDensityRateOfChangeQuantity.Instance.FromSI(val, MassDensityRateOfChangeQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeQuantity.UnitChoicesEnum.SpecificGravityPerHour).ID);
            Assert.AreEqual(3.6, unitVal, 1e-1);
            unitVal = MassDensityRateOfChangeQuantity.Instance.FromSI(val, MassDensityRateOfChangeQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeQuantity.UnitChoicesEnum.SpecificGravityPerMinute).ID);
            Assert.AreEqual(0.06, unitVal, 1e-3);
            unitVal = MassDensityRateOfChangeQuantity.Instance.FromSI(val, MassDensityRateOfChangeQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeQuantity.UnitChoicesEnum.SpecificGravityPerSecond).ID);
            Assert.AreEqual(0.001, unitVal, 1e-4);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, MassDensityRateOfChangeQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-1, MassDensityRateOfChangeQuantity.Instance.TimeDimension);
            Assert.AreEqual(1, MassDensityRateOfChangeQuantity.Instance.MassDimension);
            Assert.AreEqual(-3, MassDensityRateOfChangeQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, MassDensityRateOfChangeQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, MassDensityRateOfChangeQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, MassDensityRateOfChangeQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, MassDensityRateOfChangeQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, MassDensityRateOfChangeQuantity.Instance.SolidAngleDimension);
        }
    }
}