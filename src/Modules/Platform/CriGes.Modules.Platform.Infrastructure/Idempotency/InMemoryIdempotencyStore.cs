using CriGes.Modules.Platform.Application.Idempotency;

namespace CriGes.Modules.Platform.Infrastructure.Idempotency;

public sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly object syncRoot = new();
    private readonly Dictionary<string, Record> records = new(StringComparer.Ordinal);

    public Task<IdempotencyReservation> ReserveAsync(
        string key,
        string requestHash,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (syncRoot)
        {
            if (!records.TryGetValue(key, out var existing))
            {
                records.Add(key, new Record(requestHash, null, null));
                return Task.FromResult(new IdempotencyReservation(true, null, null));
            }

            if (!string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
            {
                return Task.FromResult(new IdempotencyReservation(false, null, IdempotencyErrors.KeyReusedWithDifferentRequest));
            }

            if (existing.StatusCode is null || existing.ResponseJson is null)
            {
                return Task.FromResult(new IdempotencyReservation(false, null, IdempotencyErrors.RequestInProgress));
            }

            return Task.FromResult(new IdempotencyReservation(
                false,
                new IdempotencyReplay(existing.StatusCode.Value, existing.ResponseJson),
                null));
        }
    }

    public Task CompleteAsync(
        string key,
        int statusCode,
        string responseJson,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (syncRoot)
        {
            var existing = records[key];
            records[key] = existing with { StatusCode = statusCode, ResponseJson = responseJson };
        }

        return Task.CompletedTask;
    }

    private sealed record Record(string RequestHash, int? StatusCode, string? ResponseJson);
}
