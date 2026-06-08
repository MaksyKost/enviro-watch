using EnviroWatch.Application.DTOs;
using EnviroWatch.Application.Interfaces;

namespace EnviroWatch.Application.Services;

public class DataSnapshotService : IDataSnapshotService
{
    private const int MaxPageSize = 200;

    private readonly IDataSnapshotRepository _repository;

    public DataSnapshotService(IDataSnapshotRepository repository)
    {
        _repository = repository;
    }

    public async Task<DataSnapshotListResponse> GetSnapshotsAsync(
        DataSnapshotQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize switch
        {
            < 1 => 50,
            > MaxPageSize => MaxPageSize,
            _ => query.PageSize
        };

        var normalizedQuery = query with { Page = page, PageSize = pageSize };

        var (items, total) = await _repository.GetFilteredAsync(
            normalizedQuery,
            cancellationToken);

        var dtos = items.Select(s => new DataSnapshotDto(
            s.Source,
            s.Metric,
            s.Value,
            s.Unit,
            s.Region,
            s.Lat,
            s.Lon,
            s.Timestamp)).ToList();

        return new DataSnapshotListResponse(dtos, total, page, pageSize);
    }
}
