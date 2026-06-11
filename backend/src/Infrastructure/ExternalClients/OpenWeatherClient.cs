using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EnviroWatch.Application.Configuration;
using EnviroWatch.Application.DTOs;
using EnviroWatch.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnviroWatch.Infrastructure.ExternalClients;

public class OpenWeatherClient : IOpenWeatherClient
{
    private readonly HttpClient _httpClient;
    private readonly OpenWeatherOptions _options;
    private readonly ILogger<OpenWeatherClient> _logger;

    public OpenWeatherClient(
        HttpClient httpClient,
        IOptions<OpenWeatherOptions> options,
        ILogger<OpenWeatherClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<CurrentWeatherData?> GetCurrentWeatherAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        if (!_options.IsConfigured)
        {
            return null;
        }

        var url =
            $"weather?lat={latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
            $"&lon={longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
            $"&appid={_options.ApiKey}&units=metric";

        var response = await _httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "OpenWeather request failed with status {StatusCode} for ({Lat}, {Lon})",
                response.StatusCode,
                latitude,
                longitude);
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<OpenWeatherResponse>(cancellationToken);
        if (payload?.Main is null)
        {
            return null;
        }

        var timestamp = payload.Dt > 0
            ? DateTimeOffset.FromUnixTimeSeconds(payload.Dt).UtcDateTime
            : DateTime.UtcNow;

        var windKmh = (payload.Wind?.Speed ?? 0) * 3.6;

        return new CurrentWeatherData(
            timestamp,
            payload.Main.Temp,
            payload.Main.Humidity,
            windKmh);
    }

    private sealed class OpenWeatherResponse
    {
        [JsonPropertyName("main")]
        public OpenWeatherMain? Main { get; set; }

        [JsonPropertyName("wind")]
        public OpenWeatherWind? Wind { get; set; }

        [JsonPropertyName("dt")]
        public long Dt { get; set; }
    }

    private sealed class OpenWeatherMain
    {
        [JsonPropertyName("temp")]
        public double Temp { get; set; }

        [JsonPropertyName("humidity")]
        public double Humidity { get; set; }
    }

    private sealed class OpenWeatherWind
    {
        [JsonPropertyName("speed")]
        public double Speed { get; set; }
    }
}
