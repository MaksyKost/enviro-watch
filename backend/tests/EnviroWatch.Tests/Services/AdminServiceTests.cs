using EnviroWatch.Application.Interfaces;
using EnviroWatch.Application.Services;
using Moq;
using Xunit;

namespace EnviroWatch.Tests.Services;

public class AdminServiceTests
{
    [Fact]
    public async Task GetStatsAsync_AggregatesCounts()
    {
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(r => r.CountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(3);

        var snapshotRepository = new Mock<IDataSnapshotRepository>();
        snapshotRepository.Setup(r => r.CountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(100);

        var alertRepository = new Mock<IAlertRepository>();
        alertRepository.Setup(r => r.CountActiveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(2);

        var dashboardRepository = new Mock<IDashboardRepository>();
        dashboardRepository.Setup(r => r.CountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(5);

        var service = new AdminService(
            userRepository.Object,
            snapshotRepository.Object,
            alertRepository.Object,
            dashboardRepository.Object);

        var stats = await service.GetStatsAsync();

        Assert.Equal(3, stats.Users);
        Assert.Equal(100, stats.Snapshots);
        Assert.Equal(2, stats.ActiveAlerts);
        Assert.Equal(5, stats.Dashboards);
    }
}
