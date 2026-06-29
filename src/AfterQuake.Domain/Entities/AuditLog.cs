using AfterQuake.Domain.Common;

namespace AfterQuake.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string? Action { get; set; }
    public string? EntityName { get; set; }
    public string? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
}
