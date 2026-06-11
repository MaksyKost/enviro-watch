using EnviroWatch.Application.Configuration;
using EnviroWatch.Application.Interfaces;
using EnviroWatch.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnviroWatch.Application.Services;

public class OpenWeatherService : IOpenWeatherService
{
    public const string SourceName = "openweather";

    private readonly IOpenWeatherClient _openWeatherClient;
    private readonly DataFetchOptions _dataFetchOptions;
    private readonly OpenWeatherOptions _openWeatherOptions;
    private readonly ILogger<OpenWeatherService> _logger;

    public OpenWeatherService(
        IOpenWeatherClient openWeatherClient,
        IOptions<DataFetchOptions> dataFetchOptions,
        IOptions<OpenWeatherOptions> openWeatherOptions,
        ILogger<OpenWeatherService> logger)
    {
        _openWeatherClient = openWeatherClient;
        _dataFetchOptions = dataFetchOptions.Value;
        _openWeatherOptions = openWeatherOptions.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DataSnapshot>> FetchCurrentWeatherSnapshotsAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_openWeatherOptions.IsConfigured)
        {
            return [];
        }

        var snapshots = new List<DataSnapshot>();

        foreach (var region in _dataFetchOptions.Regions)
        {
            try
            {
                var weather = await _openWeatherClient.GetCurrentWeatherAsync(
                    region.Latitude,
                    region.Longitude,
                    cancellationToken);

                if (weather is null)
                {
                    continue;
                }

                var timestamp = SnapshotFactory.NormalizeTimestamp(weather.Timestamp);
                snapshots.Add(SnapshotFactory.Create(SourceName, region, "temperature", weather.TemperatureCelsius, "°C", timestamp));
                snapshots.Add(SnapshotFactory.Create(SourceName, region, "humidity", weather.HumidityPercent, "%", timestamp));
                snapshots.Add(SnapshotFactory.Create(SourceName, region, "wind", weather.WindSpeedKmh, "km/h", timestamp));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch OpenWeather data for {Region}", region.Name);
            }
        }

        return snapshots;
    }
}
