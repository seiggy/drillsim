using NUnit.Framework;
using OSDC.UnitConversion.Service.Mcp.Tools;

namespace OSDC.UnitConversion.ServiceTest;

[TestFixture]
public sealed class McpNameNormalizerTests
{
    [TestCase("meter per hour", "metreperhour")]
    [TestCase("meters/hour", "metreperhour")]
    [TestCase("metres per hr", "metreperhour")]
    [TestCase("ft/h", "footperhour")]
    [TestCase("furlongs per fortnights", "furlongperfortnight")]
    public void Unit_names_accept_common_dialects_plurals_and_symbols(string input, string expected)
    {
        Assert.That(McpNameNormalizer.NormalizeUnit(input), Is.EqualTo(expected));
    }
}
