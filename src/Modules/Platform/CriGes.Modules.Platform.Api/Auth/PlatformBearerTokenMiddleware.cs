using CriGes.Application.Abstractions;
using CriGes.Modules.Platform.Application.Auth;
using Microsoft.AspNetCore.Http;

namespace CriGes.Modules.Platform.Api.Auth;

public sealed class PlatformBearerTokenMiddleware(RequestDelegate next)
{
    private static readonly TimeSpan IdleLifetime = TimeSpan.FromHours(5);

    public async Task InvokeAsync(
        HttpContext httpContext,
        IAuthSessionContext sessionContext,
        IAuthSessionStore sessionStore,
        IClock clock)
    {
        var token = ResolveBearerToken(httpContext);
        if (token is not null)
        {
            var now = clock.UtcNow;
            var session = await sessionStore
                .FindActiveSessionByAccessTokenAsync(token, now, httpContext.RequestAborted)
                .ConfigureAwait(false);

            if (session is null || session.UserStatus != 1 || session.RoleStatus != 1)
            {
                sessionContext.HasInvalidBearerToken = true;
            }
            else
            {
                var idleExpiresAtUtc = now.Add(IdleLifetime);
                await sessionStore
                    .TouchSessionAsync(session.SessionId, idleExpiresAtUtc, now, httpContext.RequestAborted)
                    .ConfigureAwait(false);

                sessionContext.Session = session with { IdleExpiresAtUtc = idleExpiresAtUtc };
            }
        }

        await next(httpContext).ConfigureAwait(false);
    }

    private static string? ResolveBearerToken(HttpContext httpContext)
    {
        var header = httpContext.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(header) ||
            !header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var token = header["Bearer ".Length..].Trim();
        return string.IsNullOrWhiteSpace(token) ? null : token;
    }
}
