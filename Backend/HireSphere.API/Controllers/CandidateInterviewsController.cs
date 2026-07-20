using HireSphere.API.DTOs.Candidate;
using HireSphere.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireSphere.API.Controllers;

[ApiController]
[Route("api/candidate")]
[Authorize(Policy = "CandidateOnly")]
public class CandidateInterviewsController : ControllerBase
{
    private readonly ICandidateInterviewService _interviews;

    public CandidateInterviewsController(ICandidateInterviewService interviews)
    {
        _interviews = interviews;
    }

    [HttpGet("interviews")]
    public async Task<IActionResult> List()
    {
        var (ok, error, result) = await _interviews.ListAsync();
        return MapResult(ok, error, result);
    }

    [HttpGet("interviews/{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var (ok, error, result) = await _interviews.GetAsync(id);
        return MapResult(ok, error, result);
    }

    [HttpPost("interviews/{id:int}/confirm")]
    public async Task<IActionResult> Confirm(int id)
    {
        var (ok, error, result) = await _interviews.ConfirmAsync(id);
        return MapResult(ok, error, result);
    }

    [HttpPost("interviews/{id:int}/reschedule-request")]
    public async Task<IActionResult> RequestReschedule(int id, [FromBody] InterviewRescheduleRequestDto dto)
    {
        var (ok, error, result) = await _interviews.RequestRescheduleAsync(id, dto);
        return MapResult(ok, error, result);
    }

    [HttpPost("interviews/{id:int}/decline")]
    public async Task<IActionResult> Decline(int id, [FromBody] InterviewDeclineDto dto)
    {
        var (ok, error, result) = await _interviews.DeclineAsync(id, dto);
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

        if (error is "Candidate profile not found." or "Interview not found.")
        {
            return NotFound(new { message = error });
        }

        return BadRequest(new { message = error });
    }
}
