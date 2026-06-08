using EnviroWatch.Application.Services;
using EnviroWatch.Domain.Models;
using Xunit;

namespace EnviroWatch.Tests.Services;

public class DataUpdateMapperTests
{
    [Fact]
    public void FromWeatherSnapshots_GroupsMetricsByRegion()
    {
        var timestamp = new DateTime(2026, 6, 7, 12, 0, 30, DateTimeKind.Utc);
        var snapshots = new[]
        {
            CreateSnapshot("Wroclaw,PL", "temperature", 18.4, timestamp),
            CreateSnapshot("Wroclaw,PL", "humidity", 65, timestamp),
            CreateSnapshot("Wroclaw,PL", "wind", 12, timestamp)
        };

        var updates = DataUpdateMapper.FromWeatherSnapshots(snapshots);

        var update = Assert.Single(updates);
        Assert.Equal("weather", update.Type);
        Assert.Equal("Wroclaw,PL", update.Region);
        Assert.Equal(18.4, update.Data.Temperature);
        Assert.Equal(65, update.Data.Humidity);
        Assert.Equal(12, update.Data.Wind);
        Assert.Equal(timestamp, update.Timestamp);
    }

    [Fact]
    public void FromWeatherSnapshots_SkipsIncompleteRegions()
    {
        var snapshots = new[]
        {
            CreateSnapshot("Wroclaw,PL", "temperature", 18.4, DateTime.UtcNow)
        };

        var updates = DataUpdateMapper.FromWeatherSnapshots(snapshots);

        Assert.Empty(updates);
    }

    private static DataSnapshot CreateSnapshot(
        string region,
        string metric,
        double value,
        DateTime timestamp) =>
        new()
        {
            Id = Guid.NewGuid(),
            Source = "openmeteo",
            Region = region,
            Metric = metric,
            Value = value,
            Timestamp = timestamp
        };
}
