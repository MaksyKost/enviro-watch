using EnviroWatch.Domain.Models;

namespace EnviroWatch.Application.Interfaces;

public interface IWeatherService
{
    Task<IReadOnlyList<DataSnapshot>> FetchCurrentWeatherSnapshotsAsync(
        CancellationToken cancellationToken = default);
}
