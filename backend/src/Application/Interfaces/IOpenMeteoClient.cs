using EnviroWatch.Application.DTOs;

namespace EnviroWatch.Application.Interfaces;

public interface IOpenMeteoClient
{
    Task<CurrentWeatherData?> GetCurrentWeatherAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default);
}
