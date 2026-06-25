using CriGes.Modules.Platform.Application.Auth;
using CriGes.Modules.Platform.Application.Administration;
using CriGes.Modules.Platform.Application.Idempotency;
using CriGes.Modules.Platform.Application.Initialization;
using CriGes.Modules.Platform.Infrastructure.Auth;
using CriGes.Modules.Platform.Infrastructure.Administration;
using CriGes.Modules.Platform.Infrastructure.Idempotency;
using CriGes.Modules.Platform.Infrastructure.Initialization;
using CriGes.Modules.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CriGes.Modules.Platform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CriGes");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'CriGes' is required.");
        }

        services.AddDbContext<PlatformDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IPlatformInitializationStore, EfPlatformInitializationStore>();
        services.AddScoped<IIdempotencyStore, EfIdempotencyStore>();
        services.AddScoped<IAuthSessionStore, EfAuthSessionStore>();
        services.AddScoped<IPlatformAdministrationStore, EfPlatformAdministrationStore>();
        services.AddSingleton<IAuthSessionTokenGenerator, CryptoAuthSessionTokenGenerator>();

        return services;
    }

    public static IServiceCollection AddPlatformInMemoryInfrastructureForTests(this IServiceCollection services)
    {
        services.AddSingleton<InMemoryPlatformInitializationStore>();
        services.AddSingleton<IPlatformInitializationStore>(provider => provider.GetRequiredService<InMemoryPlatformInitializationStore>());
        services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();
        services.AddSingleton<IAuthSessionStore, InMemoryAuthSessionStore>();
        services.AddSingleton<IPlatformAdministrationStore, InMemoryPlatformAdministrationStore>();
        services.AddSingleton<IAuthSessionTokenGenerator, CryptoAuthSessionTokenGenerator>();

        return services;
    }
}
