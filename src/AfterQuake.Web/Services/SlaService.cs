using AfterQuake.Domain.Entities;
using AfterQuake.Domain.Enumerations;
using AfterQuake.Domain.Interfaces;

namespace AfterQuake.Web.Services;

public class SlaService
{
    private readonly IUnitOfWork _uow;

    public SlaService(IUnitOfWork uow) => _uow = uow;

    public async Task<SlaReport> GetReportAsync()
    {
        var emergencyRepo = _uow.Repository<EmergencyReport>();
        var helpRepo = _uow.Repository<HelpRequest>();

        var emergencies = await emergencyRepo.FindAsync(e => e.Status == EmergencyStatus.Resolved);
        var helpRequests = await helpRepo.FindAsync(h => h.Status == HelpRequestStatus.Resolved);

        var emergencyTimes = emergencies
            .Where(e => e.ResolvedAt.HasValue)
            .Select(e => (e.ResolvedAt!.Value - e.ReportedAt).TotalMinutes);

        var helpTimes = helpRequests
            .Where(h => h.ResolvedAt.HasValue)
            .Select(h => (h.ResolvedAt!.Value - h.RequestedAt).TotalMinutes);

        return new SlaReport
        {
            AvgEmergencyResolutionMin = emergencyTimes.Any() ? emergencyTimes.Average() : 0,
            AvgHelpRequestResolutionMin = helpTimes.Any() ? helpTimes.Average() : 0,
            TotalEmergenciesResolved = emergencies.Count,
            TotalHelpRequestsResolved = helpRequests.Count,
            EmergenciesUnder1Hour = emergencies.Count(e => e.ResolvedAt.HasValue && (e.ResolvedAt.Value - e.ReportedAt).TotalHours <= 1),
            HelpRequestsUnder4Hours = helpRequests.Count(h => h.ResolvedAt.HasValue && (h.ResolvedAt.Value - h.RequestedAt).TotalHours <= 4),
        };
    }
}

public class SlaReport
{
    public double AvgEmergencyResolutionMin { get; set; }
    public double AvgHelpRequestResolutionMin { get; set; }
    public int TotalEmergenciesResolved { get; set; }
    public int TotalHelpRequestsResolved { get; set; }
    public int EmergenciesUnder1Hour { get; set; }
    public int HelpRequestsUnder4Hours { get; set; }
    public double EmergencySlaPercent => TotalEmergenciesResolved > 0
        ? (double)EmergenciesUnder1Hour / TotalEmergenciesResolved * 100 : 0;
    public double HelpSlaPercent => TotalHelpRequestsResolved > 0
        ? (double)HelpRequestsUnder4Hours / TotalHelpRequestsResolved * 100 : 0;
}
