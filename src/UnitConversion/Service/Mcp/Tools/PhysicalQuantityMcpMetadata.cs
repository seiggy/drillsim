using System.Linq;
using System.Text.Json.Nodes;
using OSDC.UnitConversion.Conversion;

namespace OSDC.UnitConversion.Service.Mcp.Tools;

internal static class PhysicalQuantityMcpMetadata
{
    public static JsonObject Create(BasePhysicalQuantity quantity)
    {
        BasePhysicalQuantity[] hierarchy = PhysicalQuantityHierarchy.Enumerate(quantity).ToArray();
        var parents = new JsonArray();
        foreach (BasePhysicalQuantity parent in hierarchy.Skip(1))
        {
            parents.Add(new JsonObject
            {
                ["id"] = parent.ID.ToString(),
                ["name"] = parent.Name,
                ["unitChoiceCount"] = parent.UnitChoices?.Count ?? 0,
                ["meaningfulPrecisionInSI"] = parent.MeaningfulPrecisionInSI
            });
        }

        return new JsonObject
        {
            ["isSpecializedPhysicalQuantity"] = hierarchy.Length > 1,
            ["parentPhysicalQuantities"] = parents,
            ["unitChoiceInheritance"] = hierarchy.Length > 1
                ? "Unit choices declared directly by this specialised quantity are preferred. If a requested unit is absent, its parent physical quantities may be searched in the listed order because they have the same inherited physical dimensions."
                : "This is a root/base physical quantity; it has no parent unit catalogue.",
            ["meaningfulPrecisionPolicy"] = quantity.MeaningfulPrecisionInSI is not null
                ? "MeaningfulPrecisionInSI belongs to this requested quantity and must be used to format results even when one or both unit choices are inherited from a parent quantity. Numeric conversion output remains unrounded."
                : "This quantity does not define MeaningfulPrecisionInSI. Numeric output remains unrounded and no domain-specific meaningful formatting precision can be inferred from this root quantity."
        };
    }
}
