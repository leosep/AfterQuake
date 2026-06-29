using AfterQuake.Domain.Enumerations;

namespace AfterQuake.Application.DTOs;

public class ShelterDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? StatusName { get; set; }
    public ShelterStatus Status { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Address { get; set; }
    public string? ZoneCode { get; set; }
    public int TotalCapacity { get; set; }
    public int CurrentOccupancy { get; set; }
    public int AvailableCapacity { get; set; }
    public bool HasElectricity { get; set; }
    public bool HasWater { get; set; }
    public bool HasMedicalPost { get; set; }
    public bool HasFoodSupply { get; set; }
    public string? ContactPhone { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateShelterDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Address { get; set; }
    public string? ZoneCode { get; set; }
    public int TotalCapacity { get; set; }
    public bool HasElectricity { get; set; }
    public bool HasWater { get; set; }
    public bool HasMedicalPost { get; set; }
    public string? ContactPhone { get; set; }
}

public class UpdateShelterCapacityDto
{
    public int CurrentOccupancy { get; set; }
}
