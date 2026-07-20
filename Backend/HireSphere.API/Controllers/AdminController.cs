using HireSphere.API.DTOs.Auth;
using HireSphere.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireSphere.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdministratorOnly")]
public class AdminController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;
    private readonly ICurrentUserService _currentUser;

    public AdminController(IAdminUserService adminUserService, ICurrentUserService currentUser)
    {
        _adminUserService = adminUserService;
        _currentUser = currentUser;
    }

    [HttpGet("recruiter-requests")]
    public async Task<ActionResult<IEnumerable<RecruiterAccessRequestDto>>> ListRecruiterRequests()
    {
        return Ok(await _adminUserService.ListRecruiterRequestsAsync());
    }

    [HttpPost("recruiter-requests/{id:int}/approve")]
    public async Task<IActionResult> ApproveRecruiterRequest(int id, [FromBody] ReviewRecruiterRequestDto? dto)
    {
        if (_currentUser.UserId is not int adminId)
        {
            return Unauthorized();
        }

        var (ok, error) = await _adminUserService.ApproveRecruiterRequestAsync(id, adminId, dto?.Notes);
        if (!ok)
        {
            return BadRequest(new { message = error });
        }

        return Ok(new { message = "Recruiter request approved." });
    }

    [HttpPost("recruiter-requests/{id:int}/reject")]
    public async Task<IActionResult> RejectRecruiterRequest(int id, [FromBody] ReviewRecruiterRequestDto? dto)
    {
        if (_currentUser.UserId is not int adminId)
        {
            return Unauthorized();
        }

        var (ok, error) = await _adminUserService.RejectRecruiterRequestAsync(id, adminId, dto?.Notes);
        if (!ok)
        {
            return BadRequest(new { message = error });
        }

        return Ok(new { message = "Recruiter request rejected." });
    }

    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<CurrentUserDto>>> ListUsers()
    {
        return Ok(await _adminUserService.ListUsersAsync());
    }

    [HttpPatch("users/{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateUserStatusDto dto)
    {
        if (_currentUser.UserId is not int adminId)
        {
            return Unauthorized();
        }

        var (ok, error) = await _adminUserService.UpdateUserStatusAsync(id, adminId, dto.Status);
        if (!ok)
        {
            return BadRequest(new { message = error });
        }

        return Ok(new { message = "User status updated." });
    }

    [HttpPatch("users/{id:int}/roles")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateUserRoleDto dto)
    {
        if (_currentUser.UserId is not int adminId)
        {
            return Unauthorized();
        }

        var (ok, error) = await _adminUserService.UpdateUserRoleAsync(id, adminId, dto.Role);
        if (!ok)
        {
            return BadRequest(new { message = error });
        }

        return Ok(new { message = "User role updated." });
    }

    [HttpPatch("users/{id:int}/organization")]
    public async Task<IActionResult> UpdateOrganization(int id, [FromBody] UpdateUserOrganizationDto dto)
    {
        if (_currentUser.UserId is not int adminId)
        {
            return Unauthorized();
        }

        var (ok, error) = await _adminUserService.UpdateUserOrganizationAsync(
            id,
            adminId,
            dto.OrganizationId,
            dto.DepartmentId);

        if (!ok)
        {
            return BadRequest(new { message = error });
        }

        return Ok(new { message = "User organization updated." });
    }
}
