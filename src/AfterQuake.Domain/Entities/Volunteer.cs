using AfterQuake.Domain.Common;
using AfterQuake.Domain.Enumerations;

namespace AfterQuake.Domain.Entities;

public class Volunteer : BaseAuditableEntity
{
    public VolunteerStatus Status { get; set; } = VolunteerStatus.Available;
    public string? Skills { get; set; }
    public string? Certifications { get; set; }
    public bool IsVerified { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ZoneCode { get; set; }
    public bool IsAvailable { get; set; } = true;
    public int MaxHoursPerDay { get; set; } = 12;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastActiveAt { get; set; }
    public string? Notes { get; set; }

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public ICollection<VolunteerTask> AssignedTasks { get; set; } = new List<VolunteerTask>();
    public ICollection<EmergencyReport> AssignedEmergencies { get; set; } = new List<EmergencyReport>();
    public ICollection<HelpRequest> AssignedHelpRequests { get; set; } = new List<HelpRequest>();
}
