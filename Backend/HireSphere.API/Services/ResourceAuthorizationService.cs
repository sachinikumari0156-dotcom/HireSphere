using HireSphere.API.Data;
using Microsoft.EntityFrameworkCore;

namespace HireSphere.API.Services;

public sealed class ResourceAuthorizationService : IResourceAuthorizationService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ResourceAuthorizationService(
        ApplicationDbContext db,
        ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public bool RequireSelf(int resourceOwnerUserId) =>
        _currentUser.UserId == resourceOwnerUserId
        || _currentUser.IsInRole("Admin");

    public async Task<bool> CandidateOwnsProfileAsync(int profileId)
    {
        if (_currentUser.IsInRole("Admin"))
        {
            return true;
        }

        if (_currentUser.UserId is not int userId)
        {
            return false;
        }

        return await _db.CandidateProfiles.AnyAsync(p => p.Id == profileId && p.UserId == userId);
    }

    public async Task<bool> RecruiterOwnsJobAsync(int jobId)
    {
        if (_currentUser.IsInRole("Admin"))
        {
            return true;
        }

        if (_currentUser.UserId is not int userId)
        {
            return false;
        }

        var job = await _db.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.Id == jobId);
        if (job == null)
        {
            return false;
        }

        if (job.RecruiterId == userId)
        {
            return true;
        }

        if (_currentUser.OrganizationId is int orgId && job.OrganizationId == orgId)
        {
            return true;
        }

        return false;
    }

    public async Task EnsureCandidateOwnsApplicationAsync(int applicationId)
    {
        // Helper used by controllers; throws InvalidOperationException when denied.
        if (_currentUser.IsInRole("Admin") || _currentUser.IsInRole("Recruiter") || _currentUser.IsInRole("HiringManager"))
        {
            return;
        }

        if (_currentUser.UserId is not int userId)
        {
            throw new UnauthorizedAccessException();
        }

        var owns = await _db.Applications.AnyAsync(a => a.Id == applicationId && a.CandidateId == userId);
        if (!owns)
        {
            throw new UnauthorizedAccessException();
        }
    }
}
