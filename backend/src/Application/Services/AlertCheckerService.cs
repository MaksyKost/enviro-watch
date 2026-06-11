using EnviroWatch.Application.Configuration;
using EnviroWatch.Application.Interfaces;
using EnviroWatch.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnviroWatch.Application.Services;

public class AlertCheckerService : IAlertCheckerService
{
    private readonly IAlertRepository _alertRepository;
    private readonly IDataSnapshotRepository _snapshotRepository;
    private readonly IAlertNotifier _alertNotifier;
    private readonly AlertOptions _options;
    private readonly ILogger<AlertCheckerService> _logger;

    public AlertCheckerService(
        IAlertRepository alertRepository,
        IDataSnapshotRepository snapshotRepository,
        IAlertNotifier alertNotifier,
        IOptions<AlertOptions> options,
        ILogger<AlertCheckerService> logger)
    {
        _alertRepository = alertRepository;
        _snapshotRepository = snapshotRepository;
        _alertNotifier = alertNotifier;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<int> ProcessActiveAlertsAsync(CancellationToken cancellationToken = default)
    {
        var alerts = await _alertRepository.GetAllActiveAsync(cancellationToken);
        var triggeredCount = 0;

        foreach (var alert in alerts)
        {
            if (!CanTriggerAgain(alert))
            {
                continue;
            }

            var snapshot = await _snapshotRepository.GetLatestAsync(
                alert.Region,
                alert.Metric,
                cancellationToken);

            if (snapshot is null)
            {
                continue;
            }

            if (!IsTriggered(snapshot.Value, alert.Threshold, alert.Condition))
            {
                continue;
            }

            var triggeredAt = DateTime.UtcNow;
            var emailSent = false;

            if (alert.NotifyEmail)
            {
                var pendingLog = new AlertLog
                {
                    Id = Guid.NewGuid(),
                    AlertId = alert.Id,
                    Value = snapshot.Value,
                    Threshold = alert.Threshold,
                    TriggeredAt = triggeredAt
                };

                await _alertNotifier.NotifyAsync(alert, pendingLog, cancellationToken);
                emailSent = true;
            }

            var log = new AlertLog
            {
                Id = Guid.NewGuid(),
                AlertId = alert.Id,
                Value = snapshot.Value,
                Threshold = alert.Threshold,
                TriggeredAt = triggeredAt,
                EmailSent = emailSent
            };

            await _alertRepository.CreateLogAsync(log, cancellationToken);
            await _alertRepository.UpdateLastTriggeredAsync(alert.Id, triggeredAt, cancellationToken);

            triggeredCount++;
            _logger.LogInformation(
                "Alert {AlertId} triggered for {Region}/{Metric}: {Value} {Condition} {Threshold}",
                alert.Id,
                alert.Region,
                alert.Metric,
                snapshot.Value,
                alert.Condition,
                alert.Threshold);
        }

        return triggeredCount;
    }

    public static bool IsTriggered(double value, double threshold, AlertCondition condition) =>
        condition switch
        {
            AlertCondition.Above => value >= threshold,
            AlertCondition.Below => value <= threshold,
            _ => false
        };

    private bool CanTriggerAgain(Alert alert)
    {
        if (alert.LastTriggeredAt is null)
        {
            return true;
        }

        var cooldown = TimeSpan.FromMinutes(Math.Max(_options.CooldownMinutes, 1));
        return DateTime.UtcNow - alert.LastTriggeredAt.Value >= cooldown;
    }
}
