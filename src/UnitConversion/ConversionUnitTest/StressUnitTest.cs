using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class StressUnitTest
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
            unitVal = StressQuantity.Instance.FromSI(val, StressQuantity.Instance.GetUnitChoice(StressQuantity.UnitChoicesEnum.Bar).ID);
            Assert.AreEqual(1e-5, unitVal);
            unitVal = StressQuantity.Instance.FromSI(val, StressQuantity.Instance.GetUnitChoice(StressQuantity.UnitChoicesEnum.DynePerSquareCentimetre).ID);
            Assert.AreEqual(10, unitVal);
            unitVal = StressQuantity.Instance.FromSI(val, StressQuantity.Instance.GetUnitChoice(StressQuantity.UnitChoicesEnum.Gigapascal).ID);
            Assert.AreEqual(1E-09, unitVal);
            unitVal = StressQuantity.Instance.FromSI(val, StressQuantity.Instance.GetUnitChoice(StressQuantity.UnitChoicesEnum.KilonewtonPerSquareMetre).ID);
            Assert.AreEqual(0.001, unitVal);
            unitVal = StressQuantity.Instance.FromSI(val, StressQuantity.Instance.GetUnitChoice(StressQuantity.UnitChoicesEnum.Kilopascal).ID);
            Assert.AreEqual(0.001, unitVal);
            unitVal = StressQuantity.Instance.FromSI(val, StressQuantity.Instance.GetUnitChoice(StressQuantity.UnitChoicesEnum.KilopoundPerSquareInch).ID);
            Assert.AreEqual(0.00000014503762645158165, unitVal, 1e-8);
            unitVal = StressQuantity.Instance.FromSI(val, StressQuantity.Instance.GetUnitChoice(StressQuantity.UnitChoicesEnum.Megapascal).ID);
            Assert.AreEqual(1e-6, unitVal);
            unitVal = StressQuantity.Instance.FromSI(val, StressQuantity.Instance.GetUnitChoice(StressQuantity.UnitChoicesEnum.MegapoundPerSquareInch).ID);
            Assert.AreEqual(1.4503762645158166E-10, unitVal, 1e-15);
            unitVal = StressQuantity.Instance.FromSI(val, StressQuantity.Instance.GetUnitChoice(StressQuantity.UnitChoicesEnum.Microbar).ID);
            Assert.AreEqual(10, unitVal);
            unitVal = StressQuantity.Instance.FromSI(val, StressQuantity.Instance.GetUnitChoice(StressQuantity.UnitChoicesEnum.Millibar).ID);
            Assert.AreEqual(0.01, unitVal);
            unitVal = StressQuantity.Instance.FromSI(val, StressQuantity.Instance.GetUnitChoice(StressQuantity.UnitChoicesEnum.NewtonPerSquareCentimetre).ID);
            Assert.AreEqual(0.0001, unitVal);
            unitVal = StressQuantity.Instance.FromSI(val, StressQuantity.Instance.GetUnitChoice(StressQuantity.UnitChoicesEnum.NewtonPerSquareMetre).ID);
            Assert.AreEqual(1, unitVal);
            unitVal = StressQuantity.Instance.FromSI(val, StressQuantity.Instance.GetUnitChoice(StressQuantity.UnitChoicesEnum.NewtonPerSquareMillimetre).ID);
            Assert.AreEqual(0.000001, unitVal);
            unitVal = StressQuantity.Instance.FromSI(val, StressQuantity.Instance.GetUnitChoice(StressQuantity.UnitChoicesEnum.Pascal).ID);
            Assert.AreEqual(1, unitVal);
            unitVal = StressQuantity.Instance.FromSI(val, StressQuantity.Instance.GetUnitChoice(StressQuantity.UnitChoicesEnum.PoundPer100SquareFoot).ID);
            Assert.AreEqual(2.0887, unitVal, 1e-3);
            unitVal = StressQuantity.Instance.FromSI(val, StressQuantity.Instance.GetUnitChoice(StressQuantity.UnitChoicesEnum.PoundPerSquareFoot).ID);
            Assert.AreEqual(0.02088543, unitVal, 1e-5);
            unitVal = StressQuantity.Instance.FromSI(val, StressQuantity.Instance.GetUnitChoice(StressQuantity.UnitChoicesEnum.PoundPerSquareInch).ID);
            Assert.AreEqual(0.00014503762645158165, unitVal, 1e-8);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, StressQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-2, StressQuantity.Instance.TimeDimension);
            Assert.AreEqual(1, StressQuantity.Instance.MassDimension);
            Assert.AreEqual(-1, StressQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, StressQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, StressQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, StressQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, StressQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, StressQuantity.Instance.SolidAngleDimension);
        }
    }
}