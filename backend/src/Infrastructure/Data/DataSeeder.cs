using EnviroWatch.Application.Interfaces;
using EnviroWatch.Domain.Models;
using Microsoft.Extensions.Logging;

namespace EnviroWatch.Infrastructure.Data;

public static class DataSeeder
{
    private const string Region = "Wroclaw,PL";
    private const double Lat = 51.1;
    private const double Lon = 17.0;

    public static async Task SeedAsync(
        IDataSnapshotRepository repository,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        if (await repository.AnyAsync(cancellationToken))
        {
            return;
        }

        logger.LogInformation("Seeding development data snapshots for {Region}", Region);

        var snapshots = new List<DataSnapshot>();
        var now = DateTime.UtcNow;
        var random = new Random(42);

        for (var i = 48; i >= 0; i--)
        {
            var timestamp = now.AddMinutes(-i * 30);
            var baseTemp = 16 + Math.Sin(i / 8.0) * 4;

            snapshots.Add(CreateSnapshot("openmeteo", "temperature", baseTemp + random.NextDouble(), "°C", timestamp));
            snapshots.Add(CreateSnapshot("openmeteo", "humidity", 55 + random.Next(0, 20), "%", timestamp));
            snapshots.Add(CreateSnapshot("openmeteo", "wind", 8 + random.NextDouble() * 8, "km/h", timestamp));
        }

        await repository.AddRangeAsync(snapshots, cancellationToken);
        logger.LogInformation("Seeded {Count} data snapshots", snapshots.Count);
    }

    private static DataSnapshot CreateSnapshot(
        string source,
        string metric,
        double value,
        string unit,
        DateTime timestamp) =>
        new()
        {
            Id = Guid.NewGuid(),
            Source = source,
            Region = Region,
            Metric = metric,
            Value = Math.Round(value, 1),
            Unit = unit,
            Lat = Lat,
            Lon = Lon,
            Timestamp = timestamp
        };
}
