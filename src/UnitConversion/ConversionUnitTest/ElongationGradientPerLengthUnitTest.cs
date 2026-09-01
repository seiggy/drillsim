using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class ElongationGradientPerLengthUnitTest
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
            unitVal = ElongationGradientPerLengthQuantity.Instance.FromSI(val, ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.CentimetrePerKilometre).ID);
            Assert.AreEqual(100000.0, unitVal);
            unitVal = ElongationGradientPerLengthQuantity.Instance.FromSI(val, ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.CentimetrePerMetre).ID);
            Assert.AreEqual(100.0, unitVal);
            unitVal = ElongationGradientPerLengthQuantity.Instance.FromSI(val, ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.DecimetrePerKilometre).ID);
            Assert.AreEqual(10000.0, unitVal);
            unitVal = ElongationGradientPerLengthQuantity.Instance.FromSI(val, ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.DecimetrePerMetre).ID);
            Assert.AreEqual(10.0, unitVal);
            unitVal = ElongationGradientPerLengthQuantity.Instance.FromSI(val, ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.FootPerFoot).ID);
            Assert.AreEqual(1.0, unitVal);
            unitVal = ElongationGradientPerLengthQuantity.Instance.FromSI(val, ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.FootPerMile).ID);
            Assert.AreEqual(5280, unitVal);
            unitVal = ElongationGradientPerLengthQuantity.Instance.FromSI(val, ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.FootPerYard).ID);
            Assert.AreEqual(3, unitVal);
            unitVal = ElongationGradientPerLengthQuantity.Instance.FromSI(val, ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.InchPerFoot).ID);
            Assert.AreEqual(12, unitVal, 1e-6);
            unitVal = ElongationGradientPerLengthQuantity.Instance.FromSI(val, ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.InchPerMile).ID);
            Assert.AreEqual(63360, unitVal, 1e-6);
            unitVal = ElongationGradientPerLengthQuantity.Instance.FromSI(val, ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.InchPerYard).ID);
            Assert.AreEqual(36.0, unitVal);
            unitVal = ElongationGradientPerLengthQuantity.Instance.FromSI(val, ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.MetrePerKilometre).ID);
            Assert.AreEqual(1000.0, unitVal);
            unitVal = ElongationGradientPerLengthQuantity.Instance.FromSI(val, ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.MetrePerMetre).ID);
            Assert.AreEqual(1.0, unitVal);
            unitVal = ElongationGradientPerLengthQuantity.Instance.FromSI(val, ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.MicrometrePerKilometre).ID);
            Assert.AreEqual(1e9, unitVal);
            unitVal = ElongationGradientPerLengthQuantity.Instance.FromSI(val, ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.MicrometrePerMetre).ID);
            Assert.AreEqual(1e6, unitVal);
            unitVal = ElongationGradientPerLengthQuantity.Instance.FromSI(val, ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.MillimetrePerKilometre).ID);
            Assert.AreEqual(1e6, unitVal);
            unitVal = ElongationGradientPerLengthQuantity.Instance.FromSI(val, ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.MillimetrePerMetre).ID);
            Assert.AreEqual(1000.0, unitVal);
            unitVal = ElongationGradientPerLengthQuantity.Instance.FromSI(val, ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.YardPerFoot).ID);
            Assert.AreEqual(1.0/3.0, unitVal);
            unitVal = ElongationGradientPerLengthQuantity.Instance.FromSI(val, ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.YardPerMile).ID);
            Assert.AreEqual(1760.0, unitVal);
            unitVal = ElongationGradientPerLengthQuantity.Instance.FromSI(val, ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.YardPerYard).ID);
            Assert.AreEqual(1.0, unitVal);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, ElongationGradientPerLengthQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(0, ElongationGradientPerLengthQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, ElongationGradientPerLengthQuantity.Instance.MassDimension);
            Assert.AreEqual(0, ElongationGradientPerLengthQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, ElongationGradientPerLengthQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, ElongationGradientPerLengthQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, ElongationGradientPerLengthQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, ElongationGradientPerLengthQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, ElongationGradientPerLengthQuantity.Instance.SolidAngleDimension);
        }
    }
}