using CriGes.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CriGes.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCriGesInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IIdGenerator, GuidIdGenerator>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

        return services;
    }
}
