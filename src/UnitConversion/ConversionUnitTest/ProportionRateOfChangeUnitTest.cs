using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class ProportionRateOfChangeUnitTest
    {
        [Test]
        public void ConvertsChokeRateUsingExistingPrefixFactors()
        {
            ProportionRateOfChangeQuantity quantity = ProportionRateOfChangeQuantity.Instance;

            Assert.That(quantity.FromSI(0.25, quantity.GetUnitChoice(ProportionRateOfChangeQuantity.UnitChoicesEnum.PercentPerSecond).ID), Is.EqualTo(25.0));
            Assert.That(quantity.ToSI(25.0, quantity.GetUnitChoice(ProportionRateOfChangeQuantity.UnitChoicesEnum.PercentPerSecond).ID), Is.EqualTo(0.25));
            Assert.That(quantity.GetUnitChoice(ProportionRateOfChangeQuantity.UnitChoicesEnum.PercentPerSecond).ConversionFactorFromSIFormula, Is.EqualTo("1.0/Factors.Centi"));
        }

        [Test]
        public void IsAProportionDerivativeRatherThanFrequency()
        {
            ProportionRateOfChangeQuantity quantity = ProportionRateOfChangeQuantity.Instance;

            Assert.That(quantity.TimeDimension, Is.EqualTo(-1));
            Assert.That(quantity.LengthDimension, Is.Zero);
            Assert.That(quantity.MassDimension, Is.Zero);
            Assert.That(quantity.UsualNames, Does.Contain("proportion rate"));
        }
    }
}
