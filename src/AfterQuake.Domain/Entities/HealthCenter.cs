using AfterQuake.Domain.Common;

namespace AfterQuake.Domain.Entities;

public class HealthCenter : BaseAuditableEntity
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Address { get; set; }
    public string? ZoneCode { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public bool IsOperational { get; set; } = true;
    public bool HasEmergencyRoom { get; set; }
    public bool HasSurgeryCapacity { get; set; }
    public int AvailableBeds { get; set; }
    public int TotalBeds { get; set; }
    public string? Services { get; set; }
    public string? Specializations { get; set; }
    

    public string? ManagedById { get; set; }
    public ApplicationUser? ManagedBy { get; set; }
}
