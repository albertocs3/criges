using CriGes.SharedKernel;

namespace CriGes.Modules.Platform.Application.Idempotency;

public static class IdempotencyErrors
{
    public static readonly AppError MissingKey = new(
        "IDEMPOTENCY.KEY_REQUIRED",
        "The Idempotency-Key header is required.");

    public static readonly AppError KeyReusedWithDifferentRequest = new(
        "IDEMPOTENCY.KEY_REUSED_WITH_DIFFERENT_REQUEST",
        "The idempotency key was already used with a different request.");

    public static readonly AppError RequestInProgress = new(
        "IDEMPOTENCY.REQUEST_IN_PROGRESS",
        "A request with the same idempotency key is still being processed.");
}
