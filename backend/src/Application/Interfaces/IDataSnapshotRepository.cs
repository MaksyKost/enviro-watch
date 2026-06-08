using EnviroWatch.Application.DTOs;
using EnviroWatch.Domain.Models;

namespace EnviroWatch.Application.Interfaces;

public interface IDataSnapshotRepository
{
    Task<(IReadOnlyList<DataSnapshot> Items, int Total)> GetFilteredAsync(
        DataSnapshotQuery query,
        CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        IEnumerable<DataSnapshot> snapshots,
        CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(CancellationToken cancellationToken = default);
}
