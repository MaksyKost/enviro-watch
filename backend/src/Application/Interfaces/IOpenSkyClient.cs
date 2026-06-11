using EnviroWatch.Application.DTOs;

namespace EnviroWatch.Application.Interfaces;

public interface IOpenSkyClient
{
    Task<FlightData?> GetFlightsInAreaAsync(
        double latitude,
        double longitude,
        double radiusDegrees,
        CancellationToken cancellationToken = default);
}
