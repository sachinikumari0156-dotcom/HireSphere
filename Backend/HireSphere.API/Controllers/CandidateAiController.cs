using HireSphere.API.DTOs.Ai;
using HireSphere.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireSphere.API.Controllers;

[ApiController]
[Route("api/candidate")]
[Authorize(Policy = "CandidateOnly")]
public sealed class CandidateAiController : ControllerBase
{
    private readonly ICandidateAiService _service;

    public CandidateAiController(ICandidateAiService service)
    {
        _service = service;
    }

    [HttpPost("resumes/{id:int}/parse")]
    public async Task<IActionResult> Parse(int id)
    {
        var (ok, error, result) = await _service.ParseResumeAsync(id);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpGet("resumes/{id:int}/analysis")]
    public async Task<IActionResult> GetAnalysis(int id)
    {
        var (ok, error, result) = await _service.GetAnalysisAsync(id);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpPost("resumes/{id:int}/analysis/confirm")]
    public async Task<IActionResult> Confirm(int id, [FromBody] ConfirmResumeAnalysisDto dto)
    {
        var (ok, error, result) = await _service.ConfirmAnalysisAsync(id, dto ?? new ConfirmResumeAnalysisDto());
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpPost("resumes/{id:int}/analysis/reject")]
    public async Task<IActionResult> Reject(int id)
    {
        var (ok, error) = await _service.RejectAnalysisAsync(id);
        return ok ? Ok(new { message = "Analysis rejected. Profile unchanged." }) : MapFailure(error);
    }

    [HttpPut("ai/consent")]
    public async Task<IActionResult> Consent([FromBody] ExternalAiConsentDto dto)
    {
        var (ok, error, result) = await _service.SetConsentAsync(dto ?? new ExternalAiConsentDto());
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpGet("ai/status")]
    public async Task<IActionResult> Status()
    {
        var (ok, error, result) = await _service.GetStatusAsync();
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    private ActionResult MapFailure(string? error)
    {
        if (string.Equals(error, "Unauthorized.", StringComparison.Ordinal))
            return Unauthorized(new { message = error });
        if (error is not null && error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            return NotFound(new { message = error });
        return BadRequest(new { message = error ?? "Request failed." });
    }
}
