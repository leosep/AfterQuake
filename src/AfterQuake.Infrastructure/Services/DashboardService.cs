using AfterQuake.Application.DTOs;
using AfterQuake.Application.Interfaces;
using AfterQuake.Domain.Entities;
using AfterQuake.Domain.Enumerations;
using AfterQuake.Domain.Interfaces;

namespace AfterQuake.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly IEmergencyService _emergencyService;
    private readonly IPersonReportService _personReportService;
    private readonly IHelpRequestService _helpRequestService;
    private readonly IShelterService _shelterService;
    private readonly IAlertService _alertService;
    private readonly IUnitOfWork _uow;

    public DashboardService(
        IEmergencyService emergencyService,
        IPersonReportService personReportService,
        IHelpRequestService helpRequestService,
        IShelterService shelterService,
        IAlertService alertService,
        IUnitOfWork uow)
    {
        _emergencyService = emergencyService;
        _personReportService = personReportService;
        _helpRequestService = helpRequestService;
        _shelterService = shelterService;
        _alertService = alertService;
        _uow = uow;
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        var activeEmergencies = await _emergencyService.GetActiveCountAsync();
        var activeMissing = await _personReportService.GetActiveMissingCountAsync();
        var foundPersons = await _personReportService.GetFoundCountAsync();
        var pendingRequests = await _helpRequestService.GetPendingCountAsync();
        var shelters = await _shelterService.GetActiveAsync();
        var latestAlert = await _alertService.GetLatestAsync();
        var urgentRequests = await _helpRequestService.GetUrgentAsync();

        var volunteerRepo = _uow.Repository<Volunteer>();
        var activeVolunteers = await volunteerRepo.CountAsync(v => v.IsAvailable);

        var donationRepo = _uow.Repository<Donation>();
        var donations = await donationRepo.FindAsync(d => d.Status == DonationStatus.Received);
        var totalDonationsAmount = donations.Where(d => d.MonetaryAmount.HasValue).Sum(d => d.MonetaryAmount ?? 0);

        return new DashboardDto
        {
            ActiveEmergencies = activeEmergencies,
            ActiveMissingPersons = activeMissing,
            FoundPersons = foundPersons,
            PendingHelpRequests = pendingRequests,
            ActiveShelters = shelters.Count,
            AvailableShelterCapacity = shelters.Sum(s => s.AvailableCapacity),
            ActiveVolunteers = activeVolunteers,
            DonationsReceived = donations.Count,
            TotalDonationsAmount = totalDonationsAmount,
            LatestAlert = latestAlert,
            UrgentRequests = urgentRequests.ToList()
        };
    }
}
