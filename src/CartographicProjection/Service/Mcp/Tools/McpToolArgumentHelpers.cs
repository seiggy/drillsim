using System;
using System.Text.Json.Nodes;
using NORCE.Drilling.CartographicProjection.Model;

namespace NORCE.Drilling.CartographicProjection.Service.Mcp.Tools;

internal static class McpToolArgumentHelpers
{
    public static JsonObject CreateEmptySchema() => new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false
    };

    public static JsonObject CreateGuidSchema(string key, string description)
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

    public static JsonObject CreateStringSchema(string key, string description)
    {
        return new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                [key] = new JsonObject
                {
                    ["type"] = "string",
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

    public static JsonObject CreateProjectionSchema(bool includeId = false) =>
        WrapBody("cartographicProjection", CreateProjectionObjectSchema(), includeId, "cartographicProjection.MetaInfo.ID");

    public static JsonObject CreateConversionSetSchema(bool includeId = false) =>
        WrapBody("cartographicConversionSet", CreateConversionSetObjectSchema(), includeId, "cartographicConversionSet.MetaInfo.ID");

    private static JsonObject WrapBody(string name, JsonObject body, bool includeId, string bodyIdPath)
    {
        var properties = new JsonObject { [name] = body };
        var required = new JsonArray { name };
        if (includeId)
        {
            properties["id"] = new JsonObject
            {
                ["type"] = "string",
                ["format"] = "uuid",
                ["description"] = $"Identifier of the stored record to update. It must equal {bodyIdPath}."
            };
            required.Add("id");
        }
        return new JsonObject
        {
            ["type"] = "object",
            ["properties"] = properties,
            ["required"] = required,
            ["additionalProperties"] = false
        };
    }

    private static JsonObject CreateProjectionObjectSchema() => new()
    {
        ["type"] = "object",
        ["description"] = "Complete cartographic projection definition. MetaInfo.ID must be a caller-generated, non-empty UUID. Use a projection-type prototype tool to learn which parameters apply to the selected ProjectionType.",
        ["properties"] = new JsonObject
        {
            ["MetaInfo"] = CreateMetaInfoSchema("cartographic projection"),
            ["Name"] = NullableString("Human-readable projection name."),
            ["Description"] = NullableString("Human-readable projection description."),
            ["CreationDate"] = NullableDateTime("UTC or offset timestamp at which the projection was created."),
            ["LastModificationDate"] = NullableDateTime("UTC or offset timestamp of the most recent modification."),
            ["ProjectionType"] = EnumSchema("Projection algorithm. Only parameters marked as used by its prototype affect the generated PROJ.4 definition.", Enum.GetNames<ProjectionType>()),
            ["GeodeticDatumID"] = NullableUuid("Identifier of the GeodeticDatum that supplies the spheroid and datum used by this projection."),
            ["LatitudeOrigin"] = NullableAngle("Latitude of projection origin."),
            ["Latitude1"] = NullableAngle("First standard parallel, used by applicable conic projections."),
            ["Latitude2"] = NullableAngle("Second standard parallel, used by applicable conic projections."),
            ["LatitudeTrueScale"] = NullableAngle("Latitude of true scale, used by applicable projections."),
            ["LongitudeOrigin"] = NullableAngle("Longitude of projection origin."),
            ["Scaling"] = NullableNumber("Dimensionless central scaling factor; defaults to 1.0 when applicable."),
            ["FalseEasting"] = NullableLength("False easting offset."),
            ["FalseNorthing"] = NullableLength("False northing offset."),
            ["Zone"] = Integer("UTM zone from 1 through 60 when ProjectionType is UTM.", 1, 60, 1),
            ["IsSouth"] = Boolean("True to select the southern hemisphere for projections that use this flag."),
            ["IsHyperbolic"] = Boolean("True to use the hyperbolic form for projection types that support it."),
            ["ProjectionHeight"] = NullableLength("Projection height for projection types that use h_0."),
            ["HeightViewPoint"] = NullableLength("Viewpoint height for satellite-view projections."),
            ["Sweep"] = EnumSchema("Sweep-axis selection for viewing-instrument projections.", Enum.GetNames<AxisType>()),
            ["AzimuthCentralLine"] = NullableAngle("Azimuth of the central line."),
            ["Weight"] = NullableNumber("Dimensionless weight parameter for projections such as Lagrange."),
            ["Landsat"] = NullableInteger("Landsat satellite identifier, normally 1 through 5."),
            ["Path"] = NullableInteger("Satellite path code for projection types that use it."),
            ["Alpha"] = NullableAngle("Azimuth of the centerline at its center point."),
            ["Gamma"] = NullableAngle("Rectified bearing of the centerline."),
            ["Longitude1"] = NullableAngle("First longitude parameter for projection types that use it."),
            ["Longitude2"] = NullableAngle("Second longitude parameter for projection types that use it."),
            ["LongitudeCentralPoint"] = NullableAngle("Longitude of the central point."),
            ["NoOffset"] = Boolean("True to enable the no-offset option for applicable projections."),
            ["NoRotation"] = Boolean("True to enable the no-rotation option for applicable projections."),
            ["AreaNormalizationTransform"] = EnumSchema("Area-normalization transform used by projections that support UV-to-ST conversion.", Enum.GetNames<AreaNormalizationTransformType>()),
            ["PegLatitude"] = NullableAngle("Peg-point latitude for spherical cross-track-height projection."),
            ["PegLongitude"] = NullableAngle("Peg-point longitude for spherical cross-track-height projection."),
            ["PegHeading"] = NullableAngle("Peg-point heading for spherical cross-track-height projection."),
            ["N"] = NullableNumber("Dimensionless n parameter for projection types that use it."),
            ["Q"] = NullableNumber("Dimensionless q parameter for projection types that use it.")
        },
        ["required"] = new JsonArray { "MetaInfo", "ProjectionType" },
        ["additionalProperties"] = false
    };

    private static JsonObject CreateConversionSetObjectSchema() => new()
    {
        ["type"] = "object",
        ["description"] = "Persistent coordinate-conversion case. Assign a new UUID in MetaInfo.ID, select an existing CartographicProjectionID, provide one supported source representation per coordinate item, create the case, retrieve it by the same UUID for calculated results, and delete it when no longer needed.",
        ["properties"] = new JsonObject
        {
            ["MetaInfo"] = CreateMetaInfoSchema("cartographic conversion case"),
            ["Name"] = NullableString("Human-readable conversion-case name."),
            ["Description"] = NullableString("Human-readable description of the conversion case or its purpose."),
            ["CreationDate"] = NullableDateTime("UTC or offset timestamp at which the case was created."),
            ["LastModificationDate"] = NullableDateTime("UTC or offset timestamp of the most recent recalculation."),
            ["CartographicProjectionID"] = Uuid("Identifier of the existing CartographicProjection that selects the projection and associated geodetic datum."),
            ["CartographicCoordinateList"] = ArraySchema("One or more coordinates to convert. Each item must provide a complete projected triplet, a complete datum or WGS84 geodetic triplet, or a valid octree representation. The retrieved case contains calculated alternatives.", CreateCoordinateSchema(), 1)
        },
        ["required"] = new JsonArray { "MetaInfo", "CartographicProjectionID", "CartographicCoordinateList" },
        ["additionalProperties"] = false
    };

    private static JsonObject CreateCoordinateSchema() => new()
    {
        ["type"] = "object",
        ["description"] = "One conversion item. A complete Northing/Easting/VerticalDepth triplet is treated as projected input; otherwise supply a complete datum/WGS84 geodetic triplet or octree code.",
        ["properties"] = new JsonObject
        {
            ["Northing"] = NullableLength("Projected northing. Supply with Easting and VerticalDepth for projected input; otherwise this is calculated output."),
            ["Easting"] = NullableLength("Projected easting. Supply with Northing and VerticalDepth for projected input; otherwise this is calculated output."),
            ["VerticalDepth"] = NullableLength("True vertical depth in the projection's reference geodetic datum. Supply with Northing and Easting for projected input."),
            ["GeodeticCoordinate"] = CreateGeodeticCoordinateSchema(),
            ["GridConvergenceDatum"] = NullableAngle("Calculated grid-convergence angle in the projection's geodetic datum; normally omit from input.")
        },
        ["additionalProperties"] = false
    };

    private static JsonObject CreateGeodeticCoordinateSchema() => new()
    {
        ["type"] = new JsonArray { "object", "null" },
        ["description"] = "Geodetic representations of the coordinate. Supply one complete source representation; the service calculates the remaining datum, WGS84, octree, and projected forms.",
        ["properties"] = new JsonObject
        {
            ["LatitudeWGS84"] = NullableAngle("WGS84 latitude. Supply with LongitudeWGS84 and VerticalDepthWGS84 for WGS84 input."),
            ["LongitudeWGS84"] = NullableAngle("WGS84 longitude. Supply with LatitudeWGS84 and VerticalDepthWGS84 for WGS84 input."),
            ["VerticalDepthWGS84"] = NullableLength("True vertical depth referenced to the WGS84 vertical datum. Supply with WGS84 latitude and longitude for WGS84 input."),
            ["LatitudeDatum"] = NullableAngle("Latitude in the geodetic datum associated with the selected projection. Supply with LongitudeDatum and VerticalDepthDatum for datum input."),
            ["LongitudeDatum"] = NullableAngle("Longitude in the selected projection's geodetic datum. Supply with LatitudeDatum and VerticalDepthDatum for datum input."),
            ["VerticalDepthDatum"] = NullableLength("True vertical depth in the selected projection's geodetic datum. Supply with datum latitude and longitude for datum input."),
            ["OctreeDepth"] = Integer("Requested or calculated octree detail level. Zero defaults to level 24 when the service calculates an octree code.", 0),
            ["OctreeCode"] = new JsonObject
            {
                ["type"] = new JsonArray { "object", "null" },
                ["description"] = "Long octree representation of a WGS84 position. For octree input, provide a positive OctreeDepth and matching Depth, CodeHigh, and CodeLow.",
                ["properties"] = new JsonObject
                {
                    ["Depth"] = Integer("Octree depth encoded by the value; it should equal OctreeDepth.", 0),
                    ["CodeHigh"] = Integer("Signed 64-bit high part of the octree code."),
                    ["CodeLow"] = Integer("Signed 64-bit low part of the octree code.")
                },
                ["required"] = new JsonArray { "Depth", "CodeHigh", "CodeLow" },
                ["additionalProperties"] = false
            }
        },
        ["additionalProperties"] = false
    };

    private static JsonObject CreateMetaInfoSchema(string resource) => new()
    {
        ["type"] = "object",
        ["description"] = $"Identity and optional HTTP location metadata for the {resource}.",
        ["properties"] = new JsonObject
        {
            ["ID"] = Uuid($"Non-empty unique identifier of the {resource}."),
            ["HttpHostName"] = NullableString($"Optional host name from which the {resource} can be retrieved."),
            ["HttpHostBasePath"] = NullableString($"Optional service base path from which the {resource} can be retrieved."),
            ["HttpEndPoint"] = NullableString($"Optional HTTP endpoint for this {resource} resource.")
        },
        ["required"] = new JsonArray { "ID" },
        ["additionalProperties"] = false
    };

    private static JsonObject EnumSchema(string description, string[] values)
    {
        var choices = new JsonArray();
        foreach (string value in values) choices.Add(value);
        return new JsonObject { ["type"] = "string", ["description"] = description, ["enum"] = choices };
    }

    private static JsonObject ArraySchema(string description, JsonObject items, int minItems) => new() { ["type"] = "array", ["description"] = description, ["items"] = items, ["minItems"] = minItems };
    private static JsonObject Uuid(string description) => new() { ["type"] = "string", ["format"] = "uuid", ["description"] = description };
    private static JsonObject NullableUuid(string description) => new() { ["type"] = new JsonArray { "string", "null" }, ["format"] = "uuid", ["description"] = description };
    private static JsonObject NullableString(string description) => new() { ["type"] = new JsonArray { "string", "null" }, ["description"] = description };
    private static JsonObject NullableDateTime(string description) => new() { ["type"] = new JsonArray { "string", "null" }, ["format"] = "date-time", ["description"] = description };
    private static JsonObject NullableNumber(string description) => new() { ["type"] = new JsonArray { "number", "null" }, ["description"] = description };
    private static JsonObject NullableAngle(string description) => NullableNumber(description + " Value is in radians (SI).");
    private static JsonObject NullableLength(string description) => NullableNumber(description + " Value is in meters (SI).");
    private static JsonObject NullableInteger(string description) => new() { ["type"] = new JsonArray { "integer", "null" }, ["description"] = description };
    private static JsonObject Integer(string description, int? minimum = null, int? maximum = null, int? defaultValue = null)
    {
        var schema = new JsonObject { ["type"] = "integer", ["description"] = description };
        if (minimum != null) schema["minimum"] = minimum;
        if (maximum != null) schema["maximum"] = maximum;
        if (defaultValue != null) schema["default"] = defaultValue;
        return schema;
    }
    private static JsonObject Boolean(string description) => new() { ["type"] = "boolean", ["description"] = description, ["default"] = false };

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

    public static bool TryParseString(JsonObject? arguments, string key, out string value, out JsonNode? error)
    {
        value = string.Empty;
        error = null;

        var node = arguments?[key];
        if (node is null)
        {
            error = McpToolResponses.CreateValidationError($"Argument '{key}' is required.");
            return false;
        }

        value = node.ToString();
        if (string.IsNullOrWhiteSpace(value))
        {
            error = McpToolResponses.CreateValidationError($"Argument '{key}' must be a non-empty string.");
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
