using EnviroWatch.Application.DTOs;

namespace EnviroWatch.Application.Interfaces;

public interface IAdminService
{
    Task<AdminStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
}
