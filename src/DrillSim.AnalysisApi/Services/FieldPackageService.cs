using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace DrillSim.AnalysisApi.Services;

public interface IFieldPackageService
{
    Task<JsonNode> GetFieldsAsync(CancellationToken cancellationToken);
    Task<AnalysisPackage> BuildPackageAsync(Guid fieldId, CancellationToken cancellationToken);
}

public sealed class FieldPackageService : IFieldPackageService
{
    public const string FieldClient = "FieldService";
    public const string ClusterClient = "ClusterService";
    public const string WellClient = "WellService";
    public const string WellBoreClient = "WellBoreService";
    public const string ArchitectureClient = "WellBoreArchitectureService";
    public const string TrajectoryClient = "TrajectoryService";
    public const string GeologyClient = "GeologicalPropertiesService";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IPackageHasher _hasher;
    private readonly TimeProvider _timeProvider;

    public FieldPackageService(IHttpClientFactory httpClientFactory, IPackageHasher hasher, TimeProvider timeProvider)
    {
        _httpClientFactory = httpClientFactory;
        _hasher = hasher;
        _timeProvider = timeProvider;
    }

    public Task<JsonNode> GetFieldsAsync(CancellationToken cancellationToken) =>
        GetAsync(FieldClient, "/field/api/Field/LightData", expectArray: true, cancellationToken);

    public async Task<AnalysisPackage> BuildPackageAsync(Guid fieldId, CancellationToken cancellationToken)
    {
        string id = fieldId.ToString();
        Task<JsonNode> fieldTask = GetAsync(FieldClient, $"/field/api/Field/{id}", false, cancellationToken);
        Task<JsonNode> clustersTask = GetAsync(ClusterClient, $"/cluster/api/Cluster/ByFieldId?guid={id}", true, cancellationToken);
        Task<JsonNode> trajectoriesTask = GetAsync(TrajectoryClient, $"/trajectory/api/Trajectory/HeavyData?fieldId={id}", true, cancellationToken);

        await Task.WhenAll(fieldTask, clustersTask, trajectoriesTask);

        JsonNode field = await fieldTask;
        IReadOnlyList<JsonNode> clusters = ToItems(await clustersTask);
        IReadOnlyList<JsonNode> trajectories = ToItems(await trajectoriesTask);
        var gaps = new List<string>();

        Guid[] clusterIds = clusters.Select(JsonAccess.MetaId).OfType<Guid>().ToArray();
        if (trajectories.Count == 0)
            gaps.Add("No trajectories were returned for the field.");
        if (clusters.Count == 0)
            gaps.Add("No clusters were returned for the field.");
        if (clusterIds.Length != clusters.Count)
            gaps.Add("One or more clusters had no MetaInfo.ID and could not be followed.");

        Task<JsonNode>[] wellTasks = clusterIds
            .Select(clusterId => GetAsync(WellClient, $"/well/api/Well/ClusterId?clusterId={clusterId}", true, cancellationToken))
            .ToArray();
        await Task.WhenAll(wellTasks);
        JsonNode[] wells = wellTasks.SelectMany(task => ToItems(task.Result)).ToArray();
        if (clusters.Count > 0 && wells.Length == 0)
            gaps.Add("No wells were returned for the field clusters.");

        Guid[] wellIds = wells.Select(JsonAccess.MetaId).OfType<Guid>().ToArray();
        if (wellIds.Length != wells.Length)
            gaps.Add("One or more wells had no MetaInfo.ID and could not be followed.");

        Task<JsonNode>[] wellBoreTasks = wellIds
            .Select(wellId => GetAsync(WellBoreClient, $"/wellbore/api/WellBore/ByWellID?wellID={wellId}", true, cancellationToken))
            .ToArray();
        await Task.WhenAll(wellBoreTasks);
        JsonNode[] wellBores = wellBoreTasks.SelectMany(task => ToItems(task.Result)).ToArray();
        if (wells.Length > 0 && wellBores.Length == 0)
            gaps.Add("No wellbores were returned for the field wells.");

        HashSet<Guid> wellBoreIds = wellBores.Select(JsonAccess.MetaId).OfType<Guid>().ToHashSet();
        if (wellBoreIds.Count != wellBores.Length)
            gaps.Add("One or more wellbores had no MetaInfo.ID and could not be matched to geology.");
        // ponytail: the architecture service has no wellbore query; replace this scan when its store grows beyond field-scale data.
        Task<JsonNode> architecturesTask = GetAsync(
            ArchitectureClient,
            "/wellborearchitecture/api/WellBoreArchitecture/HeavyData",
            true,
            cancellationToken);
        Task<JsonNode>[] geologyTasks = wellBoreIds
            .Select(wellBoreId => GetAsync(
                GeologyClient,
                $"/geologicalproperties/api/GeologicalProperties/ByWellBoreID?wellBoreId={wellBoreId}",
                true,
                cancellationToken))
            .ToArray();
        await Task.WhenAll(geologyTasks.Append(architecturesTask));
        JsonNode[] geology = geologyTasks.SelectMany(task => ToItems(task.Result)).ToArray();
        JsonNode[] architectures = ToItems(await architecturesTask)
            .Where(item => JsonAccess.Guid(item, "WellBoreID") is Guid id && wellBoreIds.Contains(id))
            .ToArray();
        if (wellBores.Length > 0 && architectures.Length == 0)
            gaps.Add("No wellbore architecture matched the included wellbores.");
        int wellBoresWithoutArchitecture = wellBoreIds.Count -
            architectures.Select(item => JsonAccess.Guid(item, "WellBoreID")).OfType<Guid>().Distinct().Count();
        if (wellBoresWithoutArchitecture > 0)
            gaps.Add($"{wellBoresWithoutArchitecture} included wellbore(s) have no architecture record.");
        if (wellBores.Length > 0 && geology.Length == 0)
            gaps.Add("No geological properties matched the included wellbores.");
        HashSet<Guid> geologyWellBoreIds = geology.Select(item => JsonAccess.Guid(item, "WellBoreID")).OfType<Guid>().ToHashSet();
        int wellBoresWithoutGeology = wellBoreIds.Count(id => !geologyWellBoreIds.Contains(id));
        if (wellBoresWithoutGeology > 0)
            gaps.Add($"{wellBoresWithoutGeology} included wellbore(s) had no geological-properties record.");
        HashSet<Guid> trajectoryWellBoreIds = trajectories
            .Select(item => JsonAccess.Guid(item, "WellBoreID"))
            .OfType<Guid>()
            .ToHashSet();
        int wellBoresWithoutTrajectory = wellBoreIds.Count(id => !trajectoryWellBoreIds.Contains(id));
        if (wellBoresWithoutTrajectory > 0)
            gaps.Add($"{wellBoresWithoutTrajectory} included wellbore(s) have no survey trajectory.");

        var counts = new SourceCounts(
            1,
            clusters.Count,
            wells.Length,
            wellBores.Length,
            architectures.Length,
            trajectories.Count,
            geology.Length);
        string[] orderedGaps = gaps.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        string hash = _hasher.Compute(
            fieldId,
            field,
            clusters,
            wells,
            wellBores,
            architectures,
            trajectories,
            geology,
            counts,
            orderedGaps);

        return new AnalysisPackage(
            _timeProvider.GetUtcNow(),
            fieldId,
            field,
            clusters,
            wells,
            wellBores,
            architectures,
            trajectories,
            geology,
            counts,
            orderedGaps,
            hash);
    }

    private async Task<JsonNode> GetAsync(string clientName, string relativeUrl, bool expectArray, CancellationToken cancellationToken)
    {
        HttpClient client = _httpClientFactory.CreateClient(clientName);
        var requestUri = new Uri(client.BaseAddress ?? throw new InvalidOperationException($"HttpClient {clientName} has no base address."), relativeUrl);

        try
        {
            using HttpResponseMessage response = await client.GetAsync(relativeUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
                throw new UpstreamServiceException(clientName, requestUri, response.StatusCode, $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}");

            await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            JsonNode node = await JsonNode.ParseAsync(stream, cancellationToken: cancellationToken)
                ?? throw new UpstreamServiceException(clientName, requestUri, response.StatusCode, "Response body was empty.");
            if (expectArray && node is not JsonArray)
                throw new UpstreamServiceException(clientName, requestUri, response.StatusCode, "Response was not a JSON array.");
            if (!expectArray && node is not JsonObject)
                throw new UpstreamServiceException(clientName, requestUri, response.StatusCode, "Response was not a JSON object.");
            return node;
        }
        catch (HttpRequestException exception)
        {
            throw new UpstreamServiceException(clientName, requestUri, exception.StatusCode, exception.Message, exception);
        }
        catch (TimeoutRejectedException exception)
        {
            throw new UpstreamServiceException(clientName, requestUri, HttpStatusCode.GatewayTimeout,
                "The upstream service timed out. Retry after its resource is ready.", exception);
        }
        catch (BrokenCircuitException exception)
        {
            throw new UpstreamServiceException(clientName, requestUri, HttpStatusCode.ServiceUnavailable,
                "The upstream service is temporarily unavailable while its circuit recovers.", exception);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new UpstreamServiceException(clientName, requestUri, HttpStatusCode.GatewayTimeout,
                "The upstream HTTP request timed out.", exception);
        }
        catch (JsonException exception)
        {
            throw new UpstreamServiceException(clientName, requestUri, null, "Upstream returned invalid JSON.", exception);
        }
    }

    private static IReadOnlyList<JsonNode> ToItems(JsonNode node) =>
        ((JsonArray)node).Where(item => item is not null).Select(item => item!.DeepClone()).ToArray();
}
