namespace HireSphere.API.DTOs.Candidate;

public class WorkExperienceDto
{
    public int Id { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Description { get; set; }

    public string? Location { get; set; }

    public bool IsCurrentRole { get; set; }
}

public class CreateWorkExperienceDto
{
    public string CompanyName { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Description { get; set; }

    public string? Location { get; set; }

    public bool IsCurrentRole { get; set; }
}

public class UpdateWorkExperienceDto
{
    public string CompanyName { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Description { get; set; }

    public string? Location { get; set; }

    public bool IsCurrentRole { get; set; }
}
