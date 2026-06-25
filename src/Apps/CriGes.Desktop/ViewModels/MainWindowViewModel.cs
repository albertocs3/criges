using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using CriGes.Desktop.Configuration;
using CriGes.Desktop.Services.Api;
using CriGes.Desktop.Services.Session;
using CriGes.Modules.Platform.Contracts.Administration;
using CriGes.Modules.Platform.Contracts.Auth;
using CriGes.Modules.Platform.Contracts.Customers;
using CriGes.Modules.Platform.Contracts.Installation;

namespace CriGes.Desktop.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly Func<string, IInstallationApiClient> _apiClientFactory;
    private readonly DesktopSession _session = new();

    private string _apiBaseUrl = "http://localhost:5099/";
    private string _statusText = "Pendiente de comprobar API.";
    private string _resultText = string.Empty;
    private string _loginMessage = string.Empty;
    private string _loginUserName = string.Empty;
    private string _loginPassword = string.Empty;
    private string _currentUserDisplayName = string.Empty;
    private string _currentUserRole = string.Empty;
    private string _platformMessage = string.Empty;
    private string _newUserFullName = string.Empty;
    private string _newUserName = string.Empty;
    private string _newUserPhone = string.Empty;
    private string _newUserPassword = string.Empty;
    private string _newUserPasswordConfirmation = string.Empty;
    private string _newRoleName = string.Empty;
    private string _customerMessage = string.Empty;
    private string _newCustomerName = string.Empty;
    private string _newCustomerTaxId = string.Empty;
    private string _newCustomerEmail = string.Empty;
    private string _newCustomerPhone = string.Empty;
    private string _rolePermissionsMessage = string.Empty;
    private string _auditMessage = string.Empty;
    private string _serverVersion = "desconocida";
    private string _clientVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "desconocida";
    private string _legalName = string.Empty;
    private string _tradeName = string.Empty;
    private string _taxId = string.Empty;
    private string _addressLine = string.Empty;
    private string _postalCode = string.Empty;
    private string _city = string.Empty;
    private string _region = string.Empty;
    private string _countryCode = "ES";
    private string _phone = string.Empty;
    private string _email = string.Empty;
    private string _adminFullName = string.Empty;
    private string _adminUserName = "admin";
    private string _adminPassword = string.Empty;
    private string _adminPasswordConfirmation = string.Empty;
    private bool _isBusy;
    private bool _requiresInitialization;
    private bool _isInitialized;
    private bool _needsAttention;
    private bool _isLoginVisible;
    private bool _isShellVisible;
    private bool _isShellHomeVisible;
    private bool _isPlatformViewVisible;
    private bool _isCustomersViewVisible;
    private bool _canShowPlatformModule;
    private bool _canShowOperationsModule;
    private bool _canShowDiagnosticsModule;
    private Guid _newUserRoleId;
    private Guid _selectedPlatformRoleId;
    private IReadOnlyList<RoleSummaryResponse> _platformRoles = [];
    private IReadOnlyList<UserSummaryResponse> _platformUsers = [];
    private IReadOnlyList<AuditEventResponse> _platformAuditEvents = [];
    private IReadOnlyList<PermissionResponse> _platformPermissions = [];
    private IReadOnlyList<CustomerSummaryResponse> _customers = [];
    private IReadOnlyList<RolePermissionOptionViewModel> _rolePermissionOptions = [];
    private ApiConnectionState _connectionState;

    public MainWindowViewModel()
        : this(DesktopSettings.Load())
    {
    }

    public MainWindowViewModel(DesktopSettings settings)
        : this(settings, CreateClient)
    {
    }

    public MainWindowViewModel(DesktopSettings settings, Func<string, IInstallationApiClient> apiClientFactory)
    {
        _apiClientFactory = apiClientFactory;
        ApiBaseUrl = settings.Api.BaseUrl;
        CheckApiCommand = new AsyncRelayCommand(CheckApiAsync, () => !IsBusy);
        InitializeCommand = new AsyncRelayCommand(InitializeAsync, CanInitialize);
        LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
        LogoutCommand = new AsyncRelayCommand(LogoutAsync, () => !IsBusy && IsShellVisible);
        RefreshSessionCommand = new AsyncRelayCommand(RefreshSessionAsync, () => !IsBusy && _session.IsAuthenticated);
        OpenPlatformCommand = new AsyncRelayCommand(OpenPlatformAsync, () => !IsBusy && IsShellVisible && CanShowPlatformModule);
        OpenCustomersCommand = new AsyncRelayCommand(OpenCustomersAsync, () => !IsBusy && IsShellVisible && CanShowOperationsModule);
        BackToShellCommand = new AsyncRelayCommand(BackToShellAsync, () => !IsBusy && (IsPlatformViewVisible || IsCustomersViewVisible));
        CreateRoleCommand = new AsyncRelayCommand(CreateRoleAsync, CanCreateRole);
        CreateUserCommand = new AsyncRelayCommand(CreateUserAsync, CanCreateUser);
        CreateCustomerCommand = new AsyncRelayCommand(CreateCustomerAsync, CanCreateCustomer);
        SaveRolePermissionsCommand = new AsyncRelayCommand(SaveRolePermissionsAsync, CanSaveRolePermissions);
        RefreshAuditCommand = new AsyncRelayCommand(RefreshAuditAsync, CanRefreshAudit);
    }

    public AsyncRelayCommand CheckApiCommand { get; }

    public AsyncRelayCommand InitializeCommand { get; }

    public AsyncRelayCommand LoginCommand { get; }

    public AsyncRelayCommand LogoutCommand { get; }

    public AsyncRelayCommand RefreshSessionCommand { get; }

    public AsyncRelayCommand OpenPlatformCommand { get; }

    public AsyncRelayCommand OpenCustomersCommand { get; }

    public AsyncRelayCommand BackToShellCommand { get; }

    public AsyncRelayCommand CreateRoleCommand { get; }

    public AsyncRelayCommand CreateUserCommand { get; }

    public AsyncRelayCommand CreateCustomerCommand { get; }

    public AsyncRelayCommand SaveRolePermissionsCommand { get; }

    public AsyncRelayCommand RefreshAuditCommand { get; }

    public string ApiBaseUrl
    {
        get => _apiBaseUrl;
        set => SetProperty(ref _apiBaseUrl, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public string ResultText
    {
        get => _resultText;
        private set => SetProperty(ref _resultText, value);
    }

    public string LoginMessage
    {
        get => _loginMessage;
        private set => SetProperty(ref _loginMessage, value);
    }

    public string LoginUserName
    {
        get => _loginUserName;
        set => SetLoginProperty(ref _loginUserName, value);
    }

    public string CurrentUserDisplayName
    {
        get => _currentUserDisplayName;
        private set => SetProperty(ref _currentUserDisplayName, value);
    }

    public string CurrentUserRole
    {
        get => _currentUserRole;
        private set => SetProperty(ref _currentUserRole, value);
    }

    public string LoginPassword
    {
        get => _loginPassword;
        set => SetLoginProperty(ref _loginPassword, value);
    }

    public string ServerVersion
    {
        get => _serverVersion;
        private set => SetProperty(ref _serverVersion, value);
    }

    public string ClientVersion
    {
        get => _clientVersion;
        private set => SetProperty(ref _clientVersion, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RaiseCommandStates();
            }
        }
    }

    public bool RequiresInitialization
    {
        get => _requiresInitialization;
        private set => SetProperty(ref _requiresInitialization, value);
    }

    public bool IsInitialized
    {
        get => _isInitialized;
        private set => SetProperty(ref _isInitialized, value);
    }

    public bool NeedsAttention
    {
        get => _needsAttention;
        private set => SetProperty(ref _needsAttention, value);
    }

    public bool IsLoginVisible
    {
        get => _isLoginVisible;
        private set => SetProperty(ref _isLoginVisible, value);
    }

    public bool IsShellVisible
    {
        get => _isShellVisible;
        private set => SetProperty(ref _isShellVisible, value);
    }

    public bool IsShellHomeVisible
    {
        get => _isShellHomeVisible;
        private set => SetProperty(ref _isShellHomeVisible, value);
    }

    public bool IsPlatformViewVisible
    {
        get => _isPlatformViewVisible;
        private set => SetProperty(ref _isPlatformViewVisible, value);
    }

    public bool IsCustomersViewVisible
    {
        get => _isCustomersViewVisible;
        private set => SetProperty(ref _isCustomersViewVisible, value);
    }

    public bool CanShowPlatformModule
    {
        get => _canShowPlatformModule;
        private set
        {
            if (SetProperty(ref _canShowPlatformModule, value))
            {
                OpenPlatformCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool CanShowOperationsModule
    {
        get => _canShowOperationsModule;
        private set => SetProperty(ref _canShowOperationsModule, value);
    }

    public bool CanShowDiagnosticsModule
    {
        get => _canShowDiagnosticsModule;
        private set => SetProperty(ref _canShowDiagnosticsModule, value);
    }

    public ApiConnectionState ConnectionState
    {
        get => _connectionState;
        private set => SetProperty(ref _connectionState, value);
    }

    public IReadOnlyList<RoleSummaryResponse> PlatformRoles
    {
        get => _platformRoles;
        private set => SetProperty(ref _platformRoles, value);
    }

    public IReadOnlyList<UserSummaryResponse> PlatformUsers
    {
        get => _platformUsers;
        private set => SetProperty(ref _platformUsers, value);
    }

    public IReadOnlyList<AuditEventResponse> PlatformAuditEvents
    {
        get => _platformAuditEvents;
        private set => SetProperty(ref _platformAuditEvents, value);
    }

    public IReadOnlyList<PermissionResponse> PlatformPermissions
    {
        get => _platformPermissions;
        private set => SetProperty(ref _platformPermissions, value);
    }

    public IReadOnlyList<CustomerSummaryResponse> Customers
    {
        get => _customers;
        private set => SetProperty(ref _customers, value);
    }

    public IReadOnlyList<RolePermissionOptionViewModel> RolePermissionOptions
    {
        get => _rolePermissionOptions;
        private set => SetProperty(ref _rolePermissionOptions, value);
    }

    public string RolePermissionsMessage
    {
        get => _rolePermissionsMessage;
        private set => SetProperty(ref _rolePermissionsMessage, value);
    }

    public string AuditMessage
    {
        get => _auditMessage;
        private set => SetProperty(ref _auditMessage, value);
    }

    public string PlatformMessage
    {
        get => _platformMessage;
        private set => SetProperty(ref _platformMessage, value);
    }

    public string CustomerMessage
    {
        get => _customerMessage;
        private set => SetProperty(ref _customerMessage, value);
    }

    public string NewUserFullName
    {
        get => _newUserFullName;
        set => SetCreateUserProperty(ref _newUserFullName, value);
    }

    public string NewUserName
    {
        get => _newUserName;
        set => SetCreateUserProperty(ref _newUserName, value);
    }

    public string NewUserPhone
    {
        get => _newUserPhone;
        set => SetCreateUserProperty(ref _newUserPhone, value);
    }

    public string NewUserPassword
    {
        get => _newUserPassword;
        set => SetCreateUserProperty(ref _newUserPassword, value);
    }

    public string NewUserPasswordConfirmation
    {
        get => _newUserPasswordConfirmation;
        set => SetCreateUserProperty(ref _newUserPasswordConfirmation, value);
    }

    public string NewRoleName
    {
        get => _newRoleName;
        set => SetCreateRoleProperty(ref _newRoleName, value);
    }

    public string NewCustomerName
    {
        get => _newCustomerName;
        set => SetCreateCustomerProperty(ref _newCustomerName, value);
    }

    public string NewCustomerTaxId
    {
        get => _newCustomerTaxId;
        set => SetCreateCustomerProperty(ref _newCustomerTaxId, value);
    }

    public string NewCustomerEmail
    {
        get => _newCustomerEmail;
        set => SetCreateCustomerProperty(ref _newCustomerEmail, value);
    }

    public string NewCustomerPhone
    {
        get => _newCustomerPhone;
        set => SetCreateCustomerProperty(ref _newCustomerPhone, value);
    }

    public Guid NewUserRoleId
    {
        get => _newUserRoleId;
        set
        {
            if (SetProperty(ref _newUserRoleId, value))
            {
                CreateUserCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public Guid SelectedPlatformRoleId
    {
        get => _selectedPlatformRoleId;
        set
        {
            if (SetProperty(ref _selectedPlatformRoleId, value))
            {
                RebuildRolePermissionOptions();
                SaveRolePermissionsCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string LegalName
    {
        get => _legalName;
        set => SetFormProperty(ref _legalName, value);
    }

    public string TradeName
    {
        get => _tradeName;
        set => SetFormProperty(ref _tradeName, value);
    }

    public string TaxId
    {
        get => _taxId;
        set => SetFormProperty(ref _taxId, value);
    }

    public string AddressLine
    {
        get => _addressLine;
        set => SetFormProperty(ref _addressLine, value);
    }

    public string PostalCode
    {
        get => _postalCode;
        set => SetFormProperty(ref _postalCode, value);
    }

    public string City
    {
        get => _city;
        set => SetFormProperty(ref _city, value);
    }

    public string Region
    {
        get => _region;
        set => SetFormProperty(ref _region, value);
    }

    public string CountryCode
    {
        get => _countryCode;
        set => SetFormProperty(ref _countryCode, value);
    }

    public string Phone
    {
        get => _phone;
        set => SetFormProperty(ref _phone, value);
    }

    public string Email
    {
        get => _email;
        set => SetFormProperty(ref _email, value);
    }

    public string AdminFullName
    {
        get => _adminFullName;
        set => SetFormProperty(ref _adminFullName, value);
    }

    public string AdminUserName
    {
        get => _adminUserName;
        set => SetFormProperty(ref _adminUserName, value);
    }

    public string AdminPassword
    {
        get => _adminPassword;
        set => SetFormProperty(ref _adminPassword, value);
    }

    public string AdminPasswordConfirmation
    {
        get => _adminPasswordConfirmation;
        set => SetFormProperty(ref _adminPasswordConfirmation, value);
    }

    private async Task CheckApiAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        ResultText = string.Empty;
        LoginMessage = string.Empty;

        try
        {
            var client = _apiClientFactory(ApiBaseUrl);
            var readiness = await client.GetReadinessAsync(cancellationToken).ConfigureAwait(true);

            if (!IsReady(readiness))
            {
                ApplyReadinessFailure(readiness);
                return;
            }

            var status = await client.GetStatusAsync(cancellationToken).ConfigureAwait(true);

            RequiresInitialization = status.RequiresInitialization;
            IsInitialized = !status.RequiresInitialization;
            NeedsAttention = false;
            IsLoginVisible = !status.RequiresInitialization;
            IsShellVisible = false;
            IsShellHomeVisible = false;
            IsPlatformViewVisible = false;
            IsCustomersViewVisible = false;
            ServerVersion = status.ProductVersion;
            ConnectionState = status.RequiresInitialization
                ? ApiConnectionState.PlatformNotInitialized
                : ApiConnectionState.Ready;
            StatusText = status.RequiresInitialization
                ? $"API conectada. Estado: {status.Status}. Se requiere inicializacion."
                : $"API conectada. Estado: {status.Status}. Plataforma inicializada.";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or UriFormatException or InstallationApiException)
        {
            RequiresInitialization = false;
            IsInitialized = false;
            NeedsAttention = true;
            IsLoginVisible = false;
            IsShellVisible = false;
            IsShellHomeVisible = false;
            IsPlatformViewVisible = false;
            IsCustomersViewVisible = false;
            ConnectionState = ApiConnectionState.ApiUnavailable;
            StatusText = $"No se pudo comprobar la API: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            RaiseCommandStates();
        }
    }

    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        ResultText = "Inicializando plataforma...";

        try
        {
            var request = new InitializePlatformRequest(
                new CompanyRequest(
                    LegalName,
                    TradeName,
                    TaxId,
                    new AddressRequest(AddressLine, PostalCode, City, Region, CountryCode),
                    Phone,
                    Email),
                new AdministratorRequest(AdminFullName, AdminUserName, AdminPassword));

            var client = _apiClientFactory(ApiBaseUrl);
            var response = await client.InitializeAsync(request, cancellationToken).ConfigureAwait(true);

            RequiresInitialization = false;
            IsInitialized = true;
            NeedsAttention = false;
            IsLoginVisible = true;
            IsShellVisible = false;
            IsShellHomeVisible = false;
            IsPlatformViewVisible = false;
            IsCustomersViewVisible = false;
            ConnectionState = ApiConnectionState.Ready;
            AdminPassword = string.Empty;
            AdminPasswordConfirmation = string.Empty;
            StatusText = $"Plataforma inicializada. Estado: {response.Status}.";
            LoginUserName = AdminUserName;
            LoginMessage = "Instalacion completada. Ya puedes iniciar sesion.";
            ResultText = $"Administrador creado: {response.AdministratorUserId}. Reinicio requerido: {(response.RequiresRestart ? "si" : "no")}.";
        }
        catch (InstallationApiException ex)
        {
            ResultText = $"{(int)ex.StatusCode} - {ex.Message}";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or UriFormatException)
        {
            ResultText = $"No se pudo inicializar: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            RaiseCommandStates();
        }
    }

    private static InstallationApiClient CreateClient(string apiBaseUrl)
    {
        var baseUrl = apiBaseUrl.Trim();
        if (baseUrl.Length == 0 || baseUrl[^1] != '/')
        {
            baseUrl += "/";
        }

        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl, UriKind.Absolute),
            Timeout = TimeSpan.FromSeconds(15)
        };

        return new InstallationApiClient(httpClient);
    }

    private Task LoginAsync(CancellationToken cancellationToken)
    {
        return LoginCoreAsync(allowDevelopmentSessionReset: true, cancellationToken);
    }

    private async Task LoginCoreAsync(bool allowDevelopmentSessionReset, CancellationToken cancellationToken)
    {
        IsBusy = true;
        LoginMessage = "Iniciando sesion...";

        try
        {
            var session = await _apiClientFactory(ApiBaseUrl)
                .LoginAsync(
                    new LoginRequest(LoginUserName, LoginPassword, Environment.MachineName, ClientVersion),
                    cancellationToken)
                .ConfigureAwait(true);

            _session.Apply(session);
            var currentUser = await GetFreshCurrentUserAsync(cancellationToken).ConfigureAwait(true);
            LoginPassword = string.Empty;
            IsLoginVisible = false;
            IsShellVisible = true;
            IsShellHomeVisible = true;
            IsPlatformViewVisible = false;
            IsCustomersViewVisible = false;
            CurrentUserDisplayName = currentUser.DisplayName;
            CurrentUserRole = currentUser.Role.Name;
            ApplyPermissions(currentUser.Permissions);
            LoginMessage = string.Empty;
            StatusText = $"Sesion iniciada como {currentUser.DisplayName}.";
        }
        catch (InstallationApiException ex)
        {
            if (allowDevelopmentSessionReset &&
                ex.Code == "AUTH.ACTIVE_SESSION_EXISTS" &&
                await TryCloseDevelopmentActiveSessionsAsync(cancellationToken).ConfigureAwait(true))
            {
                IsBusy = false;
                RaiseCommandStates();
                await LoginCoreAsync(allowDevelopmentSessionReset: false, cancellationToken).ConfigureAwait(true);
                return;
            }

            LoginMessage = $"{(int)ex.StatusCode} - {ex.Message}";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or UriFormatException)
        {
            LoginMessage = $"No se pudo iniciar sesion: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            RaiseCommandStates();
        }
    }

    private async Task<bool> TryCloseDevelopmentActiveSessionsAsync(CancellationToken cancellationToken)
    {
        try
        {
            LoginMessage = "Sesion anterior detectada. Cerrando sesion activa de desarrollo...";
            var response = await _apiClientFactory(ApiBaseUrl)
                .CloseDevelopmentActiveSessionsAsync(
                    new CloseActiveSessionsRequest(LoginUserName),
                    cancellationToken)
                .ConfigureAwait(true);

            return response.ClosedSessions > 0;
        }
        catch (Exception ex) when (ex is InstallationApiException or HttpRequestException or TaskCanceledException or UriFormatException)
        {
            LoginMessage = "Ya existe una sesion activa para este usuario.";
            return false;
        }
    }

    private async Task RefreshSessionAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;

        try
        {
            await RefreshSessionCoreAsync(cancellationToken).ConfigureAwait(true);
            StatusText = "Sesion refrescada correctamente.";
        }
        catch (InstallationApiException ex)
        {
            HandleSessionLost($"{(int)ex.StatusCode} - {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            RaiseCommandStates();
        }
    }

    private async Task OpenPlatformAsync(CancellationToken cancellationToken)
    {
        if (!CanShowPlatformModule)
        {
            return;
        }

        IsBusy = true;
        PlatformMessage = "Cargando administracion de Plataforma...";

        try
        {
            await EnsureFreshAccessTokenAsync(cancellationToken).ConfigureAwait(true);
            var client = _apiClientFactory(ApiBaseUrl);
            var roles = await client.GetRolesAsync(_session.AccessToken, cancellationToken).ConfigureAwait(true);
            var users = await client.GetUsersAsync(_session.AccessToken, cancellationToken).ConfigureAwait(true);
            var permissions = await client.GetPermissionsAsync(_session.AccessToken, cancellationToken).ConfigureAwait(true);
            var auditEvents = await LoadAuditEventsIfAllowedAsync(client, cancellationToken).ConfigureAwait(true);

            PlatformRoles = roles;
            PlatformUsers = users;
            PlatformAuditEvents = auditEvents;
            PlatformPermissions = permissions;
            NewUserRoleId = GetDefaultRoleId(roles);
            SelectedPlatformRoleId = NewUserRoleId;
            RebuildRolePermissionOptions();
            IsShellHomeVisible = false;
            IsPlatformViewVisible = true;
            IsCustomersViewVisible = false;
            PlatformMessage = $"Usuarios: {users.Count}. Roles: {roles.Count}. Permisos: {permissions.Count}.";
            RolePermissionsMessage = string.Empty;
            AuditMessage = PlatformAuditEvents.Count == 0 ? "Sin eventos recientes." : string.Empty;
        }
        catch (InstallationApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            HandleSessionLost("Sesion caducada. Vuelve a iniciar sesion.");
        }
        catch (InstallationApiException ex)
        {
            PlatformMessage = $"{(int)ex.StatusCode} - {ex.Message}";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or UriFormatException)
        {
            PlatformMessage = $"No se pudo cargar Plataforma: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            RaiseCommandStates();
        }
    }

    private async Task OpenCustomersAsync(CancellationToken cancellationToken)
    {
        if (!CanShowOperationsModule)
        {
            return;
        }

        IsBusy = true;
        CustomerMessage = "Cargando clientes...";

        try
        {
            await EnsureFreshAccessTokenAsync(cancellationToken).ConfigureAwait(true);
            Customers = await _apiClientFactory(ApiBaseUrl)
                .GetCustomersAsync(_session.AccessToken, cancellationToken)
                .ConfigureAwait(true);
            IsShellHomeVisible = false;
            IsPlatformViewVisible = false;
            IsCustomersViewVisible = true;
            CustomerMessage = Customers.Count == 0 ? "Sin clientes." : $"Clientes: {Customers.Count}.";
        }
        catch (InstallationApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            HandleSessionLost("Sesion caducada. Vuelve a iniciar sesion.");
        }
        catch (InstallationApiException ex)
        {
            CustomerMessage = $"{(int)ex.StatusCode} - {ex.Message}";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or UriFormatException)
        {
            CustomerMessage = $"No se pudieron cargar clientes: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            RaiseCommandStates();
        }
    }

    private async Task SaveRolePermissionsAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        RolePermissionsMessage = "Guardando permisos...";

        try
        {
            await EnsureFreshAccessTokenAsync(cancellationToken).ConfigureAwait(true);
            var selectedPermissions = RolePermissionOptions
                .Where(option => option.IsGranted)
                .Select(option => option.Name)
                .ToArray();

            await _apiClientFactory(ApiBaseUrl)
                .UpdateRolePermissionsAsync(
                    _session.AccessToken,
                    SelectedPlatformRoleId,
                    new UpdateRolePermissionsRequest(selectedPermissions),
                    cancellationToken)
                .ConfigureAwait(true);

            PlatformRoles = await _apiClientFactory(ApiBaseUrl)
                .GetRolesAsync(_session.AccessToken, cancellationToken)
                .ConfigureAwait(true);
            RebuildRolePermissionOptions();
            await RefreshAuditCoreAsync(cancellationToken).ConfigureAwait(true);
            RolePermissionsMessage = "Permisos guardados.";
        }
        catch (InstallationApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            HandleSessionLost("Sesion caducada. Vuelve a iniciar sesion.");
        }
        catch (InstallationApiException ex)
        {
            RolePermissionsMessage = $"{(int)ex.StatusCode} - {ex.Message}";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or UriFormatException)
        {
            RolePermissionsMessage = $"No se pudieron guardar permisos: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            RaiseCommandStates();
        }
    }

    private async Task RefreshAuditAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        AuditMessage = "Cargando auditoria...";

        try
        {
            await RefreshAuditCoreAsync(cancellationToken).ConfigureAwait(true);
            AuditMessage = PlatformAuditEvents.Count == 0 ? "Sin eventos recientes." : "Auditoria actualizada.";
        }
        catch (InstallationApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            HandleSessionLost("Sesion caducada. Vuelve a iniciar sesion.");
        }
        catch (InstallationApiException ex)
        {
            AuditMessage = $"{(int)ex.StatusCode} - {ex.Message}";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or UriFormatException)
        {
            AuditMessage = $"No se pudo cargar auditoria: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            RaiseCommandStates();
        }
    }

    private async Task RefreshAuditCoreAsync(CancellationToken cancellationToken)
    {
        await EnsureFreshAccessTokenAsync(cancellationToken).ConfigureAwait(true);
        PlatformAuditEvents = await LoadAuditEventsIfAllowedAsync(
                _apiClientFactory(ApiBaseUrl),
                cancellationToken)
            .ConfigureAwait(true);
    }

    private Task BackToShellAsync(CancellationToken cancellationToken)
    {
        IsPlatformViewVisible = false;
        IsCustomersViewVisible = false;
        IsShellHomeVisible = true;
        PlatformMessage = string.Empty;
        CustomerMessage = string.Empty;
        RaiseCommandStates();
        return Task.CompletedTask;
    }

    private async Task CreateRoleAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        RolePermissionsMessage = "Creando rol...";

        try
        {
            await EnsureFreshAccessTokenAsync(cancellationToken).ConfigureAwait(true);
            var created = await _apiClientFactory(ApiBaseUrl)
                .CreateRoleAsync(
                    _session.AccessToken,
                    new CreateRoleRequest(NewRoleName),
                    cancellationToken)
                .ConfigureAwait(true);

            NewRoleName = string.Empty;
            PlatformRoles = await _apiClientFactory(ApiBaseUrl)
                .GetRolesAsync(_session.AccessToken, cancellationToken)
                .ConfigureAwait(true);
            SelectedPlatformRoleId = created.Id;
            RebuildRolePermissionOptions();
            await RefreshAuditCoreAsync(cancellationToken).ConfigureAwait(true);
            RolePermissionsMessage = $"Rol creado: {created.Name}.";
        }
        catch (InstallationApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            HandleSessionLost("Sesion caducada. Vuelve a iniciar sesion.");
        }
        catch (InstallationApiException ex)
        {
            RolePermissionsMessage = $"{(int)ex.StatusCode} - {ex.Message}";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or UriFormatException)
        {
            RolePermissionsMessage = $"No se pudo crear el rol: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            RaiseCommandStates();
        }
    }

    private async Task CreateUserAsync(CancellationToken cancellationToken)
    {
        if (NewUserPassword != NewUserPasswordConfirmation)
        {
            PlatformMessage = "Las contrasenas no coinciden.";
            return;
        }

        IsBusy = true;
        PlatformMessage = "Creando usuario...";

        try
        {
            await EnsureFreshAccessTokenAsync(cancellationToken).ConfigureAwait(true);
            var request = new CreateUserRequest(
                NewUserFullName,
                NewUserName,
                string.IsNullOrWhiteSpace(NewUserPhone) ? null : NewUserPhone,
                NewUserRoleId,
                NewUserPassword);

            var user = await _apiClientFactory(ApiBaseUrl)
                .CreateUserAsync(_session.AccessToken, request, cancellationToken)
                .ConfigureAwait(true);

            NewUserFullName = string.Empty;
            NewUserName = string.Empty;
            NewUserPhone = string.Empty;
            NewUserPassword = string.Empty;
            NewUserPasswordConfirmation = string.Empty;
            PlatformUsers = await _apiClientFactory(ApiBaseUrl)
                .GetUsersAsync(_session.AccessToken, cancellationToken)
                .ConfigureAwait(true);
            await RefreshAuditCoreAsync(cancellationToken).ConfigureAwait(true);
            PlatformMessage = $"Usuario creado: {user.FullName} ({user.Role.Name}).";
        }
        catch (InstallationApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            HandleSessionLost("Sesion caducada. Vuelve a iniciar sesion.");
        }
        catch (InstallationApiException ex)
        {
            PlatformMessage = $"{(int)ex.StatusCode} - {ex.Message}";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or UriFormatException)
        {
            PlatformMessage = $"No se pudo crear el usuario: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            RaiseCommandStates();
        }
    }

    private async Task CreateCustomerAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        CustomerMessage = "Creando cliente...";

        try
        {
            await EnsureFreshAccessTokenAsync(cancellationToken).ConfigureAwait(true);
            var customer = await _apiClientFactory(ApiBaseUrl)
                .CreateCustomerAsync(
                    _session.AccessToken,
                    new CreateCustomerRequest(NewCustomerName, NewCustomerTaxId, NewCustomerEmail, NewCustomerPhone),
                    cancellationToken)
                .ConfigureAwait(true);

            NewCustomerName = string.Empty;
            NewCustomerTaxId = string.Empty;
            NewCustomerEmail = string.Empty;
            NewCustomerPhone = string.Empty;
            Customers = await _apiClientFactory(ApiBaseUrl)
                .GetCustomersAsync(_session.AccessToken, cancellationToken)
                .ConfigureAwait(true);
            CustomerMessage = $"Cliente creado: {customer.Name}.";
        }
        catch (InstallationApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            HandleSessionLost("Sesion caducada. Vuelve a iniciar sesion.");
        }
        catch (InstallationApiException ex)
        {
            CustomerMessage = $"{(int)ex.StatusCode} - {ex.Message}";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or UriFormatException)
        {
            CustomerMessage = $"No se pudo crear el cliente: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            RaiseCommandStates();
        }
    }

    private async Task LogoutAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;

        try
        {
            if (_session.IsAuthenticated)
            {
                await _apiClientFactory(ApiBaseUrl)
                    .LogoutAsync(_session.AccessToken, cancellationToken)
                    .ConfigureAwait(true);
            }
        }
        catch (InstallationApiException ex)
        {
            LoginMessage = $"{(int)ex.StatusCode} - {ex.Message}";
        }
        finally
        {
            ClearSessionAndShowLogin("Sesion cerrada.");
            IsBusy = false;
            RaiseCommandStates();
        }
    }

    private async Task<CurrentUserResponse> GetFreshCurrentUserAsync(CancellationToken cancellationToken)
    {
        await EnsureFreshAccessTokenAsync(cancellationToken).ConfigureAwait(true);

        return await _apiClientFactory(ApiBaseUrl)
            .GetCurrentUserAsync(_session.AccessToken, cancellationToken)
            .ConfigureAwait(true);
    }

    private async Task EnsureFreshAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (!_session.IsAuthenticated)
        {
            throw new InstallationApiException(System.Net.HttpStatusCode.Unauthorized, "Sesion no iniciada", "No hay una sesion activa.");
        }

        if (_session.AccessTokenExpiresAtUtc <= DateTimeOffset.UtcNow.AddMinutes(1))
        {
            await RefreshSessionCoreAsync(cancellationToken).ConfigureAwait(true);
        }
    }

    private async Task RefreshSessionCoreAsync(CancellationToken cancellationToken)
    {
        var refreshed = await _apiClientFactory(ApiBaseUrl)
            .RefreshAsync(new RefreshSessionRequest(_session.SessionId, _session.RefreshToken), cancellationToken)
            .ConfigureAwait(true);

        _session.Apply(refreshed);
    }

    private void HandleSessionLost(string message)
    {
        ClearSessionAndShowLogin(message);
    }

    private void ClearSessionAndShowLogin(string message)
    {
        _session.Clear();
        IsShellVisible = false;
        IsShellHomeVisible = false;
        IsPlatformViewVisible = false;
        IsCustomersViewVisible = false;
        IsLoginVisible = true;
        CurrentUserDisplayName = string.Empty;
        CurrentUserRole = string.Empty;
        PlatformRoles = [];
        PlatformUsers = [];
        PlatformAuditEvents = [];
        PlatformPermissions = [];
        Customers = [];
        RolePermissionOptions = [];
        PlatformMessage = string.Empty;
        RolePermissionsMessage = string.Empty;
        AuditMessage = string.Empty;
        CustomerMessage = string.Empty;
        NewUserRoleId = Guid.Empty;
        SelectedPlatformRoleId = Guid.Empty;
        ApplyPermissions([]);
        LoginPassword = string.Empty;
        LoginMessage = message;
        StatusText = message;
    }

    private void ApplyPermissions(IReadOnlyList<string> permissions)
    {
        CanShowPlatformModule =
            permissions.Contains(PlatformPermissionNames.ManageUsers) ||
            permissions.Contains(PlatformPermissionNames.ManageRoles) ||
            permissions.Contains(PlatformPermissionNames.ManageConfiguration) ||
            permissions.Contains(PlatformPermissionNames.ViewAudit) ||
            permissions.Contains(PlatformPermissionNames.ViewSessions);
        CanShowDiagnosticsModule = permissions.Contains(PlatformPermissionNames.ViewDiagnostics);
        CanShowOperationsModule =
            permissions.Contains(PlatformPermissionNames.UseAttachments) ||
            permissions.Contains(PlatformPermissionNames.ViewCustomers) ||
            permissions.Contains(PlatformPermissionNames.ManageCustomers);
    }

    private bool CanInitialize()
    {
        return !IsBusy &&
            RequiresInitialization &&
            HasValue(LegalName) &&
            HasValue(TaxId) &&
            HasValue(AddressLine) &&
            HasValue(PostalCode) &&
            HasValue(City) &&
            HasValue(CountryCode) &&
            HasValue(AdminFullName) &&
            HasValue(AdminUserName) &&
            HasValue(AdminPassword) &&
            AdminPassword == AdminPasswordConfirmation;
    }

    private bool CanLogin()
    {
        return !IsBusy &&
            IsLoginVisible &&
            HasValue(LoginUserName) &&
            HasValue(LoginPassword);
    }

    private bool CanCreateUser()
    {
        return !IsBusy &&
            IsPlatformViewVisible &&
            _session.Permissions.Contains(PlatformPermissionNames.ManageUsers) &&
            HasValue(NewUserFullName) &&
            HasValue(NewUserName) &&
            HasValue(NewUserPassword) &&
            NewUserPassword == NewUserPasswordConfirmation &&
            NewUserRoleId != Guid.Empty;
    }

    private bool CanCreateRole()
    {
        return !IsBusy &&
            IsPlatformViewVisible &&
            _session.Permissions.Contains(PlatformPermissionNames.ManageRoles) &&
            HasValue(NewRoleName);
    }

    private bool CanCreateCustomer()
    {
        return !IsBusy &&
            IsCustomersViewVisible &&
            _session.Permissions.Contains(PlatformPermissionNames.ManageCustomers) &&
            HasValue(NewCustomerName);
    }

    private bool CanSaveRolePermissions()
    {
        return !IsBusy &&
            IsPlatformViewVisible &&
            _session.Permissions.Contains(PlatformPermissionNames.ManageRoles) &&
            SelectedPlatformRoleId != Guid.Empty &&
            RolePermissionOptions.Count > 0;
    }

    private bool CanRefreshAudit()
    {
        return !IsBusy &&
            IsPlatformViewVisible &&
            _session.Permissions.Contains(PlatformPermissionNames.ViewAudit);
    }

    private void ApplyReadinessFailure(ApiReadinessResponse readiness)
    {
        RequiresInitialization = false;
        IsInitialized = false;
        NeedsAttention = true;
        IsLoginVisible = false;
        IsShellVisible = false;
        IsShellHomeVisible = false;
        IsPlatformViewVisible = false;
        IsCustomersViewVisible = false;
        ConnectionState = readiness.Status switch
        {
            "databaseNotMigrated" => ApiConnectionState.DatabaseNotMigrated,
            "databaseUnavailable" => ApiConnectionState.DatabaseUnavailable,
            _ => ApiConnectionState.ApiUnavailable
        };

        StatusText = ConnectionState switch
        {
            ApiConnectionState.DatabaseNotMigrated =>
                $"API conectada, pero la base de datos tiene {readiness.PendingMigrations} migracion(es) pendiente(s). Ejecuta el migrador.",
            ApiConnectionState.DatabaseUnavailable =>
                $"API conectada, pero la base de datos no esta disponible: {readiness.Detail}",
            _ => $"API conectada, pero no esta preparada: {readiness.Status}."
        };
    }

    private static bool IsReady(ApiReadinessResponse readiness)
    {
        return string.Equals(readiness.Status, "ready", StringComparison.OrdinalIgnoreCase);
    }

    private void SetFormProperty(ref string field, string value, [CallerMemberName] string? propertyName = null)
    {
        if (SetProperty(ref field, value, propertyName))
        {
            InitializeCommand.RaiseCanExecuteChanged();
        }
    }

    private void SetLoginProperty(ref string field, string value, [CallerMemberName] string? propertyName = null)
    {
        if (SetProperty(ref field, value, propertyName))
        {
            LoginCommand.RaiseCanExecuteChanged();
        }
    }

    private void SetCreateUserProperty(ref string field, string value, [CallerMemberName] string? propertyName = null)
    {
        if (SetProperty(ref field, value, propertyName))
        {
            CreateUserCommand.RaiseCanExecuteChanged();
        }
    }

    private void SetCreateRoleProperty(ref string field, string value, [CallerMemberName] string? propertyName = null)
    {
        if (SetProperty(ref field, value, propertyName))
        {
            CreateRoleCommand.RaiseCanExecuteChanged();
        }
    }

    private void SetCreateCustomerProperty(ref string field, string value, [CallerMemberName] string? propertyName = null)
    {
        if (SetProperty(ref field, value, propertyName))
        {
            CreateCustomerCommand.RaiseCanExecuteChanged();
        }
    }

    private void RaiseCommandStates()
    {
        CheckApiCommand.RaiseCanExecuteChanged();
        InitializeCommand.RaiseCanExecuteChanged();
        LoginCommand.RaiseCanExecuteChanged();
        LogoutCommand.RaiseCanExecuteChanged();
        RefreshSessionCommand.RaiseCanExecuteChanged();
        OpenPlatformCommand.RaiseCanExecuteChanged();
        OpenCustomersCommand.RaiseCanExecuteChanged();
        BackToShellCommand.RaiseCanExecuteChanged();
        CreateRoleCommand.RaiseCanExecuteChanged();
        CreateUserCommand.RaiseCanExecuteChanged();
        CreateCustomerCommand.RaiseCanExecuteChanged();
        SaveRolePermissionsCommand.RaiseCanExecuteChanged();
        RefreshAuditCommand.RaiseCanExecuteChanged();
    }

    private static bool HasValue(string value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    private static Guid GetDefaultRoleId(IReadOnlyList<RoleSummaryResponse> roles)
    {
        for (var index = 0; index < roles.Count; index++)
        {
            if (roles[index].Name == "Administrador")
            {
                return roles[index].Id;
            }
        }

        return roles.Count > 0 ? roles[0].Id : Guid.Empty;
    }

    private void RebuildRolePermissionOptions()
    {
        var selectedRole = PlatformRoles.SingleOrDefault(role => role.Id == SelectedPlatformRoleId);
        if (selectedRole is null)
        {
            RolePermissionOptions = [];
            return;
        }

        var granted = selectedRole.Permissions.ToHashSet(StringComparer.Ordinal);
        RolePermissionOptions = PlatformPermissions
            .OrderBy(permission => permission.Name, StringComparer.Ordinal)
            .Select(permission => new RolePermissionOptionViewModel(
                permission.Name,
                permission.Description,
                granted.Contains(permission.Name)))
            .ToArray();
    }

    private async Task<IReadOnlyList<AuditEventResponse>> LoadAuditEventsIfAllowedAsync(
        IInstallationApiClient client,
        CancellationToken cancellationToken)
    {
        if (!_session.Permissions.Contains(PlatformPermissionNames.ViewAudit))
        {
            return [];
        }

        try
        {
            return await client.GetAuditEventsAsync(_session.AccessToken, take: 50, cancellationToken).ConfigureAwait(true);
        }
        catch (InstallationApiException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
        {
            AuditMessage = "No tienes permiso para consultar auditoria.";
            return [];
        }
    }
}
