using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class CurvatureUnitTest
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
            unitVal = CurvatureQuantity.Instance.FromSI(val, CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.DegreePer100ft).ID);
            Assert.AreEqual(Factors.Degree*100.0*Factors.Foot, unitVal);
            unitVal = CurvatureQuantity.Instance.FromSI(val, CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.DegreePer10m).ID);
            Assert.AreEqual(Factors.Degree * 10.0, unitVal);
            unitVal = CurvatureQuantity.Instance.FromSI(val, CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.DegreePer30ft).ID);
            Assert.AreEqual(Factors.Degree * 30.0 * Factors.Foot, unitVal);
            unitVal = CurvatureQuantity.Instance.FromSI(val, CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.DegreePer30m).ID);
            Assert.AreEqual(Factors.Degree * 30.0, unitVal);
            unitVal = CurvatureQuantity.Instance.FromSI(val, CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.DegreePerCentimeter).ID);
            Assert.AreEqual(Factors.Degree * Factors.Centi, unitVal);
            unitVal = CurvatureQuantity.Instance.FromSI(val, CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.DegreePerDecameter).ID);
            Assert.AreEqual(Factors.Degree * Factors.Deca, unitVal);
            unitVal = CurvatureQuantity.Instance.FromSI(val, CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.DegreePerDecimeter).ID);
            Assert.AreEqual(Factors.Degree * Factors.Deci, unitVal);
            unitVal = CurvatureQuantity.Instance.FromSI(val, CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.DegreePerFoot).ID);
            Assert.AreEqual(Factors.Degree * Factors.Foot, unitVal);
            unitVal = CurvatureQuantity.Instance.FromSI(val, CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.DegreePerHectometer).ID);
            Assert.AreEqual(Factors.Degree * Factors.Hecto, unitVal);
            unitVal = CurvatureQuantity.Instance.FromSI(val, CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.DegreePerKilometer).ID);
            Assert.AreEqual(Factors.Degree * Factors.Kilo, unitVal);
            unitVal = CurvatureQuantity.Instance.FromSI(val, CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.DegreePerMetre).ID);
            Assert.AreEqual(Factors.Degree , unitVal);
            unitVal = CurvatureQuantity.Instance.FromSI(val, CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.DegreePerMicrometer).ID);
            Assert.AreEqual(Factors.Degree * Factors.Micro, unitVal);
            unitVal = CurvatureQuantity.Instance.FromSI(val, CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.DegreePerMile).ID);
            Assert.AreEqual(Factors.Degree * Factors.Mile, unitVal);
            unitVal = CurvatureQuantity.Instance.FromSI(val, CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.DegreePerMillimeter).ID);
            Assert.AreEqual(Factors.Degree * Factors.Milli, unitVal);
            unitVal = CurvatureQuantity.Instance.FromSI(val, CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.DegreePerNanometer).ID);
            Assert.AreEqual(Factors.Degree * Factors.Nano, unitVal);
            unitVal = CurvatureQuantity.Instance.FromSI(val, CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.DegreePerYard).ID);
            Assert.AreEqual(Factors.Degree * Factors.Yard, unitVal);
            unitVal = CurvatureQuantity.Instance.FromSI(val, CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.RadianPerCentimeter).ID);
            Assert.AreEqual(Factors.Centi, unitVal);
            unitVal = CurvatureQuantity.Instance.FromSI(val, CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.RadianPerDecameter).ID);
            Assert.AreEqual(Factors.Deca, unitVal);
            unitVal = CurvatureQuantity.Instance.FromSI(val, CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.RadianPerDecimeter).ID);
            Assert.AreEqual(Factors.Deci, unitVal);
            unitVal = CurvatureQuantity.Instance.FromSI(val, CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.RadianPerFoot).ID);
            Assert.AreEqual(Factors.Foot, unitVal);
            unitVal = CurvatureQuantity.Instance.FromSI(val, CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.RadianPerHectometer).ID);
            Assert.AreEqual(Factors.Hecto, unitVal);
            unitVal = CurvatureQuantity.Instance.FromSI(val, CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.RadianPerKilometer).ID);
            Assert.AreEqual(Factors.Kilo, unitVal);
            unitVal = CurvatureQuantity.Instance.FromSI(val, CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.RadianPerMetre).ID);
            Assert.AreEqual(Factors.Unit, unitVal);
            unitVal = CurvatureQuantity.Instance.FromSI(val, CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.RadianPerMicrometer).ID);
            Assert.AreEqual(Factors.Micro, unitVal);
            unitVal = CurvatureQuantity.Instance.FromSI(val, CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.RadianPerMile).ID);
            Assert.AreEqual(Factors.Mile, unitVal);
            unitVal = CurvatureQuantity.Instance.FromSI(val, CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.RadianPerMillimeter).ID);
            Assert.AreEqual(Factors.Milli, unitVal);
            unitVal = CurvatureQuantity.Instance.FromSI(val, CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.RadianPerNanometer).ID);
            Assert.AreEqual(Factors.Nano, unitVal);
            unitVal = CurvatureQuantity.Instance.FromSI(val, CurvatureQuantity.Instance.GetUnitChoice(CurvatureQuantity.UnitChoicesEnum.RadianPerYard).ID);
            Assert.AreEqual(Factors.Yard, unitVal);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(1, CurvatureQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(0, CurvatureQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, CurvatureQuantity.Instance.MassDimension);
            Assert.AreEqual(-1, CurvatureQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, CurvatureQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, CurvatureQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, CurvatureQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, CurvatureQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, CurvatureQuantity.Instance.SolidAngleDimension);
        }
    }
}