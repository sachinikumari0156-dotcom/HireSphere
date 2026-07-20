using HireSphere.API.DTOs.Candidate;
using HireSphere.API.Models.Enums;
using HireSphere.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireSphere.API.Controllers;

[ApiController]
[Route("api/candidate")]
[Authorize(Policy = "CandidateOnly")]
public class CandidatePortalController : ControllerBase
{
    private readonly ICandidateProfileService _profileService;

    public CandidatePortalController(ICandidateProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var (ok, error, result) = await _profileService.GetDashboardAsync();
        return ok ? Ok(result) : BadRequest(new { message = error });
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var (ok, error, result) = await _profileService.GetProfileAsync();
        if (!ok)
        {
            return error == "Unauthorized."
                ? Unauthorized()
                : error == "Candidate profile not found."
                    ? NotFound(new { message = error })
                    : BadRequest(new { message = error });
        }

        return Ok(result);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateCandidateProfileDto dto)
    {
        var (ok, error, result) = await _profileService.UpdateProfileAsync(dto);
        if (!ok)
        {
            return error == "Unauthorized."
                ? Unauthorized()
                : error == "Candidate profile not found."
                    ? NotFound(new { message = error })
                    : BadRequest(new { message = error });
        }

        return Ok(result);
    }

    [HttpPost("experience")]
    public async Task<IActionResult> AddExperience([FromBody] CreateWorkExperienceDto dto)
    {
        var (ok, error, result) = await _profileService.AddExperienceAsync(dto);
        return ok
            ? CreatedAtAction(nameof(GetProfile), result)
            : BadRequest(new { message = error });
    }

    [HttpPut("experience/{id:int}")]
    public async Task<IActionResult> UpdateExperience(int id, [FromBody] UpdateWorkExperienceDto dto)
    {
        var (ok, error, result) = await _profileService.UpdateExperienceAsync(id, dto);
        if (!ok)
        {
            return error == "Work experience not found."
                ? NotFound(new { message = error })
                : BadRequest(new { message = error });
        }

        return Ok(result);
    }

    [HttpDelete("experience/{id:int}")]
    public async Task<IActionResult> DeleteExperience(int id)
    {
        var (ok, error) = await _profileService.DeleteExperienceAsync(id);
        if (!ok)
        {
            return error == "Work experience not found."
                ? NotFound(new { message = error })
                : BadRequest(new { message = error });
        }

        return NoContent();
    }

    [HttpPost("education")]
    public async Task<IActionResult> AddEducation([FromBody] CreateEducationDto dto)
    {
        var (ok, error, result) = await _profileService.AddEducationAsync(dto);
        return ok
            ? CreatedAtAction(nameof(GetProfile), result)
            : BadRequest(new { message = error });
    }

    [HttpPut("education/{id:int}")]
    public async Task<IActionResult> UpdateEducation(int id, [FromBody] UpdateEducationDto dto)
    {
        var (ok, error, result) = await _profileService.UpdateEducationAsync(id, dto);
        if (!ok)
        {
            return error == "Education not found."
                ? NotFound(new { message = error })
                : BadRequest(new { message = error });
        }

        return Ok(result);
    }

    [HttpDelete("education/{id:int}")]
    public async Task<IActionResult> DeleteEducation(int id)
    {
        var (ok, error) = await _profileService.DeleteEducationAsync(id);
        if (!ok)
        {
            return error == "Education not found."
                ? NotFound(new { message = error })
                : BadRequest(new { message = error });
        }

        return NoContent();
    }

    [HttpGet("skills/catalog")]
    public async Task<IActionResult> ListSkillCatalog()
    {
        return Ok(await _profileService.ListSkillCatalogAsync());
    }

    [HttpPost("skills")]
    public async Task<IActionResult> AddSkill([FromBody] CreateCandidateSkillDto dto)
    {
        var (ok, error, result) = await _profileService.AddSkillAsync(dto);
        return ok
            ? CreatedAtAction(nameof(GetProfile), result)
            : BadRequest(new { message = error });
    }

    [HttpPut("skills/{id:int}")]
    public async Task<IActionResult> UpdateSkill(int id, [FromBody] UpdateCandidateSkillDto dto)
    {
        var (ok, error, result) = await _profileService.UpdateSkillAsync(id, dto);
        if (!ok)
        {
            return error == "Skill not found."
                ? NotFound(new { message = error })
                : BadRequest(new { message = error });
        }

        return Ok(result);
    }

    [HttpDelete("skills/{id:int}")]
    public async Task<IActionResult> DeleteSkill(int id)
    {
        var (ok, error) = await _profileService.DeleteSkillAsync(id);
        if (!ok)
        {
            return error == "Skill not found."
                ? NotFound(new { message = error })
                : BadRequest(new { message = error });
        }

        return NoContent();
    }

    [HttpPost("certifications")]
    public async Task<IActionResult> AddCertification([FromBody] CreateCertificationDto dto)
    {
        var (ok, error, result) = await _profileService.AddCertificationAsync(dto);
        return ok
            ? CreatedAtAction(nameof(GetProfile), result)
            : BadRequest(new { message = error });
    }

    [HttpPut("certifications/{id:int}")]
    public async Task<IActionResult> UpdateCertification(int id, [FromBody] UpdateCertificationDto dto)
    {
        var (ok, error, result) = await _profileService.UpdateCertificationAsync(id, dto);
        if (!ok)
        {
            return error == "Certification not found."
                ? NotFound(new { message = error })
                : BadRequest(new { message = error });
        }

        return Ok(result);
    }

    [HttpDelete("certifications/{id:int}")]
    public async Task<IActionResult> DeleteCertification(int id)
    {
        var (ok, error) = await _profileService.DeleteCertificationAsync(id);
        if (!ok)
        {
            return error == "Certification not found."
                ? NotFound(new { message = error })
                : BadRequest(new { message = error });
        }

        return NoContent();
    }

    [HttpGet("resumes")]
    public async Task<IActionResult> ListResumes()
    {
        var (ok, error, result) = await _profileService.ListResumesAsync();
        return ok ? Ok(result) : BadRequest(new { message = error });
    }

    [HttpPost("resumes")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadResume(IFormFile file)
    {
        var (ok, error, result) = await _profileService.UploadResumeAsync(file);
        return ok
            ? CreatedAtAction(nameof(ListResumes), result)
            : BadRequest(new { message = error });
    }

    [HttpDelete("resumes/{id:int}")]
    public async Task<IActionResult> DeleteResume(int id)
    {
        var (ok, error) = await _profileService.DeleteResumeAsync(id);
        if (!ok)
        {
            return error == "Resume not found."
                ? NotFound(new { message = error })
                : BadRequest(new { message = error });
        }

        return NoContent();
    }

    [HttpPost("resumes/{id:int}/set-primary")]
    public async Task<IActionResult> SetPrimaryResume(int id)
    {
        var (ok, error) = await _profileService.SetPrimaryResumeAsync(id);
        if (!ok)
        {
            return error == "Resume not found."
                ? NotFound(new { message = error })
                : BadRequest(new { message = error });
        }

        return NoContent();
    }

    [HttpGet("documents")]
    public async Task<IActionResult> ListDocuments()
    {
        var (ok, error, result) = await _profileService.ListDocumentsAsync();
        return ok ? Ok(result) : BadRequest(new { message = error });
    }

    [HttpPost("documents")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadDocument(
        IFormFile file,
        [FromForm] DocumentType documentType)
    {
        var (ok, error, result) = await _profileService.UploadDocumentAsync(file, documentType);
        return ok
            ? CreatedAtAction(nameof(ListDocuments), result)
            : BadRequest(new { message = error });
    }

    [HttpGet("documents/{id:int}/download")]
    public async Task<IActionResult> DownloadDocument(int id)
    {
        var (ok, error, result) = await _profileService.DownloadDocumentAsync(id);
        if (!ok || result == null)
        {
            return error == "Document not found."
                ? NotFound(new { message = error })
                : BadRequest(new { message = error });
        }

        return File(result.Content, result.ContentType, result.FileName);
    }

    [HttpDelete("documents/{id:int}")]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        var (ok, error) = await _profileService.DeleteDocumentAsync(id);
        if (!ok)
        {
            return error == "Document not found."
                ? NotFound(new { message = error })
                : BadRequest(new { message = error });
        }

        return NoContent();
    }
}
