using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class AmountSubstanceUnitTest
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
            unitVal = AmountSubstanceQuantity.Instance.FromSI(val, AmountSubstanceQuantity.Instance.GetUnitChoice(AmountSubstanceQuantity.UnitChoicesEnum.Centimole).ID);
            Assert.AreEqual(1.0 / Factors.Centi, unitVal);
            unitVal = AmountSubstanceQuantity.Instance.FromSI(val, AmountSubstanceQuantity.Instance.GetUnitChoice(AmountSubstanceQuantity.UnitChoicesEnum.Decimole).ID);
            Assert.AreEqual(1.0 / Factors.Deci, unitVal);
            unitVal = AmountSubstanceQuantity.Instance.FromSI(val, AmountSubstanceQuantity.Instance.GetUnitChoice(AmountSubstanceQuantity.UnitChoicesEnum.Kilomole).ID);
            Assert.AreEqual(1.0 / Factors.Kilo, unitVal);
            unitVal = AmountSubstanceQuantity.Instance.FromSI(val, AmountSubstanceQuantity.Instance.GetUnitChoice(AmountSubstanceQuantity.UnitChoicesEnum.Micromole).ID);
            Assert.AreEqual(1.0 / Factors.Micro, unitVal);
            unitVal = AmountSubstanceQuantity.Instance.FromSI(val, AmountSubstanceQuantity.Instance.GetUnitChoice(AmountSubstanceQuantity.UnitChoicesEnum.Millimole).ID);
            Assert.AreEqual(1.0 / Factors.Milli, unitVal);
            unitVal = AmountSubstanceQuantity.Instance.FromSI(val, AmountSubstanceQuantity.Instance.GetUnitChoice(AmountSubstanceQuantity.UnitChoicesEnum.Mole).ID);
            Assert.AreEqual(1.0 / Factors.Unit, unitVal);
            unitVal = AmountSubstanceQuantity.Instance.FromSI(val, AmountSubstanceQuantity.Instance.GetUnitChoice(AmountSubstanceQuantity.UnitChoicesEnum.Nanomole).ID);
            Assert.AreEqual(1.0 / Factors.Nano, unitVal);
            unitVal = AmountSubstanceQuantity.Instance.FromSI(val, AmountSubstanceQuantity.Instance.GetUnitChoice(AmountSubstanceQuantity.UnitChoicesEnum.Picomole).ID);
            Assert.AreEqual(1.0 / Factors.Pico, unitVal);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, AmountSubstanceQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(0, AmountSubstanceQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, AmountSubstanceQuantity.Instance.MassDimension);
            Assert.AreEqual(0, AmountSubstanceQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, AmountSubstanceQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, AmountSubstanceQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(1, AmountSubstanceQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, AmountSubstanceQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, AmountSubstanceQuantity.Instance.SolidAngleDimension);
        }
    }
}