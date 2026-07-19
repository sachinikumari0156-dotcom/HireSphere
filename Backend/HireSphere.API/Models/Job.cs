namespace HireSphere.API.Models
{
    public class Job
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string RequiredSkills { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string JobType { get; set; } = string.Empty;

        public DateTime PostedDate { get; set; }


        // Foreign Key
        public int RecruiterId { get; set; }


        // Navigation
        public User? Recruiter { get; set; }


        // Applications
        public ICollection<Application> Applications { get; set; }
            = new List<Application>();
    }
}