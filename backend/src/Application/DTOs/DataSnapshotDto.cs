namespace EnviroWatch.Application.DTOs;

public record DataSnapshotDto(
    string Source,
    string Metric,
    double Value,
    string? Unit,
    string Region,
    double? Lat,
    double? Lon,
    DateTime Timestamp);

public record DataSnapshotListResponse(
    IReadOnlyList<DataSnapshotDto> Items,
    int Total,
    int Page,
    int PageSize);
