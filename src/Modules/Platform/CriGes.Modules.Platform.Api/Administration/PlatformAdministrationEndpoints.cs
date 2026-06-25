using CriGes.Application.Abstractions;
using CriGes.Modules.Platform.Api.Auth;
using CriGes.Modules.Platform.Api.Errors;
using CriGes.Modules.Platform.Application.Administration;
using CriGes.Modules.Platform.Application.Auth;
using CriGes.Modules.Platform.Contracts.Administration;
using CriGes.Modules.Platform.Contracts.Auth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CriGes.Modules.Platform.Api.Administration;

public static class PlatformAdministrationEndpoints
{
    public static IEndpointRouteBuilder MapPlatformAdministrationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/platform");

        group.MapGet("/roles", ListRolesAsync)
            .RequirePermission(PlatformPermissionNames.ManageRoles)
            .WithName("ListPlatformRoles");

        group.MapGet("/roles/{roleId:guid}/permissions", GetRolePermissionsAsync)
            .RequirePermission(PlatformPermissionNames.ManageRoles)
            .WithName("GetPlatformRolePermissions");

        group.MapPut("/roles/{roleId:guid}/permissions", UpdateRolePermissionsAsync)
            .RequirePermission(PlatformPermissionNames.ManageRoles)
            .WithName("UpdatePlatformRolePermissions");

        group.MapGet("/users", ListUsersAsync)
            .RequirePermission(PlatformPermissionNames.ManageUsers)
            .WithName("ListPlatformUsers");

        group.MapGet("/audit-events", ListAuditEventsAsync)
            .RequirePermission(PlatformPermissionNames.ViewAudit)
            .WithName("ListPlatformAuditEvents");

        group.MapPost("/users", CreateUserAsync)
            .RequirePermission(PlatformPermissionNames.ManageUsers)
            .WithName("CreatePlatformUser");

        return endpoints;
    }

    private static async Task<IResult> ListRolesAsync(
        ListRolesHandler handler,
        CancellationToken cancellationToken)
    {
        var roles = await handler.HandleAsync(cancellationToken).ConfigureAwait(false);
        return Results.Ok(roles.Select(ToResponse));
    }

    private static async Task<IResult> GetRolePermissionsAsync(
        Guid roleId,
        GetRolePermissionsHandler handler,
        ICorrelationContext correlationContext,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(roleId, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return ApiProblemResults.FromError(result.Error, correlationContext);
        }

        return Results.Ok(new RolePermissionsResponse(roleId, result.Value));
    }

    private static async Task<IResult> UpdateRolePermissionsAsync(
        Guid roleId,
        UpdateRolePermissionsRequest request,
        UpdateRolePermissionsHandler handler,
        IAuthSessionContext sessionContext,
        ICorrelationContext correlationContext,
        CancellationToken cancellationToken)
    {
        var result = await handler
            .HandleAsync(
                new UpdateRolePermissionsCommand(roleId, request.Permissions),
                sessionContext.Session?.UserId,
                cancellationToken)
            .ConfigureAwait(false);

        if (result.IsFailure)
        {
            return ApiProblemResults.FromError(result.Error, correlationContext);
        }

        return Results.Ok(new RolePermissionsResponse(roleId, result.Value));
    }

    private static async Task<IResult> ListUsersAsync(
        ListUsersHandler handler,
        CancellationToken cancellationToken)
    {
        var users = await handler.HandleAsync(cancellationToken).ConfigureAwait(false);
        return Results.Ok(users.Select(ToResponse));
    }

    private static async Task<IResult> ListAuditEventsAsync(
        int? take,
        ListAuditEventsHandler handler,
        CancellationToken cancellationToken)
    {
        var events = await handler.HandleAsync(take ?? 50, cancellationToken).ConfigureAwait(false);
        return Results.Ok(events.Select(ToResponse));
    }

    private static async Task<IResult> CreateUserAsync(
        CreateUserRequest request,
        CreateUserHandler handler,
        IAuthSessionContext sessionContext,
        ICorrelationContext correlationContext,
        CancellationToken cancellationToken)
    {
        var result = await handler
            .HandleAsync(
                new CreateUserCommand(
                    request.FullName,
                    request.UserName,
                    request.Phone,
                    request.RoleId,
                    request.Password),
                sessionContext.Session?.UserId,
                cancellationToken)
            .ConfigureAwait(false);

        if (result.IsFailure)
        {
            return ApiProblemResults.FromError(result.Error, correlationContext);
        }

        var response = ToResponse(result.Value);
        return Results.Created($"/api/v1/platform/users/{response.Id:D}", response);
    }

    private static RoleSummaryResponse ToResponse(RoleSummary role)
    {
        return new RoleSummaryResponse(
            role.Id,
            role.Name,
            role.RoleType == 1 ? "base" : "custom",
            role.Status == 1 ? "active" : "inactive",
            role.Permissions);
    }

    private static UserSummaryResponse ToResponse(UserSummary user)
    {
        return new UserSummaryResponse(
            user.Id,
            user.FullName,
            user.UserName,
            user.Phone,
            new RoleReferenceResponse(user.RoleId, user.RoleName),
            user.Status == 1 ? "active" : user.Status == 2 ? "blocked" : "inactive",
            ToDateTimeOffset(user.LastSuccessfulLoginUtc),
            ToDateTimeOffset(user.BlockedUntilUtc));
    }

    private static AuditEventResponse ToResponse(AuditEventSummary audit)
    {
        return new AuditEventResponse(
            audit.Id,
            new DateTimeOffset(DateTime.SpecifyKind(audit.OccurredAtUtc, DateTimeKind.Utc)),
            audit.ActorUserId,
            audit.ActorDisplayName,
            audit.Module,
            audit.Action,
            audit.EntityType,
            audit.EntityId,
            audit.Description,
            audit.CorrelationId,
            audit.Result == 1 ? "success" : "failure");
    }

    private static DateTimeOffset? ToDateTimeOffset(DateTime? value)
    {
        return value is null ? null : new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc));
    }
}
