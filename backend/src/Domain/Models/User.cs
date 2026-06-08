namespace EnviroWatch.Domain.Models;

public class User
{
    public Guid Id { get; set; }

    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public UserRole Role { get; set; }

    public DateTime CreatedAt { get; set; }
}
