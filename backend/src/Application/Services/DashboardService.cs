using System.Text.Json;
using EnviroWatch.Application.DTOs;
using EnviroWatch.Application.Interfaces;
using EnviroWatch.Domain.Models;

namespace EnviroWatch.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;

    public DashboardService(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<DashboardDto> CreateDashboardAsync(
        Guid userId,
        CreateDashboardRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var dashboard = new Dashboard
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        await _dashboardRepository.CreateAsync(dashboard, cancellationToken);
        return MapDashboard(dashboard, []);
    }

    public async Task<IReadOnlyList<DashboardDto>> GetUserDashboardsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var dashboards = await _dashboardRepository.GetByUserIdAsync(userId, cancellationToken);
        return dashboards.Select(d => MapDashboard(d, d.Widgets.OrderBy(w => w.SortOrder).Select(MapWidget))).ToList();
    }

    public async Task<DashboardDto?> GetDashboardAsync(
        Guid userId,
        Guid dashboardId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var dashboard = await _dashboardRepository.GetByIdWithWidgetsAsync(dashboardId, cancellationToken);
        if (dashboard is null || !CanAccess(dashboard, userId, isAdmin))
        {
            return null;
        }

        return MapDashboard(
            dashboard,
            dashboard.Widgets.OrderBy(w => w.SortOrder).Select(MapWidget));
    }

    public async Task<DashboardDto> UpdateDashboardAsync(
        Guid userId,
        Guid dashboardId,
        UpdateDashboardRequest request,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var dashboard = await _dashboardRepository.GetByIdForUpdateAsync(dashboardId, cancellationToken)
            ?? throw new KeyNotFoundException("Dashboard not found.");

        if (!CanAccess(dashboard, userId, isAdmin))
        {
            throw new UnauthorizedAccessException("You do not have access to this dashboard.");
        }

        dashboard.Name = request.Name.Trim();
        dashboard.Description = request.Description?.Trim();
        dashboard.UpdatedAt = DateTime.UtcNow;

        await _dashboardRepository.UpdateAsync(dashboard, cancellationToken);

        var withWidgets = await _dashboardRepository.GetByIdWithWidgetsAsync(dashboardId, cancellationToken)
            ?? dashboard;

        return MapDashboard(
            withWidgets,
            withWidgets.Widgets.OrderBy(w => w.SortOrder).Select(MapWidget));
    }

    public async Task DeleteDashboardAsync(
        Guid userId,
        Guid dashboardId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        _ = await GetOwnedDashboardAsync(userId, dashboardId, isAdmin, cancellationToken);
        await _dashboardRepository.DeleteAsync(dashboardId, cancellationToken);
    }

    public async Task<WidgetDto> AddWidgetAsync(
        Guid userId,
        Guid dashboardId,
        CreateWidgetRequest request,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        _ = await GetOwnedDashboardAsync(userId, dashboardId, isAdmin, cancellationToken);
        ValidateWidgetRequest(request.Metric, request.Region, request.ConfigJson);

        var widget = new Widget
        {
            Id = Guid.NewGuid(),
            DashboardId = dashboardId,
            Title = request.Title.Trim(),
            Type = request.Type,
            Metric = request.Metric.Trim().ToLowerInvariant(),
            Region = request.Region.Trim(),
            Source = request.Source?.Trim().ToLowerInvariant(),
            ConfigJson = request.ConfigJson,
            SortOrder = request.SortOrder,
            CreatedAt = DateTime.UtcNow
        };

        await _dashboardRepository.AddWidgetAsync(widget, cancellationToken);
        return MapWidget(widget);
    }

    public async Task<WidgetDto> UpdateWidgetAsync(
        Guid userId,
        Guid dashboardId,
        Guid widgetId,
        UpdateWidgetRequest request,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        _ = await GetOwnedDashboardAsync(userId, dashboardId, isAdmin, cancellationToken);
        ValidateWidgetRequest(request.Metric, request.Region, request.ConfigJson);

        var widget = await _dashboardRepository.GetWidgetForUpdateAsync(dashboardId, widgetId, cancellationToken)
            ?? throw new KeyNotFoundException("Widget not found.");

        widget.Title = request.Title.Trim();
        widget.Type = request.Type;
        widget.Metric = request.Metric.Trim().ToLowerInvariant();
        widget.Region = request.Region.Trim();
        widget.Source = request.Source?.Trim().ToLowerInvariant();
        widget.ConfigJson = request.ConfigJson;
        widget.SortOrder = request.SortOrder;

        await _dashboardRepository.UpdateWidgetAsync(widget, cancellationToken);
        return MapWidget(widget);
    }

    public async Task DeleteWidgetAsync(
        Guid userId,
        Guid dashboardId,
        Guid widgetId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        _ = await GetOwnedDashboardAsync(userId, dashboardId, isAdmin, cancellationToken);

        var widget = await _dashboardRepository.GetWidgetAsync(dashboardId, widgetId, cancellationToken)
            ?? throw new KeyNotFoundException("Widget not found.");

        await _dashboardRepository.DeleteWidgetAsync(widget.Id, cancellationToken);
    }

    public static void ValidateWidgetRequest(string metric, string region, string? configJson)
    {
        if (string.IsNullOrWhiteSpace(metric))
        {
            throw new ArgumentException("Metric is required.");
        }

        if (string.IsNullOrWhiteSpace(region))
        {
            throw new ArgumentException("Region is required.");
        }

        if (configJson is null)
        {
            return;
        }

        try
        {
            JsonDocument.Parse(configJson);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("ConfigJson must be valid JSON.", ex);
        }
    }

    private async Task<Dashboard> GetOwnedDashboardAsync(
        Guid userId,
        Guid dashboardId,
        bool isAdmin,
        CancellationToken cancellationToken)
    {
        var dashboard = await _dashboardRepository.GetByIdAsync(dashboardId, cancellationToken)
            ?? throw new KeyNotFoundException("Dashboard not found.");

        if (!CanAccess(dashboard, userId, isAdmin))
        {
            throw new UnauthorizedAccessException("You do not have access to this dashboard.");
        }

        return dashboard;
    }

    private static bool CanAccess(Dashboard dashboard, Guid userId, bool isAdmin) =>
        isAdmin || dashboard.UserId == userId;

    private static DashboardDto MapDashboard(
        Dashboard dashboard,
        IEnumerable<WidgetDto> widgets) =>
        new(
            dashboard.Id,
            dashboard.Name,
            dashboard.Description,
            dashboard.CreatedAt,
            dashboard.UpdatedAt,
            widgets.ToList());

    private static WidgetDto MapWidget(Widget widget) =>
        new(
            widget.Id,
            widget.DashboardId,
            widget.Title,
            widget.Type,
            widget.Metric,
            widget.Region,
            widget.Source,
            widget.ConfigJson,
            widget.SortOrder,
            widget.CreatedAt);
}
