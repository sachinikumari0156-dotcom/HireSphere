namespace HireSphere.API.DTOs.Candidate;

public class EducationDto
{
    public int Id { get; set; }

    public string Institution { get; set; } = string.Empty;

    public string Degree { get; set; } = string.Empty;

    public string? FieldOfStudy { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Grade { get; set; }

    public bool IsCurrentStudy { get; set; }
}

public class CreateEducationDto
{
    public string Institution { get; set; } = string.Empty;

    public string Degree { get; set; } = string.Empty;

    public string? FieldOfStudy { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Grade { get; set; }

    public bool IsCurrentStudy { get; set; }
}

public class UpdateEducationDto
{
    public string Institution { get; set; } = string.Empty;

    public string Degree { get; set; } = string.Empty;

    public string? FieldOfStudy { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Grade { get; set; }

    public bool IsCurrentStudy { get; set; }
}
