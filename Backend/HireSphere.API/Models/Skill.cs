namespace HireSphere.API.Models;

public class Skill
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Category { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<CandidateSkill> CandidateSkills { get; set; } = new List<CandidateSkill>();

    public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
}
