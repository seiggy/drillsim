using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DrillSim.PublicationGate;

public sealed record PublicationStageRequest(Guid ScenarioId, Guid RevealId);
public sealed record PublicationGateStatus(Guid EntityId, Guid ScenarioId, Guid RevealId, string State);

public sealed class ScenarioPublicationGateStore
{
    private readonly string _connectionString;
    private readonly HttpClient? _analysis;
    private readonly SemaphoreSlim _initialization = new(1, 1);
    private bool _initialized;

    public ScenarioPublicationGateStore(string connectionString, HttpClient? analysis = null)
    {
        _connectionString = connectionString;
        _analysis = analysis;
    }

    public async Task StageAsync(Guid entityId, PublicationStageRequest request, CancellationToken cancellationToken)
    {
        if (entityId == Guid.Empty || request.ScenarioId == Guid.Empty || request.RevealId == Guid.Empty)
            throw new ArgumentException("Publication identities must be non-empty GUIDs.");

        await InitializeAsync(cancellationToken);
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO DrillSimPublicationVisibility(EntityId,ScenarioId,RevealId,State,RegisteredUtc,ActivatedUtc,VisibleUtc)
            VALUES($entity,$scenario,$reveal,'Staged',$now,NULL,NULL)
            ON CONFLICT(EntityId) DO NOTHING;
            """;
        command.Parameters.AddWithValue("$entity", entityId.ToString("D"));
        command.Parameters.AddWithValue("$scenario", request.ScenarioId.ToString("D"));
        command.Parameters.AddWithValue("$reveal", request.RevealId.ToString("D"));
        command.Parameters.AddWithValue("$now", DateTimeOffset.UtcNow.ToString("O"));
        await command.ExecuteNonQueryAsync(cancellationToken);

        PublicationGateStatus stored = await GetRequiredAsync(entityId, cancellationToken);
        if (stored.ScenarioId != request.ScenarioId || stored.RevealId != request.RevealId)
            throw new InvalidOperationException("Publication identity conflict.");
    }

    public async Task ActivateAsync(Guid entityId, CancellationToken cancellationToken)
    {
        await InitializeAsync(cancellationToken);
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE DrillSimPublicationVisibility
            SET State=CASE WHEN State='Staged' THEN 'Activated' ELSE State END,
                ActivatedUtc=CASE WHEN State='Staged' THEN COALESCE(ActivatedUtc,$now) ELSE ActivatedUtc END
            WHERE EntityId=$entity;
            """;
        command.Parameters.AddWithValue("$entity", entityId.ToString("D"));
        command.Parameters.AddWithValue("$now", DateTimeOffset.UtcNow.ToString("O"));
        if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
            throw new KeyNotFoundException("Publication marker was not found.");
    }

    public async Task<PublicationGateStatus?> GetAsync(Guid entityId, CancellationToken cancellationToken)
    {
        await InitializeAsync(cancellationToken);
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT ScenarioId,RevealId,State FROM DrillSimPublicationVisibility WHERE EntityId=$entity;";
        command.Parameters.AddWithValue("$entity", entityId.ToString("D"));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? new(entityId, Guid.Parse(reader.GetString(0)), Guid.Parse(reader.GetString(1)), reader.GetString(2))
            : null;
    }

    public async Task<HashSet<Guid>> GetRegisteredIdsAsync(CancellationToken cancellationToken)
    {
        await InitializeAsync(cancellationToken);
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT EntityId FROM DrillSimPublicationVisibility;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var ids = new HashSet<Guid>();
        while (await reader.ReadAsync(cancellationToken)) ids.Add(Guid.Parse(reader.GetString(0)));
        return ids;
    }

    public async Task<int> DeleteExpiredUnregisteredAsync(SqliteConnection serviceConnection, string table,
        DateTimeOffset cutoff, CancellationToken cancellationToken)
    {
        if (table is not ("WellBoreArchitectureTable" or "GeologicalPropertiesTable"))
            throw new ArgumentException("Unsupported publication retention table.", nameof(table));
        if (string.IsNullOrEmpty(serviceConnection.DataSource) || serviceConnection.DataSource == ":memory:")
            throw new InvalidOperationException("Publication-aware retirement requires a file-backed service database.");
        await InitializeAsync(cancellationToken);
        await using var connection = new SqliteConnection(new SqliteConnectionStringBuilder(_connectionString)
            { Pooling = false }.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using (var attach = connection.CreateCommand())
        {
            attach.CommandText = "ATTACH DATABASE $database AS retirement;";
            attach.Parameters.AddWithValue("$database", serviceConnection.DataSource);
            await attach.ExecuteNonQueryAsync(cancellationToken);
        }
        // Lock marker registration and retirement together; a copied ID list could miss a concurrent stage.
        using var transaction = connection.BeginTransaction(deferred: false);
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"""
            DELETE FROM retirement.{table}
            WHERE LastModificationDate < $cutoff
              AND NOT EXISTS (
                SELECT 1 FROM main.DrillSimPublicationVisibility
                WHERE EntityId = retirement.{table}.ID COLLATE NOCASE
              );
            """;
        command.Parameters.AddWithValue("$cutoff", cutoff.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss",
            System.Globalization.CultureInfo.InvariantCulture));
        int count = await command.ExecuteNonQueryAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return count;
    }

    public async Task<HashSet<Guid>> GetHiddenIdsAsync(CancellationToken cancellationToken)
    {
        await InitializeAsync(cancellationToken);
        var registrations = new List<PublicationGateStatus>();
        await using (var connection = await OpenAsync(cancellationToken))
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT EntityId,ScenarioId,RevealId,State FROM DrillSimPublicationVisibility WHERE State<>'Visible';";
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                registrations.Add(new(Guid.Parse(reader.GetString(0)), Guid.Parse(reader.GetString(1)), Guid.Parse(reader.GetString(2)), reader.GetString(3)));
        }

        var hidden = new HashSet<Guid>();
        foreach (var group in registrations.GroupBy(x => (x.ScenarioId, x.RevealId)))
        {
            if (await IsRevealFinalizedAsync(group.Key.ScenarioId, group.Key.RevealId, cancellationToken))
                await PromoteVisibleAsync(group.Key.ScenarioId, group.Key.RevealId, cancellationToken);
            else
                foreach (PublicationGateStatus record in group) hidden.Add(record.EntityId);
        }
        return hidden;
    }

    private async Task<bool> IsRevealFinalizedAsync(Guid scenarioId, Guid revealId, CancellationToken cancellationToken)
    {
        if (_analysis is null) return false;
        try
        {
            using HttpResponseMessage response = await _analysis.GetAsync($"api/scenarios/{scenarioId:D}/reveal", cancellationToken);
            if (!response.IsSuccessStatusCode) return false;
            using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            JsonElement root = document.RootElement;
            return Try(root, "scenarioId", out var scenario)
                && Try(root, "revealId", out var reveal)
                && Try(root, "status", out var state)
                && scenario.ValueKind == JsonValueKind.String && scenario.GetGuid() == scenarioId
                && reveal.ValueKind == JsonValueKind.String && reveal.GetGuid() == revealId
                && string.Equals(state.GetString(), "Revealed", StringComparison.Ordinal);
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException or TaskCanceledException)
        {
            return false;
        }
    }

    private async Task PromoteVisibleAsync(Guid scenarioId, Guid revealId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = connection.BeginTransaction(false);
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            UPDATE DrillSimPublicationVisibility
            SET State='Visible',VisibleUtc=COALESCE(VisibleUtc,$now)
            WHERE ScenarioId=$scenario AND RevealId=$reveal AND State IN ('Staged','Activated');
            """;
        command.Parameters.AddWithValue("$scenario", scenarioId.ToString("D"));
        command.Parameters.AddWithValue("$reveal", revealId.ToString("D"));
        command.Parameters.AddWithValue("$now", DateTimeOffset.UtcNow.ToString("O"));
        await command.ExecuteNonQueryAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<PublicationGateStatus> GetRequiredAsync(Guid entityId, CancellationToken cancellationToken) =>
        await GetAsync(entityId, cancellationToken) ?? throw new KeyNotFoundException("Publication marker was not found.");

    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (_initialized) return;
        await _initialization.WaitAsync(cancellationToken);
        try
        {
            if (_initialized) return;
            await using var connection = await OpenAsync(cancellationToken);
            string? existingSql;
            await using (var inspect = connection.CreateCommand())
            {
                inspect.CommandText = "SELECT sql FROM sqlite_master WHERE type='table' AND name='DrillSimPublicationVisibility';";
                existingSql = await inspect.ExecuteScalarAsync(cancellationToken) as string;
            }

            if (existingSql is not null && !existingSql.Contains("'Visible'", StringComparison.OrdinalIgnoreCase))
            {
                await using var migration = connection.BeginTransaction(false);
                await using var migrate = connection.CreateCommand();
                migrate.Transaction = migration;
                migrate.CommandText = """
                    ALTER TABLE DrillSimPublicationVisibility RENAME TO DrillSimPublicationVisibility_v1;
                    CREATE TABLE DrillSimPublicationVisibility(EntityId TEXT PRIMARY KEY,ScenarioId TEXT NOT NULL,RevealId TEXT NOT NULL,State TEXT NOT NULL CHECK(State IN ('Staged','Activated','Visible')),RegisteredUtc TEXT NOT NULL,ActivatedUtc TEXT NULL,VisibleUtc TEXT NULL);
                    INSERT INTO DrillSimPublicationVisibility(EntityId,ScenarioId,RevealId,State,RegisteredUtc,ActivatedUtc,VisibleUtc)
                    SELECT EntityId,ScenarioId,RevealId,State,RegisteredUtc,ActivatedUtc,NULL FROM DrillSimPublicationVisibility_v1;
                    DROP TABLE DrillSimPublicationVisibility_v1;
                    """;
                await migrate.ExecuteNonQueryAsync(cancellationToken);
                await migration.CommitAsync(cancellationToken);
            }

            await using var command = connection.CreateCommand();
            command.CommandText = """
                CREATE TABLE IF NOT EXISTS DrillSimPublicationVisibility(EntityId TEXT PRIMARY KEY,ScenarioId TEXT NOT NULL,RevealId TEXT NOT NULL,State TEXT NOT NULL CHECK(State IN ('Staged','Activated','Visible')),RegisteredUtc TEXT NOT NULL,ActivatedUtc TEXT NULL,VisibleUtc TEXT NULL);
                CREATE INDEX IF NOT EXISTS IX_DrillSimPublicationVisibility_Reveal ON DrillSimPublicationVisibility(ScenarioId,RevealId);
                """;
            await command.ExecuteNonQueryAsync(cancellationToken);
            _initialized = true;
        }
        finally { _initialization.Release(); }
    }

    private async Task<SqliteConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private static bool Try(JsonElement value, string name, out JsonElement result)
    {
        if (value.ValueKind == JsonValueKind.Object)
            foreach (JsonProperty property in value.EnumerateObject())
                if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) { result = property.Value; return true; }
        result = default;
        return false;
    }
}

public sealed class ScenarioPublicationGateMiddleware(RequestDelegate next, ScenarioPublicationGateStore store, string? internalKey)
{
    public async Task InvokeAsync(HttpContext context)
    {
        bool authorized = IsAuthorized(context.Request.Headers, internalKey);
        if (context.Request.Path.StartsWithSegments("/internal/publication"))
        {
            if (!authorized) context.Response.StatusCode = string.IsNullOrEmpty(internalKey) ? StatusCodes.Status404NotFound : StatusCodes.Status401Unauthorized;
            else await next(context);
            return;
        }
        if (authorized) { await next(context); return; }

        HashSet<Guid> registered = await store.GetRegisteredIdsAsync(context.RequestAborted);
        bool mutation=context.Request.Method is "POST" or "PUT" or "PATCH";
        if (mutation && (PathContainsId(context.Request.Path,registered) || await RequestWritesRegisteredEntityAsync(context, registered))) { context.Response.StatusCode = StatusCodes.Status409Conflict; return; }

        HashSet<Guid> hiddenBefore = await store.GetHiddenIdsAsync(context.RequestAborted);
        if (context.WebSockets.IsWebSocketRequest && context.Request.Path.Value?.Contains("mcp", StringComparison.OrdinalIgnoreCase) == true && hiddenBefore.Count != 0)
        { context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable; return; }
        if (PathContainsId(context.Request.Path, hiddenBefore)) { context.Response.StatusCode = StatusCodes.Status404NotFound; return; }

        Stream original = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;
        try { await next(context); }
        finally { context.Response.Body = original; }

        HashSet<Guid> hiddenAfter = await store.GetHiddenIdsAsync(context.RequestAborted);
        if (PathContainsId(context.Request.Path, hiddenAfter)) { context.Response.StatusCode = StatusCodes.Status404NotFound; context.Response.ContentLength = 0; return; }
        if (context.Response.StatusCode is < 200 or >= 300) { buffer.Position = 0; await buffer.CopyToAsync(original, context.RequestAborted); return; }

        if (context.Response.ContentType?.Contains("event-stream", StringComparison.OrdinalIgnoreCase) == true)
        {
            buffer.Position = 0;
            using var reader = new StreamReader(buffer, Encoding.UTF8);
            string text = await reader.ReadToEndAsync(context.RequestAborted);
            var lines = new List<string>();
            foreach (string line in text.Split('\n'))
                if (line.StartsWith("data:", StringComparison.Ordinal) && TryFilterEmbedded(line[5..].TrimStart(), hiddenAfter, out string filtered)) lines.Add("data: " + filtered);
                else lines.Add(line);
            byte[] eventBytes = Encoding.UTF8.GetBytes(string.Join("\n", lines));
            context.Response.ContentLength = eventBytes.Length;
            await original.WriteAsync(eventBytes, context.RequestAborted);
            return;
        }

        if (context.Response.ContentType?.Contains("json", StringComparison.OrdinalIgnoreCase) != true)
        { buffer.Position = 0; await buffer.CopyToAsync(original, context.RequestAborted); return; }

        buffer.Position = 0;
        JsonNode? root;
        try { root = await JsonNode.ParseAsync(buffer, cancellationToken: context.RequestAborted); }
        catch (JsonException) { buffer.Position = 0; await buffer.CopyToAsync(original, context.RequestAborted); return; }
        if (root is null) return;
        if (Filter(root, hiddenAfter)) { context.Response.StatusCode = StatusCodes.Status404NotFound; context.Response.ContentLength = 0; return; }
        byte[] bytes = Encoding.UTF8.GetBytes(root.ToJsonString());
        context.Response.ContentLength = bytes.Length;
        await original.WriteAsync(bytes, context.RequestAborted);
    }

    private static bool Filter(JsonNode node, HashSet<Guid> hidden)
    {
        if (EntityId(node) is Guid id && hidden.Contains(id)) return true;
        if (node is JsonArray array)
        {
            for (int i = array.Count - 1; i >= 0; i--)
            {
                JsonNode? item = array[i];
                if (item is null) continue;
                if (item is JsonValue value && value.TryGetValue<string>(out var text) && Guid.TryParse(text, out var guid) && hidden.Contains(guid)) { array.RemoveAt(i); continue; }
                if (Filter(item, hidden)) array.RemoveAt(i);
            }
        }
        else if (node is JsonObject obj)
            foreach (var pair in obj.ToArray())
            {
                if (pair.Value is null) continue;
                if (pair.Value is JsonValue value && value.TryGetValue<string>(out var text) && TryFilterEmbedded(text, hidden, out var replacement)) obj[pair.Key] = replacement;
                else if (Filter(pair.Value, hidden)) obj.Remove(pair.Key);
            }
        return false;
    }

    private static bool TryFilterEmbedded(string text, HashSet<Guid> hidden, out string replacement)
    {
        replacement = text;
        if (text.Length < 2 || (text[0] != '{' && text[0] != '[')) return false;
        try
        {
            JsonNode? node = JsonNode.Parse(text);
            if (node is null) return false;
            if (Filter(node, hidden)) { replacement = "null"; return true; }
            replacement = node.ToJsonString();
            return replacement != text;
        }
        catch (JsonException) { return false; }
    }

    private static Guid? EntityId(JsonNode node)
    {
        if (node is not JsonObject obj) return null;
        JsonNode? meta = obj.FirstOrDefault(x => x.Key.Equals("MetaInfo", StringComparison.OrdinalIgnoreCase)).Value;
        JsonObject candidate = meta as JsonObject ?? obj;
        JsonNode? value = candidate.FirstOrDefault(x => x.Key.Equals("ID", StringComparison.OrdinalIgnoreCase)).Value;
        if (value is not JsonValue scalar) return null;
        if (scalar.TryGetValue<Guid>(out var direct)) return direct;
        return scalar.TryGetValue<string>(out var text) && Guid.TryParse(text, out var id) ? id : null;
    }

    private static async Task<bool> RequestWritesRegisteredEntityAsync(HttpContext context, IReadOnlySet<Guid> registered)
    {
        if (context.Request.Method is not ("POST" or "PUT" or "PATCH") || context.Request.ContentType?.Contains("json", StringComparison.OrdinalIgnoreCase) != true) return false;
        context.Request.EnableBuffering();
        try
        {
            JsonNode? node = await JsonNode.ParseAsync(context.Request.Body, cancellationToken: context.RequestAborted);
            return node is not null && ContainsRegisteredEntity(node, registered);
        }
        catch (JsonException) { return false; }
        finally { context.Request.Body.Position = 0; }

        static bool ContainsRegisteredEntity(JsonNode node, IReadOnlySet<Guid> ids)
        {
            Guid? entity = EntityId(node);
            if (entity.HasValue && ids.Contains(entity.Value)) return true;
            if (node is JsonObject obj) return obj.Any(x => x.Value is not null && ContainsRegisteredEntity(x.Value, ids));
            if (node is JsonArray array) return array.Any(x => x is not null && ContainsRegisteredEntity(x, ids));
            return false;
        }
    }

    private static bool PathContainsId(PathString path, HashSet<Guid> ids) =>
        path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries).Any(x => Guid.TryParse(x, out var id) && ids.Contains(id)) == true;

    internal static bool IsAuthorized(IHeaderDictionary headers, string? expected)
    {
        if (string.IsNullOrEmpty(expected) || !headers.TryGetValue("X-DrillSim-Publication-Key", out var values) || values.Count != 1) return false;
        byte[] actual = Encoding.UTF8.GetBytes(values[0]!), configured = Encoding.UTF8.GetBytes(expected);
        return actual.Length == configured.Length && CryptographicOperations.FixedTimeEquals(actual, configured);
    }
}

public static class ScenarioPublicationGateExtensions
{
    public static WebApplicationBuilder AddScenarioPublicationGate(this WebApplicationBuilder builder)
    {
        string? connection = builder.Configuration.GetConnectionString("Sqlite");
        if (string.IsNullOrWhiteSpace(connection)) return builder;

        string gateConnection = builder.Configuration.GetConnectionString("PublicationGate") ?? DeriveGateConnection(connection);
        string? key = builder.Configuration["DRILLSIM_PUBLICATION_IMPORT_KEY"];
        string? analysis = builder.Configuration["AnalysisApiUrl"];
        bool hasKey = !string.IsNullOrWhiteSpace(key), hasAnalysis = !string.IsNullOrWhiteSpace(analysis);
        if (hasKey != hasAnalysis) throw new InvalidOperationException("DRILLSIM_PUBLICATION_IMPORT_KEY and AnalysisApiUrl must be configured together.");

        HttpClient? client = null;
        if (hasAnalysis)
        {
            if (!Uri.TryCreate(analysis, UriKind.Absolute, out Uri? uri)) throw new InvalidOperationException("AnalysisApiUrl must be an absolute URI.");
            client = new HttpClient { BaseAddress = uri, Timeout = TimeSpan.FromSeconds(5) };
        }

        builder.Services.AddSingleton(new ScenarioPublicationGateStore(gateConnection, client));
        builder.Services.AddSingleton(new ScenarioPublicationGateConfiguration(hasKey ? key : null));
        return builder;
    }

    public static string DeriveGateConnection(string serviceConnection)
    {
        var parsed=new SqliteConnectionStringBuilder(serviceConnection);if(parsed.DataSource.Equals(":memory:",StringComparison.OrdinalIgnoreCase))return serviceConnection;parsed.DataSource=parsed.DataSource+".publication-gate.db";return parsed.ConnectionString;
    }

    public static WebApplication UseScenarioPublicationGate(this WebApplication app)
    {
        ScenarioPublicationGateConfiguration? config = app.Services.GetService<ScenarioPublicationGateConfiguration>();
        if (config is not null) app.UseMiddleware<ScenarioPublicationGateMiddleware>(config.Key ?? string.Empty);
        return app;
    }

    public static WebApplication MapScenarioPublicationGateEndpoints(this WebApplication app)
    {
        ScenarioPublicationGateConfiguration? config = app.Services.GetService<ScenarioPublicationGateConfiguration>();
        if (config?.Key is null) return app;

        RouteGroupBuilder group = app.MapGroup("/internal/publication/records");
        group.AddEndpointFilter(async (context, next) => ScenarioPublicationGateMiddleware.IsAuthorized(context.HttpContext.Request.Headers, config.Key) ? await next(context) : Results.Unauthorized());
        group.MapPut("/{entityId:guid}/stage", async (Guid entityId, PublicationStageRequest request, ScenarioPublicationGateStore store, CancellationToken ct) =>
        { try { await store.StageAsync(entityId, request, ct); return Results.Ok(); } catch (Exception e) when (e is ArgumentException or InvalidOperationException or SqliteException) { return Results.Conflict(); } });
        group.MapGet("/{entityId:guid}", async (Guid entityId, ScenarioPublicationGateStore store, CancellationToken ct) => await store.GetAsync(entityId, ct) is { } value ? Results.Ok(value) : Results.NotFound());
        group.MapPut("/{entityId:guid}/activate", async (Guid entityId, ScenarioPublicationGateStore store, CancellationToken ct) =>
        { try { await store.ActivateAsync(entityId, ct); return Results.Ok(); } catch (KeyNotFoundException) { return Results.NotFound(); } });
        return app;
    }

    private sealed record ScenarioPublicationGateConfiguration(string? Key);
}
