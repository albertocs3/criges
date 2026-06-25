using CriGes.SharedKernel;

namespace CriGes.Modules.Platform.Application.Auth;

public static class AuthErrors
{
    public static readonly AppError InvalidCredentials = new(
        "AUTH.INVALID_CREDENTIALS",
        "Invalid user name or password.");

    public static readonly AppError InvalidToken = new(
        "AUTH.INVALID_TOKEN",
        "The session is not valid.");

    public static readonly AppError AccountDisabled = new(
        "AUTH.ACCOUNT_DISABLED",
        "The account cannot start a session.");

    public static readonly AppError AccountLocked = new(
        "AUTH.ACCOUNT_LOCKED",
        "The account is temporarily locked.");

    public static readonly AppError RoleDisabled = new(
        "AUTH.ROLE_DISABLED",
        "The assigned role cannot start a session.");

    public static readonly AppError PermissionDenied = new(
        "AUTH.PERMISSION_DENIED",
        "The current session does not have permission to perform this action.");

    public static readonly AppError ActiveSessionExists = new(
        "AUTH.ACTIVE_SESSION_EXISTS",
        "An active session already exists for this user.");

    public static readonly AppError PlatformNotInitialized = new(
        "PLATFORM.NOT_INITIALIZED",
        "The platform has not been initialized.");

    public static readonly AppError ValidationFailed = new(
        "AUTH.VALIDATION_FAILED",
        "The login request is invalid.");
}
