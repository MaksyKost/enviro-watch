namespace EnviroWatch.Application.Configuration;

public class AlertOptions
{
    public const string SectionName = "Alerts";

    public int CheckIntervalSeconds { get; set; } = 30;

    /// <summary>Minimum time between repeated triggers for the same alert.</summary>
    public int CooldownMinutes { get; set; } = 5;
}
