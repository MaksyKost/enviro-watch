namespace EnviroWatch.Domain.Models;

public class Dashboard
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public User? User { get; set; }

    public ICollection<Widget> Widgets { get; set; } = [];
}
