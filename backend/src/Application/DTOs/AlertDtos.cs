using System.ComponentModel.DataAnnotations;
using EnviroWatch.Domain.Models;

namespace EnviroWatch.Application.DTOs;

public record CreateAlertRequest(
    [Required, MaxLength(64)] string Metric,
    [Required, MaxLength(128)] string Region,
    [Required] double Threshold,
    [Required] AlertCondition Condition,
    bool NotifyEmail = false);

public record AlertDto(
    Guid Id,
    string Metric,
    string Region,
    double Threshold,
    AlertCondition Condition,
    bool NotifyEmail,
    bool IsActive,
    DateTime? LastTriggeredAt,
    DateTime CreatedAt);

public record AlertLogDto(
    Guid Id,
    Guid AlertId,
    string Metric,
    string Region,
    double Value,
    double Threshold,
    AlertCondition Condition,
    DateTime TriggeredAt,
    bool EmailSent);
