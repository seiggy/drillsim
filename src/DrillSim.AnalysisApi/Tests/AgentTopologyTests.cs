using System.Text.RegularExpressions;

namespace DrillSim.AnalysisApi.Tests;

[TestFixture]
public sealed class AgentTopologyTests
{
    [Test]
    public void ScenarioAgent_RegistersOnlyScenarioAwareTools()
    {
        string source = ReadProgramSource();

        string[] legacyTools = RegistrationTools(source, "legacyAgent");

        Assert.Multiple(() =>
        {
            Assert.That(legacyTools, Is.EqualTo(new[] { "packageTool", "analysisTool" }));
            Assert.That(source, Does.Not.Contain("publish_scenario_scorecard"));
            Assert.That(source, Does.Not.Contain("write_scenario_scorecard"));
            Assert.That(source, Does.Contain("app.MapAGUIServer(\"/agui\", legacyAgent)"));
            Assert.That(source, Does.Contain(
                "app.MapAGUIServer(\"/agui/scenario\", scenarioAgent)"));
            Assert.That(source, Does.Contain(
                "ScenarioAgentScopeBinding.IsScenarioEndpoint(context.Request.Path)"));
            Assert.That(source, Does.Contain("ScenarioAgentTools.Create("));
            Assert.That(source, Does.Contain("AgentResponsesOptions.Create(scenarioInstructions, [.. scenarioTools])"));
            Assert.That(source, Does.Not.Contain("GetChatClient("));
            Assert.That(Regex.Matches(source, @"responsesClient!\.AsAIAgent\("), Has.Count.EqualTo(2));
            Assert.That(Regex.Matches(source, @"model: deploymentName\)"), Has.Count.EqualTo(2));
            Assert.That(source, Does.Contain(
                "app.Services.GetRequiredService<IHttpContextAccessor>()"));
            Assert.That(source, Does.Not.Contain("builder.Services.AddAIAgent("));
        });
    }

    [Test]
    public void LegacyPredictionApproval_CannotBypassOperatorBoundary()
    {
        string source = ReadProgramSource();
        string route = SourceBetween(
            source,
            "app.MapPost(\"/api/scenarios/{scenarioId:guid}/prediction/approve\"",
            "});");

        Assert.Multiple(() =>
        {
            Assert.That(route, Does.Contain("StatusCodes.Status410Gone"));
            Assert.That(route, Does.Not.Contain("ApproveAsync"));
            Assert.That(route, Does.Not.Contain("AIFunction"));
            Assert.That(source, Does.Contain("AddLocalOperatorWorkflow(builder.Configuration)"));
            Assert.That(source, Does.Contain("app.MapLocalOperatorWorkflow()"));
        });
    }

    [Test]
    public void PredictionRoutes_UseIfMatchAndReturnRevisionEtags()
    {
        string source = ReadProgramSource();
        string putRoute = SourceBetween(
            source,
            "app.MapPut(\"/api/scenarios/{scenarioId:guid}/prediction\"",
            "});");
        string getRoute = SourceBetween(
            source,
            "app.MapGet(\"/api/scenarios/{scenarioId:guid}/prediction\"",
            "});");
        string sealRoute = SourceBetween(
            source,
            "app.MapPost(\"/api/scenarios/{scenarioId:guid}/prediction/seal\"",
            "}).RequireLocalOperatorMutation();");

        Assert.Multiple(() =>
        {
            Assert.That(putRoute, Does.Contain("request.Headers.TryGetValue(\"If-Match\""));
            Assert.That(putRoute, Does.Contain("ParseIfMatchRevision"));
            Assert.That(putRoute, Does.Contain("response.Headers.ETag"));
            Assert.That(getRoute, Does.Contain("response.Headers.ETag"));
            Assert.That(sealRoute, Does.Contain("ParseIfMatchRevision"));
            Assert.That(sealRoute, Does.Contain("SealAsync(scenarioId, cancellationToken, expectedRevision)"));
            Assert.That(sealRoute, Does.Contain("response.Headers.ETag"));
            Assert.That(source, Does.Contain("}).RequireLocalOperatorMutation();"));
        });
    }

    [Test]
    public void InternalReveal_IsAuthenticatedAndAbsentFromAgentTools()
    {
        string source = ReadProgramSource();
        string agentSurface = SourceBetween(
            source,
            "// Authentication and claims-based tenant/user isolation",
            "app.MapDefaultEndpoints()");

        Assert.Multiple(() =>
        {
            Assert.That(source, Does.Contain("Path.StartsWithSegments(\"/internal\")"));
            Assert.That(source, Does.Contain("InternalCallbackKeyValidator"));
            Assert.That(source, Does.Contain("/internal/scenarios/{scenarioId:guid}/reveal/prepare"));
            Assert.That(source, Does.Contain("/internal/scenarios/{scenarioId:guid}/reveal/finalize"));
            Assert.That(source, Does.Contain("/internal/scenarios/{scenarioId:guid}/reveal/{revealId:guid}/status"));
            Assert.That(source, Does.Contain("/internal/scenarios/{scenarioId:guid}/scorecard"));
            Assert.That(source, Does.Contain("/api/scenarios/{scenarioId:guid}/scorecard"));
            Assert.That(source, Does.Not.Contain("app.MapPost(\"/internal/scenarios/{scenarioId:guid}/reveal\","));
            Assert.That(agentSurface, Does.Not.Contain("RevealService"));
            Assert.That(agentSurface, Does.Not.Contain("DRILLING_OPERATIONS_CALLBACK_KEY"));
            Assert.That(agentSurface, Does.Not.Contain("/internal/"));
            Assert.That(agentSurface, Does.Not.Contain("seal_prediction"));
            Assert.That(agentSurface, Does.Not.Contain("approve_prediction"));
        });
    }

    [Test]
    public void CallbackSecret_IsWiredOnlyToAnalysisApiAndDrillingOperations()
    {
        string appHost = ReadRepositoryFile("aspire", "DrillSim.AppHost", "AppHost.cs");
        MatchCollection environmentUses = Regex.Matches(
            appHost,
            @"\.WithEnvironment\(\""(?<name>[^\""\r\n]+)\"",\s*drillingOperationsAnalysisCallbackKey\)");

        Assert.Multiple(() =>
        {
            Assert.That(environmentUses.Select(match => match.Groups["name"].Value), Is.EquivalentTo(new[]
            {
                "DRILLING_OPERATIONS_CALLBACK_KEY",
                "DRILLING_OPERATIONS_ANALYSIS_CALLBACK_KEY"
            }));
            Assert.That(environmentUses, Has.Count.EqualTo(2));
        });
    }

    private static string[] RegistrationTools(string source, string agentVariable)
    {
        string registration = SourceBetween(
            source,
            $"var {agentVariable} = responsesClient!.AsAIAgent(",
            ".AsBuilder()");
        Match match = Regex.Match(registration, @"AgentResponsesOptions\.Create\([^,]*,\s*\[(?<tools>[^\]]*)\]");
        Assert.That(match.Success, Is.True, $"Could not find tool registration for {agentVariable}.");
        return match.Groups["tools"].Value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static string SourceBetween(string source, string startMarker, string endMarker)
    {
        int start = source.IndexOf(startMarker, StringComparison.Ordinal);
        Assert.That(start, Is.GreaterThanOrEqualTo(0), $"Could not find '{startMarker}'.");
        int end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
        Assert.That(end, Is.GreaterThan(start), $"Could not find '{endMarker}' after '{startMarker}'.");
        return source[start..end];
    }

    private static string ReadRepositoryFile(params string[] relativePath)
    {
        DirectoryInfo? directory = new(TestContext.CurrentContext.TestDirectory);
        while (directory is not null)
        {
            string candidate = Path.Combine([directory.FullName, .. relativePath]);
            if (File.Exists(candidate))
                return File.ReadAllText(candidate);
            directory = directory.Parent;
        }
        throw new FileNotFoundException($"Could not locate {Path.Combine(relativePath)} from the test output directory.");
    }

    private static string ReadProgramSource()
    {
        DirectoryInfo? directory = new(TestContext.CurrentContext.TestDirectory);
        while (directory is not null)
        {
            string programPath = Path.Combine(directory.FullName, "Program.cs");
            string projectPath = Path.Combine(directory.FullName, "DrillSim.AnalysisApi.csproj");
            if (File.Exists(programPath) && File.Exists(projectPath))
                return File.ReadAllText(programPath);
            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not locate DrillSim.AnalysisApi/Program.cs from the test output directory.");
    }
}
