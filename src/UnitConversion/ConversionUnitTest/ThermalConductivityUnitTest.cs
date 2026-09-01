using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class ThermalConductivityUnitTest
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
            unitVal = ThermalConductivityQuantity.Instance.FromSI(val, ThermalConductivityQuantity.Instance.GetUnitChoice(ThermalConductivityQuantity.UnitChoicesEnum.BritishThermalUnitInchPerHourSquareFootDegreeFahrenheit).ID);
            Assert.AreEqual(6.933472, unitVal, 1e-2);
            unitVal = ThermalConductivityQuantity.Instance.FromSI(val, ThermalConductivityQuantity.Instance.GetUnitChoice(ThermalConductivityQuantity.UnitChoicesEnum.BritishThermalUnitPerHourFootDegreeFahrenheit).ID);
            Assert.AreEqual(0.5777893, unitVal, 1e-3);
            unitVal = ThermalConductivityQuantity.Instance.FromSI(val, ThermalConductivityQuantity.Instance.GetUnitChoice(ThermalConductivityQuantity.UnitChoicesEnum.CaloriePerCentimetreSecondDegreeCelsius).ID);
            Assert.AreEqual(0.00238846, unitVal, 1e-5);
            unitVal = ThermalConductivityQuantity.Instance.FromSI(val, ThermalConductivityQuantity.Instance.GetUnitChoice(ThermalConductivityQuantity.UnitChoicesEnum.CaloriePerMetreSecondDegreeCelsius).ID);
            Assert.AreEqual(0.238846, unitVal, 1e-3);
            unitVal = ThermalConductivityQuantity.Instance.FromSI(val, ThermalConductivityQuantity.Instance.GetUnitChoice(ThermalConductivityQuantity.UnitChoicesEnum.WattPerMetreKelvin).ID);
            Assert.AreEqual(1.0, unitVal, 1e-6);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, ThermalConductivityQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-3, ThermalConductivityQuantity.Instance.TimeDimension);
            Assert.AreEqual(1, ThermalConductivityQuantity.Instance.MassDimension);
            Assert.AreEqual(1, ThermalConductivityQuantity.Instance.LengthDimension);
            Assert.AreEqual(-1, ThermalConductivityQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, ThermalConductivityQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, ThermalConductivityQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, ThermalConductivityQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, ThermalConductivityQuantity.Instance.SolidAngleDimension);
        }
    }
}