using EnviroWatch.Application.DTOs;
using EnviroWatch.Domain.Models;

namespace EnviroWatch.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<UserDto?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);

    Task UpdateUserRoleAsync(Guid userId, UserRole role, CancellationToken cancellationToken = default);
}
