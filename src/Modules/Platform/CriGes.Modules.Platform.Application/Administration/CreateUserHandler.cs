using CriGes.Application.Abstractions;
using CriGes.Modules.Platform.Domain.Initialization;
using CriGes.SharedKernel;

namespace CriGes.Modules.Platform.Application.Administration;

public sealed class CreateUserHandler(
    IPlatformAdministrationStore store,
    IPasswordHasher passwordHasher,
    IIdGenerator idGenerator,
    IClock clock,
    ICorrelationContext correlationContext)
{
    public async Task<Result<UserSummary>> HandleAsync(
        CreateUserCommand command,
        Guid? actorUserId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.FullName) ||
            command.RoleId == Guid.Empty)
        {
            return Result.Failure<UserSummary>(AdministrationErrors.ValidationFailed);
        }

        var userName = UserName.Create(command.UserName);
        if (userName.IsFailure)
        {
            return Result.Failure<UserSummary>(AdministrationErrors.ValidationFailed);
        }

        var password = PasswordPolicy.ValidateInitialPassword(command.Password);
        if (password.IsFailure)
        {
            return Result.Failure<UserSummary>(password.Error);
        }

        var role = await store.FindRoleAsync(command.RoleId, cancellationToken).ConfigureAwait(false);
        if (role is null)
        {
            return Result.Failure<UserSummary>(AdministrationErrors.RoleNotFound);
        }

        if (role.Status != 1)
        {
            return Result.Failure<UserSummary>(AdministrationErrors.RoleInactive);
        }

        if (await store.IsUserNameReservedAsync(userName.Value.NormalizedValue, cancellationToken).ConfigureAwait(false))
        {
            return Result.Failure<UserSummary>(AdministrationErrors.UserNameAlreadyReserved);
        }

        var fullName = command.FullName.Trim();
        if (fullName.Length is < 3 or > 200)
        {
            return Result.Failure<UserSummary>(AdministrationErrors.ValidationFailed);
        }

        var user = new UserCreationData(
            idGenerator.NewId(),
            fullName,
            userName.Value.Value,
            userName.Value.NormalizedValue,
            string.IsNullOrWhiteSpace(command.Phone) ? null : command.Phone.Trim(),
            role.Id,
            passwordHasher.HashPassword(command.Password!),
            clock.UtcNow);

        var created = await store.CreateUserAsync(
                user,
                actorUserId,
                correlationContext.CorrelationId,
                cancellationToken)
            .ConfigureAwait(false);
        return Result.Success(created);
    }
}
