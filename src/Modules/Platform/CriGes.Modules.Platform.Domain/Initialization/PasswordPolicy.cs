using CriGes.SharedKernel;

namespace CriGes.Modules.Platform.Domain.Initialization;

public static class PasswordPolicy
{
    public static Result ValidateInitialPassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 12)
        {
            return Result.Failure(PlatformErrors.PasswordPolicyFailed);
        }

        var hasUpper = password.Any(char.IsUpper);
        var hasLower = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSymbol = password.Any(character => !char.IsLetterOrDigit(character));

        return hasUpper && hasLower && hasDigit && hasSymbol
            ? Result.Success()
            : Result.Failure(PlatformErrors.PasswordPolicyFailed);
    }
}
