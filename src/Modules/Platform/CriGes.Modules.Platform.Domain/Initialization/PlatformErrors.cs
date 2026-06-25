using CriGes.SharedKernel;

namespace CriGes.Modules.Platform.Domain.Initialization;

public static class PlatformErrors
{
    public static readonly AppError AlreadyInitialized = new(
        "PLATFORM.ALREADY_INITIALIZED",
        "The platform has already been initialized.");

    public static readonly AppError InitializationFailed = new(
        "PLATFORM.INITIALIZATION_FAILED",
        "The platform could not be initialized.");

    public static readonly AppError InvalidTaxId = new(
        "PLATFORM.INVALID_TAX_ID",
        "The company tax id is invalid.");

    public static readonly AppError UserNameAlreadyReserved = new(
        "SECURITY.USERNAME_ALREADY_RESERVED",
        "The user name is already reserved.");

    public static readonly AppError PasswordPolicyFailed = new(
        "SECURITY.PASSWORD_POLICY_FAILED",
        "The password does not satisfy the initial password policy.");

    public static readonly AppError ValidationFailed = new(
        "PLATFORM.VALIDATION_FAILED",
        "The initialization request is invalid.");
}
