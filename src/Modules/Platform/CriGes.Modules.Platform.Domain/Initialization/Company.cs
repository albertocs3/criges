using CriGes.SharedKernel;

namespace CriGes.Modules.Platform.Domain.Initialization;

public sealed record Company(
    Guid CompanyId,
    string LegalName,
    string? TradeName,
    string TaxId,
    CompanyAddress Address,
    string? Phone,
    string? Email)
{
    public static Result<Company> Create(
        Guid companyId,
        string? legalName,
        string? tradeName,
        string? taxId,
        CompanyAddress address,
        string? phone,
        string? email)
    {
        if (string.IsNullOrWhiteSpace(legalName))
        {
            return Result.Failure<Company>(PlatformErrors.ValidationFailed);
        }

        if (!IsValidSpanishTaxId(taxId))
        {
            return Result.Failure<Company>(PlatformErrors.InvalidTaxId);
        }

        return Result.Success(new Company(
            companyId,
            legalName.Trim(),
            string.IsNullOrWhiteSpace(tradeName) ? null : tradeName.Trim(),
            taxId!.Trim().ToUpperInvariant(),
            address,
            string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
            string.IsNullOrWhiteSpace(email) ? null : email.Trim()));
    }

    private static bool IsValidSpanishTaxId(string? taxId)
    {
        if (string.IsNullOrWhiteSpace(taxId))
        {
            return false;
        }

        var normalized = taxId.Trim().ToUpperInvariant();
        return normalized.Length == 9 && char.IsLetterOrDigit(normalized[0]) && normalized.Skip(1).All(char.IsLetterOrDigit);
    }
}
