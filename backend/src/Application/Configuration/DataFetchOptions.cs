namespace EnviroWatch.Application.Configuration;

public class DataFetchOptions
{
    public const string SectionName = "DataFetch";

    public int IntervalSeconds { get; set; } = 30;

    public List<MonitoredRegionOptions> Regions { get; set; } = [];
}

public class MonitoredRegionOptions
{
    public required string Name { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }
}
