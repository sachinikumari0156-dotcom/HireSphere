namespace HireSphere.API.Models;

public class Resume
{
    public int Id { get; set; }

    public int CandidateProfileId { get; set; }

    public string FilePath { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public string? ContentType { get; set; }

    public long? SizeBytes { get; set; }

    public string? ChecksumSha256 { get; set; }

    public string ValidationStatus { get; set; } = "Clean";

    public string ScanStatus { get; set; } = "NotConfigured";

    public string StorageProvider { get; set; } = "LocalDevelopment";

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAtUtc { get; set; }

    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public CandidateProfile CandidateProfile { get; set; } = null!;
}
