using CriGes.SharedKernel;

namespace CriGes.Modules.Platform.Domain.Initialization;

public sealed record CompanyAddress(
    string Line,
    string PostalCode,
    string City,
    string Region,
    string CountryCode)
{
    public static Result<CompanyAddress> Create(
        string? line,
        string? postalCode,
        string? city,
        string? region,
        string? countryCode)
    {
        if (string.IsNullOrWhiteSpace(line) ||
            string.IsNullOrWhiteSpace(postalCode) ||
            string.IsNullOrWhiteSpace(city) ||
            string.IsNullOrWhiteSpace(region) ||
            string.IsNullOrWhiteSpace(countryCode))
        {
            return Result.Failure<CompanyAddress>(PlatformErrors.ValidationFailed);
        }

        return Result.Success(new CompanyAddress(
            line.Trim(),
            postalCode.Trim(),
            city.Trim(),
            region.Trim(),
            countryCode.Trim().ToUpperInvariant()));
    }
}
