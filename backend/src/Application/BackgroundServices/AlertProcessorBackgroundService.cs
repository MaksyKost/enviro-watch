using EnviroWatch.Application.Configuration;
using EnviroWatch.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnviroWatch.Application.BackgroundServices;

public class AlertProcessorBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AlertOptions _options;
    private readonly ILogger<AlertProcessorBackgroundService> _logger;

    public AlertProcessorBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<AlertOptions> options,
        ILogger<AlertProcessorBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(_options.CheckIntervalSeconds, 10));
        _logger.LogInformation(
            "Alert processor started. Interval: {IntervalSeconds}s, cooldown: {CooldownMinutes}m",
            interval.TotalSeconds,
            _options.CooldownMinutes);

        using var timer = new PeriodicTimer(interval);

        await ProcessAlertsAsync(stoppingToken);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessAlertsAsync(stoppingToken);
        }
    }

    private async Task ProcessAlertsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var checker = scope.ServiceProvider.GetRequiredService<IAlertCheckerService>();
            var triggered = await checker.ProcessActiveAlertsAsync(cancellationToken);

            if (triggered > 0)
            {
                _logger.LogInformation("Processed alerts: {TriggeredCount} triggered", triggered);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Alert processing cycle failed");
        }
    }
}
