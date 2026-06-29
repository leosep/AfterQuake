using AfterQuake.Domain.Common;

namespace AfterQuake.Domain.Entities;

public class Notification : BaseEntity
{
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string? Link { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public string? Type { get; set; }

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
}
