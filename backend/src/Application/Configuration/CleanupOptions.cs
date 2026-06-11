namespace EnviroWatch.Application.Configuration;

public class CleanupOptions
{
    public const string SectionName = "Cleanup";

    /// <summary>How often to run cleanup (hours).</summary>
    public int IntervalHours { get; set; } = 24;

    /// <summary>Delete snapshots older than this many days.</summary>
    public int RetentionDays { get; set; } = 30;

    public bool Enabled { get; set; } = true;
}
