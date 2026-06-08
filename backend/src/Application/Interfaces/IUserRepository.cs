using EnviroWatch.Domain.Models;

namespace EnviroWatch.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);

    Task UpdateRoleAsync(Guid userId, UserRole role, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(CancellationToken cancellationToken = default);
}
