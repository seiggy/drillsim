using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class PowerRateOfChangeUnitTest
    {
        [Test]
        public void ConvertsUsingSymbolicPowerAndTimeFactors()
        {
            PowerRateOfChangeQuantity quantity = PowerRateOfChangeQuantity.Instance;

            Assert.That(quantity.FromSI(120000.0, quantity.GetUnitChoice(PowerRateOfChangeQuantity.UnitChoicesEnum.KilowattPerSecond).ID), Is.EqualTo(120.0));
            Assert.That(quantity.FromSI(1000.0, quantity.GetUnitChoice(PowerRateOfChangeQuantity.UnitChoicesEnum.KilowattPerMinute).ID), Is.EqualTo(Factors.Minute));
            Assert.That(quantity.ToSI(Factors.Minute, quantity.GetUnitChoice(PowerRateOfChangeQuantity.UnitChoicesEnum.KilowattPerMinute).ID), Is.EqualTo(1000.0));
            Assert.That(quantity.GetUnitChoice(PowerRateOfChangeQuantity.UnitChoicesEnum.KilowattPerMinute).ConversionFactorFromSIFormula, Is.EqualTo("Factors.Minute/Factors.Kilo"));
        }

        [Test]
        public void HasPowerPerTimeDimensionsAndDwisAlias()
        {
            PowerRateOfChangeQuantity quantity = PowerRateOfChangeQuantity.Instance;

            Assert.That(quantity.LengthDimension, Is.EqualTo(2));
            Assert.That(quantity.MassDimension, Is.EqualTo(1));
            Assert.That(quantity.TimeDimension, Is.EqualTo(-4));
            Assert.That(quantity.UsualNames, Does.Contain("power rate"));
        }
    }
}
