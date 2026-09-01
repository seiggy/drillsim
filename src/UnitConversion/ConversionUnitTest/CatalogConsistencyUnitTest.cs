using OSDC.UnitConversion.Conversion;

namespace ConversionUnitTest
{
    public class CatalogConsistencyUnitTest
    {
        [Test]
        public void QuantityAndUnitIdentifiersAreGloballyUniqueAndNonEmpty()
        {
            var quantities = BasePhysicalQuantity.AvailableBasePhysicalQuantities;

            Assert.That(quantities, Has.All.Matches<BasePhysicalQuantity>(q => q.ID != Guid.Empty));
            Assert.That(quantities.Select(q => q.ID).Distinct().Count(), Is.EqualTo(quantities.Count));

            var choices = quantities.SelectMany(q => q.UnitChoices).ToList();
            Assert.That(choices, Has.All.Matches<UnitChoice>(choice => choice.ID != Guid.Empty));

            foreach (var reusedIdentifier in choices.GroupBy(choice => choice.ID).Where(group => group.Count() > 1))
            {
                UnitChoice reference = reusedIdentifier.First();
                Assert.That(reusedIdentifier, Has.All.Matches<UnitChoice>(choice =>
                    string.Equals(choice.UnitName, reference.UnitName, StringComparison.OrdinalIgnoreCase)
                    && choice.ConversionFactorFromSIFormula == reference.ConversionFactorFromSIFormula
                    && choice.ConversionBiasFromSIFormula == reference.ConversionBiasFromSIFormula),
                    reusedIdentifier.Key.ToString());
            }
        }

        [Test]
        public void EveryQuantityHasExactlyOneMatchingCoherentSIChoice()
        {
            foreach (BasePhysicalQuantity quantity in BasePhysicalQuantity.AvailableBasePhysicalQuantities)
            {
                var siChoices = quantity.UnitChoices.Where(choice => choice.IsSI).ToList();
                Assert.That(siChoices, Has.Count.EqualTo(1), quantity.Name);
                Assert.That(siChoices[0].UnitName, Is.EqualTo(quantity.SIUnitName).IgnoreCase, quantity.Name);
                Assert.That(siChoices[0].ConversionFactorFromSI, Is.EqualTo(1.0), quantity.Name);
                Assert.That(siChoices[0].ConversionBiasFromSI, Is.EqualTo(0.0), quantity.Name);
            }
        }

        [Test]
        public void EveryUnitHasAValidAffineConversionAndRoundTrips()
        {
            foreach (BasePhysicalQuantity quantity in BasePhysicalQuantity.AvailableBasePhysicalQuantities)
            {
                foreach (UnitChoice choice in quantity.UnitChoices)
                {
                    Assert.That(choice.ConversionFactorFromSI, Is.GreaterThan(0.0), $"{quantity.Name}: {choice.UnitName}");
                    Assert.That(double.IsFinite(choice.ConversionFactorFromSI), Is.True, $"{quantity.Name}: {choice.UnitName}");
                    Assert.That(double.IsFinite(choice.ConversionBiasFromSI), Is.True, $"{quantity.Name}: {choice.UnitName}");

                    const double value = 1.23456789;
                    Assert.That(choice.ToSI(choice.FromSI(value)), Is.EqualTo(value).Within(1e-11),
                        $"{quantity.Name}: {choice.UnitName}");
                }
            }
        }

        [Test]
        public void CorrectedDerivedConstantsHaveExpectedMagnitudes()
        {
            Assert.That(Factors.YearAverageGregorian, Is.EqualTo(365.2425 * Factors.Day).Within(1e-9));
            Assert.That(Factors.InchMercury32degF, Is.EqualTo(3386.38).Within(0.02));
            Assert.That(Factors.InchMercury60degF, Is.EqualTo(3376.85).Within(0.02));
            Assert.That(Factors.MillimetreWater4degC, Is.EqualTo(9.80638).Within(0.00002));
        }

        [Test]
        public void MobilityMeaningsAndDimensionsRemainDistinct()
        {
            Assert.That(MobilityQuantity.Instance.ID, Is.Not.EqualTo(ElectricalMobilityQuantity.Instance.ID));
            Assert.That(MobilityQuantity.Instance.LengthDimension, Is.EqualTo(3));
            Assert.That(MobilityQuantity.Instance.MassDimension, Is.EqualTo(-1));
            Assert.That(MobilityQuantity.Instance.TimeDimension, Is.EqualTo(1));
            Assert.That(ElectricalMobilityQuantity.Instance.LengthDimension, Is.EqualTo(0));
            Assert.That(ElectricalMobilityQuantity.Instance.MassDimension, Is.EqualTo(-1));
            Assert.That(ElectricalMobilityQuantity.Instance.TimeDimension, Is.EqualTo(2));
            Assert.That(ElectricalMobilityQuantity.Instance.ElectricCurrentDimension, Is.EqualTo(1));

            UnitChoice fieldMobility = MobilityQuantity.Instance.GetUnitChoice(MobilityQuantity.UnitChoicesEnum.MillidarcyPerCentipoise);
            Assert.That(fieldMobility.FromSI(Factors.Darcy), Is.EqualTo(1.0).Within(1e-12));
        }

        [Test]
        public void VolumePerAngleUsesRadiansCoherentlyAndSupportsRevolutions()
        {
            var quantity = VolumePerAngleQuantity.Instance;
            Assert.That(quantity.LengthDimension, Is.EqualTo(3));
            Assert.That(quantity.PlaneAngleDimension, Is.EqualTo(-1));

            UnitChoice perRevolution = quantity.GetUnitChoice(VolumePerAngleQuantity.UnitChoicesEnum.CubicMetrePerRevolution);
            Assert.That(perRevolution.FromSI(1.0), Is.EqualTo(2.0 * Math.PI).Within(1e-14));
            Assert.That(perRevolution.ToSI(2.0 * Math.PI), Is.EqualTo(1.0).Within(1e-14));
        }

        [Test]
        public void QuantityNamesAndUnambiguousSynonymsAreResolvable()
        {
            Assert.That(BasePhysicalQuantity.GetQuantity("acoustic-intensity"),
                Is.SameAs(PowerPerAreaQuantity.Instance));
            Assert.That(BasePhysicalQuantity.GetQuantity("SPECIFIC IMPACT ENERGY"),
                Is.SameAs(EnergyPerAreaQuantity.Instance));
            Assert.That(BasePhysicalQuantity.GetQuantity("torque"),
                Is.SameAs(TorqueQuantity.Instance));
            Assert.That(BasePhysicalQuantity.GetQuantity("torque rate of change"),
                Is.SameAs(TorqueRateOfChangeQuantity.Instance));

            var quantities = BasePhysicalQuantity.AvailableBasePhysicalQuantities;
            var names = quantities
                .SelectMany(quantity => quantity.UsualNames.Append(quantity.Name)
                    .Select(name => (Name: Normalize(name), Quantity: quantity)))
                .Where(item => item.Name.Length > 0)
                .GroupBy(item => item.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var group in names)
            {
                BasePhysicalQuantity[] matches = group.Select(item => item.Quantity).DistinctBy(q => q.ID).ToArray();
                Assert.That(matches, Has.Length.EqualTo(1),
                    $"Ambiguous quantity name '{group.Key}': {string.Join(", ", matches.Select(q => q.Name))}");
                Assert.That(BasePhysicalQuantity.GetQuantity(group.First().Name), Is.SameAs(matches[0]), group.Key);
            }
        }

        [Test]
        public void UnitLabelsAndSupplementedSynonymsAreResolvableWithoutAmbiguity()
        {
            UnitChoice pressurePerVolume = PressurePerVolumeQuantity.Instance.UnitChoices.Single(choice => choice.IsSI);
            string asciiPressureLabel = pressurePerVolume.UnitLabel.Replace("³", "^3");
            Assert.That(PressurePerVolumeQuantity.Instance.GetUnitChoice(asciiPressureLabel.ToLowerInvariant()),
                Is.SameAs(pressurePerVolume));

            UnitChoice torque = TorqueQuantity.Instance.UnitChoices.Single(choice => choice.IsSI);
            string asteriskTorqueLabel = torque.UnitLabel.Replace("•", "*");
            Assert.That(TorqueQuantity.Instance.GetUnitChoice(asteriskTorqueLabel), Is.SameAs(torque));

            foreach (BasePhysicalQuantity quantity in BasePhysicalQuantity.AvailableBasePhysicalQuantities)
            {
                var alternatives = quantity.UnitChoices
                    .SelectMany(choice => choice.Synonyms.Append(choice.UnitLabel)
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .Select(name => (Name: name, Choice: choice)))
                    .GroupBy(item => item.Name, StringComparer.OrdinalIgnoreCase);

                foreach (var group in alternatives)
                {
                    UnitChoice[] matches = group.Select(item => item.Choice).DistinctBy(choice => choice.ID).ToArray();
                    if (matches.Length == 1)
                    {
                        Assert.That(quantity.GetUnitChoice(group.Key), Is.SameAs(matches[0]), $"{quantity.Name}: {group.Key}");
                    }
                }
            }
        }

        private static string Normalize(string value) =>
            new(value.Where(char.IsLetterOrDigit).ToArray());
    }
}
