using EnviroWatch.Domain.Models;

namespace EnviroWatch.Application.Interfaces;

public interface IAlertRepository
{
    Task<Alert?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Alert>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Alert>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    Task<Alert> CreateAsync(Alert alert, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task UpdateLastTriggeredAsync(Guid alertId, DateTime triggeredAt, CancellationToken cancellationToken = default);

    Task<AlertLog> CreateLogAsync(AlertLog log, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AlertLog>> GetLogsByAlertIdAsync(Guid alertId, CancellationToken cancellationToken = default);

    Task<int> CountActiveAsync(CancellationToken cancellationToken = default);
}
