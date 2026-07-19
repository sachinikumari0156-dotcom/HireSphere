namespace HireSphere.API.DTOs
{
    public class JobDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string RequiredSkills { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public DateTime PostedDate { get; set; }

        public int RecruiterId { get; set; }
    }
}