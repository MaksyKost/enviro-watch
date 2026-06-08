using System.ComponentModel.DataAnnotations;
using EnviroWatch.Domain.Models;

namespace EnviroWatch.Application.DTOs;

public record RegisterRequest(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MinLength(8), MaxLength(128)] string Password);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record UserDto(Guid Id, string Email, UserRole Role);

public record AuthResponse(string Token, DateTime ExpiresAt, UserDto User);
