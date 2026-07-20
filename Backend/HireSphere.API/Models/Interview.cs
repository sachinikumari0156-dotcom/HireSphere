namespace HireSphere.API.Models
{
    public class Interview
    {
        public int Id { get; set; }

        public int ApplicationId { get; set; }

        public DateTime InterviewDate { get; set; }

        public string InterviewType { get; set; } = string.Empty;

        public string MeetingLink { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public Application Application { get; set; } = null!;
    }
}