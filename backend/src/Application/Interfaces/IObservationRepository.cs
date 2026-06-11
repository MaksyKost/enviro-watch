using EnviroWatch.Domain.Models;

namespace EnviroWatch.Application.Interfaces;

public interface IObservationRepository
{
    Task<ManualObservation> CreateAsync(ManualObservation observation, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ManualObservation>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ManualObservation>> GetAllAsync(CancellationToken cancellationToken = default);
}
