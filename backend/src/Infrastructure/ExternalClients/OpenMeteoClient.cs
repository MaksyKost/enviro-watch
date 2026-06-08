using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EnviroWatch.Application.DTOs;
using EnviroWatch.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace EnviroWatch.Infrastructure.ExternalClients;

public class OpenMeteoClient : IOpenMeteoClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenMeteoClient> _logger;

    public OpenMeteoClient(HttpClient httpClient, ILogger<OpenMeteoClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CurrentWeatherData?> GetCurrentWeatherAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        var url =
            $"forecast?latitude={latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
            $"&longitude={longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
            "&current=temperature_2m,relative_humidity_2m,wind_speed_10m" +
            "&wind_speed_unit=kmh" +
            "&timezone=UTC";

        var response = await _httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Open-Meteo request failed with status {StatusCode} for ({Lat}, {Lon})",
                response.StatusCode,
                latitude,
                longitude);
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<OpenMeteoForecastResponse>(
            cancellationToken: cancellationToken);

        if (payload?.Current is null)
        {
            _logger.LogWarning(
                "Open-Meteo response missing current weather for ({Lat}, {Lon})",
                latitude,
                longitude);
            return null;
        }

        if (!DateTime.TryParse(payload.Current.Time, out var timestamp))
        {
            timestamp = DateTime.UtcNow;
        }

        return new CurrentWeatherData(
            timestamp,
            payload.Current.Temperature2m,
            payload.Current.RelativeHumidity2m,
            payload.Current.WindSpeed10m);
    }

    private sealed class OpenMeteoForecastResponse
    {
        [JsonPropertyName("current")]
        public OpenMeteoCurrent? Current { get; set; }
    }

    private sealed class OpenMeteoCurrent
    {
        [JsonPropertyName("time")]
        public string Time { get; set; } = string.Empty;

        [JsonPropertyName("temperature_2m")]
        public double Temperature2m { get; set; }

        [JsonPropertyName("relative_humidity_2m")]
        public double RelativeHumidity2m { get; set; }

        [JsonPropertyName("wind_speed_10m")]
        public double WindSpeed10m { get; set; }
    }
}
