using AfterQuake.Domain.Common;
using AfterQuake.Domain.Enumerations;

namespace AfterQuake.Domain.Entities;

public class VolunteerTask : BaseAuditableEntity
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ZoneCode { get; set; }
    public string? Address { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletionNotes { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int EstimatedHours { get; set; } = 1;

    public Guid? VolunteerId { get; set; }
    public Volunteer? Volunteer { get; set; }

    public string? AssignedById { get; set; }
    public ApplicationUser? AssignedBy { get; set; }
}
