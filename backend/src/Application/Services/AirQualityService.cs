using EnviroWatch.Application.Configuration;
using EnviroWatch.Application.Interfaces;
using EnviroWatch.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnviroWatch.Application.Services;

public class AirQualityService : IAirQualityService
{
    public const string SourceName = "openaq";

    private readonly IOpenAQClient _openAqClient;
    private readonly DataFetchOptions _options;
    private readonly ILogger<AirQualityService> _logger;

    public AirQualityService(
        IOpenAQClient openAqClient,
        IOptions<DataFetchOptions> options,
        ILogger<AirQualityService> logger)
    {
        _openAqClient = openAqClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DataSnapshot>> FetchCurrentAirQualitySnapshotsAsync(
        CancellationToken cancellationToken = default)
    {
        var snapshots = new List<DataSnapshot>();

        foreach (var region in _options.Regions)
        {
            try
            {
                var airQuality = await _openAqClient.GetLatestAirQualityAsync(
                    region.Latitude,
                    region.Longitude,
                    cancellationToken);

                if (airQuality is null)
                {
                    _logger.LogWarning("No air quality data returned for region {Region}", region.Name);
                    continue;
                }

                snapshots.AddRange(MapToSnapshots(region, airQuality));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch air quality for region {Region}", region.Name);
            }
        }

        return snapshots;
    }

    internal static IEnumerable<DataSnapshot> MapToSnapshots(
        MonitoredRegionOptions region,
        DTOs.AirQualityData airQuality)
    {
        var timestamp = SnapshotFactory.NormalizeTimestamp(airQuality.Timestamp);

        if (airQuality.Pm25.HasValue)
        {
            yield return SnapshotFactory.Create(SourceName, region, "pm25", airQuality.Pm25.Value, "µg/m³", timestamp);
        }

        if (airQuality.Pm10.HasValue)
        {
            yield return SnapshotFactory.Create(SourceName, region, "pm10", airQuality.Pm10.Value, "µg/m³", timestamp);
        }

        if (airQuality.Aqi.HasValue)
        {
            yield return SnapshotFactory.Create(SourceName, region, "aqi", airQuality.Aqi.Value, "AQI", timestamp);
        }
    }
}
