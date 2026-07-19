using HireSphere.API.Models;

namespace HireSphere.API.Models
{
    public class User
    {
        public int Id { get; set; }


        public string FullName { get; set; } = string.Empty;


        public string Email { get; set; } = string.Empty;


        public string PasswordHash { get; set; } = string.Empty;


        public string Role { get; set; } = string.Empty;



        // =========================
        // Candidate Profile
        // One User -> One CandidateProfile
        // =========================

        public CandidateProfile? CandidateProfile { get; set; }



        // =========================
        // Recruiter Jobs
        // One User -> Many Jobs
        // =========================

        public ICollection<Job> Jobs { get; set; }
            = new List<Job>();



        // =========================
        // Candidate Applications
        // One User -> Many Applications
        // =========================

        public ICollection<Application> Applications { get; set; }
            = new List<Application>();
    }
}