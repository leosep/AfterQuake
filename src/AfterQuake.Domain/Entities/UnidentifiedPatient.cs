using AfterQuake.Domain.Common;

namespace AfterQuake.Domain.Entities;

public class UnidentifiedPatient : BaseAuditableEntity
{
    public string? HospitalName { get; set; }
    public string? HospitalContact { get; set; }
    public string? PhotoUrl { get; set; }
    public string? EstimatedAge { get; set; }
    public string? Gender { get; set; }
    public string? PhysicalDescription { get; set; }
    public string? Clothing { get; set; }
    public string? DistinctiveMarks { get; set; }
    public string? MedicalCondition { get; set; }
    public string? ZoneCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime AdmittedAt { get; set; } = DateTime.UtcNow;
    public bool IsIdentified { get; set; }
    public DateTime? IdentifiedAt { get; set; }
    public Guid? IdentifiedAsReportId { get; set; }
    public PersonReport? IdentifiedAsReport { get; set; }
    public string? Notes { get; set; }
}
