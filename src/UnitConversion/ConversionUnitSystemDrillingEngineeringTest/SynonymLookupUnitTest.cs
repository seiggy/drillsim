using OSDC.UnitConversion.Conversion.DrillingEngineering;
using OSDC.UnitConversion.Conversion;

namespace ConversionUnitSystemDrillingEngineeringTest
{
    public class SynonymLookupUnitTest
    {
        [Test]
        public void DrillingQuantitySynonymsAreResolvable()
        {
            Assert.That(DrillingPhysicalQuantity.GetQuantity("rate-of-penetration"),
                Is.SameAs(RateOfPenetrationDrillingQuantity.Instance));
            Assert.That(DrillingPhysicalQuantity.GetQuantity("ROP"),
                Is.SameAs(RateOfPenetrationDrillingQuantity.Instance));

            var names = DrillingPhysicalQuantity.AvailablePhysicalQuantities
                .SelectMany(quantity => quantity.UsualNames.Append(quantity.Name)
                    .Select(name => (Name: Normalize(name), Quantity: quantity)))
                .Where(item => item.Name.Length > 0)
                .GroupBy(item => item.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var group in names)
            {
                BasePhysicalQuantity[] matches = group.Select(item => item.Quantity).DistinctBy(q => q.ID).ToArray();
                Assert.That(matches, Has.Length.EqualTo(1),
                    $"Ambiguous drilling quantity name '{group.Key}': {string.Join(", ", matches.Select(q => q.Name))}");
                Assert.That(DrillingPhysicalQuantity.GetQuantity(group.First().Name), Is.SameAs(matches[0]), group.Key);
            }
        }

        private static string Normalize(string value) =>
            new(value.Where(char.IsLetterOrDigit).ToArray());
    }
}
