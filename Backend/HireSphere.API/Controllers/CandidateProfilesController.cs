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
    public class CandidateProfilesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CandidateProfilesController(ApplicationDbContext context)
        {
            _context = context;
        }



        // GET: api/CandidateProfiles
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CandidateProfileDto>>> GetCandidateProfiles()
        {
            var profiles = await _context.CandidateProfiles
                .AsNoTracking()
                .Select(c => new CandidateProfileDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    FullName = c.FullName,
                    PhoneNumber = c.PhoneNumber,
                    Address = c.Address,
                    Skills = c.Skills,
                    ResumePath = c.ResumePath
                })
                .ToListAsync();


            return profiles;
        }





        // GET: api/CandidateProfiles/1
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<CandidateProfileDto>> GetCandidateProfile(int id)
        {
            var profile = await _context.CandidateProfiles
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new CandidateProfileDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    FullName = c.FullName,
                    PhoneNumber = c.PhoneNumber,
                    Address = c.Address,
                    Skills = c.Skills,
                    ResumePath = c.ResumePath
                })
                .FirstOrDefaultAsync();


            if (profile == null)
            {
                return NotFound();
            }


            return profile;
        }





        // POST: api/CandidateProfiles
        // Candidate create own profile
        [Authorize(Roles = "Candidate")]
        [HttpPost]
        public async Task<ActionResult<CandidateProfileDto>> CreateCandidateProfile(
            CandidateProfile profile)
        {

            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );


            if (userId == null)
            {
                return Unauthorized();
            }



            int id = int.Parse(userId);



            var existingProfile = await _context.CandidateProfiles
                .FirstOrDefaultAsync(c => c.UserId == id);



            if (existingProfile != null)
            {
                return BadRequest("Profile already exists");
            }



            profile.UserId = id;



            _context.CandidateProfiles.Add(profile);


            await _context.SaveChangesAsync();



            var dto = new CandidateProfileDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                FullName = profile.FullName,
                PhoneNumber = profile.PhoneNumber,
                Address = profile.Address,
                Skills = profile.Skills,
                ResumePath = profile.ResumePath
            };



            return CreatedAtAction(
                nameof(GetCandidateProfile),
                new { id = profile.Id },
                dto
            );
        }





        // PUT: api/CandidateProfiles/1
        // Candidate update own profile
        [Authorize(Roles = "Candidate")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCandidateProfile(
            int id,
            CandidateProfile profile)
        {

            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );


            if (userId == null)
            {
                return Unauthorized();
            }



            var existingProfile =
                await _context.CandidateProfiles
                .FirstOrDefaultAsync(
                    c => c.Id == id &&
                    c.UserId == int.Parse(userId)
                );



            if (existingProfile == null)
            {
                return NotFound();
            }



            existingProfile.FullName = profile.FullName;
            existingProfile.PhoneNumber = profile.PhoneNumber;
            existingProfile.Address = profile.Address;
            existingProfile.Skills = profile.Skills;
            existingProfile.ResumePath = profile.ResumePath;



            await _context.SaveChangesAsync();



            return NoContent();
        }





        // DELETE: api/CandidateProfiles/1
        [Authorize(Roles = "Candidate")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCandidateProfile(int id)
        {

            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );


            if (userId == null)
            {
                return Unauthorized();
            }



            var profile =
                await _context.CandidateProfiles
                .FirstOrDefaultAsync(
                    c => c.Id == id &&
                    c.UserId == int.Parse(userId)
                );



            if (profile == null)
            {
                return NotFound();
            }



            _context.CandidateProfiles.Remove(profile);


            await _context.SaveChangesAsync();



            return NoContent();
        }
    }
}