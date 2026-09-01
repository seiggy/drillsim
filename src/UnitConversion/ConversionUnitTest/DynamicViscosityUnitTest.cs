using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class DynamicViscosityUnitTest
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
            unitVal = DynamicViscosityQuantity.Instance.FromSI(val, DynamicViscosityQuantity.Instance.GetUnitChoice(DynamicViscosityQuantity.UnitChoicesEnum.Centipoise).ID);
            Assert.AreEqual(1000.0, unitVal);
            unitVal = DynamicViscosityQuantity.Instance.FromSI(val, DynamicViscosityQuantity.Instance.GetUnitChoice(DynamicViscosityQuantity.UnitChoicesEnum.DyneSecondPerSquareCentimetre).ID);
            Assert.AreEqual(10.0, unitVal);
            unitVal = DynamicViscosityQuantity.Instance.FromSI(val, DynamicViscosityQuantity.Instance.GetUnitChoice(DynamicViscosityQuantity.UnitChoicesEnum.MicropascalSecond).ID);
            Assert.AreEqual(1000000.0, unitVal);
            unitVal = DynamicViscosityQuantity.Instance.FromSI(val, DynamicViscosityQuantity.Instance.GetUnitChoice(DynamicViscosityQuantity.UnitChoicesEnum.Micropoise).ID);
            Assert.AreEqual(10000000.0, unitVal);
            unitVal = DynamicViscosityQuantity.Instance.FromSI(val, DynamicViscosityQuantity.Instance.GetUnitChoice(DynamicViscosityQuantity.UnitChoicesEnum.MillipascalSecond).ID);
            Assert.AreEqual(1000.0, unitVal);
            unitVal = DynamicViscosityQuantity.Instance.FromSI(val, DynamicViscosityQuantity.Instance.GetUnitChoice(DynamicViscosityQuantity.UnitChoicesEnum.Millipoise).ID);
            Assert.AreEqual(10000.0, unitVal);
            unitVal = DynamicViscosityQuantity.Instance.FromSI(val, DynamicViscosityQuantity.Instance.GetUnitChoice(DynamicViscosityQuantity.UnitChoicesEnum.PascalSecond).ID);
            Assert.AreEqual(1.0, unitVal);
            unitVal = DynamicViscosityQuantity.Instance.FromSI(val, DynamicViscosityQuantity.Instance.GetUnitChoice(DynamicViscosityQuantity.UnitChoicesEnum.Poise).ID);
            Assert.AreEqual(10.0, unitVal);
            unitVal = DynamicViscosityQuantity.Instance.FromSI(val, DynamicViscosityQuantity.Instance.GetUnitChoice(DynamicViscosityQuantity.UnitChoicesEnum.PoundSecondPer100SquareFoot).ID);
            Assert.AreEqual(2.08854, unitVal, 1e-5);
            unitVal = DynamicViscosityQuantity.Instance.FromSI(val, DynamicViscosityQuantity.Instance.GetUnitChoice(DynamicViscosityQuantity.UnitChoicesEnum.PoundSecondPerSquareFoot).ID);
            Assert.AreEqual(0.0208854, unitVal, 1e-7);
            unitVal = DynamicViscosityQuantity.Instance.FromSI(val, DynamicViscosityQuantity.Instance.GetUnitChoice(DynamicViscosityQuantity.UnitChoicesEnum.PoundSecondPerSquareInch).ID);
            Assert.AreEqual(0.0001458, unitVal, 1e-6);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, DynamicViscosityQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-1, DynamicViscosityQuantity.Instance.TimeDimension);
            Assert.AreEqual(1, DynamicViscosityQuantity.Instance.MassDimension);
            Assert.AreEqual(-1, DynamicViscosityQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, DynamicViscosityQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, DynamicViscosityQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, DynamicViscosityQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, DynamicViscosityQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, DynamicViscosityQuantity.Instance.SolidAngleDimension);
        }
    }
}