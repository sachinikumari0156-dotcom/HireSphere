namespace HireSphere.API.Models;

public class Resume
{
    public int Id { get; set; }

    public int CandidateProfileId { get; set; }

    public string FilePath { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public CandidateProfile CandidateProfile { get; set; } = null!;
}
