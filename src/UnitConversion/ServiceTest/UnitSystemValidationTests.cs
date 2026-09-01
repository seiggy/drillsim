using NUnit.Framework;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using OSDC.UnitConversion.Conversion.UnitSystem.DrillingEngineering;
using OSDC.UnitConversion.Service;

namespace OSDC.UnitConversion.ServiceTest;

[TestFixture]
public class UnitSystemValidationTests
{
    [Test]
    public void ValidateAndDerive_ValidSIPair_OverridesCallerSuppliedIsSI()
    {
        BasePhysicalQuantity quantity = DrillingPhysicalQuantity.AvailablePhysicalQuantities
            .First(item => item.UnitChoices?.Any(choice => choice.IsSI) == true);
        UnitChoice siChoice = quantity.UnitChoices!.First(choice => choice.IsSI);
        var unitSystem = new DrillingUnitSystem
        {
            ID = Guid.NewGuid(), Name = "Derived SI", IsSI = false,
            Choices = new Dictionary<string, string> { [quantity.ID.ToString()] = siChoice.ID.ToString() }
        };

        IReadOnlyList<string> errors = UnitSystemValidation.ValidateAndDerive(unitSystem);

        Assert.Multiple(() =>
        {
            Assert.That(errors, Is.Empty);
            Assert.That(unitSystem.IsSI, Is.True);
        });
    }

    [Test]
    public void ValidateAndDerive_RejectsUnknownAndIncompatibleChoices()
    {
        BasePhysicalQuantity first = DrillingPhysicalQuantity.AvailablePhysicalQuantities
            .First(item => item.UnitChoices?.Count > 0);
        BasePhysicalQuantity second = DrillingPhysicalQuantity.AvailablePhysicalQuantities
            .First(item => item.ID != first.ID && item.UnitChoices?.Count > 0);
        UnitChoice incompatibleChoice = second.UnitChoices![0];
        var unitSystem = new DrillingUnitSystem
        {
            ID = Guid.NewGuid(), Name = "Invalid",
            Choices = new Dictionary<string, string>
            {
                [first.ID.ToString()] = incompatibleChoice.ID.ToString(),
                [Guid.NewGuid().ToString()] = Guid.NewGuid().ToString()
            }
        };

        IReadOnlyList<string> errors = UnitSystemValidation.ValidateAndDerive(unitSystem);

        Assert.Multiple(() =>
        {
            Assert.That(errors, Has.Count.EqualTo(2));
            Assert.That(errors[0], Does.Contain("not available for physical quantity"));
            Assert.That(errors[1], Does.Contain("Physical quantity"));
        });
    }

    [Test]
    public void ValidateAndDerive_NonSIChoice_SetsIsSIToFalse()
    {
        BasePhysicalQuantity quantity = DrillingPhysicalQuantity.AvailablePhysicalQuantities
            .First(item => item.UnitChoices?.Any(choice => !choice.IsSI) == true);
        UnitChoice nonSiChoice = quantity.UnitChoices!.First(choice => !choice.IsSI);
        var unitSystem = new DrillingUnitSystem
        {
            ID = Guid.NewGuid(), Name = "Derived non-SI", IsSI = true,
            Choices = new Dictionary<string, string> { [quantity.ID.ToString()] = nonSiChoice.ID.ToString() }
        };

        IReadOnlyList<string> errors = UnitSystemValidation.ValidateAndDerive(unitSystem);

        Assert.Multiple(() =>
        {
            Assert.That(errors, Is.Empty);
            Assert.That(unitSystem.IsSI, Is.False);
        });
    }
}
