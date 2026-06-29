namespace AfterQuake.Application.DTOs;

public class DashboardDto
{
    public int ActiveEmergencies { get; set; }
    public int ActiveMissingPersons { get; set; }
    public int PendingHelpRequests { get; set; }
    public int ActiveShelters { get; set; }
    public int AvailableShelterCapacity { get; set; }
    public int ActiveVolunteers { get; set; }
    public int DonationsReceived { get; set; }
    public decimal TotalDonationsAmount { get; set; }
    public AlertDto? LatestAlert { get; set; }
    public List<HelpRequestDto> UrgentRequests { get; set; } = new();

    public int ActiveAlerts => LatestAlert is not null ? 1 : 0;
    public int MissingPersons => ActiveMissingPersons;
    public int FoundPersons { get; set; }
    public int AvailableVolunteers => ActiveVolunteers;
    public int TotalDonations => DonationsReceived;
    public decimal TotalDonationAmount => TotalDonationsAmount;
}
