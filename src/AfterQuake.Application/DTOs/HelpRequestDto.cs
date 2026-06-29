using AfterQuake.Domain.Enumerations;

namespace AfterQuake.Application.DTOs;

public class HelpRequestDto
{
    public Guid Id { get; set; }
    public string? RequestTypeName { get; set; }
    public HelpRequestType RequestType { get; set; }
    public string? PriorityName { get; set; }
    public HelpRequestPriority Priority { get; set; }
    public string? StatusName { get; set; }
    public HelpRequestStatus Status { get; set; }
    public string? Description { get; set; }
    public int PeopleCount { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Address { get; set; }
    public string? ZoneCode { get; set; }
    public string? RequesterName { get; set; }
    public string? RequesterPhone { get; set; }
    public string? RequesterEmail { get; set; }
    public DateTime RequestedAt { get; set; }
    public bool IsUrgent { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class CreateHelpRequestDto
{
    public HelpRequestType RequestType { get; set; }
    public string? Description { get; set; }
    public int PeopleCount { get; set; } = 1;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Address { get; set; }
    public string? ZoneCode { get; set; }
    public string? RequesterName { get; set; }
    public string? RequesterPhone { get; set; }
    public string? RequesterEmail { get; set; }
    public bool IsUrgent { get; set; }
}
