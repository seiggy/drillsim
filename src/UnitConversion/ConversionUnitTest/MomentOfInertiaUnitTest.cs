using OSDC.UnitConversion.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionUnitTest
{
    public class MomentOfInertiaUnitTest
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
            unitVal = MomentOfInertiaQuantity.Instance.FromSI(val, MomentOfInertiaQuantity.Instance.GetUnitChoice(MomentOfInertiaQuantity.UnitChoicesEnum.GramCentimetreSquared).ID);
            Assert.AreEqual(10000000, unitVal, 1e-3);
            unitVal = MomentOfInertiaQuantity.Instance.FromSI(val, MomentOfInertiaQuantity.Instance.GetUnitChoice(MomentOfInertiaQuantity.UnitChoicesEnum.KilogramMetreSquared).ID);
            Assert.AreEqual(1, unitVal, 1e-3);
            unitVal = MomentOfInertiaQuantity.Instance.FromSI(val, MomentOfInertiaQuantity.Instance.GetUnitChoice(MomentOfInertiaQuantity.UnitChoicesEnum.PoundFootSquared).ID);
            Assert.AreEqual((1.0/Factors.Pound)*Math.Pow(1.0/0.3048,2.0), unitVal, 1e-3);
            unitVal = MomentOfInertiaQuantity.Instance.FromSI(val, MomentOfInertiaQuantity.Instance.GetUnitChoice(MomentOfInertiaQuantity.UnitChoicesEnum.PoundInchSquared).ID);
            Assert.AreEqual((1.0 / Factors.Pound) * Math.Pow(1.0 / 0.0254, 2.0), unitVal);
        }
    }
}
