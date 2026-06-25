using CriGes.Modules.Platform.Contracts.Auth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CriGes.Modules.Platform.Api.Auth;

public static class PlatformPermissionEndpoints
{
    public static IEndpointRouteBuilder MapPlatformPermissionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/platform");

        group.MapGet("/permissions", GetPermissions)
            .RequirePermission(PlatformPermissionNames.ManageRoles)
            .WithName("GetPlatformPermissions");

        return endpoints;
    }

    private static IResult GetPermissions()
    {
        return Results.Ok(PlatformPermissionDescriptions.All);
    }
}
