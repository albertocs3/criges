namespace CriGes.Modules.Platform.Application.Idempotency;

public interface IIdempotencyStore
{
    Task<IdempotencyReservation> ReserveAsync(
        string key,
        string requestHash,
        DateTimeOffset now,
        CancellationToken cancellationToken);

    Task CompleteAsync(
        string key,
        int statusCode,
        string responseJson,
        DateTimeOffset now,
        CancellationToken cancellationToken);
}
