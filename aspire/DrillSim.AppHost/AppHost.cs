var builder = DistributedApplication.CreateBuilder(args);

string azureConfigDirectory = Environment.GetEnvironmentVariable("AZURE_CONFIG_DIR")
    ?? throw new InvalidOperationException(
        "AZURE_CONFIG_DIR is required. Start Aspire from the intended AzEnv profile.");
var azureOpenAiEndpoint = builder.AddParameter(
    "azure-openai-endpoint",
    builder.Configuration["Parameters:azure-openai-endpoint"] ?? string.Empty)
    .WithDescription("Azure OpenAI or Foundry model endpoint.");
var azureOpenAiDeploymentName = builder.AddParameter(
    "azure-openai-deployment-name",
    builder.Configuration["Parameters:azure-openai-deployment-name"] ?? string.Empty)
    .WithDescription("Azure OpenAI model deployment name.");
var azureOpenAiSubscriptionId = builder.AddParameter(
    "azure-openai-subscription-id",
    builder.Configuration["Parameters:azure-openai-subscription-id"] ?? string.Empty)
    .WithDescription("Azure subscription containing the OpenAI resource.");
var azureMapsClientId = builder.AddParameter(
    "azure-maps-client-id",
    builder.Configuration["Parameters:azure-maps-client-id"] ?? string.Empty)
    .WithDescription("Azure Maps account client ID.");
var azureMapsTenantId = builder.AddParameter(
    "azure-maps-tenant-id",
    builder.Configuration["Parameters:azure-maps-tenant-id"] ?? string.Empty)
    .WithDescription("Microsoft Entra tenant used to authenticate to Azure Maps.");
var azureMapsSubscriptionId = builder.AddParameter(
    "azure-maps-subscription-id",
    builder.Configuration["Parameters:azure-maps-subscription-id"] ?? string.Empty)
    .WithDescription("Azure subscription containing the Maps account.");
var reservoirSimulationOperatorKey = builder.AddParameter(
    "reservoir-simulation-operator-key",
    builder.Configuration["Parameters:reservoir-simulation-operator-key"] ?? string.Empty,
    secret: true)
    .WithDescription("Operator-only key for hidden reservoir world generation and simulation.");
var drillingOperationsInternalKey = builder.AddParameter(
    "drilling-operations-internal-key",
    builder.Configuration["Parameters:drilling-operations-internal-key"] ?? string.Empty,
    secret: true)
    .WithDescription("Internal-only key for Drilling Operations operator routes.");
var drillingOperationsAnalysisCallbackKey = builder.AddParameter(
    "drilling-operations-analysis-callback-key",
    builder.Configuration["Parameters:drilling-operations-analysis-callback-key"] ?? string.Empty,
    secret: true)
    .WithDescription("Drilling Operations credential for Analysis API callbacks.");
var publicationImportKey = builder.AddParameter(
    "publication-import-key",
    builder.Configuration["Parameters:publication-import-key"] ?? string.Empty,
    secret: true)
    .WithDescription("Internal credential for staged ontology publication.");

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

// Advertise only services that actually implement the public HTTP MCP transport.
#pragma warning disable ASPIREMCP001
cartographicProjection.WithMcpServer("/cartographicprojection/api/mcp", endpointName: "http");
cluster.WithMcpServer("/cluster/api/mcp", endpointName: "http");
drillString.WithMcpServer("/drillstring/api/mcp", endpointName: "http");
earthGravity.WithMcpServer("/EarthGravity/api/mcp", endpointName: "http");
earthMagneticField.WithMcpServer("/EarthMagneticField/api/mcp", endpointName: "http");
earthVerticalDatum.WithMcpServer("/EarthVerticalDatum/api/mcp", endpointName: "http");
fieldService.WithMcpServer("/field/api/mcp", endpointName: "http");
geodeticDatum.WithMcpServer("/geodeticdatum/api/mcp", endpointName: "http");
rig.WithMcpServer("/rig/api/mcp", endpointName: "http");
simulator4nDof.WithMcpServer("/simulator4ndof/api/mcp", endpointName: "http");
surveyInstrument.WithMcpServer("/surveyinstrument/api/mcp", endpointName: "http");
unitConversion.WithMcpServer("/unitconversion/api/mcp", endpointName: "http");
well.WithMcpServer("/well/api/mcp", endpointName: "http");
wellBore.WithMcpServer("/wellbore/api/mcp", endpointName: "http");
wellBoreArchitecture.WithMcpServer("/wellborearchitecture/api/mcp", endpointName: "http");
#pragma warning restore ASPIREMCP001

// Hidden ground truth: intentionally not referenced by analysis-api or analysis-web.
var reservoirSimulationDatabase = builder.AddSqlite(
    "reservoir-simulation-db",
    databaseDirectory,
    "ReservoirSimulation.db");
var reservoirSimulation = builder
    .AddProject("reservoir-simulation", "../../src/ReservoirSimulation/Service/Service.csproj", static options => options.ExcludeLaunchProfile = true)
    .WithReference(reservoirSimulationDatabase, "Sqlite")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("RESERVOIR_SIMULATION_OPERATOR_KEY", reservoirSimulationOperatorKey)
    .WithHttpEndpoint();
var analysisApiDatabase = builder.AddSqlite(
    "analysis-api-db",
    databaseDirectory,
    "AnalysisApi.db");
var analysisApi = builder
    .AddProject("analysis-api", "../../src/DrillSim.AnalysisApi/DrillSim.AnalysisApi.csproj", static options => options.ExcludeLaunchProfile = true)
    .WithReference(analysisApiDatabase, "Sqlite")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("FieldServiceUrl", fieldService.GetEndpoint("http"))
    .WithEnvironment("ClusterServiceUrl", cluster.GetEndpoint("http"))
    .WithEnvironment("WellServiceUrl", well.GetEndpoint("http"))
    .WithEnvironment("WellBoreServiceUrl", wellBore.GetEndpoint("http"))
    .WithEnvironment("WellBoreArchitectureServiceUrl", wellBoreArchitecture.GetEndpoint("http"))
    .WithEnvironment("TrajectoryServiceUrl", trajectory.GetEndpoint("http"))
    .WithEnvironment("GeologicalPropertiesServiceUrl", geologicalProperties.GetEndpoint("http"))
    .WithEnvironment("DRILLING_OPERATIONS_CALLBACK_KEY", drillingOperationsAnalysisCallbackKey)
    .WithEnvironment("AZURE_OPENAI_ENDPOINT", azureOpenAiEndpoint)
    .WithEnvironment("AZURE_OPENAI_DEPLOYMENT_NAME", azureOpenAiDeploymentName)
    .WithEnvironment("AZURE_OPENAI_SUBSCRIPTION_ID", azureOpenAiSubscriptionId)
    .WithEnvironment("AZURE_MAPS_CLIENT_ID", azureMapsClientId)
    .WithEnvironment("AZURE_MAPS_TENANT_ID", azureMapsTenantId)
    .WithEnvironment("AZURE_MAPS_SUBSCRIPTION_ID", azureMapsSubscriptionId)
    .WithEnvironment("AZURE_CONFIG_DIR", azureConfigDirectory)
    .WithHttpEndpoint()
    .WithHttpsEndpoint()
    .WithExternalHttpEndpoints()
    .WaitFor(fieldService)
    .WaitFor(cluster)
    .WaitFor(well)
    .WaitFor(wellBore)
    .WaitFor(wellBoreArchitecture)
    .WaitFor(trajectory)
    .WaitFor(geologicalProperties);
var analysisWeb = builder
    .AddViteApp("analysis-web", "../../src/DrillSim.AnalysisWeb")
    .WithEndpoint("http", static endpoint => endpoint.Port = 5173)
    .WithReference(analysisApi)
    .WithEnvironment("ANALYSIS_API_URL", analysisApi.GetEndpoint("http"))
    .WithEnvironment("BROWSER", "none")
    .WithExternalHttpEndpoints()
    .WaitFor(analysisApi);
var clusterUi = builder
    .AddProject("cluster-ui", "../../src/Cluster/WebApp/WebApp.csproj", static options => options.ExcludeLaunchProfile = true)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithHttpEndpoint()
    .WithHttpsEndpoint()
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/cluster/webapp/health");

fieldService.WithEnvironment("AnalysisApiUrl", analysisApi.GetEndpoint("http")).WithEnvironment("DRILLSIM_PUBLICATION_IMPORT_KEY", publicationImportKey);
cluster.WithEnvironment("AnalysisApiUrl", analysisApi.GetEndpoint("http")).WithEnvironment("DRILLSIM_PUBLICATION_IMPORT_KEY", publicationImportKey);
well.WithEnvironment("AnalysisApiUrl", analysisApi.GetEndpoint("http")).WithEnvironment("DRILLSIM_PUBLICATION_IMPORT_KEY", publicationImportKey);
wellBore.WithEnvironment("AnalysisApiUrl", analysisApi.GetEndpoint("http")).WithEnvironment("DRILLSIM_PUBLICATION_IMPORT_KEY", publicationImportKey);
wellBoreArchitecture.WithEnvironment("AnalysisApiUrl", analysisApi.GetEndpoint("http")).WithEnvironment("DRILLSIM_PUBLICATION_IMPORT_KEY", publicationImportKey);
trajectory.WithEnvironment("AnalysisApiUrl", analysisApi.GetEndpoint("http")).WithEnvironment("DRILLSIM_PUBLICATION_IMPORT_KEY", publicationImportKey);
geologicalProperties.WithEnvironment("AnalysisApiUrl", analysisApi.GetEndpoint("http")).WithEnvironment("DRILLSIM_PUBLICATION_IMPORT_KEY", publicationImportKey);

// Internal Stage B orchestration: intentionally has no external endpoint.
var drillingOperationsDatabase = builder.AddSqlite(
    "drilling-operations-db", databaseDirectory, "DrillingOperations.db");
var drillingOperations = builder
    .AddProject("drilling-operations", "../../src/DrillingOperations/Service/Service.csproj", static options => options.ExcludeLaunchProfile = true)
    .WithReference(drillingOperationsDatabase, "Sqlite")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("DRILLING_OPERATIONS_INTERNAL_KEY", drillingOperationsInternalKey)
    .WithEnvironment("ReservoirSimulationUrl", reservoirSimulation.GetEndpoint("http"))
    .WithEnvironment("RESERVOIR_SIMULATION_OPERATOR_KEY", reservoirSimulationOperatorKey)
    .WithEnvironment("AnalysisApiUrl", analysisApi.GetEndpoint("http"))
    .WithEnvironment("DRILLING_OPERATIONS_ANALYSIS_CALLBACK_KEY", drillingOperationsAnalysisCallbackKey)
    .WithEnvironment("DRILLSIM_PUBLICATION_IMPORT_KEY", publicationImportKey)
    .WithEnvironment("FieldServiceUrl", fieldService.GetEndpoint("http"))
    .WithEnvironment("ClusterServiceUrl", cluster.GetEndpoint("http"))
    .WithEnvironment("WellServiceUrl", well.GetEndpoint("http"))
    .WithEnvironment("WellBoreServiceUrl", wellBore.GetEndpoint("http"))
    .WithEnvironment("WellBoreArchitectureServiceUrl", wellBoreArchitecture.GetEndpoint("http"))
    .WithEnvironment("TrajectoryServiceUrl", trajectory.GetEndpoint("http"))
    .WithEnvironment("GeologicalPropertiesServiceUrl", geologicalProperties.GetEndpoint("http"))
    .WithEnvironment("RigServiceUrl", rig.GetEndpoint("http"))
    .WithEnvironment("DrillStringServiceUrl", drillString.GetEndpoint("http"))
    .WithEnvironment("DrillingFluidServiceUrl", drillingFluid.GetEndpoint("http"))
    .WithEnvironment("SurveyInstrumentServiceUrl", surveyInstrument.GetEndpoint("http"))
    .WithEnvironment("Simulator4nDofServiceUrl", simulator4nDof.GetEndpoint("http"))
    .WithHttpEndpoint()
    .WaitFor(reservoirSimulation)
    .WaitFor(analysisApi)
    .WaitFor(fieldService)
    .WaitFor(cluster)
    .WaitFor(well)
    .WaitFor(wellBore)
    .WaitFor(wellBoreArchitecture)
    .WaitFor(trajectory)
    .WaitFor(geologicalProperties);
analysisApi
    .WithEnvironment("LocalOperator__BackendUrl", drillingOperations.GetEndpoint("http"))
    .WithEnvironment("LocalOperator__InternalKey", drillingOperationsInternalKey)
    .WithEnvironment("LocalOperator__BrowserOrigin", analysisWeb.GetEndpoint("http"));
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
