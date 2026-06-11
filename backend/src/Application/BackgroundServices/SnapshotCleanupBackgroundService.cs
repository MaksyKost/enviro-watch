using EnviroWatch.Application.Configuration;
using EnviroWatch.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnviroWatch.Application.BackgroundServices;

public class SnapshotCleanupBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly CleanupOptions _options;
    private readonly ILogger<SnapshotCleanupBackgroundService> _logger;

    public SnapshotCleanupBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<CleanupOptions> options,
        ILogger<SnapshotCleanupBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Snapshot cleanup is disabled");
            return;
        }

        var interval = TimeSpan.FromHours(Math.Max(_options.IntervalHours, 1));
        _logger.LogInformation(
            "Snapshot cleanup started. Interval: {Hours}h, retention: {Days}d",
            interval.TotalHours,
            _options.RetentionDays);

        using var timer = new PeriodicTimer(interval);

        await CleanupAsync(stoppingToken);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await CleanupAsync(stoppingToken);
        }
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IDataSnapshotRepository>();
            var cutoff = DateTime.UtcNow.AddDays(-Math.Max(_options.RetentionDays, 1));
            var deleted = await repository.DeleteOlderThanAsync(cutoff, cancellationToken);

            if (deleted > 0)
            {
                _logger.LogInformation(
                    "Deleted {Count} snapshots older than {Cutoff:u}",
                    deleted,
                    cutoff);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Snapshot cleanup failed");
        }
    }
}
