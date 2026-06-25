using CriGes.Application.Abstractions;
using CriGes.Modules.Platform.Api.Errors;
using CriGes.Modules.Platform.Application.Auth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CriGes.Modules.Platform.Api.Auth;

public static class PlatformAuthorizationEndpointFilter
{
    public static RouteHandlerBuilder RequirePermission(this RouteHandlerBuilder builder, string permission)
    {
        return builder.AddEndpointFilter(async (context, next) =>
        {
            var sessionContext = context.HttpContext.RequestServices.GetRequiredService<IAuthSessionContext>();
            var correlationContext = context.HttpContext.RequestServices.GetRequiredService<ICorrelationContext>();

            if (sessionContext.HasInvalidBearerToken || sessionContext.Session is null)
            {
                return ApiProblemResults.FromError(AuthErrors.InvalidToken, correlationContext);
            }

            if (!sessionContext.Session.Permissions.Contains(permission, StringComparer.Ordinal))
            {
                return ApiProblemResults.FromError(AuthErrors.PermissionDenied, correlationContext);
            }

            return await next(context).ConfigureAwait(false);
        });
    }
}
