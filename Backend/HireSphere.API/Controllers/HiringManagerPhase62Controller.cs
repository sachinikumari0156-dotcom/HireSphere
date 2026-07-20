using HireSphere.API.DTOs.HiringManager;
using HireSphere.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireSphere.API.Controllers;

[ApiController]
[Route("api/hiring-manager")]
[Authorize(Policy = "HiringManagerOrAdministrator")]
public sealed class HiringManagerPhase62Controller : ControllerBase
{
    private readonly IHiringManagerPhase62Service _service;

    public HiringManagerPhase62Controller(IHiringManagerPhase62Service service)
    {
        _service = service;
    }

    [HttpGet("interviews")]
    public async Task<IActionResult> ListInterviews()
    {
        var (ok, error, result) = await _service.ListInterviewsAsync();
        return ok ? Ok(result) : MapFailure(error);
    }

    [HttpGet("interviews/{id:int}")]
    public async Task<IActionResult> GetInterview(int id)
    {
        var (ok, error, result) = await _service.GetInterviewAsync(id);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpPost("interviews/{id:int}/feedback")]
    public async Task<IActionResult> CreateFeedback(int id, [FromBody] UpsertInterviewFeedbackDto dto)
    {
        var (ok, error, result) = await _service.UpsertFeedbackAsync(id, dto, isUpdate: false);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpPut("interviews/{id:int}/feedback")]
    public async Task<IActionResult> UpdateFeedback(int id, [FromBody] UpsertInterviewFeedbackDto dto)
    {
        var (ok, error, result) = await _service.UpsertFeedbackAsync(id, dto, isUpdate: true);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpGet("applications/{id:int}/evaluation")]
    public async Task<IActionResult> GetEvaluation(int id)
    {
        var (ok, error, result) = await _service.GetEvaluationAsync(id);
        return ok ? Ok(result) : MapFailure(error);
    }

    [HttpPost("applications/{id:int}/evaluation")]
    public async Task<IActionResult> CreateEvaluation(int id, [FromBody] UpsertEvaluationDto dto)
    {
        var (ok, error, result) = await _service.UpsertEvaluationAsync(id, dto, isUpdate: false);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpPut("applications/{id:int}/evaluation")]
    public async Task<IActionResult> UpdateEvaluation(int id, [FromBody] UpsertEvaluationDto dto)
    {
        var (ok, error, result) = await _service.UpsertEvaluationAsync(id, dto, isUpdate: true);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpPost("applications/{id:int}/recommendation")]
    public async Task<IActionResult> Recommend(int id, [FromBody] CreateRecommendationDto dto)
    {
        var (ok, error, result) = await _service.SubmitRecommendationAsync(id, dto);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpGet("applications/{id:int}/decision-history")]
    public async Task<IActionResult> DecisionHistory(int id)
    {
        var (ok, error, result) = await _service.GetDecisionHistoryAsync(id);
        return ok ? Ok(result) : MapFailure(error);
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
