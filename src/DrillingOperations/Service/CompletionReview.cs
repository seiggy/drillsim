namespace DrillingOperations;

public sealed record CompletionReview(
    Guid RunId,
    Guid ScenarioId,
    string Status,
    string OpeningsHash,
    IReadOnlyList<CompletionReviewOpening> Openings)
{
    public const string ReviewedHashHeaderName = "X-DrillSim-Reviewed-Openings-Hash";

    public static CompletionReview FromDesign(RunResponse run, CompletionDesign design)
    {
        if (!RequestValidation.IsCanonicalGuid(run.RunId, out Guid runId) ||
            !RequestValidation.IsCanonicalGuid(run.ScenarioId, out Guid scenarioId) ||
            design.RunId != run.RunId || design.ScenarioId != run.ScenarioId ||
            design.Status is not ("Draft" or "Approved"))
            throw new PersistenceIntegrityException("Completion review identity or status mismatch.");
        return new(runId, scenarioId, design.Status, design.OpeningsHash,
            design.Openings.Select(opening => new CompletionReviewOpening(
                opening.ReservoirName, opening.Type, opening.TopMdM, opening.BaseMdM,
                opening.WellboreRadiusM, opening.Skin, opening.Efficiency, opening.UncertaintyM)).ToArray());
    }

    public static bool IsValidHash(string? hash) =>
        hash is { Length: 64 } && hash.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f');
}

public sealed record CompletionReviewOpening(
    string ReservoirName,
    string Type,
    double TopMdM,
    double BaseMdM,
    double WellboreRadiusM,
    double Skin,
    double Efficiency,
    double UncertaintyM);
