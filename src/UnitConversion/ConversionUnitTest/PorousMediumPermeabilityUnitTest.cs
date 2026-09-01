using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class PorousMediumPermeabilityUnitTest
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
            unitVal = PorousMediumPermeabilityQuantity.Instance.FromSI(val, PorousMediumPermeabilityQuantity.Instance.GetUnitChoice(PorousMediumPermeabilityQuantity.UnitChoicesEnum.Darcy).ID);
            Assert.AreEqual(1013249965828.14, unitVal, 1e5);
            unitVal = PorousMediumPermeabilityQuantity.Instance.FromSI(val, PorousMediumPermeabilityQuantity.Instance.GetUnitChoice(PorousMediumPermeabilityQuantity.UnitChoicesEnum.Microdarcy).ID);
            Assert.AreEqual(1013249965828140000.0, unitVal, 1e11);
            unitVal = PorousMediumPermeabilityQuantity.Instance.FromSI(val, PorousMediumPermeabilityQuantity.Instance.GetUnitChoice(PorousMediumPermeabilityQuantity.UnitChoicesEnum.Millidarcy).ID);
            Assert.AreEqual(1013249965828140.0, unitVal, 1e8);
            unitVal = PorousMediumPermeabilityQuantity.Instance.FromSI(val, PorousMediumPermeabilityQuantity.Instance.GetUnitChoice(PorousMediumPermeabilityQuantity.UnitChoicesEnum.Nanodarcy).ID);
            Assert.AreEqual(1013249965828140000000.0, unitVal, 1e14);
            unitVal = PorousMediumPermeabilityQuantity.Instance.FromSI(val, PorousMediumPermeabilityQuantity.Instance.GetUnitChoice(PorousMediumPermeabilityQuantity.UnitChoicesEnum.SquareMetre).ID);
            Assert.AreEqual(1, unitVal);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, PorousMediumPermeabilityQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(0, PorousMediumPermeabilityQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, PorousMediumPermeabilityQuantity.Instance.MassDimension);
            Assert.AreEqual(2, PorousMediumPermeabilityQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, PorousMediumPermeabilityQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, PorousMediumPermeabilityQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, PorousMediumPermeabilityQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, PorousMediumPermeabilityQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, PorousMediumPermeabilityQuantity.Instance.SolidAngleDimension);
        }
    }
}