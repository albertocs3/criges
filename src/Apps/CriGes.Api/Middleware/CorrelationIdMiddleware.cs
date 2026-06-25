using CriGes.Application.Abstractions;

namespace CriGes.Api.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context, ICorrelationContext correlationContext)
    {
        var correlationId = ResolveCorrelationId(context);
        correlationContext.CorrelationId = correlationId;
        context.Response.Headers[HeaderName] = correlationId.ToString("D");

        await next(context);
    }

    private static Guid ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var values) &&
            Guid.TryParse(values.FirstOrDefault(), out var parsed))
        {
            return parsed;
        }

        return Guid.NewGuid();
    }
}
