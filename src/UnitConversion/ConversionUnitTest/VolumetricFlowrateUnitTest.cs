using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class VolumetricFlowrateUnitTest
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
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.BarrelPerDay).ID);
            Assert.AreEqual(543455.6989255428, unitVal, 1e2);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.BarrelPerHour).ID);
            Assert.AreEqual(22643.98745523095, unitVal, 1e0);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.BarrelPerMinute).ID);
            Assert.AreEqual(377.39979092051584, unitVal, 1e-1);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.BarrelPerSecond).ID);
            Assert.AreEqual(6.28999651534193, unitVal, 1e-3);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.BarrelPerYear).ID);
            Assert.AreEqual(198497194, unitVal, 1e4);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.CubicFootPerDay).ID);
            Assert.AreEqual(3051187.488, unitVal, 1e0);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.CubicFootPerHour).ID);
            Assert.AreEqual(127132.812, unitVal, 1e-1);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.CubicFootPerMinute).ID);
            Assert.AreEqual(2118.8802, unitVal, 1e-2);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.CubicFootPerSecond).ID);
            Assert.AreEqual(35.31467, unitVal, 1e-5);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.CubicFootPerYear).ID);
            Assert.AreEqual(1114446229.992, unitVal, 1e3);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.CubicMetrePerDay).ID);
            Assert.AreEqual(86400, unitVal, 1e-6);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.CubicMetrePerHour).ID);
            Assert.AreEqual(3600, unitVal, 1e-6);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.CubicMetrePerMinute).ID);
            Assert.AreEqual(60.0, unitVal, 1e-6);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.CubicMetrePerSecond).ID);
            Assert.AreEqual(1.0, unitVal, 1e-6);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.CubicMetrePerYear).ID);
            Assert.AreEqual(31557600, unitVal, 1e-6);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.LitrePerDay).ID);
            Assert.AreEqual(86400000, unitVal, 1e-6);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.LitrePerHour).ID);
            Assert.AreEqual(3600000, unitVal, 1e-6);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.LitrePerMinute).ID);
            Assert.AreEqual(60000, unitVal, 1e-6);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.LitrePerSecond).ID);
            Assert.AreEqual(1000, unitVal, 1e-6);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.LitrePerYear).ID);
            Assert.AreEqual(31557600000, unitVal, 1e-6);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.MillionCubicMetrePerDay).ID);
            Assert.AreEqual(0.086400, unitVal, 1e-6);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.MillionLiterPerDay).ID);
            Assert.AreEqual(86.400, unitVal, 1e-6);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.MillionStandardCubicFootPerDay).ID);
            Assert.AreEqual(Factors.Day/(Factors.Mega*Factors.Foot*Factors.Foot*Factors.Foot), unitVal, 1e-6);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.MillionUKGallonPerDay).ID);
            Assert.AreEqual(19.005343052976, unitVal, 1e-6);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.MillionUSGallonPerDay).ID);
            Assert.AreEqual(22.82446522728, unitVal, 1e-6);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.ThousandStandardCubicFootPerDay).ID);
            Assert.AreEqual(3051, unitVal, 1e0);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.UKGallonPerDay).ID);
            Assert.AreEqual(19005343.052975997, unitVal, 1e-4);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.UKGallonPerHour).ID);
            Assert.AreEqual(791889.293874, unitVal, 1e-4);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.UKGallonPerMinute).ID);
            Assert.AreEqual(13198.1548979, unitVal, 1e-5);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.UKGallonPerSecond).ID);
            Assert.AreEqual(219.96924829908778, unitVal, 1e-6);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.UKGallonPerYear).ID);
            Assert.AreEqual(6941701550.0994835, unitVal, 1e-1);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.USGallonPerDay).ID);
            Assert.AreEqual(22824465.227280002, unitVal, 1e-1);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.USGallonPerHour).ID);
            Assert.AreEqual(951019.38447, unitVal, 1e-2);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.USGallonPerMinute).ID);
            Assert.AreEqual(15850.3230745, unitVal, 1e-3);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.USGallonPerSecond).ID);
            Assert.AreEqual(264.17985364436106, unitVal, 1e-2);
            unitVal = VolumetricFlowRateQuantity.Instance.FromSI(val, VolumetricFlowRateQuantity.Instance.GetUnitChoice(VolumetricFlowRateQuantity.UnitChoicesEnum.USGallonPerYear).ID);
            Assert.AreEqual(8336635924.264021, unitVal, 1e2);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, VolumetricFlowRateQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-1, VolumetricFlowRateQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, VolumetricFlowRateQuantity.Instance.MassDimension);
            Assert.AreEqual(3, VolumetricFlowRateQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, VolumetricFlowRateQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, VolumetricFlowRateQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, VolumetricFlowRateQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, VolumetricFlowRateQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, VolumetricFlowRateQuantity.Instance.SolidAngleDimension);
        }
    }
}