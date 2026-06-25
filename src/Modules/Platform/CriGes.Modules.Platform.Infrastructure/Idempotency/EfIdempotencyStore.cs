using CriGes.Modules.Platform.Application.Idempotency;
using CriGes.Modules.Platform.Infrastructure.Persistence;
using CriGes.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CriGes.Modules.Platform.Infrastructure.Idempotency;

public sealed class EfIdempotencyStore(PlatformDbContext dbContext) : IIdempotencyStore
{
    public async Task<IdempotencyReservation> ReserveAsync(
        string key,
        string requestHash,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.HttpIdempotencyRecords
            .SingleOrDefaultAsync(record => record.Key == key, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            dbContext.HttpIdempotencyRecords.Add(new HttpIdempotencyRecordEntity
            {
                Key = key,
                RequestHash = requestHash,
                CreatedAtUtc = now.UtcDateTime
            });
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new IdempotencyReservation(ShouldExecute: true, Replay: null, Error: null);
        }

        if (!string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
        {
            return new IdempotencyReservation(false, null, IdempotencyErrors.KeyReusedWithDifferentRequest);
        }

        if (existing.StatusCode is null || existing.ResponseJson is null)
        {
            return new IdempotencyReservation(false, null, IdempotencyErrors.RequestInProgress);
        }

        return new IdempotencyReservation(
            ShouldExecute: false,
            new IdempotencyReplay(existing.StatusCode.Value, existing.ResponseJson),
            Error: null);
    }

    public async Task CompleteAsync(
        string key,
        int statusCode,
        string responseJson,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.HttpIdempotencyRecords
            .SingleAsync(record => record.Key == key, cancellationToken)
            .ConfigureAwait(false);

        existing.StatusCode = statusCode;
        existing.ResponseJson = responseJson;
        existing.CompletedAtUtc = now.UtcDateTime;

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
