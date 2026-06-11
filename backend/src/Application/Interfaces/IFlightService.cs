using EnviroWatch.Domain.Models;

namespace EnviroWatch.Application.Interfaces;

public interface IFlightService
{
    Task<IReadOnlyList<DataSnapshot>> FetchCurrentFlightSnapshotsAsync(
        CancellationToken cancellationToken = default);
}
