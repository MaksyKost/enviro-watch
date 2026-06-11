using EnviroWatch.Application.DTOs;
using EnviroWatch.Application.Interfaces;
using EnviroWatch.Application.Services;
using EnviroWatch.Domain.Models;
using Moq;
using Xunit;

namespace EnviroWatch.Tests.Services;

public class DashboardServiceTests
{
    [Fact]
    public async Task CreateDashboardAsync_CreatesDashboardForUser()
    {
        var userId = Guid.NewGuid();
        var repository = new Mock<IDashboardRepository>();
        repository
            .Setup(r => r.CreateAsync(It.IsAny<Dashboard>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Dashboard dashboard, CancellationToken _) => dashboard);

        var service = new DashboardService(repository.Object);

        var result = await service.CreateDashboardAsync(
            userId,
            new CreateDashboardRequest("My Dashboard", "Weather overview"));

        Assert.Equal("My Dashboard", result.Name);
        Assert.Empty(result.Widgets);
        repository.Verify(
            r => r.CreateAsync(It.Is<Dashboard>(d => d.UserId == userId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void ValidateWidgetRequest_RejectsInvalidJson()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            DashboardService.ValidateWidgetRequest("temperature", "Wroclaw,PL", "{invalid"));

        Assert.Contains("JSON", ex.Message);
    }

    [Fact]
    public void ValidateWidgetRequest_AcceptsValidJson()
    {
        var exception = Record.Exception(() =>
            DashboardService.ValidateWidgetRequest("temperature", "Wroclaw,PL", "{\"color\":\"blue\"}"));

        Assert.Null(exception);
    }
}
