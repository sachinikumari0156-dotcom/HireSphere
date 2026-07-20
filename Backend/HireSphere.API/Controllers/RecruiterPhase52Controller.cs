using HireSphere.API.DTOs.Recruiter;
using HireSphere.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireSphere.API.Controllers;

[ApiController]
[Route("api/recruiter")]
[Authorize(Policy = "RecruiterOrAdministrator")]
public sealed class RecruiterPhase52Controller : ControllerBase
{
    private readonly IRecruiterPhase52Service _service;

    public RecruiterPhase52Controller(IRecruiterPhase52Service service)
    {
        _service = service;
    }

    [HttpGet("applications/{id:int}/ranking")]
    public async Task<ActionResult<RecruiterRankingDto>> GetRanking(int id)
    {
        var (ok, error, result) = await _service.GetRankingAsync(id);
        return ok ? Ok(result) : MapFailure(error);
    }

    [HttpPost("applications/{id:int}/ranking/review")]
    public async Task<ActionResult<RankingReviewDto>> RankingReview(int id, [FromBody] CreateRankingReviewDto dto)
    {
        var (ok, error, result) = await _service.RecordRankingReviewAsync(id, dto);
        return ok ? Ok(result) : MapFailure(error);
    }

    [HttpGet("screening-queue")]
    public async Task<ActionResult<IReadOnlyList<ScreeningQueueItemDto>>> ScreeningQueue()
    {
        var (ok, error, result) = await _service.GetScreeningQueueAsync();
        return ok ? Ok(result) : MapFailure(error);
    }

    [HttpPost("applications/{id:int}/screening-decision")]
    public async Task<IActionResult> ScreeningDecision(int id, [FromBody] ScreeningDecisionDto dto)
    {
        var (ok, error) = await _service.ApplyScreeningDecisionAsync(id, dto);
        return ok ? NoContent() : MapFailure(error);
    }

    [HttpGet("assessments")]
    public async Task<ActionResult<IReadOnlyList<RecruiterAssessmentListItemDto>>> ListAssessments()
    {
        var (ok, error, result) = await _service.ListAssessmentsAsync();
        return ok ? Ok(result) : MapFailure(error);
    }

    [HttpPost("assessments")]
    public async Task<ActionResult<RecruiterAssessmentDetailDto>> CreateAssessment([FromBody] UpsertAssessmentDto dto)
    {
        var (ok, error, result) = await _service.CreateAssessmentAsync(dto);
        return ok ? CreatedAtAction(nameof(GetAssessment), new { id = result!.Id }, result) : MapFailure(error);
    }

    [HttpGet("assessments/{id:int}")]
    public async Task<ActionResult<RecruiterAssessmentDetailDto>> GetAssessment(int id)
    {
        var (ok, error, result) = await _service.GetAssessmentAsync(id);
        return ok ? Ok(result) : MapFailure(error);
    }

    [HttpPut("assessments/{id:int}")]
    public async Task<ActionResult<RecruiterAssessmentDetailDto>> UpdateAssessment(
        int id,
        [FromBody] UpsertAssessmentDto dto)
    {
        var (ok, error, result) = await _service.UpdateAssessmentAsync(id, dto);
        return ok ? Ok(result) : MapFailure(error);
    }

    [HttpPost("assessments/{id:int}/archive")]
    public async Task<IActionResult> ArchiveAssessment(int id)
    {
        var (ok, error) = await _service.ArchiveAssessmentAsync(id);
        return ok ? NoContent() : MapFailure(error);
    }

    [HttpPost("assessments/{id:int}/questions")]
    public async Task<ActionResult<RecruiterAssessmentQuestionDto>> AddQuestion(
        int id,
        [FromBody] UpsertAssessmentQuestionDto dto)
    {
        var (ok, error, result) = await _service.AddQuestionAsync(id, dto);
        return ok ? Ok(result) : MapFailure(error);
    }

    [HttpPut("assessments/{id:int}/questions/{questionId:int}")]
    public async Task<ActionResult<RecruiterAssessmentQuestionDto>> UpdateQuestion(
        int id,
        int questionId,
        [FromBody] UpsertAssessmentQuestionDto dto)
    {
        var (ok, error, result) = await _service.UpdateQuestionAsync(id, questionId, dto);
        return ok ? Ok(result) : MapFailure(error);
    }

    [HttpDelete("assessments/{id:int}/questions/{questionId:int}")]
    public async Task<IActionResult> DeleteQuestion(int id, int questionId)
    {
        var (ok, error) = await _service.DeleteQuestionAsync(id, questionId);
        return ok ? NoContent() : MapFailure(error);
    }

    [HttpPost("applications/{applicationId:int}/assessments")]
    public async Task<ActionResult<RecruiterAssignmentDetailDto>> Assign(
        int applicationId,
        [FromBody] AssignAssessmentDto dto)
    {
        var (ok, error, result) = await _service.AssignAssessmentAsync(applicationId, dto);
        return ok ? Ok(result) : MapFailure(error);
    }

    [HttpGet("assessment-assignments/{id:int}")]
    public async Task<ActionResult<RecruiterAssignmentDetailDto>> GetAssignment(int id)
    {
        var (ok, error, result) = await _service.GetAssignmentAsync(id);
        return ok ? Ok(result) : MapFailure(error);
    }

    [HttpGet("assessment-assignments/{id:int}/attempts")]
    public async Task<ActionResult<IReadOnlyList<RecruiterAttemptSummaryDto>>> GetAttempts(int id)
    {
        var (ok, error, result) = await _service.GetAssignmentAttemptsAsync(id);
        return ok ? Ok(result) : MapFailure(error);
    }

    [HttpGet("applications/{id:int}/messages")]
    public async Task<ActionResult<ApplicationMessageThreadDto>> GetMessages(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var (ok, error, result) = await _service.GetMessagesAsync(id, page, pageSize);
        return ok ? Ok(result) : MapFailure(error);
    }

    [HttpPost("applications/{id:int}/messages")]
    public async Task<ActionResult<ApplicationMessageDto>> SendMessage(
        int id,
        [FromBody] SendApplicationMessageDto dto)
    {
        var (ok, error, result) = await _service.SendRecruiterMessageAsync(id, dto.Body);
        return ok ? Ok(result) : MapFailure(error);
    }

    [HttpPost("applications/{id:int}/messages/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var (ok, error) = await _service.MarkMessagesReadAsync(id);
        return ok ? NoContent() : MapFailure(error);
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
