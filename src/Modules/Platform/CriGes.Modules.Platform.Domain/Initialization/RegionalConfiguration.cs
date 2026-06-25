namespace CriGes.Modules.Platform.Domain.Initialization;

public sealed record RegionalConfiguration(string LanguageCode, string CurrencyCode, string TimeZoneId)
{
    public static RegionalConfiguration InitialSpain()
    {
        return new RegionalConfiguration("es", "EUR", "Europe/Madrid");
    }
}
