using CriGes.Application.Abstractions;
using CriGes.Modules.Platform.Application.Initialization;

namespace CriGes.Modules.Platform.Application.Tests.Initialization;

public sealed class InitializePlatformHandlerTests
{
    [Fact]
    public async Task HandleAsyncInitializesNewPlatform()
    {
        var store = new FakePlatformInitializationStore();
        var handler = new InitializePlatformHandler(
            store,
            new FixedClock(),
            new SequentialIdGenerator(),
            new FakePasswordHasher());

        var result = await handler.HandleAsync(CreateValidCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("initialized", result.Value.Status);
        Assert.NotNull(store.Snapshot);
        Assert.Equal("PBKDF2.TEST", store.Snapshot.Administrator.PasswordHash);
        Assert.DoesNotContain("StrongPassword1!", store.Snapshot.AuditEvent.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsyncRejectsSecondInitialization()
    {
        var store = new FakePlatformInitializationStore { IsInitialized = true };
        var handler = new InitializePlatformHandler(
            store,
            new FixedClock(),
            new SequentialIdGenerator(),
            new FakePasswordHasher());

        var result = await handler.HandleAsync(CreateValidCommand(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("PLATFORM.ALREADY_INITIALIZED", result.Error.Code);
    }

    private static InitializePlatformCommand CreateValidCommand()
    {
        return new InitializePlatformCommand(
            new CompanyInput(
                "Empresa Ejemplo SL",
                "Empresa Ejemplo",
                "B12345678",
                new AddressInput("Calle Mayor 1", "28001", "Madrid", "Madrid", "ES"),
                "+34910000000",
                "administracion@example.com"),
            new AdministratorInput("Administrador", "admin", "StrongPassword1!"));
    }

    private sealed class FakePlatformInitializationStore : IPlatformInitializationStore
    {
        public bool IsInitialized { get; init; }

        public PlatformInitializationSnapshot? Snapshot { get; private set; }

        public Task<InstallationStatusResult> GetStatusAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new InstallationStatusResult("notInitialized", "1.0.0", true));
        }

        public Task<bool> IsInitializedAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(IsInitialized);
        }

        public Task<bool> IsUserNameReservedAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public Task SaveInitializedPlatformAsync(PlatformInitializationSnapshot snapshot, CancellationToken cancellationToken)
        {
            Snapshot = snapshot;
            return Task.CompletedTask;
        }
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow { get; } = DateTimeOffset.Parse(
            "2026-01-01T00:00:00Z",
            System.Globalization.CultureInfo.InvariantCulture);
    }

    private sealed class SequentialIdGenerator : IIdGenerator
    {
        private int next = 1;

        public Guid NewId()
        {
            var bytes = new byte[16];
            BitConverter.GetBytes(next++).CopyTo(bytes, 0);
            return new Guid(bytes);
        }
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            return "PBKDF2.TEST";
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            return true;
        }
    }
}
