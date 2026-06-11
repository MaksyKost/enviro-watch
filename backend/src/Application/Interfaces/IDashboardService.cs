using EnviroWatch.Application.DTOs;

namespace EnviroWatch.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> CreateDashboardAsync(
        Guid userId,
        CreateDashboardRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DashboardDto>> GetUserDashboardsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<DashboardDto?> GetDashboardAsync(
        Guid userId,
        Guid dashboardId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    Task<DashboardDto> UpdateDashboardAsync(
        Guid userId,
        Guid dashboardId,
        UpdateDashboardRequest request,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    Task DeleteDashboardAsync(
        Guid userId,
        Guid dashboardId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    Task<WidgetDto> AddWidgetAsync(
        Guid userId,
        Guid dashboardId,
        CreateWidgetRequest request,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    Task<WidgetDto> UpdateWidgetAsync(
        Guid userId,
        Guid dashboardId,
        Guid widgetId,
        UpdateWidgetRequest request,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    Task DeleteWidgetAsync(
        Guid userId,
        Guid dashboardId,
        Guid widgetId,
        bool isAdmin,
        CancellationToken cancellationToken = default);
}
