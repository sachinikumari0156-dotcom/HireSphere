using System;

namespace HireSphere.API.Models
{
    public class Application
    {
        public int Id { get; set; }


        // Candidate FK
        public int CandidateId { get; set; }


        // Job FK
        public int JobId { get; set; }


        public DateTime AppliedDate { get; set; } = DateTime.UtcNow;


        public string Status { get; set; } = string.Empty;


        public string CoverLetter { get; set; } = string.Empty;



        // Navigation Properties

        public User Candidate { get; set; } = null!;


        public Job Job { get; set; } = null!;
    }
}