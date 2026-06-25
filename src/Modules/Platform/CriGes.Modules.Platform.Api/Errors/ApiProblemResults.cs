using CriGes.Application.Abstractions;
using CriGes.SharedKernel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CriGes.Modules.Platform.Api.Errors;

public static class ApiProblemResults
{
    public static IResult FromError(AppError error, ICorrelationContext correlationContext)
    {
        var statusCode = error.Code switch
        {
            "AUTH.INVALID_CREDENTIALS" => StatusCodes.Status401Unauthorized,
            "AUTH.INVALID_TOKEN" => StatusCodes.Status401Unauthorized,
            "AUTH.ACCOUNT_DISABLED" => StatusCodes.Status403Forbidden,
            "AUTH.ROLE_DISABLED" => StatusCodes.Status403Forbidden,
            "AUTH.PERMISSION_DENIED" => StatusCodes.Status403Forbidden,
            "AUTH.ACTIVE_SESSION_EXISTS" => StatusCodes.Status409Conflict,
            "AUTH.ACCOUNT_LOCKED" => StatusCodes.Status423Locked,
            "AUTH.TOO_MANY_ATTEMPTS" => StatusCodes.Status429TooManyRequests,
            "PLATFORM.CLIENT_VERSION_UNSUPPORTED" => StatusCodes.Status426UpgradeRequired,
            "PLATFORM.NOT_INITIALIZED" => StatusCodes.Status409Conflict,
            "PLATFORM.ALREADY_INITIALIZED" => StatusCodes.Status409Conflict,
            "SECURITY.USERNAME_ALREADY_RESERVED" => StatusCodes.Status409Conflict,
            "IDEMPOTENCY.KEY_REUSED_WITH_DIFFERENT_REQUEST" => StatusCodes.Status409Conflict,
            "IDEMPOTENCY.REQUEST_IN_PROGRESS" => StatusCodes.Status409Conflict,
            "PLATFORM.INVALID_TAX_ID" => StatusCodes.Status422UnprocessableEntity,
            "SECURITY.PASSWORD_POLICY_FAILED" => StatusCodes.Status422UnprocessableEntity,
            "SECURITY.ROLE_INACTIVE" => StatusCodes.Status422UnprocessableEntity,
            "SECURITY.ROLE_NOT_FOUND" => StatusCodes.Status422UnprocessableEntity,
            "SECURITY.UNKNOWN_PERMISSION" => StatusCodes.Status422UnprocessableEntity,
            "PLATFORM.VALIDATION_FAILED" => StatusCodes.Status422UnprocessableEntity,
            "AUTH.VALIDATION_FAILED" => StatusCodes.Status422UnprocessableEntity,
            "IDEMPOTENCY.KEY_REQUIRED" => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Problem(new ProblemDetails
        {
            Title = error.Message,
            Status = statusCode,
            Extensions =
            {
                ["code"] = error.Code,
                ["correlationId"] = correlationContext.CorrelationId.ToString("D")
            }
        });
    }
}
