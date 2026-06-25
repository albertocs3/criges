using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace CriGes.Desktop.Services.Api;

public sealed class InstallationApiException : Exception
{
    public InstallationApiException(HttpStatusCode statusCode, string title, string detail)
        : base(string.IsNullOrWhiteSpace(detail) ? title : $"{title}: {detail}")
    {
        StatusCode = statusCode;
        Title = title;
        Detail = detail;
    }

    public HttpStatusCode StatusCode { get; }

    public string Title { get; }

    public string Detail { get; }

    public static async Task<InstallationApiException> FromResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var title = response.ReasonPhrase ?? "Error de API";
        var detail = $"La API devolvio {(int)response.StatusCode}.";
        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(content))
        {
            try
            {
                using var document = JsonDocument.Parse(content);
                if (document.RootElement.TryGetProperty("title", out var titleElement))
                {
                    title = titleElement.GetString() ?? title;
                }

                if (document.RootElement.TryGetProperty("detail", out var detailElement))
                {
                    detail = detailElement.GetString() ?? detail;
                }
            }
            catch (JsonException)
            {
                detail = content;
            }
        }

        return new InstallationApiException(response.StatusCode, title, detail);
    }
}
