using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using HireSphere.API.Data;
using HireSphere.API.Models;
using HireSphere.API.DTOs;
using HireSphere.API.Models.Enums;
using HireSphere.API.Services;

namespace HireSphere.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IResourceAuthorizationService _authz;

        public ApplicationsController(
            ApplicationDbContext context,
            ICurrentUserService currentUser,
            IResourceAuthorizationService authz)
        {
            _context = context;
            _currentUser = currentUser;
            _authz = authz;
        }

        [Authorize(Policy = "RecruiterOnly")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationDto>>> GetApplications()
        {
            if (_currentUser.UserId is not int recruiterId)
            {
                return Unauthorized();
            }

            IQueryable<Application> query = _context.Applications.AsNoTracking();

            if (_currentUser.OrganizationId is int orgId)
            {
                query = query.Where(a => _context.Jobs.Any(j =>
                    j.Id == a.JobId && (j.RecruiterId == recruiterId || j.OrganizationId == orgId)));
            }
            else
            {
                query = query.Where(a => _context.Jobs.Any(j =>
                    j.Id == a.JobId && j.RecruiterId == recruiterId));
            }

            var applications = await query
                .Select(a => new ApplicationDto
                {
                    Id = a.Id,
                    CandidateId = a.CandidateId,
                    JobId = a.JobId,
                    AppliedDate = a.AppliedDate,
                    Status = a.Status.ToString(),
                    CoverLetter = a.CoverLetter
                })
                .ToListAsync();

            return Ok(applications);
        }

        [Authorize(Policy = "RecruiterOnly")]
        [HttpGet("RecruiterApplications")]
        public async Task<ActionResult<IEnumerable<ApplicationDto>>> RecruiterApplications()
        {
            return await GetApplications();
        }

        [Authorize(Policy = "RecruiterOnly")]
        [HttpGet("RecruiterApplicationDetails")]
        public async Task<ActionResult<IEnumerable<ApplicationDetailsDto>>> GetRecruiterApplicationDetails()
        {
            if (_currentUser.UserId is not int id)
            {
                return Unauthorized();
            }

            var applications = await _context.Applications
                .Include(a => a.Job)
                .AsNoTracking()
                .Where(a => a.Job != null &&
                            (a.Job.RecruiterId == id
                             || (_currentUser.OrganizationId != null
                                 && a.Job.OrganizationId == _currentUser.OrganizationId)))
                .Select(a => new ApplicationDetailsDto
                {
                    Id = a.Id,
                    CandidateId = a.CandidateId,
                    CandidateName = _context.CandidateProfiles
                        .Where(c => c.UserId == a.CandidateId)
                        .Select(c => c.FullName)
                        .FirstOrDefault(),
                    PhoneNumber = _context.CandidateProfiles
                        .Where(c => c.UserId == a.CandidateId)
                        .Select(c => c.PhoneNumber)
                        .FirstOrDefault(),
                    Skills = _context.CandidateProfiles
                        .Where(c => c.UserId == a.CandidateId)
                        .Select(c => c.Skills)
                        .FirstOrDefault(),
                    ResumePath = _context.CandidateProfiles
                        .Where(c => c.UserId == a.CandidateId)
                        .Select(c => c.ResumePath)
                        .FirstOrDefault(),
                    JobId = a.JobId,
                    JobTitle = a.Job!.Title,
                    JobDescription = a.Job!.Description,
                    AppliedDate = a.AppliedDate,
                    Status = a.Status.ToString(),
                    CoverLetter = a.CoverLetter
                })
                .ToListAsync();

            return Ok(applications);
        }

        [Authorize(Policy = "CandidateOnly")]
        [HttpGet("MyApplications")]
        public async Task<ActionResult<IEnumerable<ApplicationDto>>> MyApplications()
        {
            if (_currentUser.UserId is not int id)
            {
                return Unauthorized();
            }

            var applications = await _context.Applications
                .AsNoTracking()
                .Where(a => a.CandidateId == id)
                .Select(a => new ApplicationDto
                {
                    Id = a.Id,
                    CandidateId = a.CandidateId,
                    JobId = a.JobId,
                    AppliedDate = a.AppliedDate,
                    Status = a.Status.ToString(),
                    CoverLetter = a.CoverLetter
                })
                .ToListAsync();

            return Ok(applications);
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApplicationDto>> GetApplication(int id)
        {
            var application = await _context.Applications
                .AsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new ApplicationDto
                {
                    Id = a.Id,
                    CandidateId = a.CandidateId,
                    JobId = a.JobId,
                    AppliedDate = a.AppliedDate,
                    Status = a.Status.ToString(),
                    CoverLetter = a.CoverLetter
                })
                .FirstOrDefaultAsync();

            if (application == null)
            {
                return NotFound();
            }

            if (_currentUser.IsInRole("Candidate"))
            {
                if (!_authz.RequireSelf(application.CandidateId))
                {
                    return Forbid();
                }
            }
            else if (_currentUser.IsInRole("Recruiter"))
            {
                if (!await _authz.RecruiterOwnsJobAsync(application.JobId))
                {
                    return Forbid();
                }
            }
            else if (!_currentUser.IsInRole("Admin") && !_currentUser.IsInRole("HiringManager"))
            {
                return Forbid();
            }

            return Ok(application);
        }

        [Authorize(Policy = "CandidateOnly")]
        [HttpPost]
        public async Task<ActionResult<ApplicationDto>> Apply(Application application)
        {
            if (_currentUser.UserId is not int userId)
            {
                return Unauthorized();
            }

            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == application.JobId);
            if (job == null)
            {
                return BadRequest("Job not found");
            }

            application.CandidateId = userId;
            application.AppliedDate = DateTime.UtcNow;
            application.Status = ApplicationStatus.Pending;
            application.CreatedAtUtc = DateTime.UtcNow;

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            var dto = new ApplicationDto
            {
                Id = application.Id,
                CandidateId = application.CandidateId,
                JobId = application.JobId,
                AppliedDate = application.AppliedDate,
                Status = application.Status.ToString(),
                CoverLetter = application.CoverLetter
            };

            return CreatedAtAction(nameof(GetApplication), new { id = application.Id }, dto);
        }

        [Authorize(Policy = "RecruiterOnly")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateApplication(int id, Application application)
        {
            var existing = await _context.Applications.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            if (!await _authz.RecruiterOwnsJobAsync(existing.JobId))
            {
                return Forbid();
            }

            existing.Status = application.Status;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Policy = "CandidateOnly")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteApplication(int id)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application == null)
            {
                return NotFound();
            }

            if (_currentUser.UserId is not int userId)
            {
                return Unauthorized();
            }

            if (application.CandidateId != userId)
            {
                return Forbid();
            }

            _context.Applications.Remove(application);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
