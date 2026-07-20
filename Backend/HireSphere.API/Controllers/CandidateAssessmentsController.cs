using HireSphere.API.DTOs.Candidate;
using HireSphere.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireSphere.API.Controllers;

[ApiController]
[Route("api/candidate")]
[Authorize(Policy = "CandidateOnly")]
public class CandidateAssessmentsController : ControllerBase
{
    private readonly ICandidateAssessmentService _assessments;

    public CandidateAssessmentsController(ICandidateAssessmentService assessments)
    {
        _assessments = assessments;
    }

    [HttpGet("assessments")]
    public async Task<IActionResult> List()
    {
        var (ok, error, result) = await _assessments.ListAsync();
        return MapResult(ok, error, result);
    }

    [HttpGet("assessments/{assignmentId:int}")]
    public async Task<IActionResult> Get(int assignmentId)
    {
        var (ok, error, result) = await _assessments.GetAssignmentAsync(assignmentId);
        return MapResult(ok, error, result);
    }

    [HttpPost("assessments/{assignmentId:int}/start")]
    public async Task<IActionResult> Start(int assignmentId)
    {
        var (ok, error, result) = await _assessments.StartAsync(assignmentId);
        return MapResult(ok, error, result);
    }

    [HttpGet("assessments/attempts/{attemptId:int}")]
    public async Task<IActionResult> GetAttempt(int attemptId)
    {
        var (ok, error, result) = await _assessments.GetAttemptAsync(attemptId);
        return MapResult(ok, error, result);
    }

    [HttpPut("assessments/attempts/{attemptId:int}/answers")]
    public async Task<IActionResult> SaveAnswers(int attemptId, [FromBody] SaveAssessmentAnswersDto dto)
    {
        var (ok, error, result) = await _assessments.SaveAnswersAsync(attemptId, dto);
        return MapResult(ok, error, result);
    }

    [HttpPost("assessments/attempts/{attemptId:int}/submit")]
    public async Task<IActionResult> Submit(int attemptId)
    {
        var (ok, error, result) = await _assessments.SubmitAsync(attemptId);
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

        if (error is "Candidate profile not found." or "Assessment not found." or "Attempt not found.")
        {
            return NotFound(new { message = error });
        }

        return BadRequest(new { message = error });
    }
}
