using CriGes.SharedKernel;

namespace CriGes.Modules.Platform.Application.Administration;

public static class AdministrationErrors
{
    public static readonly AppError ValidationFailed = new(
        "PLATFORM.VALIDATION_FAILED",
        "The request is invalid.");

    public static readonly AppError RoleNotFound = new(
        "SECURITY.ROLE_NOT_FOUND",
        "The selected role does not exist.");

    public static readonly AppError RoleInactive = new(
        "SECURITY.ROLE_INACTIVE",
        "The selected role is inactive.");

    public static readonly AppError UnknownPermission = new(
        "SECURITY.UNKNOWN_PERMISSION",
        "One or more permissions are not recognized.");

    public static readonly AppError UserNameAlreadyReserved = new(
        "SECURITY.USERNAME_ALREADY_RESERVED",
        "The user name is already reserved.");
}
