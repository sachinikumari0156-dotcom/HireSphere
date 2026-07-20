using HireSphere.API.DTOs.Candidate;
using HireSphere.API.DTOs.Recruiter;
using HireSphere.API.Models.Enums;
using HireSphere.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireSphere.API.Controllers;

[ApiController]
[Route("api/candidate")]
[Authorize(Policy = "CandidateOnly")]
public class CandidateJobsController : ControllerBase
{
    private readonly ICandidateJobService _jobService;
    private readonly ICandidateApplicationService _applicationService;
    private readonly IRecruiterPhase52Service _messaging;

    public CandidateJobsController(
        ICandidateJobService jobService,
        ICandidateApplicationService applicationService,
        IRecruiterPhase52Service messaging)
    {
        _jobService = jobService;
        _applicationService = applicationService;
        _messaging = messaging;
    }

    [HttpGet("jobs")]
    public async Task<IActionResult> SearchJobs(
        [FromQuery] string? keyword,
        [FromQuery] string? location,
        [FromQuery] int? departmentId,
        [FromQuery] EmploymentType? employmentType,
        [FromQuery] WorkArrangement? workArrangement,
        [FromQuery] int? skillId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null)
    {
        var (ok, error, result) = await _jobService.SearchJobsAsync(new CandidateJobSearchQuery
        {
            Keyword = keyword,
            Location = location,
            DepartmentId = departmentId,
            EmploymentType = employmentType,
            WorkArrangement = workArrangement,
            SkillId = skillId,
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDir = sortDir
        });

        return MapResult(ok, error, result);
    }

    [HttpGet("jobs/{id:int}")]
    public async Task<IActionResult> GetJob(int id)
    {
        var (ok, error, result) = await _jobService.GetJobAsync(id);
        return MapResult(ok, error, result);
    }

    [HttpGet("jobs/{id:int}/match")]
    public async Task<IActionResult> GetJobMatch(int id)
    {
        var (ok, error, result) = await _jobService.GetJobMatchAsync(id);
        return MapResult(ok, error, result);
    }

    [HttpGet("recommendations")]
    public async Task<IActionResult> GetRecommendations([FromQuery] int? take = null)
    {
        var (ok, error, result) = await _jobService.GetRecommendationsAsync(take);
        return MapResult(ok, error, result);
    }

    [HttpGet("jobs/{id:int}/apply-options")]
    public async Task<IActionResult> GetApplyOptions(int id)
    {
        var (ok, error, result) = await _applicationService.GetApplyOptionsAsync(id);
        return MapResult(ok, error, result);
    }

    [HttpGet("applications")]
    public async Task<IActionResult> ListApplications()
    {
        var (ok, error, result) = await _applicationService.ListAsync();
        return MapResult(ok, error, result);
    }

    [HttpGet("applications/{id:int}")]
    public async Task<IActionResult> GetApplication(int id)
    {
        var (ok, error, result) = await _applicationService.GetAsync(id);
        return MapResult(ok, error, result);
    }

    [HttpPost("applications")]
    public async Task<IActionResult> SubmitApplication([FromBody] SubmitApplicationDto dto)
    {
        var (ok, error, result) = await _applicationService.SubmitAsync(dto);
        if (!ok)
        {
            return MapError(error);
        }

        return CreatedAtAction(nameof(GetApplication), new { id = result!.Id }, result);
    }

    [HttpPost("applications/{id:int}/withdraw")]
    public async Task<IActionResult> WithdrawApplication(int id)
    {
        var (ok, error, result) = await _applicationService.WithdrawAsync(id);
        return MapResult(ok, error, result);
    }

    [HttpGet("applications/{id:int}/messages")]
    public async Task<IActionResult> GetMessages(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var (ok, error, result) = await _messaging.GetMessagesAsync(id, page, pageSize);
        return MapResult(ok, error, result);
    }

    [HttpPost("applications/{id:int}/messages")]
    public async Task<IActionResult> SendMessage(int id, [FromBody] SendApplicationMessageDto dto)
    {
        var (ok, error, result) = await _messaging.SendCandidateMessageAsync(id, dto.Body);
        return MapResult(ok, error, result);
    }

    [HttpPost("applications/{id:int}/messages/read")]
    public async Task<IActionResult> MarkMessagesRead(int id)
    {
        var (ok, error) = await _messaging.MarkMessagesReadAsync(id);
        if (!ok)
        {
            return MapError(error);
        }

        return NoContent();
    }

    private IActionResult MapResult<T>(bool ok, string? error, T? result)
    {
        if (ok)
        {
            return Ok(result);
        }

        return MapError(error);
    }

    private IActionResult MapError(string? error)
    {
        if (error == "Unauthorized.")
        {
            return Unauthorized();
        }

        if (error is "Candidate profile not found." or "Job not found." or "Application not found."
            or "Selected resume was not found.")
        {
            return NotFound(new { message = error });
        }

        return BadRequest(new { message = error });
    }
}
