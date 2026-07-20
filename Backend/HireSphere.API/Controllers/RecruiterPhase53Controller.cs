using HireSphere.API.DTOs.Recruiter;
using HireSphere.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireSphere.API.Controllers;

[ApiController]
[Route("api/recruiter")]
[Authorize(Policy = "RecruiterOrAdministrator")]
public sealed class RecruiterPhase53Controller : ControllerBase
{
    private readonly IRecruiterPhase53Service _service;

    public RecruiterPhase53Controller(IRecruiterPhase53Service service) => _service = service;

    [HttpGet("interviews")]
    public async Task<ActionResult<IReadOnlyList<RecruiterInterviewDetailDto>>> ListInterviews()
    {
        var (ok, error, result) = await _service.ListInterviewsAsync();
        return ok ? Ok(result) : MapFailure(error);
    }

    [HttpGet("interviews/{id:int}")]
    public async Task<ActionResult<RecruiterInterviewDetailDto>> GetInterview(int id)
    {
        var (ok, error, result) = await _service.GetInterviewAsync(id);
        return ok ? Ok(result) : MapFailure(error);
    }

    [HttpPost("interviews")]
    public async Task<ActionResult<ScheduleInterviewResultDto>> Schedule([FromBody] ScheduleInterviewDto dto)
    {
        var (ok, error, result) = await _service.ScheduleAsync(dto);
        return ok ? Ok(result) : MapFailure(error);
    }

    [HttpPost("interviews/{id:int}/reschedule")]
    public async Task<ActionResult<ScheduleInterviewResultDto>> Reschedule(int id, [FromBody] RescheduleInterviewDto dto)
    {
        var (ok, error, result) = await _service.RescheduleAsync(id, dto);
        return ok ? Ok(result) : MapFailure(error);
    }

    [HttpPatch("interviews/{id:int}/status")]
    public async Task<ActionResult<RecruiterInterviewDetailDto>> ChangeStatus(
        int id,
        [FromBody] ChangeInterviewStatusDto dto)
    {
        var (ok, error, result) = await _service.ChangeStatusAsync(id, dto);
        return ok ? Ok(result) : MapFailure(error);
    }

    [HttpGet("reports/summary")]
    public async Task<ActionResult<RecruiterReportSummaryDto>> ReportSummary([FromQuery] ReportFilterQuery filter)
    {
        var (ok, error, result) = await _service.GetReportSummaryAsync(filter);
        return ok ? Ok(result) : MapFailure(error);
    }

    [HttpGet("reports/applications")]
    [HttpGet("reports/screening")]
    [HttpGet("reports/assessments")]
    [HttpGet("reports/interviews")]
    public async Task<ActionResult<RecruiterReportSummaryDto>> ReportSections([FromQuery] ReportFilterQuery filter)
        => await ReportSummary(filter);

    [HttpGet("reports/export")]
    public async Task<IActionResult> Export([FromQuery] ReportFilterQuery filter)
    {
        var (ok, error, result) = await _service.ExportReportCsvAsync(filter);
        if (!ok || result is null)
        {
            return MapFailure(error);
        }

        return File(result.Content, result.ContentType, result.FileName);
    }

    private ActionResult MapFailure(string? error)
    {
        if (string.Equals(error, "Unauthorized.", StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized(new { message = error });
        }

        if (error != null
            && (error.Contains("access denied", StringComparison.OrdinalIgnoreCase)
                || error.Contains("not found", StringComparison.OrdinalIgnoreCase)))
        {
            return NotFound(new { message = "Resource not found." });
        }

        return BadRequest(new { message = error ?? "Request failed." });
    }
}
