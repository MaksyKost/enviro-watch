using EnviroWatch.Application.Configuration;
using EnviroWatch.Application.DTOs;
using EnviroWatch.Application.Interfaces;
using EnviroWatch.Application.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace EnviroWatch.Tests.Services;

public class AirQualityServiceTests
{
    [Fact]
    public async Task FetchCurrentAirQualitySnapshotsAsync_MapsPmMetrics()
    {
        var region = new MonitoredRegionOptions
        {
            Name = "Wroclaw,PL",
            Latitude = 51.1,
            Longitude = 17.0
        };

        var client = new Mock<IOpenAQClient>();
        client
            .Setup(c => c.GetLatestAirQualityAsync(51.1, 17.0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AirQualityData(DateTime.UtcNow, 12.5, 20.0, 45));

        var service = new AirQualityService(
            client.Object,
            Options.Create(new DataFetchOptions { Regions = [region] }),
            NullLogger<AirQualityService>.Instance);

        var snapshots = await service.FetchCurrentAirQualitySnapshotsAsync();

        Assert.Equal(3, snapshots.Count);
        Assert.All(snapshots, s => Assert.Equal("openaq", s.Source));
        Assert.Contains(snapshots, s => s.Metric == "pm25" && s.Value == 12.5);
        Assert.Contains(snapshots, s => s.Metric == "pm10" && s.Value == 20);
        Assert.Contains(snapshots, s => s.Metric == "aqi");
    }
}
