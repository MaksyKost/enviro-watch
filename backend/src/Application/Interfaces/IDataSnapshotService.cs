using EnviroWatch.Application.DTOs;

namespace EnviroWatch.Application.Interfaces;

public interface IDataSnapshotService
{
    Task<DataSnapshotListResponse> GetSnapshotsAsync(
        DataSnapshotQuery query,
        CancellationToken cancellationToken = default);
}
