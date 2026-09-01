using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class AreaUnitTest
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
            unitVal = AreaQuantity.Instance.FromSI(val, AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.Acre).ID);
            Assert.AreEqual(1.0 / 4046.8564224, unitVal, 1e-6);
            unitVal = AreaQuantity.Instance.FromSI(val, AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.Hectare).ID);
            Assert.AreEqual(1.0 / (Factors.Hecto*Factors.Hecto), unitVal, 1e-6);
            unitVal = AreaQuantity.Instance.FromSI(val, AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.SquareCentimetre).ID);
            Assert.AreEqual(1.0 / (Factors.Centi * Factors.Centi), unitVal, 1e-6);
            unitVal = AreaQuantity.Instance.FromSI(val, AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.SquareDecametre).ID);
            Assert.AreEqual(1.0 / (Factors.Deca * Factors.Deca), unitVal, 1e-6);
            unitVal = AreaQuantity.Instance.FromSI(val, AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.SquareDecimetre).ID);
            Assert.AreEqual(1.0 / (Factors.Deci * Factors.Deci), unitVal, 1e-6);
            unitVal = AreaQuantity.Instance.FromSI(val, AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.SquareFoot).ID);
            Assert.AreEqual(1.0 / (Factors.Foot * Factors.Foot), unitVal, 1e-6);
            unitVal = AreaQuantity.Instance.FromSI(val, AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.SquareInch).ID);
            Assert.AreEqual(1.0 / (Factors.Inch * Factors.Inch), unitVal, 1e-6);
            unitVal = AreaQuantity.Instance.FromSI(val, AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.SquareKilometre).ID);
            Assert.AreEqual(1.0 / (Factors.Kilo * Factors.Kilo), unitVal, 1e-6);
            unitVal = AreaQuantity.Instance.FromSI(val, AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.SquareMetre).ID);
            Assert.AreEqual(1.0 / (Factors.Unit * Factors.Unit), unitVal, 1e-6);
            unitVal = AreaQuantity.Instance.FromSI(val, AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.SquareMicrometre).ID);
            Assert.AreEqual(1.0 / (Factors.Micro * Factors.Micro), unitVal, 1e-6);
            unitVal = AreaQuantity.Instance.FromSI(val, AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.SquareMile).ID);
            Assert.AreEqual(1.0 / (Factors.Mile * Factors.Mile), unitVal, 1e-6);
            unitVal = AreaQuantity.Instance.FromSI(val, AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.SquareMillimetre).ID);
            Assert.AreEqual(1.0 / (Factors.Milli * Factors.Milli), unitVal, 1e-6);
            unitVal = AreaQuantity.Instance.FromSI(val, AreaQuantity.Instance.GetUnitChoice(AreaQuantity.UnitChoicesEnum.SquareYard).ID);
            Assert.AreEqual(1.0 / (Factors.Yard * Factors.Yard), unitVal, 1e-6);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, AreaQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(0, AreaQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, AreaQuantity.Instance.MassDimension);
            Assert.AreEqual(2, AreaQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, AreaQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, AreaQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, AreaQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, AreaQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, AreaQuantity.Instance.SolidAngleDimension);
        }
    }
}