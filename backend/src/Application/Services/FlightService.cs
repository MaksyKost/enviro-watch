using EnviroWatch.Application.Configuration;
using EnviroWatch.Application.Interfaces;
using EnviroWatch.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnviroWatch.Application.Services;

public class FlightService : IFlightService
{
    public const string SourceName = "opensky";
    public const double DefaultRadiusDegrees = 0.15;

    private readonly IOpenSkyClient _openSkyClient;
    private readonly DataFetchOptions _options;
    private readonly ILogger<FlightService> _logger;

    public FlightService(
        IOpenSkyClient openSkyClient,
        IOptions<DataFetchOptions> options,
        ILogger<FlightService> logger)
    {
        _openSkyClient = openSkyClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DataSnapshot>> FetchCurrentFlightSnapshotsAsync(
        CancellationToken cancellationToken = default)
    {
        var snapshots = new List<DataSnapshot>();

        foreach (var region in _options.Regions)
        {
            try
            {
                var flights = await _openSkyClient.GetFlightsInAreaAsync(
                    region.Latitude,
                    region.Longitude,
                    DefaultRadiusDegrees,
                    cancellationToken);

                if (flights is null)
                {
                    _logger.LogWarning("No flight data returned for region {Region}", region.Name);
                    continue;
                }

                snapshots.AddRange(MapToSnapshots(region, flights));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch flight data for region {Region}", region.Name);
            }
        }

        return snapshots;
    }

    internal static IEnumerable<DataSnapshot> MapToSnapshots(
        MonitoredRegionOptions region,
        DTOs.FlightData flights)
    {
        var timestamp = SnapshotFactory.NormalizeTimestamp(flights.Timestamp);

        yield return SnapshotFactory.Create(
            SourceName,
            region,
            "aircraft_count",
            flights.AircraftCount,
            "count",
            timestamp);

        if (flights.AverageAltitudeMeters.HasValue)
        {
            yield return SnapshotFactory.Create(
                SourceName,
                region,
                "avg_altitude",
                flights.AverageAltitudeMeters.Value,
                "m",
                timestamp);
        }
    }
}
