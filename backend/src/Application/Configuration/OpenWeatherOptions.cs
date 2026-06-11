namespace EnviroWatch.Application.Configuration;

public class OpenWeatherOptions
{
    public const string SectionName = "OpenWeather";

    public string? ApiKey { get; set; }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);
}
