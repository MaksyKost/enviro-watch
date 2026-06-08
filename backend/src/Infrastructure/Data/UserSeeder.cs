using EnviroWatch.Application.Interfaces;
using EnviroWatch.Domain.Models;
using Microsoft.Extensions.Logging;

namespace EnviroWatch.Infrastructure.Data;

public static class UserSeeder
{
    public const string DefaultAdminEmail = "admin@envirowatch.local";
    public const string DefaultAdminPassword = "Admin123!";

    public static async Task SeedAsync(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        if (await userRepository.AnyAsync(cancellationToken))
        {
            return;
        }

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Email = DefaultAdminEmail,
            PasswordHash = passwordHasher.Hash(DefaultAdminPassword),
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };

        await userRepository.CreateAsync(admin, cancellationToken);

        logger.LogInformation(
            "Seeded default admin user {Email}. Change the password after first login.",
            DefaultAdminEmail);
    }
}
