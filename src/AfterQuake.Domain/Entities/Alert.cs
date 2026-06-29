using AfterQuake.Domain.Common;
using AfterQuake.Domain.Enumerations;

namespace AfterQuake.Domain.Entities;

public class Alert : BaseAuditableEntity
{
    public AlertType AlertType { get; set; }
    public AlertLevel Severity { get; set; } = AlertLevel.Yellow;
    public string? Title { get; set; }
    public string? Message { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? RadiusKm { get; set; }
    public string? ZoneCode { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public bool RequiresAcknowledgement { get; set; }

    public string? PublishedById { get; set; }
    public ApplicationUser? PublishedBy { get; set; }
}
