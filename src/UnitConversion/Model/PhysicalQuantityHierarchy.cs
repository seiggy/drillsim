using System;
using System.Collections.Generic;
using System.Reflection;

namespace OSDC.UnitConversion.Conversion
{
    /// <summary>
    /// Provides access to the semantic inheritance chain of a physical quantity.
    /// A specialised quantity may intentionally publish a small set of common units while
    /// remaining compatible with the larger unit catalogue declared by an ancestor quantity.
    /// </summary>
    public static class PhysicalQuantityHierarchy
    {
        /// <summary>
        /// Enumerates the requested quantity followed by its instantiable physical-quantity ancestors.
        /// </summary>
        public static IEnumerable<BasePhysicalQuantity> Enumerate(BasePhysicalQuantity quantity)
        {
            ArgumentNullException.ThrowIfNull(quantity);

            yield return quantity;
            Type? type = quantity.GetType().BaseType;
            while (type is not null && typeof(BasePhysicalQuantity).IsAssignableFrom(type))
            {
                PropertyInfo? instanceProperty = type.GetProperty(
                    "Instance",
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

                if (instanceProperty?.GetValue(null) is BasePhysicalQuantity ancestor)
                {
                    yield return ancestor;
                }

                type = type.BaseType;
            }
        }

        /// <summary>
        /// Finds a unit choice by UUID on the requested quantity or, if absent, on its ancestors.
        /// </summary>
        public static UnitChoice? FindUnitChoice(
            BasePhysicalQuantity quantity,
            Guid unitChoiceId,
            out BasePhysicalQuantity? declaringQuantity)
        {
            foreach (BasePhysicalQuantity candidate in Enumerate(quantity))
            {
                UnitChoice? choice = candidate.GetUnitChoice(unitChoiceId);
                if (choice is not null)
                {
                    declaringQuantity = candidate;
                    return choice;
                }
            }

            declaringQuantity = null;
            return null;
        }
    }
}
