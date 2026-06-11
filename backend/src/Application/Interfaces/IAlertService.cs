using EnviroWatch.Application.DTOs;

namespace EnviroWatch.Application.Interfaces;

public interface IAlertService
{
    Task<AlertDto> CreateAsync(Guid userId, CreateAlertRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AlertDto>> GetUserAlertsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AlertLogDto>> GetAlertLogsAsync(
        Guid userId,
        Guid alertId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid userId, Guid alertId, bool isAdmin, CancellationToken cancellationToken = default);
}
