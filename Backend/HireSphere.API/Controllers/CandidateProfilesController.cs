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
    public class CandidateProfilesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IResourceAuthorizationService _authz;

        public CandidateProfilesController(
            ApplicationDbContext context,
            ICurrentUserService currentUser,
            IResourceAuthorizationService authz)
        {
            _context = context;
            _currentUser = currentUser;
            _authz = authz;
        }

        [Authorize(Policy = "RecruitmentTeam")]
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

        [Authorize(Policy = "CandidateOnly")]
        [HttpGet("me")]
        public async Task<ActionResult<CandidateProfileDto>> GetMyProfile()
        {
            if (_currentUser.UserId is not int userId)
            {
                return Unauthorized();
            }

            var profile = await _context.CandidateProfiles
                .AsNoTracking()
                .Where(c => c.UserId == userId)
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

        [Authorize]
        [HttpGet("{id:int}")]
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

            if (_currentUser.IsInRole("Candidate")
                && !_authz.RequireSelf(profile.UserId))
            {
                return Forbid();
            }

            if (!_currentUser.IsInRole("Candidate")
                && !_currentUser.IsInRole("Recruiter")
                && !_currentUser.IsInRole("HiringManager")
                && !_currentUser.IsInRole("Admin"))
            {
                return Forbid();
            }

            return profile;
        }

        [Authorize(Policy = "CandidateOnly")]
        [HttpPost]
        public async Task<ActionResult<CandidateProfileDto>> CreateCandidateProfile(
            CandidateProfile profile)
        {
            if (_currentUser.UserId is not int id)
            {
                return Unauthorized();
            }

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
                dto);
        }

        [Authorize(Policy = "CandidateOnly")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCandidateProfile(
            int id,
            CandidateProfile profile)
        {
            if (_currentUser.UserId is not int userId)
            {
                return Unauthorized();
            }

            var existingProfile = await _context.CandidateProfiles
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

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

        [Authorize(Policy = "CandidateOnly")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCandidateProfile(int id)
        {
            if (_currentUser.UserId is not int userId)
            {
                return Unauthorized();
            }

            var profile = await _context.CandidateProfiles
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

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
