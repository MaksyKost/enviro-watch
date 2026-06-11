using EnviroWatch.Application.DTOs;
using EnviroWatch.Application.Interfaces;
using EnviroWatch.Domain.Models;

namespace EnviroWatch.Application.Services;

public class AlertService : IAlertService
{
    private readonly IAlertRepository _alertRepository;

    public AlertService(IAlertRepository alertRepository)
    {
        _alertRepository = alertRepository;
    }

    public async Task<AlertDto> CreateAsync(
        Guid userId,
        CreateAlertRequest request,
        CancellationToken cancellationToken = default)
    {
        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Metric = request.Metric.Trim().ToLowerInvariant(),
            Region = request.Region.Trim(),
            Threshold = request.Threshold,
            Condition = request.Condition,
            NotifyEmail = request.NotifyEmail,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _alertRepository.CreateAsync(alert, cancellationToken);
        return MapAlert(alert);
    }

    public async Task<IReadOnlyList<AlertDto>> GetUserAlertsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var alerts = await _alertRepository.GetByUserIdAsync(userId, cancellationToken);
        return alerts.Select(MapAlert).ToList();
    }

    public async Task<IReadOnlyList<AlertLogDto>> GetAlertLogsAsync(
        Guid userId,
        Guid alertId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var alert = await _alertRepository.GetByIdAsync(alertId, cancellationToken)
            ?? throw new KeyNotFoundException("Alert not found.");

        if (!isAdmin && alert.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have access to this alert.");
        }

        var logs = await _alertRepository.GetLogsByAlertIdAsync(alertId, cancellationToken);
        return logs.Select(log => MapLog(alert, log)).ToList();
    }

    public async Task DeleteAsync(
        Guid userId,
        Guid alertId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var alert = await _alertRepository.GetByIdAsync(alertId, cancellationToken)
            ?? throw new KeyNotFoundException("Alert not found.");

        if (!isAdmin && alert.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have access to this alert.");
        }

        await _alertRepository.DeleteAsync(alertId, cancellationToken);
    }

    private static AlertDto MapAlert(Alert alert) =>
        new(
            alert.Id,
            alert.Metric,
            alert.Region,
            alert.Threshold,
            alert.Condition,
            alert.NotifyEmail,
            alert.IsActive,
            alert.LastTriggeredAt,
            alert.CreatedAt);

    private static AlertLogDto MapLog(Alert alert, AlertLog log) =>
        new(
            log.Id,
            log.AlertId,
            alert.Metric,
            alert.Region,
            log.Value,
            log.Threshold,
            alert.Condition,
            log.TriggeredAt,
            log.EmailSent);
}
