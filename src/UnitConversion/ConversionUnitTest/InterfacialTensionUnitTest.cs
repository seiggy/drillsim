using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class InterfacialTensionUnitTest
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
            unitVal = InterfacialTensionQuantity.Instance.FromSI(val, InterfacialTensionQuantity.Instance.GetUnitChoice(InterfacialTensionQuantity.UnitChoicesEnum.DynePerCentimetre).ID);
            Assert.AreEqual(1000, unitVal, 1e-6);
            unitVal = InterfacialTensionQuantity.Instance.FromSI(val, InterfacialTensionQuantity.Instance.GetUnitChoice(InterfacialTensionQuantity.UnitChoicesEnum.MillinewtonPerMetre).ID);
            Assert.AreEqual(1000, unitVal, 1e-6);
            unitVal = InterfacialTensionQuantity.Instance.FromSI(val, InterfacialTensionQuantity.Instance.GetUnitChoice(InterfacialTensionQuantity.UnitChoicesEnum.NewtonPerMetre).ID);
            Assert.AreEqual(1, unitVal, 1e-6);
            unitVal = InterfacialTensionQuantity.Instance.FromSI(val, InterfacialTensionQuantity.Instance.GetUnitChoice(InterfacialTensionQuantity.UnitChoicesEnum.PoundPerSecondSquared).ID);
            Assert.AreEqual(2.2046244201837775, unitVal, 1e-5);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, InterfacialTensionQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(-2, InterfacialTensionQuantity.Instance.TimeDimension);
            Assert.AreEqual(1, InterfacialTensionQuantity.Instance.MassDimension);
            Assert.AreEqual(0, InterfacialTensionQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, InterfacialTensionQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, InterfacialTensionQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, InterfacialTensionQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, InterfacialTensionQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, InterfacialTensionQuantity.Instance.SolidAngleDimension);
        }
    }
}