using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class RandomWalkUnitTest
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
            unitVal = RandomWalkQuantity.Instance.FromSI(val, RandomWalkQuantity.Instance.GetUnitChoice(RandomWalkQuantity.UnitChoicesEnum.DegreePerSquareRootDay).ID);
            Assert.AreEqual(Factors.Degree*Math.Sqrt(Factors.Day), unitVal);
            unitVal = RandomWalkQuantity.Instance.FromSI(val, RandomWalkQuantity.Instance.GetUnitChoice(RandomWalkQuantity.UnitChoicesEnum.DegreePerSquareRootHour).ID);
            Assert.AreEqual(Factors.Degree * Math.Sqrt(Factors.Hour), unitVal);
            unitVal = RandomWalkQuantity.Instance.FromSI(val, RandomWalkQuantity.Instance.GetUnitChoice(RandomWalkQuantity.UnitChoicesEnum.DegreePerSquareRootMinute).ID);
            Assert.AreEqual(Factors.Degree * Math.Sqrt(Factors.Minute), unitVal);
            unitVal = RandomWalkQuantity.Instance.FromSI(val, RandomWalkQuantity.Instance.GetUnitChoice(RandomWalkQuantity.UnitChoicesEnum.DegreePerSquareRootSecond).ID);
            Assert.AreEqual(Factors.Degree * Math.Sqrt(Factors.Unit), unitVal);
            unitVal = RandomWalkQuantity.Instance.FromSI(val, RandomWalkQuantity.Instance.GetUnitChoice(RandomWalkQuantity.UnitChoicesEnum.RadianPerSquareRootDay).ID);
            Assert.AreEqual(Factors.Unit * Math.Sqrt(Factors.Day), unitVal);
            unitVal = RandomWalkQuantity.Instance.FromSI(val, RandomWalkQuantity.Instance.GetUnitChoice(RandomWalkQuantity.UnitChoicesEnum.RadianPerSquareRootHour).ID);
            Assert.AreEqual(Factors.Unit * Math.Sqrt(Factors.Hour), unitVal);
            unitVal = RandomWalkQuantity.Instance.FromSI(val, RandomWalkQuantity.Instance.GetUnitChoice(RandomWalkQuantity.UnitChoicesEnum.RadianPerSquareRootMinute).ID);
            Assert.AreEqual(Factors.Unit * Math.Sqrt(Factors.Minute), unitVal);
            unitVal = RandomWalkQuantity.Instance.FromSI(val, RandomWalkQuantity.Instance.GetUnitChoice(RandomWalkQuantity.UnitChoicesEnum.RadianPerSquareRootSecond).ID);
            Assert.AreEqual(Factors.Unit * Math.Sqrt(Factors.Unit), unitVal);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(1, RandomWalkQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-1.0 / 2.0, RandomWalkQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, RandomWalkQuantity.Instance.MassDimension);
            Assert.AreEqual(0, RandomWalkQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, RandomWalkQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, RandomWalkQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, RandomWalkQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, RandomWalkQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, RandomWalkQuantity.Instance.SolidAngleDimension);
        }
    }
}