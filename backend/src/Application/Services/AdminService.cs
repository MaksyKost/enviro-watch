using EnviroWatch.Application.DTOs;
using EnviroWatch.Application.Interfaces;

namespace EnviroWatch.Application.Services;

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepository;
    private readonly IDataSnapshotRepository _snapshotRepository;
    private readonly IAlertRepository _alertRepository;
    private readonly IDashboardRepository _dashboardRepository;

    public AdminService(
        IUserRepository userRepository,
        IDataSnapshotRepository snapshotRepository,
        IAlertRepository alertRepository,
        IDashboardRepository dashboardRepository)
    {
        _userRepository = userRepository;
        _snapshotRepository = snapshotRepository;
        _alertRepository = alertRepository;
        _dashboardRepository = dashboardRepository;
    }

    public async Task<AdminStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.CountAsync(cancellationToken);
        var snapshots = await _snapshotRepository.CountAsync(cancellationToken);
        var activeAlerts = await _alertRepository.CountActiveAsync(cancellationToken);
        var dashboards = await _dashboardRepository.CountAsync(cancellationToken);

        return new AdminStatsDto(users, snapshots, activeAlerts, dashboards, DateTime.UtcNow);
    }
}
