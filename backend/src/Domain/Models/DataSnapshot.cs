namespace EnviroWatch.Domain.Models;

/// <summary>
/// A single environmental measurement persisted from external APIs or manual input.
/// </summary>
public class DataSnapshot
{
    public Guid Id { get; set; }

    /// <summary>Data provider, e.g. openmeteo, openweather, manual.</summary>
    public required string Source { get; set; }

    public required string Region { get; set; }

    public required string Metric { get; set; }

    public double Value { get; set; }

    public string? Unit { get; set; }

    public double? Lat { get; set; }

    public double? Lon { get; set; }

    public DateTime Timestamp { get; set; }
}
