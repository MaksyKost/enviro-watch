using EnviroWatch.Application.Interfaces;
using EnviroWatch.Domain.Models;
using Microsoft.Extensions.Logging;

namespace EnviroWatch.Infrastructure.Notifications;

public class LoggingAlertNotifier : IAlertNotifier
{
    private readonly ILogger<LoggingAlertNotifier> _logger;

    public LoggingAlertNotifier(ILogger<LoggingAlertNotifier> logger)
    {
        _logger = logger;
    }

    public Task NotifyAsync(Alert alert, AlertLog log, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Email notification stub: alert {AlertId} for user {UserId} — {Metric}={Value} in {Region} (threshold {Threshold})",
            alert.Id,
            alert.UserId,
            alert.Metric,
            log.Value,
            alert.Region,
            log.Threshold);

        return Task.CompletedTask;
    }
}
