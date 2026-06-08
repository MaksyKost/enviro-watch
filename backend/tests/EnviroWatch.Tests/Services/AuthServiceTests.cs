using EnviroWatch.Application.Configuration;
using EnviroWatch.Application.DTOs;
using EnviroWatch.Application.Interfaces;
using EnviroWatch.Application.Services;
using EnviroWatch.Domain.Models;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace EnviroWatch.Tests.Services;

public class AuthServiceTests
{
    private static readonly JwtOptions JwtOptions = new()
    {
        Secret = "change-me-in-development-min-32-characters-long",
        Issuer = "envirowatch",
        Audience = "envirowatch",
        ExpirationMinutes = 60
    };

    [Fact]
    public async Task RegisterAsync_FirstUserBecomesAdmin()
    {
        var repository = new Mock<IUserRepository>();
        repository.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        repository.Setup(r => r.AnyAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
        repository.Setup(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User user, CancellationToken _) => user);

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hash");

        var service = new AuthService(
            repository.Object,
            hasher.Object,
            Options.Create(JwtOptions));

        var response = await service.RegisterAsync(
            new RegisterRequest("user@example.com", "Password123"));

        Assert.Equal(UserRole.Admin, response.User.Role);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_ThrowsUnauthorized()
    {
        var repository = new Mock<IUserRepository>();
        repository.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User
            {
                Id = Guid.NewGuid(),
                Email = "user@example.com",
                PasswordHash = "hash",
                Role = UserRole.Viewer,
                CreatedAt = DateTime.UtcNow
            });

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.Verify("wrong", "hash")).Returns(false);

        var service = new AuthService(
            repository.Object,
            hasher.Object,
            Options.Create(JwtOptions));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(new LoginRequest("user@example.com", "wrong")));
    }
}
