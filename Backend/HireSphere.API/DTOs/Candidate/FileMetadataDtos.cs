using HireSphere.API.Models.Enums;

namespace HireSphere.API.DTOs.Candidate;

public class ResumeMetadataDto
{
    public int Id { get; set; }

    public string StorageKey { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public DateTime UploadedAtUtc { get; set; }
}

public class DocumentMetadataDto
{
    public int Id { get; set; }

    public DocumentType DocumentType { get; set; }

    public string StorageKey { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public DateTime UploadedAtUtc { get; set; }
}

public class FileDownloadDto
{
    public Stream Content { get; set; } = Stream.Null;

    public string ContentType { get; set; } = "application/octet-stream";

    public string FileName { get; set; } = string.Empty;
}
