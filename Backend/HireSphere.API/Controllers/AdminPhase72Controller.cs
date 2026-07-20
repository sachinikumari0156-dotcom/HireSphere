using HireSphere.API.DTOs.Admin;
using HireSphere.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireSphere.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdministratorOnly")]
public sealed class AdminPhase72Controller : ControllerBase
{
    private readonly IAdminPhase72Service _service;

    public AdminPhase72Controller(IAdminPhase72Service service)
    {
        _service = service;
    }

    [HttpGet("audit-logs")]
    public async Task<IActionResult> AuditLogs([FromQuery] AdminAuditLogQuery query)
    {
        var (ok, error, result) = await _service.ListAuditLogsAsync(query);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpGet("audit-logs/export")]
    public async Task<IActionResult> ExportAudit([FromQuery] AdminAuditLogQuery query)
    {
        var (ok, error, result) = await _service.ExportAuditLogsAsync(query);
        return ok && result is not null
            ? File(result.Content, result.ContentType, result.FileName)
            : MapFailure(error);
    }

    [HttpGet("monitoring/summary")]
    public async Task<IActionResult> Monitoring()
    {
        var (ok, error, result) = await _service.GetMonitoringAsync();
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpGet("analytics/users")]
    public async Task<IActionResult> UserAnalytics([FromQuery] AdminAnalyticsFilter filter)
    {
        var (ok, error, result) = await _service.GetUserAnalyticsAsync(filter);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpGet("analytics/recruitment")]
    public async Task<IActionResult> RecruitmentAnalytics([FromQuery] AdminAnalyticsFilter filter)
    {
        var (ok, error, result) = await _service.GetRecruitmentAnalyticsAsync(filter);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpGet("analytics/departments")]
    public async Task<IActionResult> DepartmentAnalytics([FromQuery] AdminAnalyticsFilter filter)
    {
        var (ok, error, result) = await _service.GetDepartmentAnalyticsAsync(filter);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpGet("analytics/skills")]
    public async Task<IActionResult> SkillAnalytics([FromQuery] AdminAnalyticsFilter filter)
    {
        var (ok, error, result) = await _service.GetSkillAnalyticsAsync(filter);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpGet("analytics/skill-trends")]
    public async Task<IActionResult> SkillTrends([FromQuery] AdminAnalyticsFilter filter)
    {
        var (ok, error, result) = await _service.GetSkillTrendsAsync(filter);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpGet("integrations/ai/status")]
    public async Task<IActionResult> AiStatus()
    {
        var (ok, error, result) = await _service.GetAiStatusAsync();
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpGet("final-decisions/pending")]
    public async Task<IActionResult> PendingFinalDecisions()
    {
        var (ok, error, result) = await _service.ListPendingFinalDecisionsAsync();
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpGet("final-decisions/{applicationId:int}")]
    public async Task<IActionResult> FinalDecisionDetail(int applicationId)
    {
        var (ok, error, result) = await _service.GetFinalDecisionDetailAsync(applicationId);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpPost("final-decisions/{applicationId:int}")]
    public async Task<IActionResult> RecordFinalDecision(int applicationId, [FromBody] AdminFinalDecisionRequestDto dto)
    {
        var (ok, error, result) = await _service.RecordFinalDecisionAsync(applicationId, dto);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpGet("security/users/{userId:int}")]
    public async Task<IActionResult> SecurityUser(int userId)
    {
        var (ok, error, result) = await _service.GetSecurityUserAsync(userId);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpGet("exports/{type}")]
    public async Task<IActionResult> Export(string type, [FromQuery] AdminAnalyticsFilter filter)
    {
        var (ok, error, result) = await _service.ExportAsync(type, filter);
        return ok && result is not null
            ? File(result.Content, result.ContentType, result.FileName)
            : MapFailure(error);
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
