using DrillSim.AnalysisApi.Models;
using DrillSim.AnalysisApi.Services;
using Microsoft.Data.Sqlite;

namespace DrillSim.AnalysisApi.Infrastructure;

public sealed class SqliteHypothesisStore(string connectionString)
{
    private readonly SemaphoreSlim _initialize = new(1);
    private bool _initialized;
    private const string Schema = """
        CREATE TABLE IF NOT EXISTS hypotheses (
          hypothesis_id TEXT PRIMARY KEY, scope_key TEXT NOT NULL, name_key TEXT NOT NULL,
          name TEXT NOT NULL, head_revision INTEGER NOT NULL CHECK(head_revision > 0),
          UNIQUE(scope_key, name_key));
        CREATE TABLE IF NOT EXISTS hypothesis_revisions (
          hypothesis_id TEXT NOT NULL REFERENCES hypotheses(hypothesis_id),
          revision INTEGER NOT NULL CHECK(revision > 0), content_json TEXT NOT NULL, snapshot_sha256 TEXT NOT NULL,
          PRIMARY KEY(hypothesis_id,revision));
        CREATE TRIGGER IF NOT EXISTS hypothesis_revisions_no_update BEFORE UPDATE ON hypothesis_revisions
          BEGIN SELECT RAISE(ABORT, 'Hypothesis revisions are immutable'); END;
        CREATE TRIGGER IF NOT EXISTS hypothesis_revisions_no_delete BEFORE DELETE ON hypothesis_revisions
          BEGIN SELECT RAISE(ABORT, 'Hypothesis revisions are immutable'); END;
        CREATE TABLE IF NOT EXISTS hypothesis_challenges (
          challenge_id TEXT PRIMARY KEY, hypothesis_id TEXT NOT NULL, revision INTEGER NOT NULL,
          head_version INTEGER NOT NULL CHECK(head_version > 0),
          FOREIGN KEY(hypothesis_id,revision) REFERENCES hypothesis_revisions(hypothesis_id,revision));
        CREATE TABLE IF NOT EXISTS hypothesis_challenge_versions (
          challenge_id TEXT NOT NULL REFERENCES hypothesis_challenges(challenge_id),
          version INTEGER NOT NULL CHECK(version > 0), content_json TEXT NOT NULL, sha256 TEXT NOT NULL,
          PRIMARY KEY(challenge_id,version));
        CREATE TRIGGER IF NOT EXISTS hypothesis_challenges_no_update BEFORE UPDATE ON hypothesis_challenge_versions
          BEGIN SELECT RAISE(ABORT, 'Challenge versions are immutable'); END;
        CREATE TRIGGER IF NOT EXISTS hypothesis_challenges_no_delete BEFORE DELETE ON hypothesis_challenge_versions
          BEGIN SELECT RAISE(ABORT, 'Challenge versions are immutable'); END;
        CREATE TABLE IF NOT EXISTS hypothesis_mutations (
          operation TEXT NOT NULL, mutation_key TEXT NOT NULL, request_sha256 TEXT NOT NULL,
          entity_id TEXT NOT NULL, version INTEGER NOT NULL, PRIMARY KEY(operation,mutation_key));
        CREATE TRIGGER IF NOT EXISTS hypothesis_mutations_no_update BEFORE UPDATE ON hypothesis_mutations
          BEGIN SELECT RAISE(ABORT, 'Hypothesis mutation receipts are immutable'); END;
        CREATE TRIGGER IF NOT EXISTS hypothesis_mutations_no_delete BEFORE DELETE ON hypothesis_mutations
          BEGIN SELECT RAISE(ABORT, 'Hypothesis mutation receipts are immutable'); END;
        """;

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await _initialize.WaitAsync(ct);
        try
        {
            if (_initialized) return;
            await using SqliteConnection connection = await OpenAsync(ct);
            await ExecuteAsync(connection, null, Schema, ct);
            _initialized = true;
        }
        finally { _initialize.Release(); }
    }

    public async Task<HypothesisRevision?> FindRevisionAsync(
        HypothesisScope scope, Guid id, int? revision = null, CancellationToken ct = default)
    {
        await InitializeAsync(ct);
        await using SqliteConnection connection = await OpenAsync(ct);
        using SqliteTransaction transaction = connection.BeginTransaction(deferred: true);
        HypothesisRevision? result = await ReadRevisionAsync(connection, transaction,
            HypothesisValidation.ScopeKey(scope), id, revision, ct);
        transaction.Commit();
        return result;
    }

    public async Task<HypothesisRevision?> FindRevisionMutationAsync(
        HypothesisScope scope, string operation, string key, string requestHash, CancellationToken ct = default)
    {
        HypothesisValidation.Idempotency(key);
        await InitializeAsync(ct);
        await using SqliteConnection connection = await OpenAsync(ct);
        using SqliteTransaction transaction = connection.BeginTransaction(deferred: true);
        var receipt = await ReplayAsync(connection, transaction, operation, key, requestHash, ct);
        HypothesisRevision? result = receipt is { } existing
            ? await ReadRevisionAsync(connection, transaction, HypothesisValidation.ScopeKey(scope), existing.Id, existing.Version, ct)
                ?? throw new InvalidDataException("Hypothesis receipt references a missing revision.")
            : null;
        transaction.Commit();
        return result;
    }

    public async Task<IReadOnlyList<HypothesisSummary>> ListAsync(
        HypothesisScope scope, Guid? id, int limit, int offset, CancellationToken ct = default)
    {
        HypothesisValidation.Page(limit, offset);
        await InitializeAsync(ct);
        await using SqliteConnection connection = await OpenAsync(ct);
        using SqliteTransaction transaction = connection.BeginTransaction(deferred: true);
        string scopeKey = HypothesisValidation.ScopeKey(scope);
        var identities = new List<(Guid Id, int Revision)>();
        string query = id is null ? """
            SELECT h.hypothesis_id,h.head_revision FROM hypotheses h WHERE h.scope_key=$scope
            ORDER BY h.name_key,h.hypothesis_id LIMIT $limit OFFSET $offset;
            """ : """
            SELECT r.hypothesis_id,r.revision FROM hypothesis_revisions r
            JOIN hypotheses h ON h.hypothesis_id=r.hypothesis_id
            WHERE h.scope_key=$scope AND r.hypothesis_id=$id
            ORDER BY h.name_key,h.hypothesis_id,r.revision DESC LIMIT $limit OFFSET $offset;
            """;
        await using (SqliteCommand command = Command(connection, transaction, query,
            ("$scope", scopeKey), ("$id", id?.ToString("D")), ("$limit", limit), ("$offset", offset)))
        await using (SqliteDataReader reader = await command.ExecuteReaderAsync(ct))
            while (await reader.ReadAsync(ct))
                identities.Add((Guid.Parse(reader.GetString(0)), reader.GetInt32(1)));
        var result = new List<HypothesisSummary>();
        foreach (var identity in identities)
            result.Add(HypothesisIntegrity.Summary(await ReadRevisionAsync(connection, transaction, scopeKey, identity.Id, identity.Revision, ct)
                ?? throw new InvalidDataException("A listed hypothesis revision is missing.")));
        transaction.Commit();
        return result;
    }

    public async Task<HypothesisRevision> SaveRevisionAsync(
        HypothesisRevision value, int? expected, string operation, string key, string requestHash, CancellationToken ct = default)
    {
        HypothesisValidation.Idempotency(key);
        string json = HypothesisIntegrity.Serialize(value);
        _ = HypothesisIntegrity.ReadRevision(json, value.SnapshotSha256);
        string scopeKey = HypothesisValidation.ScopeKey(value.Scope);
        await InitializeAsync(ct);
        await using SqliteConnection connection = await OpenAsync(ct);
        using SqliteTransaction transaction = connection.BeginTransaction(deferred: false);
        var replay = await ReplayAsync(connection, transaction, operation, key, requestHash, ct);
        if (replay is { } receipt)
        {
            HypothesisRevision result = await ReadRevisionAsync(connection, transaction, scopeKey, receipt.Id, receipt.Version, ct)
                ?? throw new InvalidDataException("Hypothesis mutation receipt references a missing revision.");
            transaction.Commit();
            return result;
        }
        if (expected is null)
        {
            if (value.Revision != 1) throw HypothesisValidation.Invalid("A new hypothesis starts at revision 1.");
            try
            {
                await ExecuteAsync(connection, transaction, """
                    INSERT INTO hypotheses(hypothesis_id,scope_key,name_key,name,head_revision)
                    VALUES($id,$scope,$nameKey,$name,1);
                    """, ct, ("$id", value.HypothesisId.ToString("D")), ("$scope", scopeKey),
                    ("$nameKey", value.Name.ToUpperInvariant()), ("$name", value.Name));
            }
            catch (SqliteException exception) when (exception.SqliteExtendedErrorCode is 1555 or 2067)
            {
                throw HypothesisValidation.Conflict("A hypothesis with this name or ID already exists in the scope.");
            }
        }
        else
        {
            if (value.Revision != expected + 1) throw HypothesisValidation.Invalid("The next revision must follow If-Match.");
            int updated = await ExecuteAsync(connection, transaction, """
                UPDATE hypotheses SET head_revision=$next
                WHERE hypothesis_id=$id AND scope_key=$scope AND name=$name AND head_revision=$expected;
                """, ct, ("$id", value.HypothesisId.ToString("D")), ("$scope", scopeKey), ("$name", value.Name),
                ("$next", value.Revision), ("$expected", expected));
            if (updated != 1) throw HypothesisValidation.Conflict("The hypothesis changed; reload before appending a revision.");
        }
        await ExecuteAsync(connection, transaction, """
            INSERT INTO hypothesis_revisions(hypothesis_id,revision,content_json,snapshot_sha256) VALUES($id,$version,$json,$hash);
            """, ct, ("$id", value.HypothesisId.ToString("D")), ("$version", value.Revision), ("$json", json), ("$hash", value.SnapshotSha256));
        await ReceiptAsync(connection, transaction, operation, key, requestHash, value.HypothesisId, value.Revision, ct);
        transaction.Commit();
        return PredictionJson.Deserialize<HypothesisRevision>(json, "saved hypothesis");
    }

    public async Task<HypothesisChallenge> SaveChallengeAsync(
        HypothesisChallenge value, int? expected, string operation, string key, string requestHash, CancellationToken ct = default)
    {
        HypothesisValidation.Idempotency(key);
        string json = HypothesisIntegrity.Serialize(value);
        _ = HypothesisIntegrity.ReadChallenge(json, value.Sha256);
        await InitializeAsync(ct);
        await using SqliteConnection connection = await OpenAsync(ct);
        using SqliteTransaction transaction = connection.BeginTransaction(deferred: false);
        var replay = await ReplayAsync(connection, transaction, operation, key, requestHash, ct);
        if (replay is { } receipt)
        {
            HypothesisChallenge result = await ReadChallengeAsync(connection, transaction, value.Hypothesis, receipt.Id, receipt.Version, ct)
                ?? throw new InvalidDataException("Challenge mutation receipt references a missing version.");
            transaction.Commit();
            return result;
        }
        if (expected is null)
        {
            if (value.Version != 1) throw HypothesisValidation.Invalid("A new challenge starts at version 1.");
            await ExecuteAsync(connection, transaction, """
                INSERT INTO hypothesis_challenges(challenge_id,hypothesis_id,revision,head_version) VALUES($id,$hypothesis,$revision,1);
                """, ct, ("$id", value.ChallengeId.ToString("D")),
                ("$hypothesis", value.Hypothesis.HypothesisId.ToString("D")), ("$revision", value.Hypothesis.Revision));
        }
        else
        {
            if (value.Version != expected + 1) throw HypothesisValidation.Invalid("The next challenge version must follow If-Match.");
            int updated = await ExecuteAsync(connection, transaction, """
                UPDATE hypothesis_challenges SET head_version=$next WHERE challenge_id=$id
                AND hypothesis_id=$hypothesis AND revision=$revision AND head_version=$expected;
                """, ct, ("$id", value.ChallengeId.ToString("D")), ("$hypothesis", value.Hypothesis.HypothesisId.ToString("D")),
                ("$revision", value.Hypothesis.Revision), ("$expected", expected), ("$next", value.Version));
            if (updated != 1) throw HypothesisValidation.Conflict("Challenge dispositions changed; reload before editing.");
        }
        await ExecuteAsync(connection, transaction, """
            INSERT INTO hypothesis_challenge_versions(challenge_id,version,content_json,sha256) VALUES($id,$version,$json,$hash);
            """, ct, ("$id", value.ChallengeId.ToString("D")), ("$version", value.Version), ("$json", json), ("$hash", value.Sha256));
        await ReceiptAsync(connection, transaction, operation, key, requestHash, value.ChallengeId, value.Version, ct);
        transaction.Commit();
        return PredictionJson.Deserialize<HypothesisChallenge>(json, "saved challenge");
    }

    public async Task<HypothesisChallenge?> FindChallengeAsync(
        HypothesisReference parent, Guid id, int? version, CancellationToken ct = default)
    {
        await InitializeAsync(ct);
        await using SqliteConnection connection = await OpenAsync(ct);
        using SqliteTransaction transaction = connection.BeginTransaction(deferred: true);
        HypothesisChallenge? result = await ReadChallengeAsync(connection, transaction, parent, id, version, ct);
        transaction.Commit();
        return result;
    }

    public async Task<IReadOnlyList<HypothesisChallenge>> ListChallengesAsync(
        HypothesisReference parent, int limit, int offset, CancellationToken ct = default)
    {
        HypothesisValidation.Page(limit, offset);
        await InitializeAsync(ct);
        await using SqliteConnection connection = await OpenAsync(ct);
        using SqliteTransaction transaction = connection.BeginTransaction(deferred: true);
        var ids = new List<Guid>();
        await using (SqliteCommand command = Command(connection, transaction, """
            SELECT challenge_id FROM hypothesis_challenges WHERE hypothesis_id=$parent AND revision=$revision
            ORDER BY challenge_id LIMIT $limit OFFSET $offset;
            """, ("$parent", parent.HypothesisId.ToString("D")), ("$revision", parent.Revision), ("$limit", limit), ("$offset", offset)))
        await using (SqliteDataReader reader = await command.ExecuteReaderAsync(ct))
            while (await reader.ReadAsync(ct)) ids.Add(Guid.Parse(reader.GetString(0)));
        var result = new List<HypothesisChallenge>();
        foreach (Guid id in ids)
            result.Add(await ReadChallengeAsync(connection, transaction, parent, id, null, ct)
                ?? throw new InvalidDataException("A listed challenge is missing."));
        transaction.Commit();
        return result;
    }

    private static async Task<HypothesisRevision?> ReadRevisionAsync(
        SqliteConnection connection, SqliteTransaction transaction, string scope, Guid id, int? revision, CancellationToken ct)
    {
        await using (SqliteCommand head = Command(connection, transaction, """
            SELECT h.head_revision,(SELECT MAX(revision) FROM hypothesis_revisions WHERE hypothesis_id=h.hypothesis_id)
            FROM hypotheses h WHERE h.hypothesis_id=$id AND h.scope_key=$scope;
            """, ("$id", id.ToString("D")), ("$scope", scope)))
        await using (SqliteDataReader headReader = await head.ExecuteReaderAsync(ct))
            if (await headReader.ReadAsync(ct) && (headReader.IsDBNull(1) || headReader.GetInt32(0) != headReader.GetInt32(1)))
                throw new InvalidDataException("Hypothesis head does not identify its latest immutable revision.");
        await using SqliteCommand command = Command(connection, transaction, """
            SELECT r.content_json,r.snapshot_sha256,r.revision,h.name,h.head_revision,
            (SELECT MAX(revision) FROM hypothesis_revisions WHERE hypothesis_id=h.hypothesis_id)
            FROM hypothesis_revisions r
            JOIN hypotheses h ON h.hypothesis_id=r.hypothesis_id WHERE h.hypothesis_id=$id
            AND h.scope_key=$scope AND r.revision=COALESCE($revision,h.head_revision);
            """, ("$id", id.ToString("D")), ("$scope", scope), ("$revision", revision));
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;
        HypothesisRevision value = HypothesisIntegrity.ReadRevision(reader.GetString(0), reader.GetString(1));
        if (value.HypothesisId != id || value.Revision != reader.GetInt32(2) ||
            HypothesisValidation.ScopeKey(value.Scope) != scope || value.Name != reader.GetString(3) ||
            reader.GetInt32(4) != reader.GetInt32(5))
            throw new InvalidDataException("Hypothesis revision identity differs from its index.");
        return value;
    }

    private static async Task<HypothesisChallenge?> ReadChallengeAsync(
        SqliteConnection connection, SqliteTransaction transaction, HypothesisReference parent, Guid id, int? version, CancellationToken ct)
    {
        await using (SqliteCommand head = Command(connection, transaction, """
            SELECT c.head_version,(SELECT MAX(version) FROM hypothesis_challenge_versions WHERE challenge_id=c.challenge_id)
            FROM hypothesis_challenges c WHERE c.challenge_id=$id AND c.hypothesis_id=$parent AND c.revision=$revision;
            """, ("$id", id.ToString("D")), ("$parent", parent.HypothesisId.ToString("D")), ("$revision", parent.Revision)))
        await using (SqliteDataReader headReader = await head.ExecuteReaderAsync(ct))
            if (await headReader.ReadAsync(ct) && (headReader.IsDBNull(1) || headReader.GetInt32(0) != headReader.GetInt32(1)))
                throw new InvalidDataException("Challenge head does not identify its latest immutable version.");
        await using SqliteCommand command = Command(connection, transaction, """
            SELECT v.content_json,v.sha256,v.version,c.head_version,
            (SELECT MAX(version) FROM hypothesis_challenge_versions WHERE challenge_id=c.challenge_id)
            FROM hypothesis_challenge_versions v
            JOIN hypothesis_challenges c ON c.challenge_id=v.challenge_id
            WHERE c.challenge_id=$id AND c.hypothesis_id=$parent AND c.revision=$revision
            AND v.version=COALESCE($version,c.head_version);
            """, ("$id", id.ToString("D")), ("$parent", parent.HypothesisId.ToString("D")), ("$revision", parent.Revision), ("$version", version));
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;
        HypothesisChallenge value = HypothesisIntegrity.ReadChallenge(reader.GetString(0), reader.GetString(1));
        if (value.ChallengeId != id || value.Version != reader.GetInt32(2) || value.Hypothesis != parent ||
            reader.GetInt32(3) != reader.GetInt32(4))
            throw new InvalidDataException("Challenge identity differs from its index.");
        return value;
    }

    private static async Task<(Guid Id, int Version)?> ReplayAsync(
        SqliteConnection connection, SqliteTransaction transaction, string operation, string key, string hash, CancellationToken ct)
    {
        await using SqliteCommand command = Command(connection, transaction, """
            SELECT request_sha256,entity_id,version FROM hypothesis_mutations WHERE operation=$operation AND mutation_key=$key;
            """, ("$operation", operation), ("$key", key));
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;
        if (reader.GetString(0) != hash) throw HypothesisValidation.Conflict("Idempotency-Key was already used for different content.");
        return (Guid.Parse(reader.GetString(1)), reader.GetInt32(2));
    }

    private static Task<int> ReceiptAsync(SqliteConnection connection, SqliteTransaction transaction,
        string operation, string key, string hash, Guid id, int version, CancellationToken ct) =>
        ExecuteAsync(connection, transaction, """
            INSERT INTO hypothesis_mutations(operation,mutation_key,request_sha256,entity_id,version)
            VALUES($operation,$key,$hash,$id,$version);
            """, ct, ("$operation", operation), ("$key", key), ("$hash", hash), ("$id", id.ToString("D")), ("$version", version));

    private async Task<SqliteConnection> OpenAsync(CancellationToken ct)
    {
        var connection = new SqliteConnection(connectionString);
        try
        {
            await connection.OpenAsync(ct);
            await ExecuteAsync(connection, null, "PRAGMA foreign_keys=ON; PRAGMA busy_timeout=10000;", ct);
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    private static SqliteCommand Command(SqliteConnection connection, SqliteTransaction? transaction,
        string sql, params (string Name, object? Value)[] parameters)
    {
        SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        foreach (var parameter in parameters) command.Parameters.AddWithValue(parameter.Name, parameter.Value ?? DBNull.Value);
        return command;
    }

    private static async Task<int> ExecuteAsync(SqliteConnection connection, SqliteTransaction? transaction,
        string sql, CancellationToken ct, params (string Name, object? Value)[] parameters)
    {
        await using SqliteCommand command = Command(connection, transaction, sql, parameters);
        return await command.ExecuteNonQueryAsync(ct);
    }
}
