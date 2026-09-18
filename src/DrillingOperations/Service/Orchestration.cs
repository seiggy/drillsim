using System.Threading.Channels;

namespace DrillingOperations;

public interface IRunCheckpointExecutor
{
    Task<bool> AdvanceOneAsync(string runId, CancellationToken cancellationToken);
}
public sealed class StoreRunCheckpointExecutor(DrillingOperationsStore store) : IRunCheckpointExecutor
{
    public Task<bool> AdvanceOneAsync(string runId, CancellationToken cancellationToken) => store.AdvanceOneCheckpointAsync(runId, cancellationToken);
}

public sealed class RunOrchestrator(DrillingOperationsStore store, IRunCheckpointExecutor executor, ILogger<RunOrchestrator> logger)
{
    private readonly Channel<bool> _signals = Channel.CreateBounded<bool>(new BoundedChannelOptions(1)
    {
        FullMode = BoundedChannelFullMode.DropWrite, SingleReader = true, SingleWriter = false
    });
    public void Signal() => _signals.Writer.TryWrite(true);

    public async Task ResumeAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<string> runs = await store.GetResumableRunIdsAsync(cancellationToken);
        foreach (string runId in runs)
        {
            try
            {
                while (await executor.AdvanceOneAsync(runId, cancellationToken)) { }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (RunStageFailureException exception)
            {
                logger.LogWarning(exception, "Run stage validation failed for run {RunId}: {DiagnosticCode}.",
                    runId, exception.DiagnosticCode);
                await store.MarkRunFailedAsync(runId, exception.DiagnosticCode, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Checkpoint recovery failed for run {RunId}; continuing with other runs.", runId);
                try { await store.MarkRunFailedAsync(runId, "CheckpointRecoveryFailure", cancellationToken); }
                catch (Exception persistenceException) { logger.LogError(persistenceException, "Could not persist checkpoint recovery failure for run {RunId}.", runId); }
            }
        }
    }
    public async Task WaitAsync(CancellationToken cancellationToken) => await _signals.Reader.ReadAsync(cancellationToken);
}

public sealed class RunWorker(RunOrchestrator orchestrator, ILogger<RunWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await orchestrator.ResumeAllAsync(stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                await orchestrator.WaitAsync(stoppingToken);
                await orchestrator.ResumeAllAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
        catch (Exception exception) { logger.LogCritical(exception, "Drilling run recovery worker stopped unexpectedly."); }
    }
}
