using AfterQuake.Domain.Common;
using AfterQuake.Domain.Enumerations;

namespace AfterQuake.Domain.Entities;

public class HelpRequest : BaseAuditableEntity
{
    public HelpRequestType RequestType { get; set; }
    public HelpRequestPriority Priority { get; set; } = HelpRequestPriority.Medium;
    public HelpRequestStatus Status { get; set; } = HelpRequestStatus.Pending;
    public string? Description { get; set; }
    public int PeopleCount { get; set; } = 1;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Address { get; set; }
    public string? ZoneCode { get; set; }
    public string? RequesterName { get; set; }
    public string? RequesterPhone { get; set; }
    public string? RequesterEmail { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
    public bool IsUrgent { get; set; }

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public Guid? AssignedToVolunteerId { get; set; }
    public Volunteer? AssignedToVolunteer { get; set; }
    public Guid? AssignedToShelterId { get; set; }
    public Shelter? AssignedToShelter { get; set; }
}
