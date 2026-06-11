using EnviroWatch.Application.Interfaces;
using EnviroWatch.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnviroWatch.Infrastructure.Repositories;

public class AlertRepository : IAlertRepository
{
    private readonly AppDbContext _db;

    public AlertRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Alert?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Alerts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Alert>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _db.Alerts.AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Alert>> GetAllActiveAsync(CancellationToken cancellationToken = default) =>
        await _db.Alerts.AsNoTracking()
            .Where(a => a.IsActive)
            .ToListAsync(cancellationToken);

    public async Task<Alert> CreateAsync(Alert alert, CancellationToken cancellationToken = default)
    {
        _db.Alerts.Add(alert);
        await _db.SaveChangesAsync(cancellationToken);
        return alert;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var alert = await _db.Alerts.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (alert is null)
        {
            return;
        }

        _db.Alerts.Remove(alert);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateLastTriggeredAsync(
        Guid alertId,
        DateTime triggeredAt,
        CancellationToken cancellationToken = default)
    {
        var alert = await _db.Alerts.FirstOrDefaultAsync(a => a.Id == alertId, cancellationToken);
        if (alert is null)
        {
            return;
        }

        alert.LastTriggeredAt = triggeredAt;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<AlertLog> CreateLogAsync(AlertLog log, CancellationToken cancellationToken = default)
    {
        _db.AlertLogs.Add(log);
        await _db.SaveChangesAsync(cancellationToken);
        return log;
    }

    public async Task<IReadOnlyList<AlertLog>> GetLogsByAlertIdAsync(
        Guid alertId,
        CancellationToken cancellationToken = default) =>
        await _db.AlertLogs.AsNoTracking()
            .Where(l => l.AlertId == alertId)
            .OrderByDescending(l => l.TriggeredAt)
            .ToListAsync(cancellationToken);

    public Task<int> CountActiveAsync(CancellationToken cancellationToken = default) =>
        _db.Alerts.CountAsync(a => a.IsActive, cancellationToken);
}
