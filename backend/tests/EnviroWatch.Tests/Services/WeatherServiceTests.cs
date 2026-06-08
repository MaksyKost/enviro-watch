using EnviroWatch.Application.Configuration;
using EnviroWatch.Application.DTOs;
using EnviroWatch.Application.Interfaces;
using EnviroWatch.Application.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace EnviroWatch.Tests.Services;

public class WeatherServiceTests
{
    [Fact]
    public async Task FetchCurrentWeatherSnapshotsAsync_MapsMetricsForEachRegion()
    {
        var region = new MonitoredRegionOptions
        {
            Name = "Wroclaw,PL",
            Latitude = 51.1,
            Longitude = 17.0
        };

        var options = Options.Create(new DataFetchOptions
        {
            Regions = [region]
        });

        var weather = new CurrentWeatherData(
            new DateTime(2026, 6, 7, 12, 0, 0, DateTimeKind.Utc),
            18.4,
            65,
            12);

        var client = new Mock<IOpenMeteoClient>();
        client
            .Setup(c => c.GetCurrentWeatherAsync(51.1, 17.0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(weather);

        var service = new WeatherService(
            client.Object,
            options,
            NullLogger<WeatherService>.Instance);

        var snapshots = await service.FetchCurrentWeatherSnapshotsAsync();

        Assert.Equal(3, snapshots.Count);
        Assert.All(snapshots, s => Assert.Equal("openmeteo", s.Source));
        Assert.All(snapshots, s => Assert.Equal("Wroclaw,PL", s.Region));
        Assert.Contains(snapshots, s => s.Metric == "temperature" && s.Value == 18.4);
        Assert.Contains(snapshots, s => s.Metric == "humidity" && s.Value == 65);
        Assert.Contains(snapshots, s => s.Metric == "wind" && s.Value == 12);
    }

    [Fact]
    public async Task FetchCurrentWeatherSnapshotsAsync_ContinuesWhenOneRegionFails()
    {
        var options = Options.Create(new DataFetchOptions
        {
            Regions =
            [
                new MonitoredRegionOptions { Name = "Wroclaw,PL", Latitude = 51.1, Longitude = 17.0 },
                new MonitoredRegionOptions { Name = "Warsaw,PL", Latitude = 52.2, Longitude = 21.0 }
            ]
        });

        var weather = new CurrentWeatherData(DateTime.UtcNow, 20, 50, 5);

        var client = new Mock<IOpenMeteoClient>();
        client
            .Setup(c => c.GetCurrentWeatherAsync(51.1, 17.0, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("API unavailable"));
        client
            .Setup(c => c.GetCurrentWeatherAsync(52.2, 21.0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(weather);

        var service = new WeatherService(
            client.Object,
            options,
            NullLogger<WeatherService>.Instance);

        var snapshots = await service.FetchCurrentWeatherSnapshotsAsync();

        Assert.Equal(3, snapshots.Count);
        Assert.All(snapshots, s => Assert.Equal("Warsaw,PL", s.Region));
    }
}
