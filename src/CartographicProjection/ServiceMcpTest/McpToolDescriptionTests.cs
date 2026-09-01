using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging.Abstractions;
using NORCE.Drilling.CartographicProjection.Service.Managers;
using NORCE.Drilling.CartographicProjection.Service.Mcp;
using NORCE.Drilling.CartographicProjection.Service.Mcp.Tools;
using NUnit.Framework;

namespace NORCE.Drilling.CartographicProjection.ServiceMcpTest;

[TestFixture]
public sealed class McpToolDescriptionTests
{
    private IReadOnlyDictionary<string, IMcpTool> _tools = null!;

    [SetUp]
    public void SetUp()
    {
        var loggerFactory = NullLoggerFactory.Instance;
        SqlConnectionManager connectionManager = null!;
        IMcpTool[] tools =
        {
            new GetAllCartographicProjectionIdsMcpTool(loggerFactory, connectionManager),
            new GetAllCartographicProjectionMetaInfoMcpTool(loggerFactory, connectionManager),
            new GetCartographicProjectionByIdMcpTool(loggerFactory, connectionManager),
            new GetAllCartographicProjectionLightMcpTool(loggerFactory, connectionManager),
            new GetAllCartographicProjectionMcpTool(loggerFactory, connectionManager),
            new PostCartographicProjectionMcpTool(loggerFactory, connectionManager),
            new PutCartographicProjectionByIdMcpTool(loggerFactory, connectionManager),
            new DeleteCartographicProjectionByIdMcpTool(loggerFactory, connectionManager),
            new GetAllCartographicConversionSetIdsMcpTool(loggerFactory, connectionManager),
            new GetAllCartographicConversionSetMetaInfoMcpTool(loggerFactory, connectionManager),
            new GetCartographicConversionSetByIdMcpTool(loggerFactory, connectionManager),
            new GetAllCartographicConversionSetLightMcpTool(loggerFactory, connectionManager),
            new GetAllCartographicConversionSetMcpTool(loggerFactory, connectionManager),
            new PostCartographicConversionSetMcpTool(loggerFactory, connectionManager),
            new PutCartographicConversionSetByIdMcpTool(loggerFactory, connectionManager),
            new DeleteCartographicConversionSetByIdMcpTool(loggerFactory, connectionManager),
            new GetAllCartographicProjectionTypeIdsMcpTool(loggerFactory),
            new GetCartographicProjectionTypeByIdMcpTool(loggerFactory),
            new GetAllCartographicProjectionTypeMcpTool(loggerFactory),
            new GetCartographicProjectionUsageStatisticsMcpTool(loggerFactory)
        };
        _tools = tools.ToDictionary(tool => tool.Name);
    }

    [Test]
    public void Every_rest_tool_has_a_detailed_description_and_explicit_schema()
    {
        Assert.That(_tools, Has.Count.EqualTo(20));
        Assert.That(_tools.Keys.All(name => !name.Contains('.')), Is.True);
        foreach (IMcpTool tool in _tools.Values)
        {
            Assert.That(tool.Description, Has.Length.GreaterThan(100), tool.Name);
            Assert.That(tool.InputSchema, Is.TypeOf<JsonObject>(), tool.Name);
        }
    }

    [Test]
    public void Conversion_tools_explain_create_retrieve_delete_lifecycle()
    {
        Assert.Multiple(() =>
        {
            Assert.That(_tools["cartographic_conversion_set_create"].Description, Does.Contain("Generate a new UUID"));
            Assert.That(_tools["cartographic_conversion_set_create"].Description, Does.Contain("get_by_id"));
            Assert.That(_tools["cartographic_conversion_set_get_by_id"].Description, Does.Contain("calculated results"));
            Assert.That(_tools["cartographic_conversion_set_get_by_id"].Description, Does.Contain("delete"));
            Assert.That(_tools["cartographic_conversion_set_delete_by_id"].Description, Does.Contain("after get_by_id"));
        });
    }

    [Test]
    public void Conversion_create_schema_describes_supported_inputs_units_and_datums()
    {
        JsonObject root = RequireObject(_tools["cartographic_conversion_set_create"].InputSchema);
        JsonObject conversionCase = Property(root, "cartographicConversionSet");
        Assert.That(RequiredNames(conversionCase), Is.EquivalentTo(new[] { "MetaInfo", "CartographicProjectionID", "CartographicCoordinateList" }));
        JsonObject coordinateList = Property(conversionCase, "CartographicCoordinateList");
        JsonObject coordinate = RequireObject(coordinateList["items"]);
        JsonObject geodetic = Property(coordinate, "GeodeticCoordinate");
        Assert.Multiple(() =>
        {
            Assert.That(conversionCase["description"]?.GetValue<string>(), Does.Contain("retrieve it by the same UUID"));
            Assert.That(coordinateList["minItems"]?.GetValue<int>(), Is.EqualTo(1));
            Assert.That(Property(coordinate, "Northing")["description"]?.GetValue<string>(), Does.Contain("meters (SI)"));
            Assert.That(Property(geodetic, "LatitudeWGS84")["description"]?.GetValue<string>(), Does.Contain("radians (SI)"));
            Assert.That(Property(geodetic, "VerticalDepthWGS84")["description"]?.GetValue<string>(), Does.Contain("WGS84 vertical datum"));
            Assert.That(Property(geodetic, "OctreeDepth")["description"]?.GetValue<string>(), Does.Contain("24"));
        });
    }

    [Test]
    public void Projection_schema_lists_algorithms_and_parameter_units()
    {
        JsonObject root = RequireObject(_tools["cartographic_projection_create"].InputSchema);
        JsonObject projection = Property(root, "cartographicProjection");
        JsonArray algorithms = (JsonArray)Property(projection, "ProjectionType")["enum"]!;
        Assert.Multiple(() =>
        {
            Assert.That(algorithms.Select(value => value!.GetValue<string>()), Does.Contain("UTM"));
            Assert.That(Property(projection, "LatitudeOrigin")["description"]?.GetValue<string>(), Does.Contain("radians (SI)"));
            Assert.That(Property(projection, "FalseEasting")["description"]?.GetValue<string>(), Does.Contain("meters (SI)"));
            Assert.That(Property(projection, "Zone")["maximum"]?.GetValue<int>(), Is.EqualTo(60));
        });
    }

    [Test]
    public void Conversion_update_requires_matching_case_identifier()
    {
        JsonObject root = RequireObject(_tools["cartographic_conversion_set_update_by_id"].InputSchema);
        Assert.That(RequiredNames(root), Is.EquivalentTo(new[] { "cartographicConversionSet", "id" }));
        Assert.That(Property(root, "id")["description"]?.GetValue<string>(), Does.Contain("cartographicConversionSet.MetaInfo.ID"));
    }

    private static JsonObject RequireObject(JsonNode? node)
    {
        Assert.That(node, Is.TypeOf<JsonObject>());
        return (JsonObject)node!;
    }

    private static JsonObject Property(JsonObject schema, string name) =>
        RequireObject(RequireObject(schema["properties"])[name]);

    private static string[] RequiredNames(JsonObject schema)
    {
        Assert.That(schema["required"], Is.TypeOf<JsonArray>());
        return ((JsonArray)schema["required"]!).Select(node => node!.GetValue<string>()).ToArray();
    }
}
