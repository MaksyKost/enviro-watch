using EnviroWatch.Application.Interfaces;
using EnviroWatch.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnviroWatch.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _db.Users.AsNoTracking()
            .OrderBy(u => u.Email)
            .ToListAsync(cancellationToken);

    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task UpdateRoleAsync(Guid userId, UserRole role, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        user.Role = role;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> AnyAsync(CancellationToken cancellationToken = default) =>
        _db.Users.AnyAsync(cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        _db.Users.CountAsync(cancellationToken);
}
