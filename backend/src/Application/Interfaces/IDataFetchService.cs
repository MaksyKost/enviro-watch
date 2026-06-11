using EnviroWatch.Domain.Models;

namespace EnviroWatch.Application.Interfaces;

public interface IDataFetchService
{
    Task<IReadOnlyList<DataSnapshot>> FetchAllSnapshotsAsync(
        CancellationToken cancellationToken = default);
}
