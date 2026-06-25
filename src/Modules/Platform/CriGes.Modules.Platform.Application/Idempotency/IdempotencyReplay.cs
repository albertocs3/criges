namespace CriGes.Modules.Platform.Application.Idempotency;

public sealed record IdempotencyReplay(int StatusCode, string ResponseJson);
