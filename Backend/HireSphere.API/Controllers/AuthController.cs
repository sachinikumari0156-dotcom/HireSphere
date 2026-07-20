using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HireSphere.API.Data;
using HireSphere.API.Models;
using HireSphere.API.Models.Enums;
using HireSphere.API.DTOs;
using HireSphere.API.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace HireSphere.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;


        public AuthController(
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }



        // POST: api/Auth/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            // Phase 1: only allow public candidate registration.
            // Privileged roles (Recruiter/HiringManager/Admin) must be assigned via admin-approved workflows later.
            if (!AuthRegistrationRules.IsPublicRegistrationAllowed(dto.Role))
            {
                return BadRequest("Only Candidate registration is allowed publicly.");
            }

            var normalizedEmail = dto.Email.Trim().ToUpperInvariant();

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);


            if (existingUser != null)
            {
                return BadRequest("Email already exists");
            }


            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email.Trim(),
                NormalizedEmail = normalizedEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "Candidate",
                Status = UserStatus.Active,
                CreatedAtUtc = DateTime.UtcNow
            };


            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            var candidateRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == "Candidate");
            if (candidateRole != null)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = candidateRole.Id
                });
                await _context.SaveChangesAsync();
            }


            return Ok(new
            {
                message = "Registration successful",
                userId = user.Id,
                role = user.Role
            });
        }




        // POST: api/Auth/Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var normalizedEmail = dto.Email.Trim().ToUpperInvariant();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid email or password");
            }



            // JWT Claims
            var claims = new[]
            {
                new Claim(
                    JwtRegisteredClaimNames.Sub,
                    user.Id.ToString()
                ),

                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()
                ),

                new Claim(
                    JwtRegisteredClaimNames.Email,
                    user.Email
                ),

                new Claim(
                    ClaimTypes.Role,
                    user.Role
                )
            };



            // JWT Key
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"]!
                )
            );



            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );



            // Create Token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],

                audience: _configuration["Jwt:Audience"],

                claims: claims,

                expires: DateTime.UtcNow.AddHours(2),

                signingCredentials: credentials
            );



            var jwtToken = new JwtSecurityTokenHandler()
                .WriteToken(token);



            return Ok(new
            {
                message = "Login successful",

                userId = user.Id,

                fullName = user.FullName,

                role = user.Role,

                token = jwtToken
            });
        }
    }
}