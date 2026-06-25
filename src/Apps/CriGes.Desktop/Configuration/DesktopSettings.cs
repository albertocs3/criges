using System.IO;
using System.Text.Json;

namespace CriGes.Desktop.Configuration;

public sealed class DesktopSettings
{
    private const string DefaultApiBaseUrl = "http://localhost:5099/";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public ApiSettings Api { get; init; } = new();

    public static DesktopSettings Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path))
        {
            return new DesktopSettings();
        }

        try
        {
            using var stream = File.OpenRead(path);
            return JsonSerializer.Deserialize<DesktopSettings>(stream, JsonOptions) ?? new DesktopSettings();
        }
        catch (JsonException)
        {
            return new DesktopSettings();
        }
        catch (IOException)
        {
            return new DesktopSettings();
        }
    }

    public sealed class ApiSettings
    {
        public string BaseUrl { get; init; } = DefaultApiBaseUrl;
    }
}
