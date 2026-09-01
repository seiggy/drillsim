using System;
using System.Text.Json.Nodes;

namespace NORCE.Drilling.GeodeticDatum.Service.Mcp.Tools;

internal static class McpToolArgumentHelpers
{
    public static JsonObject CreateEmptySchema() => new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false
    };

    public static JsonObject CreateGuidSchema(string key, string description = "Identifier of the requested record.")
    {
        return new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                [key] = new JsonObject
                {
                    ["type"] = "string",
                    ["format"] = "uuid",
                    ["description"] = description
                }
            },
            ["required"] = new JsonArray
            {
                key
            },
            ["additionalProperties"] = false
        };
    }

    public static JsonObject CreateSpheroidSchema(bool includeId = false) => WrapBody(
        "spheroid",
        "Complete reference spheroid. MetaInfo.ID must be a caller-generated UUID. Provide at least two defined shape parameters, including SemiMajorAxis or SemiMinorAxis; the service calculates the remaining parameters.",
        includeId,
        "spheroid.MetaInfo.ID",
        new JsonObject
        {
            ["MetaInfo"] = MetaInfo("spheroid"),
            ["Name"] = NullableString("Human-readable spheroid name."),
            ["Description"] = NullableString("Human-readable spheroid description."),
            ["CreationDate"] = NullableDate("Creation timestamp."),
            ["LastModificationDate"] = NullableDate("Last-modification timestamp."),
            ["IsDefault"] = Boolean("Whether this is service-supplied default reference data."),
            ["SemiMajorAxis"] = Scalar("Semi-major axis length in meters (SI)."),
            ["IsSemiMajorAxisSet"] = Boolean("True when SemiMajorAxis was supplied rather than calculated."),
            ["SemiMinorAxis"] = Scalar("Semi-minor axis length in meters (SI)."),
            ["IsSemiMinorAxisSet"] = Boolean("True when SemiMinorAxis was supplied rather than calculated."),
            ["Eccentricity"] = Scalar("Dimensionless eccentricity."),
            ["IsEccentricitySet"] = Boolean("True when Eccentricity was supplied rather than calculated."),
            ["SquaredEccentricity"] = Scalar("Dimensionless squared eccentricity."),
            ["IsSquaredEccentricitySet"] = Boolean("True when SquaredEccentricity was supplied rather than calculated."),
            ["Flattening"] = Scalar("Dimensionless flattening."),
            ["IsFlatteningSet"] = Boolean("True when Flattening was supplied rather than calculated."),
            ["InverseFlattening"] = Scalar("Dimensionless inverse flattening."),
            ["IsInverseFlatteningSet"] = Boolean("True when InverseFlattening was supplied rather than calculated.")
        },
        new JsonArray { "MetaInfo" });

    public static JsonObject CreateGeodeticDatumSchema(bool includeId = false) => WrapBody(
        "geodeticDatum", "Complete geodetic datum definition and seven-parameter transformation to WGS84. MetaInfo.ID must be a caller-generated UUID and Spheroid must be a valid complete spheroid.", includeId, "geodeticDatum.MetaInfo.ID", DatumProperties(), new JsonArray { "MetaInfo", "Spheroid" });

    public static JsonObject CreateConversionSetSchema(bool includeId = false) => WrapBody(
        "geodeticConversionSet",
        "Persistent batch conversion case. Assign MetaInfo.ID, embed the datum, supply one source representation per coordinate, create the case, retrieve calculated results by the same UUID, then delete temporary cases.",
        includeId, "geodeticConversionSet.MetaInfo.ID",
        new JsonObject
        {
            ["MetaInfo"] = MetaInfo("geodetic conversion case"),
            ["Name"] = NullableString("Human-readable conversion-case name."),
            ["Description"] = NullableString("Purpose or provenance of the conversion case."),
            ["CreationDate"] = NullableDate("Creation timestamp."),
            ["LastModificationDate"] = NullableDate("Last-calculation timestamp."),
            ["GeodeticDatum"] = Object("Datum used for datum-coordinate input or output.", DatumProperties(), new JsonArray { "MetaInfo", "Spheroid" }),
            ["OctreeBounds"] = NullableObject("Optional custom octree bounds; omit to use global service defaults."),
            ["GeodeticCoordinates"] = new JsonObject { ["type"] = "array", ["description"] = "Coordinates to convert. Each must contain one complete datum triplet, WGS84 triplet, or valid octree representation.", ["items"] = Coordinate(), ["minItems"] = 1 }
        },
        new JsonArray { "MetaInfo", "GeodeticDatum", "GeodeticCoordinates" });

    private static JsonObject DatumProperties() => new()
    {
        ["MetaInfo"] = MetaInfo("geodetic datum"), ["Name"] = NullableString("Human-readable datum name."), ["Description"] = NullableString("Datum description or area of applicability."),
        ["CreationDate"] = NullableDate("Creation timestamp."), ["LastModificationDate"] = NullableDate("Last-modification timestamp."), ["IsDefault"] = Boolean("Whether this is service-supplied default reference data."),
        ["Spheroid"] = Object("Complete reference spheroid used by this datum."),
        ["DeltaX"] = Scalar("X translation to WGS84 in meters (SI)."), ["DeltaY"] = Scalar("Y translation to WGS84 in meters (SI)."), ["DeltaZ"] = Scalar("Z translation to WGS84 in meters (SI)."),
        ["RotationX"] = Scalar("X rotation to WGS84 in radians (SI)."), ["RotationY"] = Scalar("Y rotation to WGS84 in radians (SI)."), ["RotationZ"] = Scalar("Z rotation to WGS84 in radians (SI)."), ["ScaleFactor"] = Scalar("Dimensionless transformation scale factor.")
    };

    private static JsonObject Coordinate() => new()
    {
        ["type"] = "object", ["description"] = "Input precedence is a complete datum triplet, then a complete WGS84 triplet, then octree. Partial triplets cannot be converted.",
        ["properties"] = new JsonObject
        {
            ["LatitudeWGS84"] = NullableNumber("WGS84 latitude in radians (SI)."), ["LongitudeWGS84"] = NullableNumber("WGS84 longitude in radians (SI)."), ["VerticalDepthWGS84"] = NullableNumber("True vertical depth in meters (SI), referenced to WGS84; positive is downward."),
            ["LatitudeDatum"] = NullableNumber("Latitude in the embedded datum in radians (SI)."), ["LongitudeDatum"] = NullableNumber("Longitude in the embedded datum in radians (SI)."), ["VerticalDepthDatum"] = NullableNumber("True vertical depth in meters (SI), referenced to the embedded datum; positive is downward."),
            ["OctreeDepth"] = new JsonObject { ["type"] = "integer", ["minimum"] = 0, ["maximum"] = 255, ["description"] = "Octree level. Zero defaults to 24 when calculating a code." }, ["OctreeCode"] = NullableObject("Long octree code; for octree input its encoded depth must equal a positive OctreeDepth.")
        }, ["additionalProperties"] = false
    };

    private static JsonObject WrapBody(string name, string description, bool includeId, string idPath, JsonObject properties, JsonArray bodyRequired)
    {
        var body = Object(description, properties, bodyRequired);
        var wrapperProperties = new JsonObject { [name] = body };
        var required = new JsonArray { name };
        if (includeId) { wrapperProperties["id"] = new JsonObject { ["type"] = "string", ["format"] = "uuid", ["description"] = $"Stored-record UUID; must equal {idPath}." }; required.Add("id"); }
        return new JsonObject { ["type"] = "object", ["properties"] = wrapperProperties, ["required"] = required, ["additionalProperties"] = false };
    }

    private static JsonObject MetaInfo(string resource) => Object($"Identity and optional HTTP location metadata for the {resource}.", new JsonObject { ["ID"] = new JsonObject { ["type"] = "string", ["format"] = "uuid", ["description"] = $"Non-empty unique identifier of the {resource}." }, ["HttpHostName"] = NullableString("Optional host name."), ["HttpHostBasePath"] = NullableString("Optional service base path."), ["HttpEndPoint"] = NullableString("Optional resource endpoint.") }, new JsonArray { "ID" });
    private static JsonObject Object(string description, JsonObject? properties = null, JsonArray? required = null) { var o = new JsonObject { ["type"] = "object", ["description"] = description }; if (properties is not null) o["properties"] = properties; if (required is not null) o["required"] = required; return o; }
    private static JsonObject NullableObject(string description) => new() { ["type"] = new JsonArray { "object", "null" }, ["description"] = description };
    private static JsonObject NullableString(string description) => new() { ["type"] = new JsonArray { "string", "null" }, ["description"] = description };
    private static JsonObject NullableDate(string description) => new() { ["type"] = new JsonArray { "string", "null" }, ["format"] = "date-time", ["description"] = description };
    private static JsonObject NullableNumber(string description) => new() { ["type"] = new JsonArray { "number", "null" }, ["description"] = description };
    private static JsonObject Boolean(string description) => new() { ["type"] = "boolean", ["description"] = description };
    private static JsonObject Scalar(string description) => new() { ["type"] = new JsonArray { "object", "null" }, ["description"] = description + " Encode as {\"ScalarValue\": number}.", ["properties"] = new JsonObject { ["ScalarValue"] = NullableNumber("Numeric scalar value.") } };

    public static bool TryParseGuid(JsonObject? arguments, string key, out Guid value, out JsonNode? error)
    {
        value = Guid.Empty;
        error = null;

        var node = arguments?[key];
        if (node is null)
        {
            error = McpToolResponses.CreateValidationError($"Argument '{key}' is required.");
            return false;
        }

        if (!Guid.TryParse(node.ToString(), out value))
        {
            error = McpToolResponses.CreateValidationError($"Argument '{key}' must be a valid UUID.");
            return false;
        }

        return true;
    }

    public static bool TryParseDouble(JsonObject? arguments, string key, out double value, out JsonNode? error)
    {
        value = 0d;
        error = null;

        var node = arguments?[key];
        if (node is null)
        {
            error = McpToolResponses.CreateValidationError($"Argument '{key}' is required.");
            return false;
        }

        try
        {
            value = node.GetValue<double>();
        }
        catch (Exception ex) when (ex is InvalidOperationException or FormatException)
        {
            error = McpToolResponses.CreateValidationError($"Argument '{key}' must be a number.");
            return false;
        }

        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            error = McpToolResponses.CreateValidationError($"Argument '{key}' must be a finite number.");
            return false;
        }

        return true;
    }
}
