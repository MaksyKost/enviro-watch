using EnviroWatch.Application.Configuration;
using EnviroWatch.Application.DTOs;
using EnviroWatch.Application.Interfaces;
using EnviroWatch.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnviroWatch.Application.Services;

public class WeatherService : IWeatherService
{
    public const string SourceName = "openmeteo";

    private readonly IOpenMeteoClient _openMeteoClient;
    private readonly DataFetchOptions _options;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(
        IOpenMeteoClient openMeteoClient,
        IOptions<DataFetchOptions> options,
        ILogger<WeatherService> logger)
    {
        _openMeteoClient = openMeteoClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DataSnapshot>> FetchCurrentWeatherSnapshotsAsync(
        CancellationToken cancellationToken = default)
    {
        var snapshots = new List<DataSnapshot>();

        foreach (var region in _options.Regions)
        {
            try
            {
                var weather = await _openMeteoClient.GetCurrentWeatherAsync(
                    region.Latitude,
                    region.Longitude,
                    cancellationToken);

                if (weather is null)
                {
                    _logger.LogWarning(
                        "No weather data returned for region {Region}",
                        region.Name);
                    continue;
                }

                snapshots.AddRange(MapToSnapshots(region, weather, SourceName));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to fetch weather for region {Region}",
                    region.Name);
            }
        }

        return snapshots;
    }

    internal static IEnumerable<DataSnapshot> MapToSnapshots(
        MonitoredRegionOptions region,
        CurrentWeatherData weather,
        string source)
    {
        var timestamp = SnapshotFactory.NormalizeTimestamp(weather.Timestamp);

        yield return SnapshotFactory.Create(source, region, "temperature", weather.TemperatureCelsius, "°C", timestamp);
        yield return SnapshotFactory.Create(source, region, "humidity", weather.HumidityPercent, "%", timestamp);
        yield return SnapshotFactory.Create(source, region, "wind", weather.WindSpeedKmh, "km/h", timestamp);
    }
}
