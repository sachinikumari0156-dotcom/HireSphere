using HireSphere.API.DTOs.Auth;
using HireSphere.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireSphere.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUser;

    public AuthController(IAuthService authService, ICurrentUserService currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    [HttpPost("register/candidate")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterCandidate([FromBody] CandidateRegisterDto dto)
    {
        var (ok, error, result) = await _authService.RegisterCandidateAsync(dto);
        if (!ok)
        {
            return BadRequest(new { message = error });
        }

        return Ok(result);
    }

    [HttpPost("recruiter-requests")]
    [AllowAnonymous]
    public async Task<IActionResult> SubmitRecruiterRequest([FromBody] CreateRecruiterAccessRequestDto dto)
    {
        var (ok, error, result) = await _authService.SubmitRecruiterRequestAsync(dto);
        if (!ok)
        {
            return BadRequest(new { message = error });
        }

        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var (ok, error, result) = await _authService.LoginAsync(dto);
        if (!ok)
        {
            return Unauthorized(new { message = error });
        }

        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        if (_currentUser.UserId is not int userId)
        {
            return Unauthorized();
        }

        var (ok, error, result) = await _authService.GetCurrentUserAsync(userId);
        if (!ok)
        {
            return NotFound(new { message = error });
        }

        return Ok(result);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (_currentUser.UserId is not int userId)
        {
            return Unauthorized();
        }

        var (ok, error) = await _authService.ChangePasswordAsync(userId, dto);
        if (!ok)
        {
            return BadRequest(new { message = error });
        }

        return Ok(new { message = "Password changed successfully." });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        if (_currentUser.UserId is int userId)
        {
            await _authService.LogLogoutAsync(userId);
        }

        return Ok(new { message = "Logged out. Discard the client token." });
    }
}
