using EnviroWatch.Application.DTOs;
using EnviroWatch.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EnviroWatch.API.Controllers;

[ApiController]
[Route("api/data")]
public class DataController : ControllerBase
{
    private readonly IDataSnapshotService _dataSnapshotService;

    public DataController(IDataSnapshotService dataSnapshotService)
    {
        _dataSnapshotService = dataSnapshotService;
    }

    /// <summary>
    /// Returns filtered environmental data snapshots for charts and history views.
    /// </summary>
    /// <param name="region">Partial region match, e.g. PL or Wroclaw,PL.</param>
    /// <param name="metric">Exact metric name, e.g. temperature.</param>
    /// <param name="source">Exact data source, e.g. openmeteo.</param>
    /// <param name="from">Inclusive UTC start of the time range.</param>
    /// <param name="to">Inclusive UTC end of the time range.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Items per page (max 200).</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("snapshots")]
    [ProducesResponseType(typeof(DataSnapshotListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<DataSnapshotListResponse>> GetSnapshots(
        [FromQuery] string? region,
        [FromQuery] string? metric,
        [FromQuery] string? source,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = new DataSnapshotQuery(region, metric, source, from, to, page, pageSize);
        var result = await _dataSnapshotService.GetSnapshotsAsync(query, cancellationToken);
        return Ok(result);
    }
}
