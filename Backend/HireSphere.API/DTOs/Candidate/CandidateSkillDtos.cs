namespace HireSphere.API.DTOs.Candidate;

public class CandidateSkillDto
{
    public int Id { get; set; }

    public int SkillId { get; set; }

    public string SkillName { get; set; } = string.Empty;

    public string? ProficiencyLevel { get; set; }

    public int? YearsOfExperience { get; set; }
}

public class CreateCandidateSkillDto
{
    public int SkillId { get; set; }

    public string? ProficiencyLevel { get; set; }

    public int? YearsOfExperience { get; set; }
}

public class UpdateCandidateSkillDto
{
    public string? ProficiencyLevel { get; set; }

    public int? YearsOfExperience { get; set; }
}

public class SkillCatalogItemDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
