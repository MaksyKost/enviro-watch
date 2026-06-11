using EnviroWatch.Application.Configuration;
using EnviroWatch.Application.DTOs;
using EnviroWatch.Application.Interfaces;
using EnviroWatch.Application.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace EnviroWatch.Tests.Services;

public class FlightServiceTests
{
    [Fact]
    public async Task FetchCurrentFlightSnapshotsAsync_MapsAircraftMetrics()
    {
        var region = new MonitoredRegionOptions
        {
            Name = "Wroclaw,PL",
            Latitude = 51.1,
            Longitude = 17.0
        };

        var client = new Mock<IOpenSkyClient>();
        client
            .Setup(c => c.GetFlightsInAreaAsync(51.1, 17.0, FlightService.DefaultRadiusDegrees, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FlightData(DateTime.UtcNow, 7, 8500));

        var service = new FlightService(
            client.Object,
            Options.Create(new DataFetchOptions { Regions = [region] }),
            NullLogger<FlightService>.Instance);

        var snapshots = await service.FetchCurrentFlightSnapshotsAsync();

        Assert.Equal(2, snapshots.Count);
        Assert.All(snapshots, s => Assert.Equal("opensky", s.Source));
        Assert.Contains(snapshots, s => s.Metric == "aircraft_count" && s.Value == 7);
        Assert.Contains(snapshots, s => s.Metric == "avg_altitude" && s.Value == 8500);
    }
}
