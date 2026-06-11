using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EnviroWatch.Application.Authorization;
using EnviroWatch.Application.DTOs;
using EnviroWatch.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnviroWatch.API.Controllers;

[ApiController]
[Route("api/dashboards")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpPost]
    [Authorize(Roles = Roles.AnalystOrAbove)]
    [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<DashboardDto>> Create(
        [FromBody] CreateDashboardRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var dashboard = await _dashboardService.CreateDashboardAsync(userId.Value, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = dashboard.Id }, dashboard);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DashboardDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DashboardDto>>> GetAll(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var dashboards = await _dashboardService.GetUserDashboardsAsync(userId.Value, cancellationToken);
        return Ok(dashboards);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DashboardDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var dashboard = await _dashboardService.GetDashboardAsync(
            userId.Value,
            id,
            User.IsInRole(Roles.Admin),
            cancellationToken);

        return dashboard is null ? NotFound() : Ok(dashboard);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.AnalystOrAbove)]
    [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardDto>> Update(
        Guid id,
        [FromBody] UpdateDashboardRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var dashboard = await _dashboardService.UpdateDashboardAsync(
                userId.Value,
                id,
                request,
                User.IsInRole(Roles.Admin),
                cancellationToken);
            return Ok(dashboard);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.AnalystOrAbove)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            await _dashboardService.DeleteDashboardAsync(
                userId.Value,
                id,
                User.IsInRole(Roles.Admin),
                cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("{id:guid}/widgets")]
    [Authorize(Roles = Roles.AnalystOrAbove)]
    [ProducesResponseType(typeof(WidgetDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<WidgetDto>> AddWidget(
        Guid id,
        [FromBody] CreateWidgetRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var widget = await _dashboardService.AddWidgetAsync(
                userId.Value,
                id,
                request,
                User.IsInRole(Roles.Admin),
                cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id }, widget);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{dashboardId:guid}/widgets/{widgetId:guid}")]
    [Authorize(Roles = Roles.AnalystOrAbove)]
    [ProducesResponseType(typeof(WidgetDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<WidgetDto>> UpdateWidget(
        Guid dashboardId,
        Guid widgetId,
        [FromBody] UpdateWidgetRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var widget = await _dashboardService.UpdateWidgetAsync(
                userId.Value,
                dashboardId,
                widgetId,
                request,
                User.IsInRole(Roles.Admin),
                cancellationToken);
            return Ok(widget);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{dashboardId:guid}/widgets/{widgetId:guid}")]
    [Authorize(Roles = Roles.AnalystOrAbove)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteWidget(
        Guid dashboardId,
        Guid widgetId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            await _dashboardService.DeleteWidgetAsync(
                userId.Value,
                dashboardId,
                widgetId,
                User.IsInRole(Roles.Admin),
                cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(value, out var userId) ? userId : null;
    }
}
