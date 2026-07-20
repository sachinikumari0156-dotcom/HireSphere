using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using HireSphere.API.Data;
using HireSphere.API.Models;
using HireSphere.API.DTOs;
using HireSphere.API.Models.Enums;
using System.Security.Claims;

namespace HireSphere.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationsController : ControllerBase
    {

        private readonly ApplicationDbContext _context;


        public ApplicationsController(ApplicationDbContext context)
        {
            _context = context;
        }



        // =====================================
        // Recruiter View All Applications
        // GET api/Applications
        // =====================================
        [Authorize(Roles = "Recruiter")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationDto>>> GetApplications()
        {

            var applications = await _context.Applications
                .AsNoTracking()
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







        // =====================================
        // Recruiter Own Job Applications
        // GET api/Applications/RecruiterApplications
        // =====================================
        [Authorize(Roles = "Recruiter")]
        [HttpGet("RecruiterApplications")]
        public async Task<ActionResult<IEnumerable<ApplicationDto>>>
            RecruiterApplications()
        {

            var recruiterId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );


            if (recruiterId == null)
            {
                return Unauthorized();
            }



            int id = int.Parse(recruiterId);



            var applications = await _context.Applications
                .AsNoTracking()
                .Where(a => _context.Jobs
                    .Any(j => j.Id == a.JobId &&
                              j.RecruiterId == id))
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







        [Authorize(Roles = "Recruiter")]
        [HttpGet("RecruiterApplicationDetails")]
        public async Task<ActionResult<IEnumerable<ApplicationDetailsDto>>>
     GetRecruiterApplicationDetails()
        {
            var recruiterId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );


            if (recruiterId == null)
            {
                return Unauthorized();
            }


            int id = int.Parse(recruiterId);



            var applications = await _context.Applications
                .Include(a => a.Job)
                .AsNoTracking()
                .Where(a => a.Job != null &&
                            a.Job.RecruiterId == id)
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



        // =====================================
        // Candidate View Own Applications
        // GET api/Applications/MyApplications
        // =====================================
        [Authorize(Roles = "Candidate")]
        [HttpGet("MyApplications")]
        public async Task<ActionResult<IEnumerable<ApplicationDto>>>
            MyApplications()
        {

            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );


            if (userId == null)
            {
                return Unauthorized();
            }



            int id = int.Parse(userId);



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








        // =====================================
        // Get Single Application
        // GET api/Applications/{id}
        // =====================================
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicationDto>>
            GetApplication(int id)
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



            return Ok(application);

        }







        // =====================================
        // Candidate Apply Job
        // POST api/Applications
        // =====================================
        [Authorize(Roles = "Candidate")]
        [HttpPost]
        public async Task<ActionResult<ApplicationDto>>
            Apply(Application application)
        {

            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );


            if (userId == null)
            {
                return Unauthorized();
            }



            var job = await _context.Jobs
                .FirstOrDefaultAsync(j =>
                    j.Id == application.JobId);



            if (job == null)
            {
                return BadRequest("Job not found");
            }



            application.CandidateId = int.Parse(userId);

            application.AppliedDate = DateTime.Now;

            application.Status = ApplicationStatus.Pending;
            application.CreatedAtUtc = DateTime.UtcNow;



            _context.Applications.Add(application);


            await _context.SaveChangesAsync();



            return CreatedAtAction(
                nameof(GetApplication),
                new { id = application.Id },
                application
            );

        }








        // =====================================
        // Recruiter Accept / Reject
        // PUT api/Applications/{id}
        // =====================================
        [Authorize(Roles = "Recruiter")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateApplication(
            int id,
            Application application)
        {

            var existing = await _context.Applications
                .FindAsync(id);



            if (existing == null)
            {
                return NotFound();
            }



            existing.Status = application.Status;



            await _context.SaveChangesAsync();



            return NoContent();

        }








        // =====================================
        // Candidate Delete Application
        // DELETE api/Applications/{id}
        // =====================================
        [Authorize(Roles = "Candidate")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApplication(int id)
        {

            var application = await _context.Applications
                .FindAsync(id);



            if (application == null)
            {
                return NotFound();
            }



            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );



            if (userId == null)
            {
                return Unauthorized();
            }



            if (application.CandidateId != int.Parse(userId))
            {
                return Forbid();
            }



            _context.Applications.Remove(application);


            await _context.SaveChangesAsync();



            return NoContent();

        }

    }
}