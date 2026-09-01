namespace OSDC.Drilling.EarthMagneticField.Model;

public sealed class UsageStatisticsEarthMagneticField
{
    private long restEvaluations_;
    private long mcpEvaluations_;
    private long failedEvaluations_;
    private long samplesEvaluated_;
    private long modelInfoRequests_;
    private long statisticsRequests_;

    public DateTimeOffset StartedAt { get; } = DateTimeOffset.UtcNow;
    public string Scope => "process-replica";
    public long RestEvaluations => Interlocked.Read(ref restEvaluations_);
    public long MCPEvaluations => Interlocked.Read(ref mcpEvaluations_);
    public long FailedEvaluations => Interlocked.Read(ref failedEvaluations_);
    public long SamplesEvaluated => Interlocked.Read(ref samplesEvaluated_);
    public long ModelInfoRequests => Interlocked.Read(ref modelInfoRequests_);
    public long StatisticsRequests => Interlocked.Read(ref statisticsRequests_);

    public void IncrementEvaluation(bool mcp, int samples)
    {
        if (mcp) Interlocked.Increment(ref mcpEvaluations_);
        else Interlocked.Increment(ref restEvaluations_);
        Interlocked.Add(ref samplesEvaluated_, Math.Max(samples, 0));
    }

    public void IncrementFailedEvaluation() => Interlocked.Increment(ref failedEvaluations_);
    public void IncrementModelInfo() => Interlocked.Increment(ref modelInfoRequests_);
    public void IncrementStatistics() => Interlocked.Increment(ref statisticsRequests_);
}
