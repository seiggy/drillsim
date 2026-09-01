using OSDC.UnitConversion.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionUnitTest
{
    public class SpecificVolumeUnitTest
    {
        [Test]
        public void Test1()
        {
            double val = 1.0;
            double unitVal;
            unitVal = SpecificVolumeQuantity.Instance.FromSI(val, SpecificVolumeQuantity.Instance.GetUnitChoice(SpecificVolumeQuantity.UnitChoicesEnum.CentilitrePerGram).ID);
            Assert.AreEqual(100, unitVal, 1e-5);
            unitVal = SpecificVolumeQuantity.Instance.FromSI(val, SpecificVolumeQuantity.Instance.GetUnitChoice(SpecificVolumeQuantity.UnitChoicesEnum.CentilitrePerKilogram).ID);
            Assert.AreEqual(100000, unitVal, 1e-5);
            unitVal = SpecificVolumeQuantity.Instance.FromSI(val, SpecificVolumeQuantity.Instance.GetUnitChoice(SpecificVolumeQuantity.UnitChoicesEnum.CubicCentimetrePerGram).ID);
            Assert.AreEqual(1000, unitVal, 1e-5);
            unitVal = SpecificVolumeQuantity.Instance.FromSI(val, SpecificVolumeQuantity.Instance.GetUnitChoice(SpecificVolumeQuantity.UnitChoicesEnum.CubicCentimetrePerKilogram).ID);
            Assert.AreEqual(1000000, unitVal, 1e-5);
            unitVal = SpecificVolumeQuantity.Instance.FromSI(val, SpecificVolumeQuantity.Instance.GetUnitChoice(SpecificVolumeQuantity.UnitChoicesEnum.CubicDecimetrePerGram).ID);
            Assert.AreEqual(1, unitVal, 1e-5);
            unitVal = SpecificVolumeQuantity.Instance.FromSI(val, SpecificVolumeQuantity.Instance.GetUnitChoice(SpecificVolumeQuantity.UnitChoicesEnum.CubicFeetPerOunce).ID);
            Assert.AreEqual(1.00117, unitVal, 1e-4);
            unitVal = SpecificVolumeQuantity.Instance.FromSI(val, SpecificVolumeQuantity.Instance.GetUnitChoice(SpecificVolumeQuantity.UnitChoicesEnum.CubicFeetPerPound).ID);
            Assert.AreEqual(16.02, unitVal, 1e-2);
            unitVal = SpecificVolumeQuantity.Instance.FromSI(val, SpecificVolumeQuantity.Instance.GetUnitChoice(SpecificVolumeQuantity.UnitChoicesEnum.CubicInchesPerOunce).ID);
            Assert.AreEqual(1730, unitVal, 1e0);
            unitVal = SpecificVolumeQuantity.Instance.FromSI(val, SpecificVolumeQuantity.Instance.GetUnitChoice(SpecificVolumeQuantity.UnitChoicesEnum.CubicInchesPerPound).ID);
            Assert.AreEqual(27680, unitVal, 1e0);
            unitVal = SpecificVolumeQuantity.Instance.FromSI(val, SpecificVolumeQuantity.Instance.GetUnitChoice(SpecificVolumeQuantity.UnitChoicesEnum.CubicMetrePerGram).ID);
            Assert.AreEqual(0.001, unitVal, 1e-6);
            unitVal = SpecificVolumeQuantity.Instance.FromSI(val, SpecificVolumeQuantity.Instance.GetUnitChoice(SpecificVolumeQuantity.UnitChoicesEnum.CubicMetrePerKilogram).ID);
            Assert.AreEqual(1.0, unitVal, 1e-6);
            unitVal = SpecificVolumeQuantity.Instance.FromSI(val, SpecificVolumeQuantity.Instance.GetUnitChoice(SpecificVolumeQuantity.UnitChoicesEnum.CubicMillimetrePerGram).ID);
            Assert.AreEqual(1000000, unitVal, 1e0);
            unitVal = SpecificVolumeQuantity.Instance.FromSI(val, SpecificVolumeQuantity.Instance.GetUnitChoice(SpecificVolumeQuantity.UnitChoicesEnum.CubicMillimetrePerKilogram).ID);
            Assert.AreEqual(1000000000, unitVal, 1e0);
            unitVal = SpecificVolumeQuantity.Instance.FromSI(val, SpecificVolumeQuantity.Instance.GetUnitChoice(SpecificVolumeQuantity.UnitChoicesEnum.CubicYardPerOunce).ID);
            Assert.AreEqual(0.0371, unitVal, 1e-4);
            unitVal = SpecificVolumeQuantity.Instance.FromSI(val, SpecificVolumeQuantity.Instance.GetUnitChoice(SpecificVolumeQuantity.UnitChoicesEnum.CubicYardPerPound).ID);
            Assert.AreEqual(0.5935, unitVal, 1e-3);
            unitVal = SpecificVolumeQuantity.Instance.FromSI(val, SpecificVolumeQuantity.Instance.GetUnitChoice(SpecificVolumeQuantity.UnitChoicesEnum.DecilitrePerGram).ID);
            Assert.AreEqual(10.0, unitVal, 1e-6);
            unitVal = SpecificVolumeQuantity.Instance.FromSI(val, SpecificVolumeQuantity.Instance.GetUnitChoice(SpecificVolumeQuantity.UnitChoicesEnum.DecilitrePerKilogram).ID);
            Assert.AreEqual(10000, unitVal, 1e-6);
            unitVal = SpecificVolumeQuantity.Instance.FromSI(val, SpecificVolumeQuantity.Instance.GetUnitChoice(SpecificVolumeQuantity.UnitChoicesEnum.GallonUKPerOunce).ID);
            Assert.AreEqual(6.24, unitVal, 1e-2);
            unitVal = SpecificVolumeQuantity.Instance.FromSI(val, SpecificVolumeQuantity.Instance.GetUnitChoice(SpecificVolumeQuantity.UnitChoicesEnum.GallonUKPerPound).ID);
            Assert.AreEqual(99.86, unitVal, 1e-1);
            unitVal = SpecificVolumeQuantity.Instance.FromSI(val, SpecificVolumeQuantity.Instance.GetUnitChoice(SpecificVolumeQuantity.UnitChoicesEnum.GallonUSPerOunce).ID);
            Assert.AreEqual(7.485, unitVal, 1e-2);
            unitVal = SpecificVolumeQuantity.Instance.FromSI(val, SpecificVolumeQuantity.Instance.GetUnitChoice(SpecificVolumeQuantity.UnitChoicesEnum.GallonUSPerPound).ID);
            Assert.AreEqual(119.78, unitVal, 1e-1);
            unitVal = SpecificVolumeQuantity.Instance.FromSI(val, SpecificVolumeQuantity.Instance.GetUnitChoice(SpecificVolumeQuantity.UnitChoicesEnum.LitrePerGram).ID);
            Assert.AreEqual(1.0, unitVal, 1e-6);
            unitVal = SpecificVolumeQuantity.Instance.FromSI(val, SpecificVolumeQuantity.Instance.GetUnitChoice(SpecificVolumeQuantity.UnitChoicesEnum.LitrePerKilogram).ID);
            Assert.AreEqual(1000, unitVal, 1e-6);
            unitVal = SpecificVolumeQuantity.Instance.FromSI(val, SpecificVolumeQuantity.Instance.GetUnitChoice(SpecificVolumeQuantity.UnitChoicesEnum.MillilitrePerGram).ID);
            Assert.AreEqual(1000, unitVal, 1e-6);
            unitVal = SpecificVolumeQuantity.Instance.FromSI(val, SpecificVolumeQuantity.Instance.GetUnitChoice(SpecificVolumeQuantity.UnitChoicesEnum.MillilitrePerKilogram).ID);
            Assert.AreEqual(1000000, unitVal, 1e-6);
        }

        [Test]
        public void Test2()
        {
            Assert.AreEqual(0, SpecificVolumeQuantity.Instance.PlaneAngleDimension);
            Assert.AreEqual(0, SpecificVolumeQuantity.Instance.TimeDimension);
            Assert.AreEqual(-1, SpecificVolumeQuantity.Instance.MassDimension);
            Assert.AreEqual(3, SpecificVolumeQuantity.Instance.LengthDimension);
            Assert.AreEqual(0, SpecificVolumeQuantity.Instance.TemperatureDimension);
            Assert.AreEqual(0, SpecificVolumeQuantity.Instance.ElectricCurrentDimension);
            Assert.AreEqual(0, SpecificVolumeQuantity.Instance.AmountSubstanceDimension);
            Assert.AreEqual(0, SpecificVolumeQuantity.Instance.LuminousIntensityDimension);
            Assert.AreEqual(0, SpecificVolumeQuantity.Instance.SolidAngleDimension);
        }
    }
}
