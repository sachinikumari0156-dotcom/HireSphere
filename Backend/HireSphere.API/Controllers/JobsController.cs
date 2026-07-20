using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HireSphere.API.Data;
using HireSphere.API.Models;
using HireSphere.API.DTOs;
using HireSphere.API.Services;
using Microsoft.AspNetCore.Authorization;

namespace HireSphere.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IResourceAuthorizationService _authz;

        public JobsController(
            ApplicationDbContext context,
            ICurrentUserService currentUser,
            IResourceAuthorizationService authz)
        {
            _context = context;
            _currentUser = currentUser;
            _authz = authz;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobDto>>> GetJobs()
        {
            var jobs = await _context.Jobs
                .AsNoTracking()
                .Select(j => new JobDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    Description = j.Description,
                    RequiredSkills = j.RequiredSkills,
                    Location = j.Location,
                    PostedDate = j.PostedDate,
                    RecruiterId = j.RecruiterId
                })
                .ToListAsync();

            return Ok(jobs);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<JobDto>>> SearchJobs(string keyword)
        {
            var jobs = await _context.Jobs
                .Where(j =>
                    j.Title.Contains(keyword) ||
                    j.RequiredSkills.Contains(keyword) ||
                    j.Location.Contains(keyword))
                .Select(j => new JobDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    Description = j.Description,
                    RequiredSkills = j.RequiredSkills,
                    Location = j.Location,
                    PostedDate = j.PostedDate,
                    RecruiterId = j.RecruiterId
                })
                .ToListAsync();

            return Ok(jobs);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<JobDto>> GetJob(int id)
        {
            var job = await _context.Jobs
                .AsNoTracking()
                .Where(j => j.Id == id)
                .Select(j => new JobDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    Description = j.Description,
                    RequiredSkills = j.RequiredSkills,
                    Location = j.Location,
                    PostedDate = j.PostedDate,
                    RecruiterId = j.RecruiterId
                })
                .FirstOrDefaultAsync();

            if (job == null)
            {
                return NotFound();
            }

            return Ok(job);
        }

        [Authorize(Policy = "RecruiterOnly")]
        [HttpGet("MyJobs")]
        public async Task<ActionResult<IEnumerable<JobDto>>> MyJobs()
        {
            if (_currentUser.UserId is not int userId)
            {
                return Unauthorized();
            }

            var jobs = await _context.Jobs
                .Where(j => j.RecruiterId == userId
                    || (_currentUser.OrganizationId != null
                        && j.OrganizationId == _currentUser.OrganizationId))
                .Select(j => new JobDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    Description = j.Description,
                    RequiredSkills = j.RequiredSkills,
                    Location = j.Location,
                    PostedDate = j.PostedDate,
                    RecruiterId = j.RecruiterId
                })
                .ToListAsync();

            return Ok(jobs);
        }

        [Authorize(Policy = "RecruiterOnly")]
        [HttpPost]
        public async Task<ActionResult<JobDto>> CreateJob(Job job)
        {
            if (_currentUser.UserId is not int userId)
            {
                return Unauthorized();
            }

            job.RecruiterId = userId;
            job.OrganizationId = _currentUser.OrganizationId;
            job.DepartmentId = _currentUser.DepartmentId;
            job.PostedDate = DateTime.UtcNow;
            job.CreatedAtUtc = DateTime.UtcNow;

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            var dto = new JobDto
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                RequiredSkills = job.RequiredSkills,
                Location = job.Location,
                PostedDate = job.PostedDate,
                RecruiterId = job.RecruiterId
            };

            return CreatedAtAction(nameof(GetJob), new { id = job.Id }, dto);
        }

        [Authorize(Policy = "RecruiterOnly")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateJob(int id, Job job)
        {
            if (_currentUser.UserId is null)
            {
                return Unauthorized();
            }

            var existingJob = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id);
            if (existingJob == null)
            {
                return NotFound();
            }

            if (!await _authz.RecruiterOwnsJobAsync(id))
            {
                return Forbid();
            }

            existingJob.Title = job.Title;
            existingJob.Description = job.Description;
            existingJob.RequiredSkills = job.RequiredSkills;
            existingJob.Location = job.Location;
            existingJob.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Policy = "RecruiterOnly")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            if (_currentUser.UserId is null)
            {
                return Unauthorized();
            }

            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id);
            if (job == null)
            {
                return NotFound();
            }

            if (!await _authz.RecruiterOwnsJobAsync(id))
            {
                return Forbid();
            }

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
