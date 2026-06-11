using System.ComponentModel.DataAnnotations;

namespace EnviroWatch.Application.DTOs;

public record CreateObservationRequest(
    [Required, MaxLength(128)] string Region,
    [Required, MaxLength(64)] string Metric,
    [Required] double Value,
    [MaxLength(16)] string? Unit,
    double? Lat,
    double? Lon,
    [MaxLength(512)] string? Notes,
    DateTime? ObservedAt);

public record ObservationDto(
    Guid Id,
    string Region,
    string Metric,
    double Value,
    string? Unit,
    double? Lat,
    double? Lon,
    string? Notes,
    DateTime ObservedAt,
    DateTime CreatedAt);
