using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class MassDensityUnitTest
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
            unitVal = MassDensityQuantity.Instance.FromSI(val, MassDensityQuantity.Instance.GetUnitChoice(MassDensityQuantity.UnitChoicesEnum.GramPerCubicCentimetre).ID);
            Assert.AreEqual(0.001, unitVal, 1e-6);
            unitVal = MassDensityQuantity.Instance.FromSI(val, MassDensityQuantity.Instance.GetUnitChoice(MassDensityQuantity.UnitChoicesEnum.GramPerCubicMetre).ID);
            Assert.AreEqual(1000, unitVal, 1e-6);
            unitVal = MassDensityQuantity.Instance.FromSI(val, MassDensityQuantity.Instance.GetUnitChoice(MassDensityQuantity.UnitChoicesEnum.KilogramPerCubicMetre).ID);
            Assert.AreEqual(1.0, unitVal, 1e-6);
            unitVal = MassDensityQuantity.Instance.FromSI(val, MassDensityQuantity.Instance.GetUnitChoice(MassDensityQuantity.UnitChoicesEnum.PoundPerCubicFoot).ID);
            Assert.AreEqual(0.062427962, unitVal, 1e-6);
            unitVal = MassDensityQuantity.Instance.FromSI(val, MassDensityQuantity.Instance.GetUnitChoice(MassDensityQuantity.UnitChoicesEnum.PoundPerCubicInch).ID);
            Assert.AreEqual(3.6127292e-5, unitVal, 1e-8);
            unitVal = MassDensityQuantity.Instance.FromSI(val, MassDensityQuantity.Instance.GetUnitChoice(MassDensityQuantity.UnitChoicesEnum.PoundPerCubicYard).ID);
            Assert.AreEqual(1.685555, unitVal, 1e-6);
            unitVal = MassDensityQuantity.Instance.FromSI(val, MassDensityQuantity.Instance.GetUnitChoice(MassDensityQuantity.UnitChoicesEnum.PoundPerGallonUK).ID);
            Assert.AreEqual(0.0100224129, unitVal, 1e-6);
            unitVal = MassDensityQuantity.Instance.FromSI(val, MassDensityQuantity.Instance.GetUnitChoice(MassDensityQuantity.UnitChoicesEnum.PoundPerGallonUS).ID);
            Assert.AreEqual(0.00834543421, unitVal, 1e-6);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, MassDensityQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(0, MassDensityQuantity.Instance.TimeDimension);
            Assert.AreEqual(1, MassDensityQuantity.Instance.MassDimension);
            Assert.AreEqual(-3, MassDensityQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, MassDensityQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, MassDensityQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, MassDensityQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, MassDensityQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, MassDensityQuantity.Instance.SolidAngleDimension);
        }
    }
}