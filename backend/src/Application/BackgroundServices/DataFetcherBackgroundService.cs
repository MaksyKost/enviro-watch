using EnviroWatch.Application.Configuration;
using EnviroWatch.Application.Interfaces;
using EnviroWatch.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnviroWatch.Application.BackgroundServices;

public class DataFetcherBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DataFetchOptions _options;
    private readonly ILogger<DataFetcherBackgroundService> _logger;

    public DataFetcherBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<DataFetchOptions> options,
        ILogger<DataFetcherBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_options.Regions.Count == 0)
        {
            _logger.LogWarning("Data fetch is enabled but no regions are configured");
            return;
        }

        var interval = TimeSpan.FromSeconds(Math.Max(_options.IntervalSeconds, 10));
        _logger.LogInformation(
            "Data fetcher started. Interval: {IntervalSeconds}s, regions: {RegionCount}",
            interval.TotalSeconds,
            _options.Regions.Count);

        using var timer = new PeriodicTimer(interval);

        await FetchAndPersistAsync(stoppingToken);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await FetchAndPersistAsync(stoppingToken);
        }
    }

    private async Task FetchAndPersistAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var weatherService = scope.ServiceProvider.GetRequiredService<IWeatherService>();
            var repository = scope.ServiceProvider.GetRequiredService<IDataSnapshotRepository>();
            var notifier = scope.ServiceProvider.GetRequiredService<IDataUpdateNotifier>();

            var snapshots = await weatherService.FetchCurrentWeatherSnapshotsAsync(cancellationToken);

            if (snapshots.Count == 0)
            {
                _logger.LogWarning("No weather snapshots fetched in this cycle");
                return;
            }

            await repository.AddRangeAsync(snapshots, cancellationToken);

            var updates = DataUpdateMapper.FromWeatherSnapshots(snapshots);
            if (updates.Count > 0)
            {
                await notifier.NotifyDataUpdatesAsync(updates, cancellationToken);
            }

            _logger.LogInformation(
                "Persisted {Count} weather snapshots and pushed {UpdateCount} live updates",
                snapshots.Count,
                updates.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Data fetch cycle failed");
        }
    }
}
