using EnviroWatch.Domain.Models;

namespace EnviroWatch.Application.Interfaces;

public interface IOpenWeatherService
{
    Task<IReadOnlyList<DataSnapshot>> FetchCurrentWeatherSnapshotsAsync(
        CancellationToken cancellationToken = default);
}
