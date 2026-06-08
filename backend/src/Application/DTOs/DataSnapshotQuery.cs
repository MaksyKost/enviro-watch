namespace EnviroWatch.Application.DTOs;

public record DataSnapshotQuery(
    string? Region,
    string? Metric,
    string? Source,
    DateTime? From,
    DateTime? To,
    int Page = 1,
    int PageSize = 50);
