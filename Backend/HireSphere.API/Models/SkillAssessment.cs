namespace HireSphere.API.Models
{
    public class SkillAssessment
    {
        public int Id { get; set; }

        public int CandidateId { get; set; }

        public string SkillName { get; set; }

        public int Score { get; set; }

        public string AssessmentResult { get; set; }

        public DateTime AssessmentDate { get; set; }

        public User Candidate { get; set; }
    }
}