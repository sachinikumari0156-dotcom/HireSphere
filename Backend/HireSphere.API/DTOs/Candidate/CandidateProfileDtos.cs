using HireSphere.API.Models.Enums;

namespace HireSphere.API.DTOs.Candidate;

public class CandidateProfileDetailDto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string? Summary { get; set; }

    public string? Location { get; set; }

    public int? YearsOfExperience { get; set; }

    public string? DesiredJobTitle { get; set; }

    public WorkArrangement? PreferredWorkArrangement { get; set; }

    public int? SalaryExpectation { get; set; }

    public string? Availability { get; set; }

    public string? PortfolioUrl { get; set; }

    public string? LinkedInUrl { get; set; }

    public string? GitHubUrl { get; set; }

    public int ProfileCompletionPercent { get; set; }

    public IReadOnlyList<WorkExperienceDto> WorkExperiences { get; set; } = Array.Empty<WorkExperienceDto>();

    public IReadOnlyList<EducationDto> Educations { get; set; } = Array.Empty<EducationDto>();

    public IReadOnlyList<CandidateSkillDto> Skills { get; set; } = Array.Empty<CandidateSkillDto>();

    public IReadOnlyList<CertificationDto> Certifications { get; set; } = Array.Empty<CertificationDto>();
}

public class UpdateCandidateProfileDto
{
    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string? Summary { get; set; }

    public string? Location { get; set; }

    public int? YearsOfExperience { get; set; }

    public string? DesiredJobTitle { get; set; }

    public WorkArrangement? PreferredWorkArrangement { get; set; }

    public int? SalaryExpectation { get; set; }

    public string? Availability { get; set; }

    public string? PortfolioUrl { get; set; }

    public string? LinkedInUrl { get; set; }

    public string? GitHubUrl { get; set; }
}
