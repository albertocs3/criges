using CriGes.SharedKernel;

namespace CriGes.Modules.Platform.Application.Idempotency;

public sealed record IdempotencyReservation(
    bool ShouldExecute,
    IdempotencyReplay? Replay,
    AppError? Error);
