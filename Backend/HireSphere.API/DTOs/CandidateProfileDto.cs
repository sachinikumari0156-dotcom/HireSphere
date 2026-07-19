namespace HireSphere.API.DTOs
{
    public class CandidateProfileDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public string Skills { get; set; }

        public string ResumePath { get; set; }
    }
}