using CriGes.SharedKernel;

namespace CriGes.Modules.Platform.Domain.Initialization;

public sealed record UserName
{
    private UserName(string value, string normalizedValue)
    {
        Value = value;
        NormalizedValue = normalizedValue;
    }

    public string Value { get; }

    public string NormalizedValue { get; }

    public static Result<UserName> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<UserName>(PlatformErrors.ValidationFailed);
        }

        var trimmed = value.Trim();
        if (trimmed.Length is < 3 or > 100 || trimmed.Contains(' ', StringComparison.Ordinal))
        {
            return Result.Failure<UserName>(PlatformErrors.ValidationFailed);
        }

        return Result.Success(new UserName(trimmed, trimmed.ToUpperInvariant()));
    }
}
