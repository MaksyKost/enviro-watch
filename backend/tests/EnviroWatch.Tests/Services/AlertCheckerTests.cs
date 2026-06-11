using EnviroWatch.Application.Configuration;
using EnviroWatch.Application.Interfaces;
using EnviroWatch.Application.Services;
using EnviroWatch.Domain.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace EnviroWatch.Tests.Services;

public class AlertCheckerTests
{
    [Theory]
    [InlineData(36, 35, AlertCondition.Above, true)]
    [InlineData(35, 35, AlertCondition.Above, true)]
    [InlineData(34, 35, AlertCondition.Above, false)]
    [InlineData(10, 15, AlertCondition.Below, true)]
    [InlineData(15, 15, AlertCondition.Below, true)]
    [InlineData(16, 15, AlertCondition.Below, false)]
    public void IsTriggered_EvaluatesThresholdCorrectly(
        double value,
        double threshold,
        AlertCondition condition,
        bool expected)
    {
        Assert.Equal(expected, AlertCheckerService.IsTriggered(value, threshold, condition));
    }

    [Fact]
    public async Task ProcessActiveAlertsAsync_TriggersAlert_WhenThresholdExceeded()
    {
        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Metric = "temperature",
            Region = "Wroclaw,PL",
            Threshold = 30,
            Condition = AlertCondition.Above,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var snapshot = new DataSnapshot
        {
            Id = Guid.NewGuid(),
            Source = "openmeteo",
            Region = "Wroclaw,PL",
            Metric = "temperature",
            Value = 35,
            Timestamp = DateTime.UtcNow
        };

        var alertRepository = new Mock<IAlertRepository>();
        alertRepository.Setup(r => r.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { alert });
        alertRepository.Setup(r => r.CreateLogAsync(It.IsAny<AlertLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AlertLog log, CancellationToken _) => log);
        alertRepository.Setup(r => r.UpdateLastTriggeredAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var snapshotRepository = new Mock<IDataSnapshotRepository>();
        snapshotRepository.Setup(r => r.GetLatestAsync("Wroclaw,PL", "temperature", It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshot);

        var notifier = new Mock<IAlertNotifier>();

        var service = new AlertCheckerService(
            alertRepository.Object,
            snapshotRepository.Object,
            notifier.Object,
            Options.Create(new AlertOptions { CooldownMinutes = 5 }),
            NullLogger<AlertCheckerService>.Instance);

        var triggered = await service.ProcessActiveAlertsAsync();

        Assert.Equal(1, triggered);
        alertRepository.Verify(r => r.CreateLogAsync(It.IsAny<AlertLog>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessActiveAlertsAsync_SkipsAlert_InCooldown()
    {
        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Metric = "temperature",
            Region = "Wroclaw,PL",
            Threshold = 30,
            Condition = AlertCondition.Above,
            IsActive = true,
            LastTriggeredAt = DateTime.UtcNow.AddMinutes(-1),
            CreatedAt = DateTime.UtcNow
        };

        var alertRepository = new Mock<IAlertRepository>();
        alertRepository.Setup(r => r.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { alert });

        var service = new AlertCheckerService(
            alertRepository.Object,
            Mock.Of<IDataSnapshotRepository>(),
            Mock.Of<IAlertNotifier>(),
            Options.Create(new AlertOptions { CooldownMinutes = 5 }),
            NullLogger<AlertCheckerService>.Instance);

        var triggered = await service.ProcessActiveAlertsAsync();

        Assert.Equal(0, triggered);
        alertRepository.Verify(r => r.CreateLogAsync(It.IsAny<AlertLog>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
