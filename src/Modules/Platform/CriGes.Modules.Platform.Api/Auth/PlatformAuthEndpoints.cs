using CriGes.Application.Abstractions;
using CriGes.Modules.Platform.Api.Errors;
using CriGes.Modules.Platform.Application.Auth;
using CriGes.Modules.Platform.Contracts.Auth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CriGes.Modules.Platform.Api.Auth;

public static class PlatformAuthEndpoints
{
    public static IEndpointRouteBuilder MapPlatformAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/platform/auth");

        group.MapPost("/login", LoginAsync)
            .WithName("PlatformAuthLogin");
        group.MapPost("/refresh", RefreshAsync)
            .WithName("PlatformAuthRefresh");
        group.MapPost("/logout", LogoutAsync)
            .WithName("PlatformAuthLogout");
        group.MapGet("/me", GetCurrentUser)
            .WithName("PlatformAuthMe");

        return endpoints;
    }

    private static async Task<IResult> LoginAsync(
        HttpContext httpContext,
        LoginRequest request,
        LoginHandler handler,
        ICorrelationContext correlationContext,
        CancellationToken cancellationToken)
    {
        var result = await handler
            .HandleAsync(
                new LoginCommand(request.UserName, request.Password, request.DeviceId, request.ClientVersion),
                httpContext.Connection.RemoteIpAddress?.ToString(),
                httpContext.Request.Headers.UserAgent.FirstOrDefault(),
                cancellationToken)
            .ConfigureAwait(false);

        if (result.IsFailure)
        {
            return ApiProblemResults.FromError(result.Error, correlationContext);
        }

        return ToSessionResponse(result.Value);
    }

    private static async Task<IResult> RefreshAsync(
        RefreshSessionRequest request,
        RefreshSessionHandler handler,
        ICorrelationContext correlationContext,
        CancellationToken cancellationToken)
    {
        var result = await handler
            .HandleAsync(new RefreshSessionCommand(request.SessionId, request.RefreshToken), cancellationToken)
            .ConfigureAwait(false);

        if (result.IsFailure)
        {
            return ApiProblemResults.FromError(result.Error, correlationContext);
        }

        return ToSessionResponse(result.Value);
    }

    private static async Task<IResult> LogoutAsync(
        LogoutHandler handler,
        ICorrelationContext correlationContext,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(cancellationToken).ConfigureAwait(false);
        return result.IsFailure
            ? ApiProblemResults.FromError(result.Error, correlationContext)
            : Results.NoContent();
    }

    private static IResult GetCurrentUser(
        GetCurrentUserHandler handler,
        ICorrelationContext correlationContext)
    {
        var result = handler.Handle();
        if (result.IsFailure)
        {
            return ApiProblemResults.FromError(result.Error, correlationContext);
        }

        var session = result.Value;
        return Results.Ok(new CurrentUserResponse(
            session.UserId,
            session.DisplayName,
            session.UserName,
            new CurrentUserRoleResponse(session.RoleId, session.RoleName),
            session.Permissions,
            new CurrentUserSessionResponse(session.SessionId, session.IdleExpiresAtUtc)));
    }

    private static IResult ToSessionResponse(LoginResult value)
    {
        return Results.Ok(new SessionResponse(
            value.AccessToken,
            value.AccessTokenExpiresAtUtc,
            value.RefreshToken,
            value.RefreshTokenExpiresAtUtc,
            new AuthSessionResponse(
                value.SessionId,
                value.StartedAtUtc,
                value.IdleExpiresAtUtc),
            new AuthUserResponse(
                value.UserId,
                value.DisplayName,
                value.UserName,
                value.Role,
                value.Permissions)));
    }
}
