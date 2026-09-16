using ReservoirSimulation.Contracts;

namespace ReservoirSimulation.Domain;

internal static class WorldSetupRequestValidator
{
    internal static Guid ParseFieldId(string? value)
    {
        if (WorldSetupFieldIdJsonConverter.TryParseCanonical(value, out Guid fieldId))
            return fieldId;
        throw new ReservoirValidationException(new Dictionary<string, string[]>
        {
            ["fieldId"] = ["Field ID must be a canonical lowercase, hyphenated GUID."]
        });
    }

    internal static void ValidateScope(Guid fieldId, string? reservoirName)
    {
        var errors = new ValidationErrors();
        ValidateScope(fieldId, reservoirName, errors);
        errors.ThrowIfAny();
    }

    internal static void Validate(WorldSetupRequest? request)
    {
        var errors = new ValidationErrors();
        if (request is null)
        {
            errors.Add("request", "A request body is required.");
            errors.ThrowIfAny();
            return;
        }

        ValidateScope(request.FieldId, request.ReservoirName, errors);
        if (request.ProfileId is not { Length: 68 } ||
            !request.ProfileId.StartsWith("rsp_", StringComparison.Ordinal) ||
            !request.ProfileId.AsSpan(4).ContainsOnlyLowerHex())
            errors.Add("profileId", "Select a profile ID issued by the setup profile catalog.");
        if (request.Resolution is not ("Preview" or "Standard"))
            errors.Add("resolution", "Resolution must be exactly Preview or Standard.");
        if (request.RealizationSeed < 0)
            errors.Add("realizationSeed", "Realization seed must be between 0 and 2147483647.");
        errors.ThrowIfAny();
    }

    private static void ValidateScope(Guid fieldId, string? reservoirName, ValidationErrors errors)
    {
        if (fieldId == Guid.Empty)
            errors.Add("fieldId", "Field ID must not be empty.");
        if (string.IsNullOrWhiteSpace(reservoirName))
            errors.Add("reservoirName", "Reservoir name is required.");
        else if (reservoirName.Length > 200)
            errors.Add("reservoirName", "Reservoir name must not exceed 200 characters.");
    }

    private static bool ContainsOnlyLowerHex(this ReadOnlySpan<char> value)
    {
        foreach (char character in value)
            if (character is not (>= '0' and <= '9' or >= 'a' and <= 'f'))
                return false;
        return true;
    }
}
