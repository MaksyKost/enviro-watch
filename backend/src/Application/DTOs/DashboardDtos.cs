using System.ComponentModel.DataAnnotations;
using EnviroWatch.Domain.Models;

namespace EnviroWatch.Application.DTOs;

public record CreateDashboardRequest(
    [Required, MaxLength(128)] string Name,
    [MaxLength(512)] string? Description);

public record UpdateDashboardRequest(
    [Required, MaxLength(128)] string Name,
    [MaxLength(512)] string? Description);

public record DashboardDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<WidgetDto> Widgets);

public record CreateWidgetRequest(
    [Required, MaxLength(128)] string Title,
    [Required] WidgetType Type,
    [Required, MaxLength(64)] string Metric,
    [Required, MaxLength(128)] string Region,
    [MaxLength(64)] string? Source,
    string? ConfigJson,
    int SortOrder = 0);

public record UpdateWidgetRequest(
    [Required, MaxLength(128)] string Title,
    [Required] WidgetType Type,
    [Required, MaxLength(64)] string Metric,
    [Required, MaxLength(128)] string Region,
    [MaxLength(64)] string? Source,
    string? ConfigJson,
    int SortOrder = 0);

public record WidgetDto(
    Guid Id,
    Guid DashboardId,
    string Title,
    WidgetType Type,
    string Metric,
    string Region,
    string? Source,
    string? ConfigJson,
    int SortOrder,
    DateTime CreatedAt);
