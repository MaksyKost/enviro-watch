using EnviroWatch.Domain.Models;

namespace EnviroWatch.Application.Interfaces;

public interface IDashboardRepository
{
    Task<Dashboard?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Dashboard?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Dashboard?> GetByIdWithWidgetsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Dashboard>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Dashboard> CreateAsync(Dashboard dashboard, CancellationToken cancellationToken = default);

    Task UpdateAsync(Dashboard dashboard, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Widget?> GetWidgetAsync(Guid dashboardId, Guid widgetId, CancellationToken cancellationToken = default);

    Task<Widget?> GetWidgetForUpdateAsync(Guid dashboardId, Guid widgetId, CancellationToken cancellationToken = default);

    Task<Widget> AddWidgetAsync(Widget widget, CancellationToken cancellationToken = default);

    Task UpdateWidgetAsync(Widget widget, CancellationToken cancellationToken = default);

    Task DeleteWidgetAsync(Guid widgetId, CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
