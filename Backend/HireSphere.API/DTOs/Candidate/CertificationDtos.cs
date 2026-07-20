namespace HireSphere.API.DTOs.Candidate;

public class CertificationDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string IssuingOrganization { get; set; } = string.Empty;

    public DateTime IssueDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public string? CredentialId { get; set; }

    public string? CredentialUrl { get; set; }
}

public class CreateCertificationDto
{
    public string Name { get; set; } = string.Empty;

    public string IssuingOrganization { get; set; } = string.Empty;

    public DateTime IssueDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public string? CredentialId { get; set; }

    public string? CredentialUrl { get; set; }
}

public class UpdateCertificationDto
{
    public string Name { get; set; } = string.Empty;

    public string IssuingOrganization { get; set; } = string.Empty;

    public DateTime IssueDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public string? CredentialId { get; set; }

    public string? CredentialUrl { get; set; }
}
