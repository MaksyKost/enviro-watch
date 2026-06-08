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

                snapshots.AddRange(MapToSnapshots(region, weather));
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
        CurrentWeatherData weather)
    {
        var timestamp = weather.Timestamp.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(weather.Timestamp, DateTimeKind.Utc)
            : weather.Timestamp.ToUniversalTime();

        yield return CreateSnapshot(region, "temperature", weather.TemperatureCelsius, "°C", timestamp);
        yield return CreateSnapshot(region, "humidity", weather.HumidityPercent, "%", timestamp);
        yield return CreateSnapshot(region, "wind", weather.WindSpeedKmh, "km/h", timestamp);
    }

    private static DataSnapshot CreateSnapshot(
        MonitoredRegionOptions region,
        string metric,
        double value,
        string unit,
        DateTime timestamp) =>
        new()
        {
            Id = Guid.NewGuid(),
            Source = SourceName,
            Region = region.Name,
            Metric = metric,
            Value = Math.Round(value, 1),
            Unit = unit,
            Lat = region.Latitude,
            Lon = region.Longitude,
            Timestamp = timestamp
        };
}
