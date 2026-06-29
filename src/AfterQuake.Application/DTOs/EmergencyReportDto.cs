using AfterQuake.Domain.Enumerations;

namespace AfterQuake.Application.DTOs;

public class EmergencyReportDto
{
    public Guid Id { get; set; }
    public string? Description { get; set; }
    public string? EmergencyTypeName { get; set; }
    public EmergencyType EmergencyType { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Address { get; set; }
    public string? ZoneCode { get; set; }
    public int AffectedPeople { get; set; }
    public bool RequiresImmediateRescue { get; set; }
    public string? ReporterPhone { get; set; }
    public DateTime ReportedAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class CreateEmergencyReportDto
{
    public string? Description { get; set; }
    public EmergencyType EmergencyType { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Address { get; set; }
    public string? ZoneCode { get; set; }
    public int AffectedPeople { get; set; }
    public bool RequiresImmediateRescue { get; set; }
    public string? ReporterPhone { get; set; }
}
