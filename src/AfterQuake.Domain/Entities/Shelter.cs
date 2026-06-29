using AfterQuake.Domain.Common;
using AfterQuake.Domain.Enumerations;

namespace AfterQuake.Domain.Entities;

public class Shelter : BaseAuditableEntity
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public ShelterStatus Status { get; set; } = ShelterStatus.Active;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Address { get; set; }
    public string? ZoneCode { get; set; }
    public int TotalCapacity { get; set; }
    public int CurrentOccupancy { get; set; }
    public int AvailableCapacity => TotalCapacity - CurrentOccupancy;
    public bool HasElectricity { get; set; }
    public bool HasWater { get; set; }
    public bool HasMedicalPost { get; set; }
    public bool HasFoodSupply { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    

    public string? ManagedById { get; set; }
    public ApplicationUser? ManagedBy { get; set; }

    public ICollection<HelpRequest> AssignedRequests { get; set; } = new List<HelpRequest>();
}
