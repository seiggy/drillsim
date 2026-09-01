using System.Text.Json.Nodes;

namespace OSDC.Drilling.EarthMagneticField.Service.Mcp.Tools;

internal static class EarthMagneticFieldMcpSchemas
{
    public static JsonNode EvaluateInput(int maximumSamples) => JsonNode.Parse($$"""
    {
      "type":"object",
      "properties":{
        "Model":{"type":"string","enum":["WMM2025","IGRF14"],"description":"Reference model for the complete batch. WMM2025 covers 2025-2030; IGRF14 covers 1900-2030."},
        "Samples":{
          "type":"array","minItems":1,"maxItems":{{maximumSamples}},
          "description":"Independent points evaluated in order. One invalid point rejects the complete batch.",
          "items":{
            "type":"object",
            "properties":{
              "Latitude":{"type":"number","minimum":-1.5707963267948966,"maximum":1.5707963267948966,"description":"WGS84 geodetic latitude in SI radians; never degrees."},
              "Longitude":{"type":"number","minimum":-3.141592653589793,"maximum":3.141592653589793,"description":"WGS84 longitude in SI radians; never degrees."},
              "Depth":{"type":"number","description":"SI metres positive downward from the WGS84 ellipsoid. Model-dependent bounds apply."},
              "DateTimeUtc":{"type":"string","format":"date-time","description":"UTC instant containing Z or +00:00. Other offsets and unspecified local times are rejected."}
            },
            "required":["Latitude","Longitude","Depth","DateTimeUtc"],
            "additionalProperties":false
          }
        }
      },
      "required":["Model","Samples"],
      "examples":[{"Model":"WMM2025","Samples":[{"Latitude":1.0471975511965976,"Longitude":0.17453292519943295,"Depth":1000.0,"DateTimeUtc":"2026-08-24T10:00:00Z"}]}],
      "additionalProperties":false
    }
    """)!;

    public static JsonNode EvaluateOutput() => JsonNode.Parse("""
    {
      "type":"object",
      "properties":{
        "Model":{"$ref":"#/$defs/modelInfo"},
        "Samples":{"type":"array","items":{"$ref":"#/$defs/result"}}
      },
      "required":["Model","Samples"],
      "additionalProperties":false,
      "$defs":{
        "input":{
          "type":"object",
          "properties":{
            "Latitude":{"type":"number","description":"WGS84 latitude in SI radians."},
            "Longitude":{"type":"number","description":"WGS84 longitude in SI radians."},
            "Depth":{"type":"number","description":"SI metres positive downward from the WGS84 ellipsoid."},
            "DateTimeUtc":{"type":"string","format":"date-time","description":"Normalized UTC evaluation instant."}
          },
          "required":["Latitude","Longitude","Depth","DateTimeUtc"],"additionalProperties":false
        },
        "result":{
          "type":"object",
          "properties":{
            "Input":{"$ref":"#/$defs/input"},
            "North":{"type":"number","description":"Northerly component in SI teslas."},
            "East":{"type":"number","description":"Easterly component in SI teslas."},
            "Down":{"type":"number","description":"Downward component in SI teslas."},
            "HorizontalIntensity":{"type":"number","minimum":0,"description":"Horizontal magnitude in SI teslas."},
            "TotalIntensity":{"type":"number","minimum":0,"description":"Total magnitude in SI teslas."},
            "Declination":{"type":["number","null"],"description":"SI radians positive east of north; null if horizontal intensity is zero."},
            "Inclination":{"type":["number","null"],"description":"SI radians positive downward; null if total intensity is zero."}
          },
          "required":["Input","North","East","Down","HorizontalIntensity","TotalIntensity","Declination","Inclination"],"additionalProperties":false
        },
        "modelInfo":{
          "type":"object",
          "properties":{
            "Model":{"type":"string","enum":["WMM2025","IGRF14"]},"Name":{"type":"string"},"ID":{"type":"string"},"Description":{"type":"string"},
            "ReleaseDate":{"type":["string","null"],"format":"date-time"},"MinimumUtc":{"type":"string","format":"date-time"},"MaximumUtc":{"type":"string","format":"date-time"},
            "MinimumDepth":{"type":"number"},"MaximumDepth":{"type":"number"},"Degree":{"type":"integer"},"Order":{"type":"integer"},"GeographicLibVersion":{"type":"string"},
            "ReferenceEllipsoid":{"type":"string","const":"WGS84"},"CoordinateFrame":{"type":"string","const":"north-east-down"},"MagneticFluxDensityUnit":{"type":"string","const":"tesla"},
            "AngleUnit":{"type":"string","const":"radian"},"DepthPositiveDirection":{"type":"string","const":"down"},"ConcurrentEvaluationEnabled":{"type":"boolean"},
            "MetadataSHA256":{"type":"string","pattern":"^[0-9a-f]{64}$"},"CoefficientSHA256":{"type":"string","pattern":"^[0-9a-f]{64}$"}
          },
          "required":["Model","Name","ID","Description","ReleaseDate","MinimumUtc","MaximumUtc","MinimumDepth","MaximumDepth","Degree","Order","GeographicLibVersion","ReferenceEllipsoid","CoordinateFrame","MagneticFluxDensityUnit","AngleUnit","DepthPositiveDirection","ConcurrentEvaluationEnabled","MetadataSHA256","CoefficientSHA256"],
          "additionalProperties":false
        }
      }
    }
    """)!;

    public static JsonNode ServiceInfo() => JsonNode.Parse("""
    {
      "type":"object",
      "properties":{
        "Name":{"type":"string","const":"OSDC Earth Magnetic Field"},
        "Description":{"type":"string"},
        "CoordinateFrame":{"type":"string","const":"north-east-down"},
        "TimeConvention":{"type":"string","const":"UTC"},
        "DepthReference":{"type":"string","const":"WGS84 reference ellipsoid"},
        "DepthPositiveDirection":{"type":"string","const":"down"},
        "Models":{"type":"array","minItems":2,"items":{"$ref":"#/$defs/modelInfo"}}
      },
      "required":["Name","Description","CoordinateFrame","TimeConvention","DepthReference","DepthPositiveDirection","Models"],
      "additionalProperties":false,
      "$defs":{
        "modelInfo":{
          "type":"object",
          "properties":{
            "Model":{"type":"string","enum":["WMM2025","IGRF14"]},"Name":{"type":"string"},"ID":{"type":"string"},"Description":{"type":"string"},
            "ReleaseDate":{"type":["string","null"],"format":"date-time"},"MinimumUtc":{"type":"string","format":"date-time"},"MaximumUtc":{"type":"string","format":"date-time"},
            "MinimumDepth":{"type":"number"},"MaximumDepth":{"type":"number"},"Degree":{"type":"integer"},"Order":{"type":"integer"},"GeographicLibVersion":{"type":"string"},
            "ReferenceEllipsoid":{"type":"string","const":"WGS84"},"CoordinateFrame":{"type":"string","const":"north-east-down"},"MagneticFluxDensityUnit":{"type":"string","const":"tesla"},
            "AngleUnit":{"type":"string","const":"radian"},"DepthPositiveDirection":{"type":"string","const":"down"},"ConcurrentEvaluationEnabled":{"type":"boolean"},
            "MetadataSHA256":{"type":"string","pattern":"^[0-9a-f]{64}$"},"CoefficientSHA256":{"type":"string","pattern":"^[0-9a-f]{64}$"}
          },
          "required":["Model","Name","ID","Description","ReleaseDate","MinimumUtc","MaximumUtc","MinimumDepth","MaximumDepth","Degree","Order","GeographicLibVersion","ReferenceEllipsoid","CoordinateFrame","MagneticFluxDensityUnit","AngleUnit","DepthPositiveDirection","ConcurrentEvaluationEnabled","MetadataSHA256","CoefficientSHA256"],
          "additionalProperties":false
        }
      }
    }
    """)!;
}
