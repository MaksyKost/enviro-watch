using EnviroWatch.Application.DTOs;
using EnviroWatch.Application.Interfaces;
using EnviroWatch.API.Controllers;
using EnviroWatch.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EnviroWatch.Tests.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task Login_ReturnsOk_WithToken()
    {
        var authService = new Mock<IAuthService>();
        authService
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthResponse(
                "token",
                DateTime.UtcNow.AddHours(1),
                new UserDto(Guid.NewGuid(), "user@example.com", UserRole.Viewer)));

        var controller = new AuthController(authService.Object);

        var result = await controller.Login(
            new LoginRequest("user@example.com", "Password123"),
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuthResponse>(ok.Value);
        Assert.Equal("token", response.Token);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        var authService = new Mock<IAuthService>();
        authService
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid email or password."));

        var controller = new AuthController(authService.Object);

        var result = await controller.Login(
            new LoginRequest("user@example.com", "wrong"),
            CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }
}
