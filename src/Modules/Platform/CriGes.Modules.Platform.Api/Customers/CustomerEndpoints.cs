using CriGes.Application.Abstractions;
using CriGes.Modules.Platform.Api.Auth;
using CriGes.Modules.Platform.Api.Errors;
using CriGes.Modules.Platform.Application.Auth;
using CriGes.Modules.Platform.Application.Customers;
using CriGes.Modules.Platform.Contracts.Auth;
using CriGes.Modules.Platform.Contracts.Customers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CriGes.Modules.Platform.Api.Customers;

public static class CustomerEndpoints
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/customers");

        group.MapGet("/", ListCustomersAsync)
            .RequirePermission(PlatformPermissionNames.ViewCustomers)
            .WithName("ListCustomers");

        group.MapPost("/", CreateCustomerAsync)
            .RequirePermission(PlatformPermissionNames.ManageCustomers)
            .WithName("CreateCustomer");

        return endpoints;
    }

    private static async Task<IResult> ListCustomersAsync(
        ListCustomersHandler handler,
        CancellationToken cancellationToken)
    {
        var customers = await handler.HandleAsync(cancellationToken).ConfigureAwait(false);
        return Results.Ok(customers.Select(ToResponse));
    }

    private static async Task<IResult> CreateCustomerAsync(
        CreateCustomerRequest request,
        CreateCustomerHandler handler,
        IAuthSessionContext sessionContext,
        ICorrelationContext correlationContext,
        CancellationToken cancellationToken)
    {
        var result = await handler
            .HandleAsync(
                new CreateCustomerCommand(request.Name, request.TaxId, request.Email, request.Phone),
                sessionContext.Session?.UserId,
                cancellationToken)
            .ConfigureAwait(false);

        if (result.IsFailure)
        {
            return ApiProblemResults.FromError(result.Error, correlationContext);
        }

        var response = ToResponse(result.Value);
        return Results.Created($"/api/v1/customers/{response.Id:D}", response);
    }

    private static CustomerSummaryResponse ToResponse(CustomerSummary customer)
    {
        return new CustomerSummaryResponse(
            customer.Id,
            customer.Name,
            customer.TaxId,
            customer.Email,
            customer.Phone,
            customer.Status == 1 ? "active" : "inactive",
            new DateTimeOffset(DateTime.SpecifyKind(customer.CreatedAtUtc, DateTimeKind.Utc)));
    }
}
