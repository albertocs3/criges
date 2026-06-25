using CriGes.Api.Correlation;
using CriGes.Api.Middleware;
using CriGes.Application.Abstractions;
using CriGes.Infrastructure;
using CriGes.Modules.Platform.Api;
using CriGes.Modules.Platform.Api.Administration;
using CriGes.Modules.Platform.Api.Auth;
using CriGes.Modules.Platform.Api.Installation;
using CriGes.Modules.Platform.Application;
using CriGes.Modules.Platform.Infrastructure;
using CriGes.Modules.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddScoped<ICorrelationContext, CorrelationContext>();
builder.Services.AddCriGesInfrastructure();
builder.Services.AddPlatformApplication();
builder.Services.AddPlatformInfrastructure(builder.Configuration);
builder.Services.AddPlatformApi();
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("platform-initialization", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<PlatformBearerTokenMiddleware>();
app.UseRateLimiter();
app.MapHealthChecks("/health/live");
app.MapGet("/health/ready", GetReadinessAsync);
app.MapGet("/", () => Results.Ok(new { Name = "CriGes.Api", Status = "running" }));
app.MapPlatformAuthEndpoints();
app.MapPlatformPermissionEndpoints();
app.MapPlatformAdministrationEndpoints();
app.MapPlatformEndpoints();
if (app.Environment.IsDevelopment())
{
    app.MapPlatformDevelopmentAuthEndpoints();
}

app.Run();

static async Task<IResult> GetReadinessAsync(PlatformDbContext dbContext, CancellationToken cancellationToken)
{
    try
    {
        if (!await dbContext.Database.CanConnectAsync(cancellationToken).ConfigureAwait(false))
        {
            return Results.Json(
                new { Status = "databaseUnavailable", Detail = "No se pudo conectar con la base de datos.", PendingMigrations = 0 },
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        var pendingMigrations = await dbContext.Database
            .GetPendingMigrationsAsync(cancellationToken)
            .ConfigureAwait(false);
        var pendingMigrationsCount = pendingMigrations.Count();

        if (pendingMigrationsCount > 0)
        {
            return Results.Json(
                new { Status = "databaseNotMigrated", Detail = "Hay migraciones pendientes.", PendingMigrations = pendingMigrationsCount },
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        return Results.Ok(new { Status = "ready", Detail = (string?)null, PendingMigrations = 0 });
    }
    catch (Exception ex) when (ex is InvalidOperationException or Microsoft.Data.SqlClient.SqlException)
    {
        return Results.Json(
            new { Status = "databaseUnavailable", Detail = ex.Message, PendingMigrations = 0 },
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
}

public partial class Program;
