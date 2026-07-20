using HireSphere.API.DTOs.Recruiter;
using HireSphere.API.Models.Enums;
using HireSphere.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireSphere.API.Controllers;

[ApiController]
[Route("api/recruiter")]
[Authorize(Policy = "RecruiterOrAdministrator")]
public sealed class RecruiterPortalController : ControllerBase
{
    private readonly IRecruiterPortalService _service;

    public RecruiterPortalController(IRecruiterPortalService service)
    {
        _service = service;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<RecruiterDashboardDto>> GetDashboard()
    {
        var (ok, error, result) = await _service.GetDashboardAsync();
        if (!ok || result is null)
        {
            return MapFailure(error);
        }

        return Ok(result);
    }

    [HttpGet("jobs")]
    public async Task<ActionResult<DTOs.Candidate.PagedResultDto<RecruiterJobListItemDto>>> ListJobs(
        [FromQuery] RecruiterJobListQuery query)
    {
        var (ok, error, result) = await _service.ListJobsAsync(query);
        if (!ok || result is null)
        {
            return MapFailure(error);
        }

        return Ok(result);
    }

    [HttpPost("jobs")]
    public async Task<ActionResult<RecruiterJobDetailDto>> CreateJob([FromBody] UpsertRecruiterJobDto dto)
    {
        var (ok, error, result) = await _service.CreateJobAsync(dto);
        if (!ok || result is null)
        {
            return MapFailure(error);
        }

        return CreatedAtAction(nameof(GetJob), new { id = result.Id }, result);
    }

    [HttpGet("jobs/{id:int}")]
    public async Task<ActionResult<RecruiterJobDetailDto>> GetJob(int id)
    {
        var (ok, error, result) = await _service.GetJobAsync(id);
        if (!ok || result is null)
        {
            return MapFailure(error);
        }

        return Ok(result);
    }

    [HttpPut("jobs/{id:int}")]
    public async Task<ActionResult<RecruiterJobDetailDto>> UpdateJob(int id, [FromBody] UpsertRecruiterJobDto dto)
    {
        var (ok, error, result) = await _service.UpdateJobAsync(id, dto);
        if (!ok || result is null)
        {
            return MapFailure(error);
        }

        return Ok(result);
    }

    [HttpDelete("jobs/{id:int}")]
    public async Task<IActionResult> DeleteJob(int id)
    {
        var (ok, error) = await _service.DeleteJobAsync(id);
        if (!ok)
        {
            return MapFailure(error);
        }

        return NoContent();
    }

    [HttpPatch("jobs/{id:int}/status")]
    public async Task<ActionResult<RecruiterJobDetailDto>> ChangeJobStatus(
        int id,
        [FromBody] ChangeJobStatusDto dto)
    {
        var (ok, error, result) = await _service.ChangeJobStatusAsync(id, dto.Status);
        if (!ok || result is null)
        {
            return MapFailure(error);
        }

        return Ok(result);
    }

    [HttpGet("jobs/{id:int}/applications")]
    public async Task<ActionResult<DTOs.Candidate.PagedResultDto<RecruiterApplicantListItemDto>>> ListApplications(
        int id,
        [FromQuery] RecruiterPipelineQuery query)
    {
        var (ok, error, result) = await _service.ListApplicantsAsync(id, query);
        if (!ok || result is null)
        {
            return MapFailure(error);
        }

        return Ok(result);
    }

    [HttpGet("applications/{id:int}")]
    public async Task<ActionResult<RecruiterApplicationDetailDto>> GetApplication(int id)
    {
        var (ok, error, result) = await _service.GetApplicationAsync(id);
        if (!ok || result is null)
        {
            return MapFailure(error);
        }

        return Ok(result);
    }

    [HttpPatch("applications/{id:int}/status")]
    public async Task<IActionResult> ChangeApplicationStatus(int id, [FromBody] ChangeApplicationStatusDto dto)
    {
        var (ok, error) = await _service.ChangeApplicationStatusAsync(id, dto.Status, dto.Notes);
        if (!ok)
        {
            return MapFailure(error);
        }

        return NoContent();
    }

    [HttpGet("applications/{id:int}/notes")]
    public async Task<ActionResult<IReadOnlyList<RecruiterNoteDto>>> ListNotes(int id)
    {
        var (ok, error, detail) = await _service.GetApplicationAsync(id);
        if (!ok || detail is null)
        {
            return MapFailure(error);
        }

        return Ok(detail.InternalNotes);
    }

    [HttpPost("applications/{id:int}/notes")]
    public async Task<ActionResult<RecruiterNoteDto>> AddNote(int id, [FromBody] UpsertApplicationNoteDto dto)
    {
        var (ok, error, result) = await _service.AddNoteAsync(id, dto.Content);
        if (!ok || result is null)
        {
            return MapFailure(error);
        }

        return Ok(result);
    }

    [HttpDelete("notes/{id:int}")]
    public async Task<IActionResult> DeleteNote(int id)
    {
        var (ok, error) = await _service.DeleteNoteAsync(id);
        if (!ok)
        {
            return MapFailure(error);
        }

        return NoContent();
    }

    [HttpPost("applications/compare")]
    public async Task<ActionResult<CandidateComparisonDto>> Compare([FromBody] CompareApplicantsRequestDto dto)
    {
        var (ok, error, result) = await _service.CompareApplicantsAsync(dto.ApplicationIds);
        if (!ok || result is null)
        {
            return MapFailure(error);
        }

        return Ok(result);
    }

    private ActionResult MapFailure(string? error)
    {
        if (string.Equals(error, "Unauthorized.", StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized(new { message = error });
        }

        if (error != null
            && (error.Contains("access denied", StringComparison.OrdinalIgnoreCase)
                || error.Contains("not accessible", StringComparison.OrdinalIgnoreCase)))
        {
            return NotFound(new { message = "Resource not found." });
        }

        if (error != null
            && (error.Contains("Cannot transition", StringComparison.OrdinalIgnoreCase)
                || error.Contains("must", StringComparison.OrdinalIgnoreCase)
                || error.Contains("required", StringComparison.OrdinalIgnoreCase)
                || error.Contains("cannot exceed", StringComparison.OrdinalIgnoreCase)
                || error.Contains("Select between", StringComparison.OrdinalIgnoreCase)
                || error.Contains("Only Draft", StringComparison.OrdinalIgnoreCase)
                || error.Contains("not authorized", StringComparison.OrdinalIgnoreCase)
                || error.Contains("not part of", StringComparison.OrdinalIgnoreCase)))
        {
            return BadRequest(new { message = error });
        }

        return BadRequest(new { message = error ?? "Request failed." });
    }
}
