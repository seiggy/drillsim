using System;
using System.Collections.Generic;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using OSDC.UnitConversion.Conversion.UnitSystem.DrillingEngineering;

namespace OSDC.UnitConversion.Service;

/// <summary>
/// Validates the physical-quantity-to-unit-choice mappings persisted in a
/// <see cref="DrillingUnitSystem"/> and derives properties determined by that mapping.
/// </summary>
public static class UnitSystemValidation
{
    /// <summary>
    /// Validates every declared choice. A valid non-empty mapping is SI only when all
    /// of its selected unit choices are SI; the caller-provided <c>IsSI</c> value is
    /// therefore ignored and replaced by the derived value.
    /// </summary>
    public static IReadOnlyList<string> ValidateAndDerive(DrillingUnitSystem unitSystem)
    {
        ArgumentNullException.ThrowIfNull(unitSystem);

        var errors = new List<string>();
        Dictionary<string, string>? choices = unitSystem.Choices;
        if (choices is null)
        {
            errors.Add("Choices must be a dictionary of physical-quantity UUIDs to unit-choice UUIDs.");
            return errors;
        }

        bool isSI = choices.Count > 0;
        foreach (KeyValuePair<string, string> mapping in choices)
        {
            if (!Guid.TryParse(mapping.Key, out Guid quantityId) || quantityId == Guid.Empty)
            {
                errors.Add($"Physical quantity key '{mapping.Key}' is not a non-empty UUID.");
                isSI = false;
                continue;
            }

            BasePhysicalQuantity? quantity = DrillingPhysicalQuantity.GetQuantity(quantityId);
            if (quantity is null)
            {
                errors.Add($"Physical quantity '{quantityId}' is not available.");
                isSI = false;
                continue;
            }

            if (!Guid.TryParse(mapping.Value, out Guid unitChoiceId) || unitChoiceId == Guid.Empty)
            {
                errors.Add($"Unit choice '{mapping.Value}' for physical quantity '{quantity.Name}' is not a non-empty UUID.");
                isSI = false;
                continue;
            }

            UnitChoice? unitChoice = quantity.GetUnitChoice(unitChoiceId);
            if (unitChoice is null)
            {
                errors.Add($"Unit choice '{unitChoiceId}' is not available for physical quantity '{quantity.Name}' ({quantityId}).");
                isSI = false;
                continue;
            }

            isSI &= unitChoice.IsSI;
        }

        if (errors.Count == 0)
        {
            unitSystem.IsSI = isSI;
        }

        return errors;
    }
}
