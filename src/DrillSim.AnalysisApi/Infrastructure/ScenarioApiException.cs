namespace DrillSim.AnalysisApi.Infrastructure;

public sealed class ScenarioApiException : Exception
{
    public ScenarioApiException(int statusCode, string title, string message)
        : base(message)
    {
        StatusCode = statusCode;
        Title = title;
    }

    public int StatusCode { get; }
    public string Title { get; }
}
