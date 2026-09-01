using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using OSDC.UnitConversion.Model;

namespace ModelTest;

[TestFixture]
public sealed class PhysicalQuantityHierarchyTests
{
    [Test]
    public void Rate_of_penetration_inherits_velocity_unit_choices()
    {
        BasePhysicalQuantity[] hierarchy = PhysicalQuantityHierarchy
            .Enumerate(RateOfPenetrationDrillingQuantity.Instance)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(hierarchy.Select(quantity => quantity.Name),
                Is.EqualTo(new[] { "RateOfPenetrationDrilling", "Velocity" }));
            Assert.That(hierarchy[0].MeaningfulPrecisionInSI, Is.Not.Null);
            Assert.That(hierarchy[1].MeaningfulPrecisionInSI, Is.Null);
        });
    }

    [Test]
    public void Conversion_uses_parent_units_and_child_meaningful_precision()
    {
        BasePhysicalQuantity rop = RateOfPenetrationDrillingQuantity.Instance;
        UnitChoice metrePerHour = RateOfPenetrationDrillingQuantity.Instance
            .GetUnitChoice(RateOfPenetrationDrillingQuantity.UnitChoicesEnum.MetrePerHour);
        UnitChoice furlongPerFortnight = VelocityQuantity.Instance
            .GetUnitChoice(VelocityQuantity.UnitChoicesEnum.FurlongPerFortnight);
        var value = new ValueConversion { DataIn = 30d };
        var conversion = new QuantityUnitConversion
        {
            QuantityID = rop.ID,
            UnitChoiceIDIn = metrePerHour.ID,
            UnitChoiceIDOut = furlongPerFortnight.ID,
            ValueConversionList = [value]
        };

        bool success = conversion.Calculate();
        double expected = 30d * 336d / 201.168d;

        Assert.Multiple(() =>
        {
            Assert.That(success, Is.True);
            Assert.That(value.DataOut, Is.EqualTo(expected).Within(1e-10));
            Assert.That(value.DataOutString,
                Is.EqualTo(furlongPerFortnight.ToStringInUnit(expected, rop.MeaningfulPrecisionInSI)));
        });
    }
}
