using CriGes.Modules.Platform.Application.Administration;
using CriGes.Modules.Platform.Application.Auth;
using CriGes.Modules.Platform.Application.Initialization;
using Microsoft.Extensions.DependencyInjection;

namespace CriGes.Modules.Platform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformApplication(this IServiceCollection services)
    {
        services.AddScoped<InitializePlatformHandler>();
        services.AddScoped<GetInstallationStatusHandler>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<GetCurrentUserHandler>();
        services.AddScoped<RefreshSessionHandler>();
        services.AddScoped<LogoutHandler>();
        services.AddScoped<ListRolesHandler>();
        services.AddScoped<ListUsersHandler>();
        services.AddScoped<ListAuditEventsHandler>();
        services.AddScoped<GetRolePermissionsHandler>();
        services.AddScoped<UpdateRolePermissionsHandler>();
        services.AddScoped<CreateUserHandler>();
        services.AddScoped<AuthSessionContext>();
        services.AddScoped<IAuthSessionContext>(provider => provider.GetRequiredService<AuthSessionContext>());

        return services;
    }
}
