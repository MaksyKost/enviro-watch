using EnviroWatch.Domain.Models;

namespace EnviroWatch.Application.Interfaces;

public interface IAirQualityService
{
    Task<IReadOnlyList<DataSnapshot>> FetchCurrentAirQualitySnapshotsAsync(
        CancellationToken cancellationToken = default);
}
