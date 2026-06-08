using EnviroWatch.Application.DTOs;
using EnviroWatch.Application.Interfaces;
using EnviroWatch.Application.Services;
using EnviroWatch.Domain.Models;
using Moq;
using Xunit;

namespace EnviroWatch.Tests.Services;

public class DataSnapshotServiceTests
{
    [Fact]
    public async Task GetSnapshotsAsync_ClampsPageSize_ToMaximum()
    {
        var repository = new Mock<IDataSnapshotRepository>();
        repository
            .Setup(r => r.GetFilteredAsync(It.IsAny<DataSnapshotQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Array.Empty<DataSnapshot>(), 0));

        var service = new DataSnapshotService(repository.Object);

        var result = await service.GetSnapshotsAsync(
            new DataSnapshotQuery(null, null, null, null, null, Page: 1, PageSize: 500));

        Assert.Equal(200, result.PageSize);
        repository.Verify(r => r.GetFilteredAsync(
            It.Is<DataSnapshotQuery>(q => q.PageSize == 200),
            It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task GetSnapshotsAsync_MapsEntitiesToDtos()
    {
        var timestamp = new DateTime(2026, 6, 7, 12, 0, 0, DateTimeKind.Utc);
        var snapshot = new DataSnapshot
        {
            Id = Guid.NewGuid(),
            Source = "openmeteo",
            Region = "Wroclaw,PL",
            Metric = "temperature",
            Value = 18.4,
            Unit = "°C",
            Lat = 51.1,
            Lon = 17.0,
            Timestamp = timestamp
        };

        var repository = new Mock<IDataSnapshotRepository>();
        repository
            .Setup(r => r.GetFilteredAsync(It.IsAny<DataSnapshotQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { snapshot }, 1));

        var service = new DataSnapshotService(repository.Object);

        var result = await service.GetSnapshotsAsync(
            new DataSnapshotQuery("PL", "temperature", null, null, null));

        Assert.Equal(1, result.Total);
        var item = Assert.Single(result.Items);
        Assert.Equal("openmeteo", item.Source);
        Assert.Equal("temperature", item.Metric);
        Assert.Equal(18.4, item.Value);
        Assert.Equal("Wroclaw,PL", item.Region);
    }
}
