namespace HireSphere.API.Models
{
    public class Interview
    {
        public int Id { get; set; }

        public int ApplicationId { get; set; }

        public DateTime InterviewDate { get; set; }

        public string InterviewType { get; set; }

        public string MeetingLink { get; set; }

        public string Status { get; set; }

        public Application Application { get; set; }
    }
}