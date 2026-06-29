using AfterQuake.Domain.Common;
using AfterQuake.Domain.Enumerations;

namespace AfterQuake.Domain.Entities;

public class EmergencyReport : BaseAuditableEntity
{
    public string? Description { get; set; }
    public EmergencyType EmergencyType { get; set; }
    public EmergencySeverity Severity { get; set; } = EmergencySeverity.Medium;
    public EmergencyStatus Status { get; set; } = EmergencyStatus.Pending;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Address { get; set; }
    public string? ZoneCode { get; set; }
    public int AffectedPeople { get; set; }
    public bool RequiresImmediateRescue { get; set; }
    public string? ReporterPhone { get; set; }
    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public Guid? AssignedToVolunteerId { get; set; }
    public Volunteer? AssignedToVolunteer { get; set; }
}
