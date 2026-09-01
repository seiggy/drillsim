using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;

namespace ConversionUnitSystemDrillingEngineeringTest
{
    public class AllDrillingUnitChoiceConversionUnitTest
    {
        private static readonly IReadOnlyDictionary<Guid, UnitChoice> BaseChoicesById =
            BasePhysicalQuantity.AvailableBasePhysicalQuantities
                .SelectMany(quantity => quantity.UnitChoices)
                .GroupBy(choice => choice.ID)
                .ToDictionary(group => group.Key, group => group.First());

        public static IEnumerable<TestCaseData> AllDrillingUnitChoices()
        {
            foreach (BasePhysicalQuantity quantity in DrillingPhysicalQuantity.AvailablePhysicalQuantities)
            {
                foreach (UnitChoice choice in quantity.UnitChoices)
                {
                    yield return new TestCaseData(quantity.Name, choice)
                        .SetName($"{quantity.Name}: {choice.UnitName}");
                }
            }
        }

        [TestCaseSource(nameof(AllDrillingUnitChoices))]
        public void DrillingChoiceMatchesItsSymbolicallyVerifiedBaseChoice(string quantityName, UnitChoice choice)
        {
            Assert.That(BaseChoicesById.TryGetValue(choice.ID, out UnitChoice? reference), Is.True,
                $"{quantityName}: {choice.UnitName} has no base-catalog definition");
            Assert.That(choice.ConversionFactorFromSIFormula, Is.EqualTo(reference!.ConversionFactorFromSIFormula),
                $"{quantityName}: {choice.UnitName} factor formula");
            Assert.That(choice.ConversionBiasFromSIFormula, Is.EqualTo(reference.ConversionBiasFromSIFormula),
                $"{quantityName}: {choice.UnitName} bias formula");

            foreach (double siValue in new[] { -37.25, 0.0, 84.75 })
            {
                double expected = reference.FromSI(siValue);
                double scale = Math.Max(Math.Abs(expected),
                    Math.Abs(reference.ConversionFactorFromSI * siValue) + Math.Abs(reference.ConversionBiasFromSI));
                double tolerance = 2e-12 * Math.Max(scale, 1e-300);
                Assert.That(choice.FromSI(siValue), Is.EqualTo(expected).Within(tolerance),
                    $"{quantityName}: {choice.UnitName}, FromSI({siValue:R})");
                Assert.That(choice.ToSI(expected), Is.EqualTo(siValue).Within(2e-12 * Math.Max(Math.Abs(siValue), 1e-300)),
                    $"{quantityName}: {choice.UnitName}, ToSI({expected:R})");
            }
        }
    }
}
