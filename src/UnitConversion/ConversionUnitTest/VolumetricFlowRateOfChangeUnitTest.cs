using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class VolumetricFlowRateOfChangeUnitTest
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
            unitVal = VolumetricFlowRateOfChangeQuantity.Instance.FromSI(val, VolumetricFlowRateOfChangeQuantity.Instance.GetUnitChoice(VolumetricFlowRateOfChangeQuantity.UnitChoicesEnum.CubicMetrePerSecondSquared).ID);
            Assert.AreEqual(1.0, unitVal);
            unitVal = VolumetricFlowRateOfChangeQuantity.Instance.FromSI(val, VolumetricFlowRateOfChangeQuantity.Instance.GetUnitChoice(VolumetricFlowRateOfChangeQuantity.UnitChoicesEnum.LitrePerMinutePerSecond).ID);
            Assert.AreEqual(60000, unitVal);
            unitVal = VolumetricFlowRateOfChangeQuantity.Instance.FromSI(val, VolumetricFlowRateOfChangeQuantity.Instance.GetUnitChoice(VolumetricFlowRateOfChangeQuantity.UnitChoicesEnum.LitrePerMinuteSquared).ID);
            Assert.AreEqual(3600000, unitVal);
            unitVal = VolumetricFlowRateOfChangeQuantity.Instance.FromSI(val, VolumetricFlowRateOfChangeQuantity.Instance.GetUnitChoice(VolumetricFlowRateOfChangeQuantity.UnitChoicesEnum.LitrePerSecondSquared).ID);
            Assert.AreEqual(1000, unitVal);
            unitVal = VolumetricFlowRateOfChangeQuantity.Instance.FromSI(val, VolumetricFlowRateOfChangeQuantity.Instance.GetUnitChoice(VolumetricFlowRateOfChangeQuantity.UnitChoicesEnum.UKGallonPerMinutePerSecond).ID);
            Assert.AreEqual(13198.1549, unitVal, 1e-3);
            unitVal = VolumetricFlowRateOfChangeQuantity.Instance.FromSI(val, VolumetricFlowRateOfChangeQuantity.Instance.GetUnitChoice(VolumetricFlowRateOfChangeQuantity.UnitChoicesEnum.UKGallonPerMinuteSquared).ID);
            Assert.AreEqual(791889.293874, unitVal, 1e-3);
            unitVal = VolumetricFlowRateOfChangeQuantity.Instance.FromSI(val, VolumetricFlowRateOfChangeQuantity.Instance.GetUnitChoice(VolumetricFlowRateOfChangeQuantity.UnitChoicesEnum.USGallonPerMinutePerSecond).ID);
            Assert.AreEqual(15850.32307, unitVal,1e-3);
            unitVal = VolumetricFlowRateOfChangeQuantity.Instance.FromSI(val, VolumetricFlowRateOfChangeQuantity.Instance.GetUnitChoice(VolumetricFlowRateOfChangeQuantity.UnitChoicesEnum.USGallonPerMinuteSquared).ID);
            Assert.AreEqual(951019.38447, unitVal, 1e-2);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, VolumetricFlowRateOfChangeQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-2, VolumetricFlowRateOfChangeQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, VolumetricFlowRateOfChangeQuantity.Instance.MassDimension);
            Assert.AreEqual(3, VolumetricFlowRateOfChangeQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, VolumetricFlowRateOfChangeQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, VolumetricFlowRateOfChangeQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, VolumetricFlowRateOfChangeQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, VolumetricFlowRateOfChangeQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, VolumetricFlowRateOfChangeQuantity.Instance.SolidAngleDimension);
        }
    }
}