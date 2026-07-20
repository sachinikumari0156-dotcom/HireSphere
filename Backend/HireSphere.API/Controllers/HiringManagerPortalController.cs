using HireSphere.API.DTOs.HiringManager;
using HireSphere.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireSphere.API.Controllers;

[ApiController]
[Route("api/hiring-manager")]
[Authorize(Policy = "HiringManagerOrAdministrator")]
public sealed class HiringManagerPortalController : ControllerBase
{
    private readonly IHiringManagerPortalService _service;

    public HiringManagerPortalController(IHiringManagerPortalService service)
    {
        _service = service;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<HiringManagerDashboardDto>> GetDashboard()
    {
        var (ok, error, result) = await _service.GetDashboardAsync();
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpGet("jobs")]
    public async Task<IActionResult> ListJobs([FromQuery] HiringManagerJobListQuery query)
    {
        var (ok, error, result) = await _service.ListJobsAsync(query);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpGet("jobs/{id:int}")]
    public async Task<ActionResult<HiringManagerJobDetailDto>> GetJob(int id)
    {
        var (ok, error, result) = await _service.GetJobAsync(id);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpGet("jobs/{id:int}/candidates")]
    public async Task<IActionResult> ListCandidates(int id, [FromQuery] HiringManagerCandidateListQuery query)
    {
        var (ok, error, result) = await _service.ListCandidatesAsync(id, query);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpGet("applications/{id:int}")]
    public async Task<ActionResult<HiringManagerApplicationDetailDto>> GetApplication(int id)
    {
        var (ok, error, result) = await _service.GetApplicationAsync(id);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpPost("candidates/compare")]
    public async Task<ActionResult<HiringManagerComparisonDto>> Compare(
        [FromBody] HiringManagerCompareRequestDto dto)
    {
        var (ok, error, result) = await _service.CompareCandidatesAsync(dto.ApplicationIds);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpPost("jobs/{id:int}/review-comments")]
    public async Task<ActionResult<HiringManagerReviewCommentDto>> AddReviewComment(
        int id,
        [FromBody] CreateJobReviewCommentDto dto)
    {
        var (ok, error, result) = await _service.AddReviewCommentAsync(id, dto.Content);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    private ActionResult MapFailure(string? error)
    {
        if (string.Equals(error, "Unauthorized.", StringComparison.Ordinal))
        {
            return Unauthorized(new { message = error });
        }

        if (error is not null
            && (error.Contains("not found", StringComparison.OrdinalIgnoreCase)
                || error.Contains("access denied", StringComparison.OrdinalIgnoreCase)))
        {
            return NotFound(new { message = error });
        }

        return BadRequest(new { message = error ?? "Request failed." });
    }
}
