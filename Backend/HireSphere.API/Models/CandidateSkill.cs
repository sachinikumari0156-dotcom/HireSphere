namespace HireSphere.API.Models;

public class CandidateSkill
{
    public int Id { get; set; }

    public int CandidateProfileId { get; set; }

    public int SkillId { get; set; }

    public string? ProficiencyLevel { get; set; }

    public int? YearsOfExperience { get; set; }

    public CandidateProfile CandidateProfile { get; set; } = null!;

    public Skill Skill { get; set; } = null!;
}
