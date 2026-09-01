using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class MaterialStrengthUnitTest
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
            unitVal = MaterialStrengthQuantity.Instance.FromSI(val, MaterialStrengthQuantity.Instance.GetUnitChoice(MaterialStrengthQuantity.UnitChoicesEnum.Gigapascal).ID);
            Assert.AreEqual(1E-09, unitVal);
            unitVal = MaterialStrengthQuantity.Instance.FromSI(val, MaterialStrengthQuantity.Instance.GetUnitChoice(MaterialStrengthQuantity.UnitChoicesEnum.Megapascal).ID);
            Assert.AreEqual(1E-06, unitVal);
            unitVal = MaterialStrengthQuantity.Instance.FromSI(val, MaterialStrengthQuantity.Instance.GetUnitChoice(MaterialStrengthQuantity.UnitChoicesEnum.MegapoundPerSquareInch).ID);
            Assert.AreEqual(1.4503762645158166E-10, unitVal, 1e-15);
            unitVal = MaterialStrengthQuantity.Instance.FromSI(val, MaterialStrengthQuantity.Instance.GetUnitChoice(MaterialStrengthQuantity.UnitChoicesEnum.Pascal).ID);
            Assert.AreEqual(1, unitVal);
            unitVal = MaterialStrengthQuantity.Instance.FromSI(val, MaterialStrengthQuantity.Instance.GetUnitChoice(MaterialStrengthQuantity.UnitChoicesEnum.PoundPer100SquareFoot).ID);
            Assert.AreEqual(2.0887, unitVal, 1e-3);
            unitVal = MaterialStrengthQuantity.Instance.FromSI(val, MaterialStrengthQuantity.Instance.GetUnitChoice(MaterialStrengthQuantity.UnitChoicesEnum.Psi).ID);
            Assert.AreEqual(0.00014503762645158165, unitVal, 1e-7);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, MaterialStrengthQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-2, MaterialStrengthQuantity.Instance.TimeDimension);
            Assert.AreEqual(1, MaterialStrengthQuantity.Instance.MassDimension);
            Assert.AreEqual(-1, MaterialStrengthQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, MaterialStrengthQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, MaterialStrengthQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, MaterialStrengthQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, MaterialStrengthQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, MaterialStrengthQuantity.Instance.SolidAngleDimension);
        }
    }
}