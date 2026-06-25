using CriGes.Application.Abstractions;
using CriGes.Modules.Platform.Api.Errors;
using CriGes.Modules.Platform.Application.Auth;
using CriGes.Modules.Platform.Contracts.Auth;
using CriGes.Modules.Platform.Domain.Initialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CriGes.Modules.Platform.Api.Auth;

public static class PlatformDevelopmentAuthEndpoints
{
    public static IEndpointRouteBuilder MapPlatformDevelopmentAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/development/platform/auth");

        group.MapPost("/active-sessions/close", CloseActiveSessionsAsync)
            .WithName("CloseDevelopmentActivePlatformSessions");

        return endpoints;
    }

    private static async Task<IResult> CloseActiveSessionsAsync(
        CloseActiveSessionsRequest request,
        IAuthSessionStore sessionStore,
        IClock clock,
        ICorrelationContext correlationContext,
        CancellationToken cancellationToken)
    {
        var userName = UserName.Create(request.UserName);
        if (userName.IsFailure)
        {
            return ApiProblemResults.FromError(AuthErrors.ValidationFailed, correlationContext);
        }

        var closedSessions = await sessionStore
            .CloseActiveSessionsForUserAsync(userName.Value.NormalizedValue, clock.UtcNow, cancellationToken)
            .ConfigureAwait(false);

        return Results.Ok(new CloseActiveSessionsResponse(closedSessions));
    }
}
