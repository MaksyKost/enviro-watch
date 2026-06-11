using EnviroWatch.Application.DTOs;

namespace EnviroWatch.Application.Interfaces;

public interface IObservationService
{
    Task<ObservationDto> CreateAsync(
        Guid userId,
        CreateObservationRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ObservationDto>> GetObservationsAsync(
        Guid userId,
        bool isAdmin,
        CancellationToken cancellationToken = default);
}
