using EnviroWatch.Domain.Models;

namespace EnviroWatch.Application.DTOs;

public record AdminStatsDto(
    int Users,
    long Snapshots,
    int ActiveAlerts,
    int Dashboards,
    DateTime GeneratedAt);

public record UpdateUserRoleRequest(UserRole Role);
