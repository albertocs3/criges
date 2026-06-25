using System.Globalization;
using CriGes.Desktop.Converters;
using CriGes.Desktop.Configuration;
using CriGes.Desktop.Services.Api;
using CriGes.Desktop.ViewModels;
using CriGes.Modules.Platform.Contracts.Administration;
using CriGes.Modules.Platform.Contracts.Auth;
using CriGes.Modules.Platform.Contracts.Customers;
using CriGes.Modules.Platform.Contracts.Installation;

namespace CriGes.Desktop.EndToEndTests;

public sealed class MainWindowViewModelTests
{
    [Fact]
    public void LocalDateTimeDisplayConverterFormatsUtcAuditDatesAsLocalDayMonthYear()
    {
        var converter = new LocalDateTimeDisplayConverter();
        var utcDate = new DateTimeOffset(2026, 6, 25, 10, 5, 0, TimeSpan.Zero);

        var result = converter.Convert(utcDate, typeof(string), null!, CultureInfo.InvariantCulture);

        Assert.Equal(utcDate.ToLocalTime().ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture), result);
    }

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

        viewModel.NewRoleName = "Soporte";

        Assert.True(viewModel.CreateRoleCommand.CanExecute(null));

        await viewModel.CreateRoleCommand.ExecuteAsync();

        var supportRole = viewModel.PlatformRoles.Single(role => role.Name == "Soporte");
        Assert.Equal(supportRole.Id, viewModel.SelectedPlatformRoleId);
        Assert.Contains("Rol creado", viewModel.RolePermissionsMessage);
        Assert.Contains(viewModel.PlatformAuditEvents, audit => audit.Action == "RoleCreated");

        var viewAuditOption = viewModel.RolePermissionOptions.Single(option => option.Name == PlatformPermissionNames.ViewAudit);
        viewAuditOption.IsGranted = false;

        await viewModel.SaveRolePermissionsCommand.ExecuteAsync();

        Assert.DoesNotContain(PlatformPermissionNames.ViewAudit, apiClient.LastUpdatedRolePermissions);
        Assert.Contains("guardados", viewModel.RolePermissionsMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(viewModel.PlatformAuditEvents, audit => audit.Action == "RolePermissionsUpdated");

        viewModel.NewUserFullName = "Usuario Desktop";
        viewModel.NewUserName = "usuario-desktop";
        viewModel.NewUserPhone = "+34600000000";
        viewModel.NewUserRoleId = supportRole.Id;
        viewModel.NewUserPassword = "Password123!";
        viewModel.NewUserPasswordConfirmation = "Password123!";

        Assert.True(viewModel.CreateUserCommand.CanExecute(null));

        await viewModel.CreateUserCommand.ExecuteAsync();

        Assert.Equal("usuario-desktop", apiClient.LastCreateUserRequest?.UserName);
        Assert.Equal(initialUserCount + 1, viewModel.PlatformUsers.Count);
        Assert.Contains(viewModel.PlatformUsers, user => user.UserName == "usuario-desktop" && user.Role.Name == "Soporte");
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

    [Fact]
    public async Task LoginClosesDevelopmentActiveSessionAndRetriesOnce()
    {
        var apiClient = new ReadyInstallationApiClient
        {
            FailNextLoginWithActiveSession = true
        };
        var viewModel = new MainWindowViewModel(
            new DesktopSettings
            {
                Api = new DesktopSettings.ApiSettings { BaseUrl = "http://localhost:5099/" }
            },
            _ => apiClient);

        await viewModel.CheckApiCommand.ExecuteAsync();
        viewModel.LoginUserName = "admin";
        viewModel.LoginPassword = "Password123!";

        await viewModel.LoginCommand.ExecuteAsync();

        Assert.Equal(1, apiClient.CloseDevelopmentActiveSessionsCalls);
        Assert.True(viewModel.IsShellVisible);
        Assert.Equal("Administrador", viewModel.CurrentUserDisplayName);
    }

    [Fact]
    public async Task OpenPlatformFailureKeepsShellHomeVisibleAndShowsMessage()
    {
        var apiClient = new ReadyInstallationApiClient
        {
            FailNextGetUsers = true
        };
        var viewModel = new MainWindowViewModel(
            new DesktopSettings
            {
                Api = new DesktopSettings.ApiSettings { BaseUrl = "http://localhost:5099/" }
            },
            _ => apiClient);

        await viewModel.CheckApiCommand.ExecuteAsync();
        viewModel.LoginUserName = "admin";
        viewModel.LoginPassword = "Password123!";
        await viewModel.LoginCommand.ExecuteAsync();

        await viewModel.OpenPlatformCommand.ExecuteAsync();

        Assert.True(viewModel.IsShellHomeVisible);
        Assert.False(viewModel.IsPlatformViewVisible);
        Assert.Contains("No se pudo cargar Plataforma", viewModel.PlatformMessage);
    }

    [Fact]
    public async Task LoginAppliesCurrentUserPermissionsToVisibleShellModules()
    {
        var apiClient = new ReadyInstallationApiClient
        {
            CurrentUserPermissions = [PlatformPermissionNames.UseAttachments]
        };
        var viewModel = new MainWindowViewModel(
            new DesktopSettings
            {
                Api = new DesktopSettings.ApiSettings { BaseUrl = "http://localhost:5099/" }
            },
            _ => apiClient);

        await viewModel.CheckApiCommand.ExecuteAsync();
        viewModel.LoginUserName = "facturacion";
        viewModel.LoginPassword = "Password123!";

        await viewModel.LoginCommand.ExecuteAsync();

        Assert.True(viewModel.IsShellVisible);
        Assert.False(viewModel.CanShowPlatformModule);
        Assert.True(viewModel.CanShowOperationsModule);
        Assert.False(viewModel.CanShowDiagnosticsModule);
        Assert.False(viewModel.OpenPlatformCommand.CanExecute(null));
    }

    [Fact]
    public async Task CustomersModuleListsAndCreatesCustomersWhenUserHasPermission()
    {
        var apiClient = new ReadyInstallationApiClient
        {
            CurrentUserPermissions =
            [
                PlatformPermissionNames.ViewCustomers,
                PlatformPermissionNames.ManageCustomers
            ]
        };
        var viewModel = new MainWindowViewModel(
            new DesktopSettings
            {
                Api = new DesktopSettings.ApiSettings { BaseUrl = "http://localhost:5099/" }
            },
            _ => apiClient);

        await viewModel.CheckApiCommand.ExecuteAsync();
        viewModel.LoginUserName = "facturacion";
        viewModel.LoginPassword = "Password123!";
        await viewModel.LoginCommand.ExecuteAsync();

        Assert.True(viewModel.CanShowOperationsModule);
        Assert.True(viewModel.OpenCustomersCommand.CanExecute(null));

        await viewModel.OpenCustomersCommand.ExecuteAsync();

        Assert.True(viewModel.IsCustomersViewVisible);
        Assert.False(viewModel.IsShellHomeVisible);

        viewModel.NewCustomerName = "Cliente Desktop";
        viewModel.NewCustomerTaxId = "B11111111";
        viewModel.NewCustomerEmail = "cliente@example.com";
        viewModel.NewCustomerPhone = "+34910000000";

        Assert.True(viewModel.CreateCustomerCommand.CanExecute(null));

        await viewModel.CreateCustomerCommand.ExecuteAsync();

        Assert.Equal("Cliente Desktop", apiClient.LastCreateCustomerRequest?.Name);
        Assert.Contains(viewModel.Customers, customer => customer.Name == "Cliente Desktop");
        Assert.Contains("Cliente creado", viewModel.CustomerMessage);
    }

    private sealed class ReadyInstallationApiClient : IInstallationApiClient
    {
        private readonly Guid _administratorRoleId = Guid.NewGuid();
        private readonly List<RoleSummaryResponse> _roles = [];
        private readonly List<UserSummaryResponse> _users = [];
        private readonly List<CustomerSummaryResponse> _customers = [];
        private readonly List<AuditEventResponse> _auditEvents = [];
        private readonly Dictionary<Guid, IReadOnlyList<string>> _rolePermissions = [];

        public CreateUserRequest? LastCreateUserRequest { get; private set; }

        public CreateRoleRequest? LastCreateRoleRequest { get; private set; }

        public CreateCustomerRequest? LastCreateCustomerRequest { get; private set; }

        public IReadOnlyList<string> LastUpdatedRolePermissions { get; private set; } = [];

        public bool FailNextLoginWithActiveSession { get; set; }

        public bool FailNextGetUsers { get; set; }

        public IReadOnlyList<string> CurrentUserPermissions { get; set; } = PlatformPermissionNames.All;

        public int CloseDevelopmentActiveSessionsCalls { get; private set; }

        public ReadyInstallationApiClient()
        {
            _roles.Add(new RoleSummaryResponse(
                _administratorRoleId,
                "Administrador",
                "system",
                "active",
                PlatformPermissionNames.All));
            _rolePermissions[_administratorRoleId] = PlatformPermissionNames.All;
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
            if (FailNextLoginWithActiveSession)
            {
                FailNextLoginWithActiveSession = false;
                throw new InstallationApiException(
                    System.Net.HttpStatusCode.Conflict,
                    "An active session already exists for this user.",
                    "La sesion anterior sigue activa.",
                    "AUTH.ACTIVE_SESSION_EXISTS");
            }

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
                CurrentUserPermissions,
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

        public Task<CloseActiveSessionsResponse> CloseDevelopmentActiveSessionsAsync(
            CloseActiveSessionsRequest request,
            CancellationToken cancellationToken = default)
        {
            CloseDevelopmentActiveSessionsCalls++;
            return Task.FromResult(new CloseActiveSessionsResponse(1));
        }

        public Task<IReadOnlyList<RoleSummaryResponse>> GetRolesAsync(
            string accessToken,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<RoleSummaryResponse>>(_roles
                .Select(role => role with
                {
                    Permissions = _rolePermissions.TryGetValue(role.Id, out var permissions)
                        ? permissions
                        : []
                })
                .ToArray());
        }

        public Task<RoleSummaryResponse> CreateRoleAsync(
            string accessToken,
            CreateRoleRequest request,
            CancellationToken cancellationToken = default)
        {
            LastCreateRoleRequest = request;
            var role = new RoleSummaryResponse(
                Guid.NewGuid(),
                request.Name ?? string.Empty,
                "custom",
                "active",
                []);
            _roles.Add(role);
            _rolePermissions[role.Id] = [];
            _auditEvents.Insert(0, new AuditEventResponse(
                _auditEvents.Count + 1,
                DateTimeOffset.UtcNow,
                Guid.NewGuid(),
                "Administrador",
                "Platform",
                "RoleCreated",
                "Role",
                role.Id.ToString("D"),
                "Role created.",
                Guid.NewGuid(),
                "success"));

            return Task.FromResult(role);
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
            return Task.FromResult(new RolePermissionsResponse(
                roleId,
                _rolePermissions.TryGetValue(roleId, out var permissions) ? permissions : []));
        }

        public Task<RolePermissionsResponse> UpdateRolePermissionsAsync(
            string accessToken,
            Guid roleId,
            UpdateRolePermissionsRequest request,
            CancellationToken cancellationToken = default)
        {
            _rolePermissions[roleId] = request.Permissions ?? [];
            LastUpdatedRolePermissions = _rolePermissions[roleId];
            _auditEvents.Insert(0, new AuditEventResponse(
                _auditEvents.Count + 1,
                DateTimeOffset.UtcNow,
                Guid.NewGuid(),
                "Administrador",
                "Platform",
                "RolePermissionsUpdated",
                "Role",
                roleId.ToString("D"),
                "Permissions updated.",
                Guid.NewGuid(),
                "success"));

            return Task.FromResult(new RolePermissionsResponse(roleId, _rolePermissions[roleId]));
        }

        public Task<IReadOnlyList<UserSummaryResponse>> GetUsersAsync(
            string accessToken,
            CancellationToken cancellationToken = default)
        {
            if (FailNextGetUsers)
            {
                FailNextGetUsers = false;
                throw new HttpRequestException("Simulated users endpoint failure.");
            }

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
                new RoleReferenceResponse(request.RoleId, _roles.Single(role => role.Id == request.RoleId).Name),
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

        public Task<IReadOnlyList<CustomerSummaryResponse>> GetCustomersAsync(
            string accessToken,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<CustomerSummaryResponse>>(_customers.ToArray());
        }

        public Task<CustomerSummaryResponse> CreateCustomerAsync(
            string accessToken,
            CreateCustomerRequest request,
            CancellationToken cancellationToken = default)
        {
            LastCreateCustomerRequest = request;
            var customer = new CustomerSummaryResponse(
                Guid.NewGuid(),
                request.Name ?? string.Empty,
                request.TaxId,
                request.Email,
                request.Phone,
                "active",
                DateTimeOffset.UtcNow);
            _customers.Add(customer);

            return Task.FromResult(customer);
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
