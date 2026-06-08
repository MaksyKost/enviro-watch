using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EnviroWatch.Application.Configuration;
using EnviroWatch.Application.DTOs;
using EnviroWatch.Application.Interfaces;
using EnviroWatch.Domain.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EnviroWatch.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IOptions<JwtOptions> jwtOptions)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _userRepository.GetByEmailAsync(email, cancellationToken) is not null)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var isFirstUser = !await _userRepository.AnyAsync(cancellationToken);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = isFirstUser ? UserRole.Admin : UserRole.Viewer,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user, cancellationToken);
        return CreateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        return CreateAuthResponse(user);
    }

    public async Task<UserDto?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        return user is null ? null : MapUser(user);
    }

    public async Task<IReadOnlyList<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users.Select(MapUser).ToList();
    }

    public Task UpdateUserRoleAsync(Guid userId, UserRole role, CancellationToken cancellationToken = default) =>
        _userRepository.UpdateRoleAsync(userId, role, cancellationToken);

    private AuthResponse CreateAuthResponse(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);
        var token = GenerateToken(user, expiresAt);
        return new AuthResponse(token, expiresAt, MapUser(user));
    }

    private string GenerateToken(User user, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(_jwtOptions.Secret) || _jwtOptions.Secret.Length < 32)
        {
            throw new InvalidOperationException("JWT secret must be at least 32 characters.");
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserDto MapUser(User user) =>
        new(user.Id, user.Email, user.Role);
}
