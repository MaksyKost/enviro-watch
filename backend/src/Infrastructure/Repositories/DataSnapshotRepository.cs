using EnviroWatch.Application.DTOs;
using EnviroWatch.Application.Interfaces;
using EnviroWatch.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnviroWatch.Infrastructure.Repositories;

public class DataSnapshotRepository : IDataSnapshotRepository
{
    private readonly AppDbContext _db;

    public DataSnapshotRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(IReadOnlyList<DataSnapshot> Items, int Total)> GetFilteredAsync(
        DataSnapshotQuery query,
        CancellationToken cancellationToken = default)
    {
        var q = _db.DataSnapshots.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Region))
        {
            var region = query.Region.Trim();
            q = q.Where(s => EF.Functions.ILike(s.Region, $"%{region}%"));
        }

        if (!string.IsNullOrWhiteSpace(query.Metric))
        {
            var metric = query.Metric.Trim().ToLowerInvariant();
            q = q.Where(s => s.Metric.ToLower() == metric);
        }

        if (!string.IsNullOrWhiteSpace(query.Source))
        {
            var source = query.Source.Trim().ToLowerInvariant();
            q = q.Where(s => s.Source.ToLower() == source);
        }

        if (query.From.HasValue)
        {
            var from = query.From.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(query.From.Value, DateTimeKind.Utc)
                : query.From.Value.ToUniversalTime();
            q = q.Where(s => s.Timestamp >= from);
        }

        if (query.To.HasValue)
        {
            var to = query.To.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(query.To.Value, DateTimeKind.Utc)
                : query.To.Value.ToUniversalTime();
            q = q.Where(s => s.Timestamp <= to);
        }

        var total = await q.CountAsync(cancellationToken);

        var items = await q
            .OrderByDescending(s => s.Timestamp)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task AddRangeAsync(
        IEnumerable<DataSnapshot> snapshots,
        CancellationToken cancellationToken = default)
    {
        await _db.DataSnapshots.AddRangeAsync(snapshots, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> AnyAsync(CancellationToken cancellationToken = default) =>
        _db.DataSnapshots.AnyAsync(cancellationToken);

    public async Task<int> DeleteOlderThanAsync(DateTime cutoff, CancellationToken cancellationToken = default) =>
        await _db.DataSnapshots
            .Where(s => s.Timestamp < cutoff)
            .ExecuteDeleteAsync(cancellationToken);

    public Task<long> CountAsync(CancellationToken cancellationToken = default) =>
        _db.DataSnapshots.LongCountAsync(cancellationToken);

    public Task<DataSnapshot?> GetLatestAsync(
        string region,
        string metric,
        CancellationToken cancellationToken = default)
    {
        var normalizedMetric = metric.Trim().ToLowerInvariant();
        var normalizedRegion = region.Trim();

        return _db.DataSnapshots.AsNoTracking()
            .Where(s => s.Region == normalizedRegion && s.Metric.ToLower() == normalizedMetric)
            .OrderByDescending(s => s.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
