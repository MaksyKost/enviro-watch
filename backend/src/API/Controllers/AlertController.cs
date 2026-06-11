using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EnviroWatch.Application.Authorization;
using EnviroWatch.Application.DTOs;
using EnviroWatch.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnviroWatch.API.Controllers;

[ApiController]
[Route("api/alerts")]
[Authorize]
public class AlertController : ControllerBase
{
    private readonly IAlertService _alertService;

    public AlertController(IAlertService alertService)
    {
        _alertService = alertService;
    }

    /// <summary>
    /// Create a threshold alert. Analyst or Admin only.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.AnalystOrAbove)]
    [ProducesResponseType(typeof(AlertDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AlertDto>> Create(
        [FromBody] CreateAlertRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var alert = await _alertService.CreateAsync(userId.Value, request, cancellationToken);
        return CreatedAtAction(nameof(GetLogs), new { id = alert.Id }, alert);
    }

    /// <summary>
    /// List alerts for the current user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AlertDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AlertDto>>> GetMine(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var alerts = await _alertService.GetUserAlertsAsync(userId.Value, cancellationToken);
        return Ok(alerts);
    }

    /// <summary>
    /// Get trigger history for an alert.
    /// </summary>
    [HttpGet("{id:guid}/logs")]
    [ProducesResponseType(typeof(IReadOnlyList<AlertLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AlertLogDto>>> GetLogs(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var logs = await _alertService.GetAlertLogsAsync(
                userId.Value,
                id,
                User.IsInRole(Roles.Admin),
                cancellationToken);
            return Ok(logs);
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

    /// <summary>
    /// Delete an alert. Analyst can delete own alerts; Admin can delete any.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.AnalystOrAbove)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            await _alertService.DeleteAsync(
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

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(value, out var userId) ? userId : null;
    }
}
