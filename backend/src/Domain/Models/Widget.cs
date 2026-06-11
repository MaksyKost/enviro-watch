namespace EnviroWatch.Domain.Models;

public class Widget
{
    public Guid Id { get; set; }

    public Guid DashboardId { get; set; }

    public required string Title { get; set; }

    public WidgetType Type { get; set; }

    public required string Metric { get; set; }

    public required string Region { get; set; }

    public string? Source { get; set; }

    public string? ConfigJson { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public Dashboard? Dashboard { get; set; }
}
