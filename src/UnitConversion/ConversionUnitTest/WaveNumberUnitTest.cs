using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class WaveNumberUnitTest
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
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalAstronomicalUnit).ID);
            Assert.AreEqual(1.495978707e11, unitVal, 1e5);
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalCentimetre).ID);
            Assert.AreEqual(0.01, unitVal);
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalDecametre).ID);
            Assert.AreEqual(10, unitVal);
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalDecimetre).ID);
            Assert.AreEqual(0.1, unitVal);
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalFathom).ID);
            Assert.AreEqual(1.8288, unitVal, 1e-4);
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalFoot).ID);
            Assert.AreEqual(0.3048, unitVal, 1e-6);
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalFurlong).ID);
            Assert.AreEqual(201.168, unitVal, 1e-2);
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalHand).ID);
            Assert.AreEqual(0.1016, unitVal, 1e-4);
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalHectometre).ID);
            Assert.AreEqual(100, unitVal, 1e-5);
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalInch).ID);
            Assert.AreEqual(0.0254, unitVal, 1e-5);
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalInchPer32).ID);
            Assert.AreEqual(0.0254 / 32.0, unitVal, 1e-5);
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalInternationalNauticalMile).ID);
            Assert.AreEqual(1852.0, unitVal, 1e-1);
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalKilometre).ID);
            Assert.AreEqual(1000, unitVal, 1e-5);
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalLightYear).ID);
            Assert.AreEqual(9460730472580800.0, unitVal, 1e2);
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalMetre).ID);
            Assert.AreEqual(1, unitVal, 1e-6);
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalMicrometre).ID);
            Assert.AreEqual(1e-6, unitVal, 1e-8);
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalMil).ID);
            Assert.AreEqual(0.0000254, unitVal, 1e-7);
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalMile).ID);
            Assert.AreEqual(1609.344, unitVal, 1e-3);
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalMillimetre).ID);
            Assert.AreEqual(0.001, unitVal, 1e-7);
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalNanometre).ID);
            Assert.AreEqual(1e-9, unitVal, 1e-15);
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalParsec).ID);
            Assert.AreEqual(30856775814913673.0, unitVal, 1e-1);
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalPicometre).ID);
            Assert.AreEqual(1e-12, unitVal, 1e-15);
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalScandinavianMile).ID);
            Assert.AreEqual(10000, unitVal, 1e-6);
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalSurveyorsChain).ID);
            Assert.AreEqual((22.0 * 0.9144), unitVal, 1e-6);
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalThou).ID);
            Assert.AreEqual(0.0000254, unitVal, 1e-6);
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalUKNauticalMile).ID);
            Assert.AreEqual(1853.184, unitVal, 1e-6);
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalUSSurveyFoot).ID);
            Assert.AreEqual(0.304801, unitVal, 1e-6);
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalYard).ID);
            Assert.AreEqual(0.9144, unitVal, 1e-6);
            unitVal = WaveNumberQuantity.Instance.FromSI(val, WaveNumberQuantity.Instance.GetUnitChoice(WaveNumberQuantity.UnitChoicesEnum.ReciprocalÅngstrøm).ID);
            Assert.AreEqual(1.0 / 10000000000.0, unitVal, 1e-15);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, WaveNumberQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(0, WaveNumberQuantity.Instance.TimeDimension);
            Assert.AreEqual(0, WaveNumberQuantity.Instance.MassDimension);
            Assert.AreEqual(-1, WaveNumberQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, WaveNumberQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, WaveNumberQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, WaveNumberQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, WaveNumberQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, WaveNumberQuantity.Instance.SolidAngleDimension);
        }
    }
}