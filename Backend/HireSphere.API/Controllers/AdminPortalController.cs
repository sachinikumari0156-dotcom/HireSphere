using HireSphere.API.DTOs.Admin;
using HireSphere.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireSphere.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdministratorOnly")]
public sealed class AdminPortalController : ControllerBase
{
    private readonly IAdminPortalService _service;

    public AdminPortalController(IAdminPortalService service)
    {
        _service = service;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var (ok, error, result) = await _service.GetDashboardAsync();
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpGet("users")]
    public async Task<IActionResult> ListUsers([FromQuery] AdminUserListQuery query)
    {
        var (ok, error, result) = await _service.ListUsersAsync(query);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpGet("users/{id:int}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var (ok, error, result) = await _service.GetUserAsync(id);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpPatch("users/{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateAdminUserStatusDto dto)
    {
        var (ok, error) = await _service.UpdateUserStatusAsync(id, dto.Status);
        return ok ? Ok(new { message = "User status updated." }) : MapFailure(error);
    }

    [HttpPut("users/{id:int}/organization")]
    public async Task<IActionResult> AssignOrganization(int id, [FromBody] AssignOrganizationDto dto)
    {
        var (ok, error) = await _service.AssignOrganizationAsync(id, dto.OrganizationId, dto.DepartmentId);
        return ok ? Ok(new { message = "Organization updated." }) : MapFailure(error);
    }

    [HttpPut("users/{id:int}/department")]
    public async Task<IActionResult> AssignDepartment(int id, [FromBody] AssignDepartmentDto dto)
    {
        var (ok, error) = await _service.AssignDepartmentAsync(id, dto.DepartmentId);
        return ok ? Ok(new { message = "Department updated." }) : MapFailure(error);
    }

    [HttpGet("users/{id:int}/roles")]
    public async Task<IActionResult> GetUserRoles(int id)
    {
        var (ok, error, result) = await _service.GetUserRolesAsync(id);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpPost("users/{id:int}/roles")]
    public async Task<IActionResult> AddUserRole(int id, [FromBody] AssignUserRoleDto dto)
    {
        var (ok, error) = await _service.AddUserRoleAsync(id, dto.RoleName);
        return ok ? Ok(new { message = "Role assigned." }) : MapFailure(error);
    }

    [HttpDelete("users/{id:int}/roles/{roleId:int}")]
    public async Task<IActionResult> RemoveUserRole(int id, int roleId)
    {
        var (ok, error) = await _service.RemoveUserRoleAsync(id, roleId);
        return ok ? Ok(new { message = "Role removed." }) : MapFailure(error);
    }

    [HttpGet("roles")]
    public async Task<IActionResult> ListRoles()
    {
        var (ok, error, result) = await _service.ListRolesAsync();
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpGet("permissions")]
    public async Task<IActionResult> ListPermissions()
    {
        var (ok, error, result) = await _service.ListPermissionsAsync();
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpPut("roles/{id:int}/permissions")]
    public async Task<IActionResult> UpdateRolePermissions(int id, [FromBody] UpdateRolePermissionsDto dto)
    {
        var (ok, error) = await _service.UpdateRolePermissionsAsync(id, dto.PermissionIds);
        return ok ? Ok(new { message = "Role permissions updated." }) : MapFailure(error);
    }

    [HttpGet("recruiter-requests/{id:int}")]
    public async Task<IActionResult> GetRecruiterRequest(int id)
    {
        var (ok, error, result) = await _service.GetRecruiterRequestAsync(id);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpPost("recruiter-requests/{id:int}/approve")]
    public async Task<IActionResult> ApproveRecruiterRequest(int id, [FromBody] ReviewRecruiterRequestAdminDto? dto)
    {
        var (ok, error) = await _service.ApproveRecruiterRequestAsync(id, dto ?? new ReviewRecruiterRequestAdminDto());
        return ok ? Ok(new { message = "Recruiter request approved." }) : MapFailure(error);
    }

    [HttpPost("recruiter-requests/{id:int}/reject")]
    public async Task<IActionResult> RejectRecruiterRequest(int id, [FromBody] ReviewRecruiterRequestAdminDto? dto)
    {
        var (ok, error) = await _service.RejectRecruiterRequestAsync(id, dto?.Notes);
        return ok ? Ok(new { message = "Recruiter request rejected." }) : MapFailure(error);
    }

    [HttpGet("organizations")]
    public async Task<IActionResult> ListOrganizations()
    {
        var (ok, error, result) = await _service.ListOrganizationsAsync();
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpGet("organizations/{id:int}")]
    public async Task<IActionResult> GetOrganization(int id)
    {
        var (ok, error, result) = await _service.GetOrganizationAsync(id);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpPost("organizations")]
    public async Task<IActionResult> CreateOrganization([FromBody] UpsertOrganizationDto dto)
    {
        var (ok, error, result) = await _service.CreateOrganizationAsync(dto);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpPut("organizations/{id:int}")]
    public async Task<IActionResult> UpdateOrganization(int id, [FromBody] UpsertOrganizationDto dto)
    {
        var (ok, error, result) = await _service.UpdateOrganizationAsync(id, dto);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpPatch("organizations/{id:int}/status")]
    public async Task<IActionResult> UpdateOrganizationStatus(int id, [FromBody] UpdateOrganizationStatusDto dto)
    {
        var (ok, error) = await _service.UpdateOrganizationStatusAsync(id, dto.Status);
        return ok ? Ok(new { message = "Organization status updated." }) : MapFailure(error);
    }

    [HttpGet("departments")]
    public async Task<IActionResult> ListDepartments([FromQuery] int? organizationId)
    {
        var (ok, error, result) = await _service.ListDepartmentsAsync(organizationId);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpPost("departments")]
    public async Task<IActionResult> CreateDepartment([FromBody] UpsertDepartmentDto dto)
    {
        var (ok, error, result) = await _service.CreateDepartmentAsync(dto);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpPut("departments/{id:int}")]
    public async Task<IActionResult> UpdateDepartment(int id, [FromBody] UpsertDepartmentDto dto)
    {
        var (ok, error, result) = await _service.UpdateDepartmentAsync(id, dto);
        return ok && result is not null ? Ok(result) : MapFailure(error);
    }

    [HttpPatch("departments/{id:int}/status")]
    public async Task<IActionResult> UpdateDepartmentStatus(int id, [FromBody] UpdateDepartmentStatusDto dto)
    {
        var (ok, error) = await _service.UpdateDepartmentStatusAsync(id, dto.Status);
        return ok ? Ok(new { message = "Department status updated." }) : MapFailure(error);
    }

    [HttpPost("hiring-managers/assign")]
    public async Task<IActionResult> AssignHiringManager([FromBody] AssignHiringManagerDto dto)
    {
        var (ok, error) = await _service.AssignHiringManagerAsync(dto);
        return ok ? Ok(new { message = "Hiring Manager assignment updated." }) : MapFailure(error);
    }

    private ActionResult MapFailure(string? error)
    {
        if (string.Equals(error, "Unauthorized.", StringComparison.Ordinal))
            return Unauthorized(new { message = error });

        if (error is not null
            && (error.Contains("not found", StringComparison.OrdinalIgnoreCase)
                || error.Contains("access denied", StringComparison.OrdinalIgnoreCase)))
            return NotFound(new { message = error });

        return BadRequest(new { message = error ?? "Request failed." });
    }
}
