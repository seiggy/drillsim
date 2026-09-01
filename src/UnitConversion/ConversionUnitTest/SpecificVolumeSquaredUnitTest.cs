using OSDC.UnitConversion.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionUnitTest
{
    public class SpecificVolumeSquaredSquaredUnitTest
    {
        [Test]
        public void Test1()
        {
            double val = 1.0;
            double unitVal;
            unitVal = SpecificVolumeSquaredQuantity.Instance.FromSI(val, SpecificVolumeSquaredQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredQuantity.UnitChoicesEnum.CentilitreSquaredPerGramSquared).ID);
            Assert.AreEqual(100.0*100.0, unitVal, 1e-3);
            unitVal = SpecificVolumeSquaredQuantity.Instance.FromSI(val, SpecificVolumeSquaredQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredQuantity.UnitChoicesEnum.CentilitreSquaredPerKilogramSquared).ID);
            Assert.AreEqual(100000.0*100000.0, unitVal, 1e-3);
            unitVal = SpecificVolumeSquaredQuantity.Instance.FromSI(val, SpecificVolumeSquaredQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredQuantity.UnitChoicesEnum.CubicCentimetreSquaredPerGramSquared).ID);
            Assert.AreEqual(1000* 1000, unitVal, 1e-3);
            unitVal = SpecificVolumeSquaredQuantity.Instance.FromSI(val, SpecificVolumeSquaredQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredQuantity.UnitChoicesEnum.CubicCentimetreSquaredPerKilogramSquared).ID);
            Assert.AreEqual(1000000.0* 1000000.0, unitVal, 1e-3);
            unitVal = SpecificVolumeSquaredQuantity.Instance.FromSI(val, SpecificVolumeSquaredQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredQuantity.UnitChoicesEnum.CubicDecimetreSquaredPerGramSquared).ID);
            Assert.AreEqual(1, unitVal, 1e-5);
            unitVal = SpecificVolumeSquaredQuantity.Instance.FromSI(val, SpecificVolumeSquaredQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredQuantity.UnitChoicesEnum.CubicFeetSquaredPerOunceSquared).ID);
            Assert.AreEqual(1.00117* 1.00117, unitVal, 1e-4);
            unitVal = SpecificVolumeSquaredQuantity.Instance.FromSI(val, SpecificVolumeSquaredQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredQuantity.UnitChoicesEnum.CubicFeetSquaredPerPoundSquared).ID);
            Assert.AreEqual(16.02 * 16.02, unitVal, 1e-1);
            unitVal = SpecificVolumeSquaredQuantity.Instance.FromSI(val, SpecificVolumeSquaredQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredQuantity.UnitChoicesEnum.CubicInchesSquaredPerOunceSquared).ID);
            Assert.AreEqual(1730 * 1730, unitVal, 100);
            unitVal = SpecificVolumeSquaredQuantity.Instance.FromSI(val, SpecificVolumeSquaredQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredQuantity.UnitChoicesEnum.CubicInchesSquaredPerPoundSquared).ID);
            Assert.AreEqual(27680 * 27680, unitVal, 10000);
            unitVal = SpecificVolumeSquaredQuantity.Instance.FromSI(val, SpecificVolumeSquaredQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredQuantity.UnitChoicesEnum.CubicMetreSquaredPerGramSquared).ID);
            Assert.AreEqual(0.001 * 0.001, unitVal, 1e-6);
            unitVal = SpecificVolumeSquaredQuantity.Instance.FromSI(val, SpecificVolumeSquaredQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredQuantity.UnitChoicesEnum.CubicMetreSquaredPerKilogramSquared).ID);
            Assert.AreEqual(1.0 * 1.0, unitVal, 1e-3);
            unitVal = SpecificVolumeSquaredQuantity.Instance.FromSI(val, SpecificVolumeSquaredQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredQuantity.UnitChoicesEnum.CubicMillimetreSquaredPerGramSquared).ID);
            Assert.AreEqual(1000000.0* 1000000.0, unitVal, 1e0);
            unitVal = SpecificVolumeSquaredQuantity.Instance.FromSI(val, SpecificVolumeSquaredQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredQuantity.UnitChoicesEnum.CubicMillimetreSquaredPerKilogramSquared).ID);
            Assert.AreEqual(1000000000.0* 1000000000.0, unitVal, 1000);
            unitVal = SpecificVolumeSquaredQuantity.Instance.FromSI(val, SpecificVolumeSquaredQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredQuantity.UnitChoicesEnum.CubicYardSquaredPerOunceSquared).ID);
            Assert.AreEqual(0.0371 * 0.0371, unitVal, 1e-4);
            unitVal = SpecificVolumeSquaredQuantity.Instance.FromSI(val, SpecificVolumeSquaredQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredQuantity.UnitChoicesEnum.CubicYardSquaredPerPoundSquared).ID);
            Assert.AreEqual(0.5935 * 0.5935, unitVal, 1e-3);
            unitVal = SpecificVolumeSquaredQuantity.Instance.FromSI(val, SpecificVolumeSquaredQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredQuantity.UnitChoicesEnum.DecilitreSquaredPerGramSquared).ID);
            Assert.AreEqual(10.0 * 10.0, unitVal, 1e-3);
            unitVal = SpecificVolumeSquaredQuantity.Instance.FromSI(val, SpecificVolumeSquaredQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredQuantity.UnitChoicesEnum.DecilitreSquaredPerKilogramSquared).ID);
            Assert.AreEqual(10000 * 10000, unitVal, 1e-3);
            unitVal = SpecificVolumeSquaredQuantity.Instance.FromSI(val, SpecificVolumeSquaredQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredQuantity.UnitChoicesEnum.GallonUKSquaredPerOunceSquared).ID);
            Assert.AreEqual(6.24 * 6.24, unitVal, 1e-1);
            unitVal = SpecificVolumeSquaredQuantity.Instance.FromSI(val, SpecificVolumeSquaredQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredQuantity.UnitChoicesEnum.GallonUKSquaredPerPoundSquared).ID);
            Assert.AreEqual(99.86 * 99.86, unitVal, 100);
            unitVal = SpecificVolumeSquaredQuantity.Instance.FromSI(val, SpecificVolumeSquaredQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredQuantity.UnitChoicesEnum.GallonUSSquaredPerOunceSquared).ID);
            Assert.AreEqual(7.485 * 7.485, unitVal, 1e-1);
            unitVal = SpecificVolumeSquaredQuantity.Instance.FromSI(val, SpecificVolumeSquaredQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredQuantity.UnitChoicesEnum.GallonUSSquaredPerPoundSquared).ID);
            Assert.AreEqual(119.78 * 119.78, unitVal, 20);
            unitVal = SpecificVolumeSquaredQuantity.Instance.FromSI(val, SpecificVolumeSquaredQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredQuantity.UnitChoicesEnum.LitreSquaredPerGramSquared).ID);
            Assert.AreEqual(1.0 * 1.0, unitVal, 1e-3);
            unitVal = SpecificVolumeSquaredQuantity.Instance.FromSI(val, SpecificVolumeSquaredQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredQuantity.UnitChoicesEnum.LitreSquaredPerKilogramSquared).ID);
            Assert.AreEqual(1000 * 1000, unitVal, 1e-3);
            unitVal = SpecificVolumeSquaredQuantity.Instance.FromSI(val, SpecificVolumeSquaredQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredQuantity.UnitChoicesEnum.MillilitreSquaredPerGramSquared).ID);
            Assert.AreEqual(1000 * 1000, unitVal, 1e-3);
            unitVal = SpecificVolumeSquaredQuantity.Instance.FromSI(val, SpecificVolumeSquaredQuantity.Instance.GetUnitChoice(SpecificVolumeSquaredQuantity.UnitChoicesEnum.MillilitreSquaredPerKilogramSquared).ID);
            Assert.AreEqual(1000000.0* 1000000.0, unitVal, 1e-3);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, SpecificVolumeSquaredQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(0, SpecificVolumeSquaredQuantity.Instance.TimeDimension);
            Assert.AreEqual(-2, SpecificVolumeSquaredQuantity.Instance.MassDimension);
            Assert.AreEqual(6, SpecificVolumeSquaredQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, SpecificVolumeSquaredQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, SpecificVolumeSquaredQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, SpecificVolumeSquaredQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, SpecificVolumeSquaredQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, SpecificVolumeSquaredQuantity.Instance.SolidAngleDimension);
        }
    }
}
