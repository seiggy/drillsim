using OSDC.UnitConversion.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionUnitTest
{
    public class MomentOfAreaUnitTest
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
            unitVal = MomentOfAreaQuantity.Instance.FromSI(val, MomentOfAreaQuantity.Instance.GetUnitChoice(MomentOfAreaQuantity.UnitChoicesEnum.CentimetresToTheFourthPower).ID);
            Assert.AreEqual(100000000, unitVal, 1e-3);
            unitVal = MomentOfAreaQuantity.Instance.FromSI(val, MomentOfAreaQuantity.Instance.GetUnitChoice(MomentOfAreaQuantity.UnitChoicesEnum.FeetToTheFourthPower).ID);
            Assert.AreEqual(Math.Pow(1.0/0.3048, 4.0), unitVal, 1e-3);
            unitVal = MomentOfAreaQuantity.Instance.FromSI(val, MomentOfAreaQuantity.Instance.GetUnitChoice(MomentOfAreaQuantity.UnitChoicesEnum.InchesToTheFourthPower).ID);
            Assert.AreEqual(Math.Pow(1.0/0.0254, 4.0), unitVal, 1e-3);
            unitVal = MomentOfAreaQuantity.Instance.FromSI(val, MomentOfAreaQuantity.Instance.GetUnitChoice(MomentOfAreaQuantity.UnitChoicesEnum.MetresToTheFourthPower).ID);
            Assert.AreEqual(1.0, unitVal);
        }
    }
}
