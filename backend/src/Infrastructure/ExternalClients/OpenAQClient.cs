using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EnviroWatch.Application.DTOs;
using EnviroWatch.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace EnviroWatch.Infrastructure.ExternalClients;

public class OpenAQClient : IOpenAQClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAQClient> _logger;

    public OpenAQClient(HttpClient httpClient, ILogger<OpenAQClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AirQualityData?> GetLatestAirQualityAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        var url =
            $"latest?coordinates={latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
            $"{longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&radius=25000&limit=1";

        var response = await _httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "OpenAQ request failed with status {StatusCode} for ({Lat}, {Lon})",
                response.StatusCode,
                latitude,
                longitude);
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<OpenAqLatestResponse>(cancellationToken);
        var measurements = payload?.Results?.FirstOrDefault()?.Measurements;

        if (measurements is null || measurements.Count == 0)
        {
            _logger.LogWarning("OpenAQ response missing measurements for ({Lat}, {Lon})", latitude, longitude);
            return null;
        }

        double? pm25 = null;
        double? pm10 = null;
        DateTime? latestTime = null;

        foreach (var measurement in measurements)
        {
            var parameter = measurement.Parameter?.ToLowerInvariant();
            if (parameter is "pm25" or "pm2.5")
            {
                pm25 = measurement.Value;
            }
            else if (parameter is "pm10")
            {
                pm10 = measurement.Value;
            }

            if (DateTime.TryParse(measurement.LastUpdated, out var updated))
            {
                latestTime = latestTime is null || updated > latestTime
                    ? updated
                    : latestTime;
            }
        }

        if (pm25 is null && pm10 is null)
        {
            return null;
        }

        double? aqi = pm25.HasValue ? EstimateUsAqiFromPm25(pm25.Value) : null;

        return new AirQualityData(
            latestTime ?? DateTime.UtcNow,
            pm25,
            pm10,
            aqi);
    }

    private static double EstimateUsAqiFromPm25(double pm25) =>
        pm25 switch
        {
            <= 12 => pm25 / 12.0 * 50,
            <= 35.4 => 50 + (pm25 - 12.1) / (35.4 - 12.1) * 50,
            <= 55.4 => 100 + (pm25 - 35.5) / (55.4 - 35.5) * 50,
            <= 150.4 => 150 + (pm25 - 55.5) / (150.4 - 55.5) * 50,
            <= 250.4 => 200 + (pm25 - 150.5) / (250.4 - 150.5) * 100,
            _ => 300 + Math.Min((pm25 - 250.5) / (500 - 250.5) * 200, 200)
        };

    private sealed class OpenAqLatestResponse
    {
        [JsonPropertyName("results")]
        public List<OpenAqResult>? Results { get; set; }
    }

    private sealed class OpenAqResult
    {
        [JsonPropertyName("measurements")]
        public List<OpenAqMeasurement>? Measurements { get; set; }
    }

    private sealed class OpenAqMeasurement
    {
        [JsonPropertyName("parameter")]
        public string? Parameter { get; set; }

        [JsonPropertyName("value")]
        public double Value { get; set; }

        [JsonPropertyName("lastUpdated")]
        public string? LastUpdated { get; set; }
    }
}
