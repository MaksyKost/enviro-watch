using EnviroWatch.Application.DTOs;
using EnviroWatch.Domain.Models;

namespace EnviroWatch.Application.Services;

public static class DataUpdateMapper
{
    public const string WeatherType = "weather";

    public static IReadOnlyList<DataUpdateDto> FromWeatherSnapshots(
        IEnumerable<DataSnapshot> snapshots)
    {
        return snapshots
            .GroupBy(s => s.Region)
            .Select(BuildWeatherUpdate)
            .Where(update => update is not null)
            .Cast<DataUpdateDto>()
            .ToList();
    }

    private static DataUpdateDto? BuildWeatherUpdate(IGrouping<string, DataSnapshot> regionGroup)
    {
        var metrics = regionGroup.ToDictionary(s => s.Metric, s => s.Value);
        var timestamp = regionGroup.Max(s => s.Timestamp);

        if (!metrics.TryGetValue("temperature", out var temperature)
            || !metrics.TryGetValue("humidity", out var humidity)
            || !metrics.TryGetValue("wind", out var wind))
        {
            return null;
        }

        return new DataUpdateDto(
            WeatherType,
            regionGroup.Key,
            new WeatherMetricsDto(
                Math.Round(temperature, 1),
                Math.Round(humidity, 1),
                Math.Round(wind, 1)),
            timestamp);
    }
}
