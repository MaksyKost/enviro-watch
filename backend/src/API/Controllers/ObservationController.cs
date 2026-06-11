using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EnviroWatch.Application.Authorization;
using EnviroWatch.Application.DTOs;
using EnviroWatch.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnviroWatch.API.Controllers;

[ApiController]
[Route("api/observations")]
[Authorize]
public class ObservationController : ControllerBase
{
    private readonly IObservationService _observationService;

    public ObservationController(IObservationService observationService)
    {
        _observationService = observationService;
    }

    /// <summary>
    /// Submit a manual environmental measurement. Analyst or Admin only.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.AnalystOrAbove)]
    [ProducesResponseType(typeof(ObservationDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ObservationDto>> Create(
        [FromBody] CreateObservationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var observation = await _observationService.CreateAsync(userId.Value, request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { id = observation.Id }, observation);
    }

    /// <summary>
    /// List manual observations for the current user. Admin sees all.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ObservationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ObservationDto>>> GetAll(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var observations = await _observationService.GetObservationsAsync(
            userId.Value,
            User.IsInRole(Roles.Admin),
            cancellationToken);

        return Ok(observations);
    }

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(value, out var userId) ? userId : null;
    }
}
