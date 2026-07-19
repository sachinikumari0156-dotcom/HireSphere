namespace HireSphere.API.DTOs
{
    public class ApplicationDto
    {
        public int Id { get; set; }

        public int CandidateId { get; set; }

        public int JobId { get; set; }

        public DateTime AppliedDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public string CoverLetter { get; set; } = string.Empty;
    }
}