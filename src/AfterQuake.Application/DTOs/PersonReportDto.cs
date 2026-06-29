using AfterQuake.Domain.Enumerations;

namespace AfterQuake.Application.DTOs;

public class PersonReportDto
{
    public Guid Id { get; set; }
    public string? ReportTypeName { get; set; }
    public PersonReportType ReportType { get; set; }
    public string? StatusName { get; set; }
    public PersonReportStatus Status { get; set; }
    public string? MissingPersonName { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? Description { get; set; }
    public string? PhotoUrl { get; set; }
    public double? LastKnownLatitude { get; set; }
    public double? LastKnownLongitude { get; set; }
    public string? LastKnownAddress { get; set; }
    public string? ZoneCode { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public DateTime ReportedAt { get; set; }
    public Guid? MatchedToReportId { get; set; }
}

public class CreatePersonReportDto
{
    public PersonReportType ReportType { get; set; }
    public string? MissingPersonName { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? Description { get; set; }
    public string? PhysicalCharacteristics { get; set; }
    public string? LastKnownClothing { get; set; }
    public string? PhotoBase64 { get; set; }
    public double? LastKnownLatitude { get; set; }
    public double? LastKnownLongitude { get; set; }
    public string? LastKnownAddress { get; set; }
    public string? ZoneCode { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
}

public class ReportFoundDto
{
    public Guid ReportId { get; set; }
    public string? FoundByName { get; set; }
    public string? FoundByPhone { get; set; }
    public double? FoundLatitude { get; set; }
    public double? FoundLongitude { get; set; }
    public string? FoundNotes { get; set; }
}

public class PersonSearchDto
{
    public string? Query { get; set; }
    public string? ZoneCode { get; set; }
    public PersonReportType? ReportType { get; set; }
    public PersonReportStatus? Status { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
}
