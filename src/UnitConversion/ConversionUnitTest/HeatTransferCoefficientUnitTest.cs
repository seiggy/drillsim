using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class HeatTransferCoefficientUnitTest
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
            unitVal = HeatTransferCoefficientQuantity.Instance.FromSI(val, HeatTransferCoefficientQuantity.Instance.GetUnitChoice(HeatTransferCoefficientQuantity.UnitChoicesEnum.BritishThermalUnitPerHourPerSquareFootPerDegreeFahrenheit).ID);
            Assert.AreEqual(0.176110184, unitVal, 1e-3);
            unitVal = HeatTransferCoefficientQuantity.Instance.FromSI(val, HeatTransferCoefficientQuantity.Instance.GetUnitChoice(HeatTransferCoefficientQuantity.UnitChoicesEnum.WattPerSquareMetrePerKelvin).ID);
            Assert.AreEqual(1.0, unitVal);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, HeatTransferCoefficientQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-3, HeatTransferCoefficientQuantity.Instance.TimeDimension);
            Assert.AreEqual(1, HeatTransferCoefficientQuantity.Instance.MassDimension);
            Assert.AreEqual(0, HeatTransferCoefficientQuantity.Instance.LengthDimension);
            Assert.AreEqual(-1, HeatTransferCoefficientQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, HeatTransferCoefficientQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, HeatTransferCoefficientQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, HeatTransferCoefficientQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, HeatTransferCoefficientQuantity.Instance.SolidAngleDimension);
        }
    }
}