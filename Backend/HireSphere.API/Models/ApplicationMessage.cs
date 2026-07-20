namespace HireSphere.API.Models;

public class ApplicationMessage
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }

    public int SenderUserId { get; set; }

    public string SenderRole { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public bool IsReadByRecipient { get; set; }

    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;

    public Application Application { get; set; } = null!;

    public User Sender { get; set; } = null!;
}
