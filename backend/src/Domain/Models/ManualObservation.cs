namespace EnviroWatch.Domain.Models;

public class ManualObservation
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public required string Region { get; set; }

    public required string Metric { get; set; }

    public double Value { get; set; }

    public string? Unit { get; set; }

    public double? Lat { get; set; }

    public double? Lon { get; set; }

    public string? Notes { get; set; }

    public DateTime ObservedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
}
