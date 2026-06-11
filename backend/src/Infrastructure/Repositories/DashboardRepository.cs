using EnviroWatch.Application.Interfaces;
using EnviroWatch.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnviroWatch.Infrastructure.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly AppDbContext _db;

    public DashboardRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Dashboard?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Dashboards.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public Task<Dashboard?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Dashboards.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public Task<Dashboard?> GetByIdWithWidgetsAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Dashboards.AsNoTracking()
            .Include(d => d.Widgets)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Dashboard>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _db.Dashboards.AsNoTracking()
            .Include(d => d.Widgets)
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.UpdatedAt)
            .ToListAsync(cancellationToken);

    public async Task<Dashboard> CreateAsync(Dashboard dashboard, CancellationToken cancellationToken = default)
    {
        _db.Dashboards.Add(dashboard);
        await _db.SaveChangesAsync(cancellationToken);
        return dashboard;
    }

    public async Task UpdateAsync(Dashboard dashboard, CancellationToken cancellationToken = default)
    {
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dashboard = await _db.Dashboards.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (dashboard is null)
        {
            return;
        }

        _db.Dashboards.Remove(dashboard);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<Widget?> GetWidgetAsync(
        Guid dashboardId,
        Guid widgetId,
        CancellationToken cancellationToken = default) =>
        _db.Widgets.AsNoTracking()
            .FirstOrDefaultAsync(w => w.DashboardId == dashboardId && w.Id == widgetId, cancellationToken);

    public Task<Widget?> GetWidgetForUpdateAsync(
        Guid dashboardId,
        Guid widgetId,
        CancellationToken cancellationToken = default) =>
        _db.Widgets.FirstOrDefaultAsync(w => w.DashboardId == dashboardId && w.Id == widgetId, cancellationToken);

    public async Task<Widget> AddWidgetAsync(Widget widget, CancellationToken cancellationToken = default)
    {
        _db.Widgets.Add(widget);

        var dashboard = await _db.Dashboards.FirstOrDefaultAsync(d => d.Id == widget.DashboardId, cancellationToken);
        if (dashboard is not null)
        {
            dashboard.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return widget;
    }

    public async Task UpdateWidgetAsync(Widget widget, CancellationToken cancellationToken = default)
    {
        var dashboard = await _db.Dashboards.FirstOrDefaultAsync(d => d.Id == widget.DashboardId, cancellationToken);
        if (dashboard is not null)
        {
            dashboard.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteWidgetAsync(Guid widgetId, CancellationToken cancellationToken = default)
    {
        var widget = await _db.Widgets.FirstOrDefaultAsync(w => w.Id == widgetId, cancellationToken);
        if (widget is null)
        {
            return;
        }

        var dashboard = await _db.Dashboards.FirstOrDefaultAsync(d => d.Id == widget.DashboardId, cancellationToken);
        if (dashboard is not null)
        {
            dashboard.UpdatedAt = DateTime.UtcNow;
        }

        _db.Widgets.Remove(widget);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        _db.Dashboards.CountAsync(cancellationToken);
}
