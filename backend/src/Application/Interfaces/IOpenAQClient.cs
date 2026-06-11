using EnviroWatch.Application.DTOs;

namespace EnviroWatch.Application.Interfaces;

public interface IOpenAQClient
{
    Task<AirQualityData?> GetLatestAirQualityAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default);
}
