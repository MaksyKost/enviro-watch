using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnviroWatch.Application.DTOs;
using EnviroWatch.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace EnviroWatch.Infrastructure.ExternalClients;

public class OpenSkyClient : IOpenSkyClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenSkyClient> _logger;

    public OpenSkyClient(HttpClient httpClient, ILogger<OpenSkyClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<FlightData?> GetFlightsInAreaAsync(
        double latitude,
        double longitude,
        double radiusDegrees,
        CancellationToken cancellationToken = default)
    {
        var lamin = latitude - radiusDegrees;
        var lamax = latitude + radiusDegrees;
        var lomin = longitude - radiusDegrees;
        var lomax = longitude + radiusDegrees;

        var url =
            $"states/all?lamin={lamin.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
            $"&lamax={lamax.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
            $"&lomin={lomin.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
            $"&lomax={lomax.ToString(System.Globalization.CultureInfo.InvariantCulture)}";

        var response = await _httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "OpenSky request failed with status {StatusCode} for bbox around ({Lat}, {Lon})",
                response.StatusCode,
                latitude,
                longitude);
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<OpenSkyResponse>(cancellationToken);
        if (payload?.States is null)
        {
            return null;
        }

        var airborne = payload.States
            .Where(IsAirborne)
            .ToList();

        var timestamp = payload.Time > 0
            ? DateTimeOffset.FromUnixTimeSeconds(payload.Time).UtcDateTime
            : DateTime.UtcNow;

        double? averageAltitude = null;
        var altitudes = airborne
            .Select(state => ReadDouble(state[7]))
            .Where(altitude => altitude > 0)
            .Select(altitude => altitude!.Value)
            .ToList();

        if (altitudes.Count > 0)
        {
            averageAltitude = altitudes.Average();
        }

        return new FlightData(timestamp, airborne.Count, averageAltitude);
    }

    private static bool IsAirborne(List<object?> state) =>
        state.Count >= 9 && !ReadBool(state[8]);

    private static bool ReadBool(object? value) => value switch
    {
        bool boolean => boolean,
        JsonElement element when element.ValueKind == JsonValueKind.True => true,
        JsonElement element when element.ValueKind == JsonValueKind.False => false,
        _ => false
    };

    private static double? ReadDouble(object? value) => value switch
    {
        double number => number,
        float number => number,
        int number => number,
        long number => number,
        JsonElement element when element.ValueKind == JsonValueKind.Number => element.GetDouble(),
        _ => null
    };

    private sealed class OpenSkyResponse
    {
        [JsonPropertyName("time")]
        public long Time { get; set; }

        [JsonPropertyName("states")]
        public List<List<object?>>? States { get; set; }
    }
}
