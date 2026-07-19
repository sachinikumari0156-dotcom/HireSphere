namespace HireSphere.API.Models
{
    public class CandidateProfile
    {
        public int Id { get; set; }


        // Foreign Key
        public int UserId { get; set; }


        public string FullName { get; set; } = string.Empty;


        public string PhoneNumber { get; set; } = string.Empty;


        public string Address { get; set; } = string.Empty;


        public string Skills { get; set; } = string.Empty;


        public string ResumePath { get; set; } = string.Empty;



        // Navigation
        public User User { get; set; } = null!;
    }
}