using EnviroWatch.Application.DTOs;
using EnviroWatch.Application.Interfaces;
using EnviroWatch.Application.Services;
using EnviroWatch.Domain.Models;
using Moq;
using Xunit;

namespace EnviroWatch.Tests.Services;

public class ObservationServiceTests
{
    [Fact]
    public async Task CreateAsync_PersistsObservationAndSnapshot()
    {
        var userId = Guid.NewGuid();
        var observationRepository = new Mock<IObservationRepository>();
        observationRepository
            .Setup(r => r.CreateAsync(It.IsAny<ManualObservation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ManualObservation observation, CancellationToken _) => observation);

        var snapshotRepository = new Mock<IDataSnapshotRepository>();
        snapshotRepository
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<DataSnapshot>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new ObservationService(observationRepository.Object, snapshotRepository.Object);

        var result = await service.CreateAsync(
            userId,
            new CreateObservationRequest(
                "Wroclaw,PL",
                "temperature",
                22.5,
                "°C",
                51.1,
                17.0,
                "Field reading",
                null));

        Assert.Equal("Wroclaw,PL", result.Region);
        Assert.Equal("temperature", result.Metric);
        snapshotRepository.Verify(
            r => r.AddRangeAsync(
                It.Is<IEnumerable<DataSnapshot>>(snapshots =>
                    snapshots.Any(s => s.Source == "manual" && s.Metric == "temperature")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
