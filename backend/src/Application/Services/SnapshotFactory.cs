using EnviroWatch.Application.Configuration;
using EnviroWatch.Domain.Models;

namespace EnviroWatch.Application.Services;

public static class SnapshotFactory
{
    public static DataSnapshot Create(
        string source,
        MonitoredRegionOptions region,
        string metric,
        double value,
        string unit,
        DateTime timestamp) =>
        new()
        {
            Id = Guid.NewGuid(),
            Source = source,
            Region = region.Name,
            Metric = metric,
            Value = Math.Round(value, 1),
            Unit = unit,
            Lat = region.Latitude,
            Lon = region.Longitude,
            Timestamp = NormalizeTimestamp(timestamp)
        };

    public static DateTime NormalizeTimestamp(DateTime timestamp) =>
        timestamp.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(timestamp, DateTimeKind.Utc)
            : timestamp.ToUniversalTime();
}
