using EnviroWatch.Application.DTOs;

namespace EnviroWatch.Application.Interfaces;

public interface IOpenWeatherClient
{
    Task<CurrentWeatherData?> GetCurrentWeatherAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default);
}
