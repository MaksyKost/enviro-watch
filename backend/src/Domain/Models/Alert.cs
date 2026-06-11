namespace EnviroWatch.Domain.Models;

public class Alert
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public required string Metric { get; set; }

    public required string Region { get; set; }

    public double Threshold { get; set; }

    public AlertCondition Condition { get; set; }

    public bool NotifyEmail { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? LastTriggeredAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }

    public ICollection<AlertLog> Logs { get; set; } = [];
}
