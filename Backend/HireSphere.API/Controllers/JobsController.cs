using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HireSphere.API.Data;
using HireSphere.API.Models;
using HireSphere.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace HireSphere.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {

        private readonly ApplicationDbContext _context;


        public JobsController(ApplicationDbContext context)
        {
            _context = context;
        }




        // GET: api/Jobs
        // Anyone can view jobs
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






        // GET: api/Jobs/search?keyword=React
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<JobDto>>> SearchJobs(string keyword)
        {

            var jobs = await _context.Jobs
                .Where(j =>
                    j.Title.Contains(keyword) ||
                    j.RequiredSkills.Contains(keyword) ||
                    j.Location.Contains(keyword)
                )
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







        // GET: api/Jobs/1
        [HttpGet("{id}")]
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








        // GET: api/Jobs/MyJobs
        // Recruiter own jobs
        [Authorize(Roles = "Recruiter")]
        [HttpGet("MyJobs")]
        public async Task<ActionResult<IEnumerable<JobDto>>> MyJobs()
        {

            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );


            if (userId == null)
            {
                return Unauthorized();
            }



            var jobs = await _context.Jobs
                .Where(j => j.RecruiterId == int.Parse(userId))
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









        // POST: api/Jobs
        [Authorize(Roles = "Recruiter")]
        [HttpPost]
        public async Task<ActionResult<JobDto>> CreateJob(Job job)
        {


            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );


            if (userId == null)
            {
                return Unauthorized();
            }



            job.RecruiterId = int.Parse(userId);

            job.PostedDate = DateTime.Now;



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



            return CreatedAtAction(
                nameof(GetJob),
                new { id = job.Id },
                dto
            );

        }









        // PUT: api/Jobs/1
        [Authorize(Roles = "Recruiter")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJob(
            int id,
            Job job
        )
        {

            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );


            if (userId == null)
            {
                return Unauthorized();
            }



            var existingJob = await _context.Jobs
                .FirstOrDefaultAsync(j => j.Id == id);



            if (existingJob == null)
            {
                return NotFound();
            }




            if (existingJob.RecruiterId != int.Parse(userId))
            {
                return Forbid();
            }




            existingJob.Title = job.Title;

            existingJob.Description = job.Description;

            existingJob.RequiredSkills = job.RequiredSkills;

            existingJob.Location = job.Location;



            await _context.SaveChangesAsync();



            return NoContent();

        }









        // DELETE: api/Jobs/1
        [Authorize(Roles = "Recruiter")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJob(int id)
        {


            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );


            if (userId == null)
            {
                return Unauthorized();
            }



            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.Id == id);



            if (job == null)
            {
                return NotFound();
            }




            if (job.RecruiterId != int.Parse(userId))
            {
                return Forbid();
            }




            _context.Jobs.Remove(job);


            await _context.SaveChangesAsync();



            return NoContent();

        }


    }
}