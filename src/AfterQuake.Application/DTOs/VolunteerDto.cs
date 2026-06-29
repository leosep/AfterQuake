using AfterQuake.Domain.Enumerations;

namespace AfterQuake.Application.DTOs;

public class VolunteerDto
{
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? StatusName { get; set; }
    public VolunteerStatus Status { get; set; }
    public string? Skills { get; set; }
    public bool IsVerified { get; set; }
    public bool IsAvailable { get; set; }
    public string? ZoneCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public class RegisterVolunteerDto
{
    public string? Skills { get; set; }
    public string? Certifications { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ZoneCode { get; set; }
    public int MaxHoursPerDay { get; set; } = 12;
    public string? Notes { get; set; }
}
