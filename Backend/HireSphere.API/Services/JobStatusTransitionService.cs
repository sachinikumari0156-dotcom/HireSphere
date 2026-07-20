using HireSphere.API.Models.Enums;

namespace HireSphere.API.Services;

public interface IJobStatusTransitionService
{
    bool CanTransition(JobStatus from, JobStatus to);

    (bool Ok, string? Error) Apply(Models.Job job, JobStatus to);
}

public sealed class JobStatusTransitionService : IJobStatusTransitionService
{
    private static readonly Dictionary<JobStatus, HashSet<JobStatus>> Allowed = new()
    {
        [JobStatus.Draft] = new() { JobStatus.PendingApproval, JobStatus.Published, JobStatus.Open },
        [JobStatus.PendingApproval] = new() { JobStatus.Published, JobStatus.Open, JobStatus.Draft },
        [JobStatus.Open] = new() { JobStatus.Paused, JobStatus.Closed, JobStatus.Published },
        [JobStatus.Published] = new() { JobStatus.Paused, JobStatus.Closed, JobStatus.Open },
        [JobStatus.Paused] = new() { JobStatus.Published, JobStatus.Open, JobStatus.Closed },
        [JobStatus.Closed] = new() { JobStatus.Archived },
        [JobStatus.Archived] = new()
    };

    public bool CanTransition(JobStatus from, JobStatus to) =>
        from == to || (Allowed.TryGetValue(from, out var set) && set.Contains(to));

    public (bool Ok, string? Error) Apply(Models.Job job, JobStatus to)
    {
        if (job.Status == to)
        {
            return (true, null);
        }

        if (!CanTransition(job.Status, to))
        {
            return (false, $"Cannot transition job from {job.Status} to {to}.");
        }

        job.Status = to;
        job.UpdatedAtUtc = DateTime.UtcNow;

        if (to is JobStatus.Published or JobStatus.Open)
        {
            job.PublishedAtUtc ??= DateTime.UtcNow;
            if (job.PostedDate == default)
            {
                job.PostedDate = DateTime.UtcNow;
            }
        }

        if (to == JobStatus.Closed)
        {
            job.ClosedAtUtc = DateTime.UtcNow;
        }

        return (true, null);
    }
}
