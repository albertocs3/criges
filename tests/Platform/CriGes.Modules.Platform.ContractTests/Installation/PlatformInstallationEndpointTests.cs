using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CriGes.Modules.Platform.Application.Initialization;
using CriGes.Modules.Platform.Contracts.Installation;
using CriGes.Modules.Platform.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CriGes.Modules.Platform.ContractTests.Installation;

public sealed class PlatformInstallationEndpointTests
{
    [Fact]
    public async Task GetInstallationReturnsNotInitializedBeforeInitialization()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetFromJsonAsync<InstallationStatusResponse>("/api/v1/platform/installation/");

        Assert.NotNull(response);
        Assert.Equal("notInitialized", response.Status);
        Assert.True(response.RequiresInitialization);
    }

    [Fact]
    public async Task PostInitializeCreatesInstallationAndRejectsSecondAttempt()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var request = CreateRequest();

        var createdResponse = await PostInitializeAsync(client, request, "contract-key-1");
        var created = await createdResponse.Content.ReadFromJsonAsync<InitializePlatformResponse>();

        Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
        Assert.NotNull(created);
        Assert.Equal("initialized", created.Status);

        var replay = await PostInitializeAsync(client, request, "contract-key-1");

        Assert.Equal(HttpStatusCode.Created, replay.StatusCode);
    }

    [Fact]
    public async Task PostInitializeRequiresIdempotencyKeyAndReturnsProblemDetails()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/platform/installation/initialize", CreateRequest());
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("IDEMPOTENCY.KEY_REQUIRED", problem.GetProperty("code").GetString());
        Assert.True(problem.TryGetProperty("correlationId", out _));
        Assert.True(response.Headers.Contains("X-Correlation-Id"));
    }

    [Fact]
    public async Task PostInitializeRejectsSameIdempotencyKeyWithDifferentBody()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var first = await PostInitializeAsync(client, CreateRequest(), "contract-key-reused");
        var second = await PostInitializeAsync(client, CreateDifferentRequest(), "contract-key-reused");

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    private static InitializePlatformRequest CreateRequest()
    {
        return new InitializePlatformRequest(
            new CompanyRequest(
                "Empresa Ejemplo SL",
                "Empresa Ejemplo",
                "B12345678",
                new AddressRequest("Calle Mayor 1", "28001", "Madrid", "Madrid", "ES"),
                "+34910000000",
                "administracion@example.com"),
            new AdministratorRequest("Administrador", "admin", "StrongPassword1!"));
    }

    private static InitializePlatformRequest CreateDifferentRequest()
    {
        return CreateRequest() with
        {
            Company = CreateRequest().Company with { LegalName = "Otra Empresa SL" }
        };
    }

    private static Task<HttpResponseMessage> PostInitializeAsync(
        HttpClient client,
        InitializePlatformRequest request,
        string idempotencyKey)
    {
        var message = JsonContent.Create(request);
        message.Headers.Add("Idempotency-Key", idempotencyKey);
        return client.PostAsync("/api/v1/platform/installation/initialize", message);
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPlatformInitializationStore>();
                services.AddPlatformInMemoryInfrastructureForTests();
            });
        });
    }
}
