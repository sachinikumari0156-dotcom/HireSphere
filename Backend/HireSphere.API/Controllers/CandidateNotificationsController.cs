using HireSphere.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireSphere.API.Controllers;

[ApiController]
[Route("api/candidate")]
[Authorize(Policy = "CandidateOnly")]
public class CandidateNotificationsController : ControllerBase
{
    private readonly ICandidateNotificationService _notifications;

    public CandidateNotificationsController(ICandidateNotificationService notifications)
    {
        _notifications = notifications;
    }

    [HttpGet("notifications")]
    public async Task<IActionResult> List([FromQuery] int? take = null)
    {
        var (ok, error, result) = await _notifications.ListAsync(take);
        return MapResult(ok, error, result);
    }

    [HttpPost("notifications/{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var (ok, error, result) = await _notifications.MarkReadAsync(id);
        return MapResult(ok, error, result);
    }

    [HttpPost("notifications/read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var (ok, error, result) = await _notifications.MarkAllReadAsync();
        return MapResult(ok, error, result);
    }

    private IActionResult MapResult<T>(bool ok, string? error, T? result)
    {
        if (ok)
        {
            return Ok(result);
        }

        if (error == "Unauthorized.")
        {
            return Unauthorized();
        }

        if (error is "Notification not found.")
        {
            return NotFound(new { message = error });
        }

        return BadRequest(new { message = error });
    }
}
