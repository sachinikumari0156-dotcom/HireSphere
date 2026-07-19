namespace HireSphere.API.DTOs
{
    public class ApplicationDetailsDto
    {
        public int Id { get; set; }

        public int CandidateId { get; set; }

        public string? CandidateName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Skills { get; set; }

        public string? ResumePath { get; set; }

        public int JobId { get; set; }

        public string? JobTitle { get; set; }

        public string? JobDescription { get; set; }

        public DateTime AppliedDate { get; set; }

        public string? Status { get; set; }

        public string? CoverLetter { get; set; }
    }
}