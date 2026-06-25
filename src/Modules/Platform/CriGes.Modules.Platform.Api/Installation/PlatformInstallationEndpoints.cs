using System.Security.Cryptography;
using System.Text;
using CriGes.Modules.Platform.Application.Initialization;
using CriGes.Modules.Platform.Application.Idempotency;
using CriGes.Modules.Platform.Api.Errors;
using CriGes.Modules.Platform.Contracts.Installation;
using CriGes.Application.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Text.Json;

namespace CriGes.Modules.Platform.Api.Installation;

public static class PlatformInstallationEndpoints
{
    public static IEndpointRouteBuilder MapPlatformEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/platform/installation");

        group.MapGet("/", GetInstallationStatusAsync)
            .WithName("GetPlatformInstallationStatus");

        group.MapPost("/initialize", InitializePlatformAsync)
            .RequireRateLimiting("platform-initialization")
            .WithName("InitializePlatform");

        return endpoints;
    }

    private static async Task<IResult> GetInstallationStatusAsync(
        GetInstallationStatusHandler handler,
        CancellationToken cancellationToken)
    {
        var status = await handler.HandleAsync(cancellationToken).ConfigureAwait(false);
        return Results.Ok(new InstallationStatusResponse(
            status.Status,
            status.ProductVersion,
            status.RequiresInitialization));
    }

    private static async Task<IResult> InitializePlatformAsync(
        HttpContext httpContext,
        InitializePlatformRequest request,
        InitializePlatformHandler handler,
        IIdempotencyStore idempotencyStore,
        IClock clock,
        ICorrelationContext correlationContext,
        CancellationToken cancellationToken)
    {
        if (!httpContext.Request.Headers.TryGetValue("Idempotency-Key", out var keyValues) ||
            string.IsNullOrWhiteSpace(keyValues.FirstOrDefault()))
        {
            return ApiProblemResults.FromError(IdempotencyErrors.MissingKey, correlationContext);
        }

        var idempotencyKey = keyValues.FirstOrDefault()!.Trim();
        var requestHash = ComputeRequestHash(request);
        var reservation = await idempotencyStore
            .ReserveAsync(idempotencyKey, requestHash, clock.UtcNow, cancellationToken)
            .ConfigureAwait(false);

        if (reservation.Error is not null)
        {
            return ApiProblemResults.FromError(reservation.Error, correlationContext);
        }

        if (reservation.Replay is not null)
        {
            var replayed = JsonSerializer.Deserialize<InitializePlatformResponse>(reservation.Replay.ResponseJson);
            return Results.Json(replayed, statusCode: reservation.Replay.StatusCode);
        }

        var command = new InitializePlatformCommand(
            new CompanyInput(
                request.Company.LegalName,
                request.Company.TradeName,
                request.Company.TaxId,
                new AddressInput(
                    request.Company.Address.Line,
                    request.Company.Address.PostalCode,
                    request.Company.Address.City,
                    request.Company.Address.Region,
                    request.Company.Address.CountryCode),
                request.Company.Phone,
                request.Company.Email),
            new AdministratorInput(
                request.Administrator.FullName,
                request.Administrator.UserName,
                request.Administrator.Password));

        var result = await handler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return ApiProblemResults.FromError(result.Error, correlationContext);
        }

        var response = new InitializePlatformResponse(
            result.Value.InstallationId,
            result.Value.Status,
            result.Value.AdministratorUserId,
            result.Value.RequiresRestart);

        await idempotencyStore
            .CompleteAsync(
                idempotencyKey,
                StatusCodes.Status201Created,
                JsonSerializer.Serialize(response),
                clock.UtcNow,
                cancellationToken)
            .ConfigureAwait(false);

        return Results.Created("/api/v1/platform/installation", response);
    }

    private static string ComputeRequestHash(InitializePlatformRequest request)
    {
        var json = JsonSerializer.Serialize(request);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToBase64String(hash);
    }
}
