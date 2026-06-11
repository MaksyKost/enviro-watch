using EnviroWatch.Application.Authorization;
using EnviroWatch.Application.DTOs;
using EnviroWatch.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnviroWatch.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class AdminController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IAdminService _adminService;

    public AdminController(IAuthService authService, IAdminService adminService)
    {
        _authService = authService;
        _adminService = adminService;
    }

    /// <summary>
    /// Platform statistics. Admin only.
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(AdminStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminStatsDto>> GetStats(CancellationToken cancellationToken)
    {
        var stats = await _adminService.GetStatsAsync(cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// List all users. Admin only.
    /// </summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(IReadOnlyList<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetUsers(CancellationToken cancellationToken)
    {
        var users = await _authService.GetAllUsersAsync(cancellationToken);
        return Ok(users);
    }

    /// <summary>
    /// Change a user's role. Admin only.
    /// </summary>
    [HttpPut("users/{userId:guid}/role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserRole(
        Guid userId,
        [FromBody] UpdateUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _authService.UpdateUserRoleAsync(userId, request.Role, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
