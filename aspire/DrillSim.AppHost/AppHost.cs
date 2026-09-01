var builder = DistributedApplication.CreateBuilder(args);

var databaseDirectory = Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", "..", "data"));
var addService = (string name, string project, string databaseFileName) =>
{
    var database = builder.AddSqlite($"{name}-db", databaseDirectory, databaseFileName);

    return builder
        .AddProject(name, project, static options => options.ExcludeLaunchProfile = true)
        .WithReference(database, "Sqlite")
        .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
        .WithHttpEndpoint()
        .WithHttpsEndpoint()
        .WithExternalHttpEndpoints();
};

var cartographicProjection = addService("cartographic-projection", "../../src/CartographicProjection/Service/Service.csproj", "CartographicProjection.db");
var cluster = addService("cluster", "../../src/Cluster/Service/Service.csproj", "Cluster.db");
var drillingFluid = addService("drilling-fluid", "../../src/DrillingFluid/Service/Service.csproj", "DrillingFluid.db");
var drillString = addService("drill-string", "../../src/DrillString/Service/Service.csproj", "DrillString.db");
var earthGravity = addService("earth-gravity", "../../src/EarthGravity/Service/Service.csproj", "EarthGravity.db");
var earthMagneticField = addService("earth-magnetic-field", "../../src/EarthMagneticField/Service/Service.csproj", "EarthMagneticField.db");
var earthVerticalDatum = addService("earth-vertical-datum", "../../src/EarthVerticalDatum/Service/Service.csproj", "EarthVerticalDatum.db");
var fieldService = addService("field", "../../src/Field/Service/Service.csproj", "Field.db");
var geodeticDatum = addService("geodetic-datum", "../../src/GeodeticDatum/Service/Service.csproj", "GeodeticDatum.db");
var geologicalProperties = addService("geological-properties", "../../src/GeologicalProperties/Service/Service.csproj", "GeologicalProperties.db");
var geothermalProperties = addService("geothermal-properties", "../../src/GeothermalProperties/Service/Service.csproj", "GeothermalProperties.db");
var rig = addService("rig", "../../src/Rig/Service/Service.csproj", "Rig.db");
var simulator4nDof = addService("simulator-4ndof", "../../src/Simulator4nDOF/Service/Service.csproj", "Simulator4nDOF.db");
var surveyInstrument = addService("survey-instrument", "../../src/SurveyInstrument/Service/Service.csproj", "SurveyInstrument.db");
var trajectory = addService("trajectory", "../../src/Trajectory/Service/Service.csproj", "Trajectory.db");
var unitConversion = addService("unit-conversion", "../../src/UnitConversion/Service/Service.csproj", "UnitConversion.db");
var well = addService("well", "../../src/Well/Service/Service.csproj", "Well.db");
var wellBore = addService("well-bore", "../../src/WellBore/Service/Service.csproj", "WellBore.db");
var wellBoreArchitecture = addService("well-bore-architecture", "../../src/WellBoreArchitecture/Service/Service.csproj", "WellBoreArchitecture.db");
var yplCalibration = addService("ypl-calibration", "../../src/YPLCalibrationFromRheometer/YPLCalibrationFromRheometer.Service/YPLCalibrationFromRheometer.Service.csproj", "YPLCalibrationFromRheometer.db");
var clusterUi = builder
    .AddProject("cluster-ui", "../../src/Cluster/WebApp/WebApp.csproj", static options => options.ExcludeLaunchProfile = true)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithHttpEndpoint()
    .WithHttpsEndpoint()
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/cluster/webapp/health");

cartographicProjection.WithEnvironment("GeodeticDatumHostURL", geodeticDatum.GetEndpoint("http"));

cluster
    .WithEnvironment("WellHostURL", well.GetEndpoint("http"))
    .WithEnvironment("FieldHostURL", fieldService.GetEndpoint("http"))
    .WithEnvironment("RigHostURL", rig.GetEndpoint("http"));

fieldService
    .WithEnvironment("EarthCartographicProjectionHostURL", cartographicProjection.GetEndpoint("http"))
    .WithEnvironment("EarthGeodesyHostURL", geodeticDatum.GetEndpoint("http"));

rig.WithEnvironment("ClusterHostURL", cluster.GetEndpoint("http"));

trajectory
    .WithEnvironment("FieldHostURL", fieldService.GetEndpoint("http"))
    .WithEnvironment("ClusterHostURL", cluster.GetEndpoint("http"))
    .WithEnvironment("WellBoreHostURL", wellBore.GetEndpoint("http"))
    .WithEnvironment("WellBoreArchitectureHostURL", wellBoreArchitecture.GetEndpoint("http"))
    .WithEnvironment("WellHostURL", well.GetEndpoint("http"))
    .WithEnvironment("SurveyInstrumentHostURL", surveyInstrument.GetEndpoint("http"));

simulator4nDof
    .WithEnvironment("ClusterHostURL", cluster.GetEndpoint("http"))
    .WithEnvironment("WellBoreHostURL", wellBore.GetEndpoint("http"))
    .WithEnvironment("WellHostURL", well.GetEndpoint("http"))
    .WithEnvironment("DrillStringHostURL", drillString.GetEndpoint("http"))
    .WithEnvironment("TrajectoryHostURL", trajectory.GetEndpoint("http"))
    .WithEnvironment("DrillingFluidHostURL", drillingFluid.GetEndpoint("http"))
    .WithEnvironment("WellBoreArchitectureHostURL", wellBoreArchitecture.GetEndpoint("http"))
    .WithEnvironment("RigHostURL", rig.GetEndpoint("http"))
    .WithEnvironment("GeothermalPropertiesHostURL", geothermalProperties.GetEndpoint("http"));

clusterUi
    .WithEnvironment("ClusterHostURL", cluster.GetEndpoint("http"))
    .WithEnvironment("FieldHostURL", fieldService.GetEndpoint("http"))
    .WithEnvironment("RigHostURL", rig.GetEndpoint("http"))
    .WithEnvironment("TrajectoryHostURL", trajectory.GetEndpoint("http"))
    .WithEnvironment("EarthCartographicProjectionHostURL", cartographicProjection.GetEndpoint("http"))
    .WithEnvironment("EarthGeodesyHostURL", geodeticDatum.GetEndpoint("http"))
    .WithEnvironment("EarthGravityHostURL", earthGravity.GetEndpoint("http"))
    .WithEnvironment("EarthMagneticFieldHostURL", earthMagneticField.GetEndpoint("http"))
    .WithEnvironment("EarthVerticalDatumHostURL", earthVerticalDatum.GetEndpoint("http"))
    .WithEnvironment("UnitConversionHostURL", unitConversion.GetEndpoint("http"))
    .WaitFor(cluster)
    .WaitFor(fieldService)
    .WaitFor(rig)
    .WaitFor(trajectory)
    .WaitFor(cartographicProjection)
    .WaitFor(geodeticDatum)
    .WaitFor(earthGravity)
    .WaitFor(earthMagneticField)
    .WaitFor(earthVerticalDatum)
    .WaitFor(unitConversion);

builder.Build().Run();
