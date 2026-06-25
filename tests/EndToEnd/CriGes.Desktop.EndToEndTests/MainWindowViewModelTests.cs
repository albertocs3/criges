using CriGes.Desktop.Configuration;
using CriGes.Desktop.Services.Api;
using CriGes.Desktop.ViewModels;
using CriGes.Modules.Platform.Contracts.Administration;
using CriGes.Modules.Platform.Contracts.Auth;
using CriGes.Modules.Platform.Contracts.Installation;

namespace CriGes.Desktop.EndToEndTests;

public sealed class MainWindowViewModelTests
{
    [Fact]
    public async Task CheckApiCommandShowsLoginWhenPlatformIsInitialized()
    {
        var apiClient = new ReadyInstallationApiClient();
        var viewModel = new MainWindowViewModel(
            new DesktopSettings
            {
                Api = new DesktopSettings.ApiSettings { BaseUrl = "http://localhost:5099/" }
            },
            _ => apiClient);

        await viewModel.CheckApiCommand.ExecuteAsync();

        Assert.True(viewModel.IsLoginVisible);
        Assert.False(viewModel.RequiresInitialization);
        Assert.False(viewModel.NeedsAttention);
        Assert.Equal(ApiConnectionState.Ready, viewModel.ConnectionState);
        Assert.Equal("1.0.0", viewModel.ServerVersion);

        Assert.False(viewModel.LoginCommand.CanExecute(null));
        viewModel.LoginUserName = "admin";
        viewModel.LoginPassword = "Password123!";

        Assert.True(viewModel.LoginCommand.CanExecute(null));

        await viewModel.LoginCommand.ExecuteAsync();

        Assert.True(viewModel.IsShellVisible);
        Assert.False(viewModel.IsLoginVisible);
        Assert.Equal("Administrador", viewModel.CurrentUserDisplayName);
        Assert.Equal("Administrador", viewModel.CurrentUserRole);
        Assert.True(viewModel.CanShowPlatformModule);
        Assert.True(viewModel.CanShowOperationsModule);
        Assert.True(viewModel.CanShowDiagnosticsModule);

        await viewModel.OpenPlatformCommand.ExecuteAsync();

        Assert.False(viewModel.IsShellHomeVisible);
        Assert.True(viewModel.IsPlatformViewVisible);
        Assert.NotEmpty(viewModel.PlatformRoles);
        Assert.Contains(viewModel.PlatformUsers, user => user.UserName == "admin");
        Assert.NotEmpty(viewModel.PlatformPermissions);
        Assert.Contains(viewModel.PlatformAuditEvents, audit => audit.Action == "PlatformInitialized");
        var initialUserCount = viewModel.PlatformUsers.Count;

        var viewAuditOption = viewModel.RolePermissionOptions.Single(option => option.Name == PlatformPermissionNames.ViewAudit);
        viewAuditOption.IsGranted = false;

        await viewModel.SaveRolePermissionsCommand.ExecuteAsync();

        Assert.DoesNotContain(PlatformPermissionNames.ViewAudit, apiClient.LastUpdatedRolePermissions);
        Assert.Contains("guardados", viewModel.RolePermissionsMessage, StringComparison.OrdinalIgnoreCase);

        viewModel.NewUserFullName = "Usuario Desktop";
        viewModel.NewUserName = "usuario-desktop";
        viewModel.NewUserPhone = "+34600000000";
        viewModel.NewUserRoleId = viewModel.PlatformRoles[0].Id;
        viewModel.NewUserPassword = "Password123!";
        viewModel.NewUserPasswordConfirmation = "Password123!";

        Assert.True(viewModel.CreateUserCommand.CanExecute(null));

        await viewModel.CreateUserCommand.ExecuteAsync();

        Assert.Equal("usuario-desktop", apiClient.LastCreateUserRequest?.UserName);
        Assert.Equal(initialUserCount + 1, viewModel.PlatformUsers.Count);
        Assert.Contains(viewModel.PlatformUsers, user => user.UserName == "usuario-desktop");
        Assert.Contains(viewModel.PlatformAuditEvents, audit => audit.Action == "UserCreated");
        Assert.Contains("Usuario creado", viewModel.PlatformMessage);

        await viewModel.BackToShellCommand.ExecuteAsync();

        Assert.True(viewModel.IsShellHomeVisible);
        Assert.False(viewModel.IsPlatformViewVisible);

        await viewModel.RefreshSessionCommand.ExecuteAsync();

        Assert.True(viewModel.IsShellVisible);
        Assert.Contains("refrescada", viewModel.StatusText, StringComparison.OrdinalIgnoreCase);

        await viewModel.LogoutCommand.ExecuteAsync();

        Assert.False(viewModel.IsShellVisible);
        Assert.True(viewModel.IsLoginVisible);
    }

    private sealed class ReadyInstallationApiClient : IInstallationApiClient
    {
        private readonly Guid _administratorRoleId = Guid.NewGuid();
        private readonly List<UserSummaryResponse> _users = [];
        private readonly List<AuditEventResponse> _auditEvents = [];
        private IReadOnlyList<string> _administratorPermissions = PlatformPermissionNames.All;

        public CreateUserRequest? LastCreateUserRequest { get; private set; }

        public IReadOnlyList<string> LastUpdatedRolePermissions { get; private set; } = [];

        public ReadyInstallationApiClient()
        {
            _users.Add(new UserSummaryResponse(
                Guid.NewGuid(),
                "Administrador",
                "admin",
                null,
                new RoleReferenceResponse(_administratorRoleId, "Administrador"),
                "active",
                null,
                null));
            _auditEvents.Add(new AuditEventResponse(
                1,
                DateTimeOffset.UtcNow,
                null,
                "Sistema",
                "Platform",
                "PlatformInitialized",
                "Installation",
                Guid.NewGuid().ToString("D"),
                "Platform initialized.",
                Guid.NewGuid(),
                "success"));
        }

        public Task<ApiReadinessResponse> GetReadinessAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ApiReadinessResponse("ready", null, 0));
        }

        public Task<InstallationStatusResponse> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new InstallationStatusResponse("initialized", "1.0.0", RequiresInitialization: false));
        }

        public Task<InitializePlatformResponse> InitializeAsync(
            InitializePlatformRequest request,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<SessionResponse> LoginAsync(
            LoginRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CreateSessionResponse());
        }

        public Task<CurrentUserResponse> GetCurrentUserAsync(
            string accessToken,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new CurrentUserResponse(
                Guid.NewGuid(),
                "Administrador",
                "admin",
                new CurrentUserRoleResponse(Guid.NewGuid(), "Administrador"),
                PlatformPermissionNames.All,
                new CurrentUserSessionResponse(Guid.NewGuid(), DateTimeOffset.UtcNow.AddHours(5))));
        }

        public Task<SessionResponse> RefreshAsync(
            RefreshSessionRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CreateSessionResponse());
        }

        public Task LogoutAsync(
            string accessToken,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<RoleSummaryResponse>> GetRolesAsync(
            string accessToken,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<RoleSummaryResponse> roles =
            [
                new RoleSummaryResponse(_administratorRoleId, "Administrador", "system", "active", _administratorPermissions)
            ];

            return Task.FromResult(roles);
        }

        public Task<IReadOnlyList<PermissionResponse>> GetPermissionsAsync(
            string accessToken,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<PermissionResponse>>(
            [
                new PermissionResponse(PlatformPermissionNames.ManageUsers, "Crear y modificar usuarios"),
                new PermissionResponse(PlatformPermissionNames.ManageRoles, "Gestionar roles y permisos"),
                new PermissionResponse(PlatformPermissionNames.ViewAudit, "Consultar auditoria")
            ]);
        }

        public Task<RolePermissionsResponse> GetRolePermissionsAsync(
            string accessToken,
            Guid roleId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new RolePermissionsResponse(roleId, _administratorPermissions));
        }

        public Task<RolePermissionsResponse> UpdateRolePermissionsAsync(
            string accessToken,
            Guid roleId,
            UpdateRolePermissionsRequest request,
            CancellationToken cancellationToken = default)
        {
            _administratorPermissions = request.Permissions ?? [];
            LastUpdatedRolePermissions = _administratorPermissions;

            return Task.FromResult(new RolePermissionsResponse(roleId, _administratorPermissions));
        }

        public Task<IReadOnlyList<UserSummaryResponse>> GetUsersAsync(
            string accessToken,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<UserSummaryResponse>>(_users.ToArray());
        }

        public Task<IReadOnlyList<AuditEventResponse>> GetAuditEventsAsync(
            string accessToken,
            int take = 50,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<AuditEventResponse>>(_auditEvents.Take(take).ToArray());
        }

        public Task<UserSummaryResponse> CreateUserAsync(
            string accessToken,
            CreateUserRequest request,
            CancellationToken cancellationToken = default)
        {
            LastCreateUserRequest = request;

            var user = new UserSummaryResponse(
                Guid.NewGuid(),
                request.FullName ?? string.Empty,
                request.UserName ?? string.Empty,
                request.Phone,
                new RoleReferenceResponse(request.RoleId, "Administrador"),
                "active",
                null,
                null);

            _users.Add(user);
            _auditEvents.Insert(0, new AuditEventResponse(
                _auditEvents.Count + 1,
                DateTimeOffset.UtcNow,
                Guid.NewGuid(),
                "Administrador",
                "Platform",
                "UserCreated",
                "User",
                user.Id.ToString("D"),
                "User created.",
                Guid.NewGuid(),
                "success"));

            return Task.FromResult(user);
        }

        private static SessionResponse CreateSessionResponse()
        {
            var now = DateTimeOffset.UtcNow;
            return new SessionResponse(
                $"access-token-{Guid.NewGuid():N}",
                now.AddMinutes(15),
                $"refresh-token-{Guid.NewGuid():N}",
                now.AddDays(30),
                new AuthSessionResponse(Guid.NewGuid(), now, now.AddHours(5)),
                new AuthUserResponse(Guid.NewGuid(), "Administrador", "admin", "Administrador", PlatformPermissionNames.All));
        }
    }
}
