using HireSphere.API.DTOs.Auth;
using HireSphere.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireSphere.API.Controllers;

/// <summary>Legacy Phase 3 admin endpoints retained for recruiter-request listing compatibility.</summary>
[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdministratorOnly")]
public class AdminController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    [HttpGet("recruiter-requests")]
    public async Task<ActionResult<IEnumerable<RecruiterAccessRequestDto>>> ListRecruiterRequests()
    {
        return Ok(await _adminUserService.ListRecruiterRequestsAsync());
    }
}
