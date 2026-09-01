using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class MassGradientPerLengthUnitTest
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
            unitVal = MassGradientPerLengthQuantity.Instance.FromSI(val, MassGradientPerLengthQuantity.Instance.GetUnitChoice(MassGradientPerLengthQuantity.UnitChoicesEnum.GramPerMetre).ID);
            Assert.AreEqual(1000, unitVal);
            unitVal = MassGradientPerLengthQuantity.Instance.FromSI(val, MassGradientPerLengthQuantity.Instance.GetUnitChoice(MassGradientPerLengthQuantity.UnitChoicesEnum.KilogramPerMetre).ID);
            Assert.AreEqual(1, unitVal);
            unitVal = MassGradientPerLengthQuantity.Instance.FromSI(val, MassGradientPerLengthQuantity.Instance.GetUnitChoice(MassGradientPerLengthQuantity.UnitChoicesEnum.PoundPerFoot).ID);
            Assert.AreEqual(0.6719689753966845, unitVal, 1e-6);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, MassGradientPerLengthQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(0, MassGradientPerLengthQuantity.Instance.TimeDimension);
            Assert.AreEqual(1, MassGradientPerLengthQuantity.Instance.MassDimension);
            Assert.AreEqual(-1, MassGradientPerLengthQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, MassGradientPerLengthQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, MassGradientPerLengthQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, MassGradientPerLengthQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, MassGradientPerLengthQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, MassGradientPerLengthQuantity.Instance.SolidAngleDimension);
        }
    }
}