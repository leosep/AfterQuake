using AfterQuake.Application.Interfaces;
using AfterQuake.Domain.Entities;
using AfterQuake.Domain.Enumerations;
using AfterQuake.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AfterQuake.Infrastructure.Services;

public class BackgroundJobsService : IHostedService, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BackgroundJobsService> _logger;
    private Timer? _crossReferenceTimer;
    private Timer? _alertTimer;
    private Timer? _matchTimer;

    public BackgroundJobsService(IServiceScopeFactory scopeFactory, ILogger<BackgroundJobsService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _crossReferenceTimer = new Timer(
            async _ => await ExecuteWithLoggingAsync(CrossReferenceMissingAsync, "Cross-reference missing persons with hospital patients"),
            null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));

        _alertTimer = new Timer(
            async _ => await ExecuteWithLoggingAsync(DeactivateExpiredAlertsAsync, "Deactivate expired alerts"),
            null, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(15));

        _matchTimer = new Timer(
            async _ => await ExecuteWithLoggingAsync(MatchHelpOffersAsync, "Match help offers to pending requests"),
            null, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _crossReferenceTimer?.Change(Timeout.Infinite, 0);
        _alertTimer?.Change(Timeout.Infinite, 0);
        _matchTimer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _crossReferenceTimer?.Dispose();
        _alertTimer?.Dispose();
        _matchTimer?.Dispose();
    }

    private async Task ExecuteWithLoggingAsync(Func<Task> task, string jobName)
    {
        try
        {
            _logger.LogInformation("Starting background job: {JobName}", jobName);
            await task();
            _logger.LogInformation("Completed background job: {JobName}", jobName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Background job failed: {JobName}", jobName);
        }
    }

    private async Task CrossReferenceMissingAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var personRepo = uow.Repository<PersonReport>();
        var patientRepo = uow.Repository<UnidentifiedPatient>();

        var missingPersons = await personRepo.FindAsync(p =>
            p.ReportType == PersonReportType.Missing && p.Status == PersonReportStatus.Active);

        var patients = await patientRepo.FindAsync(p => !p.IsIdentified);

        foreach (var patient in patients)
        {
            var matches = missingPersons.Where(m =>
                (string.IsNullOrEmpty(patient.ZoneCode) || string.IsNullOrEmpty(m.ZoneCode) || patient.ZoneCode == m.ZoneCode) &&
                PatientMatchesPerson(m, patient))
            .ToList();

            foreach (var match in matches)
            {
                _logger.LogInformation(
                    "Potential match: Patient {PatientId} (Hospital: {Hospital}) matches Missing Person {PersonId} ({Name}) in zone {Zone}",
                    patient.Id, patient.HospitalName, match.Id, match.MissingPersonName, match.ZoneCode);
            }
        }

        if (patients.Count > 0 || missingPersons.Count > 0)
        {
            _logger.LogInformation("Cross-reference check: {PatientCount} patients, {MissingCount} missing persons, {MatchCount} potential matches found",
                patients.Count, missingPersons.Count, patients.Sum(p => missingPersons.Count(m => PatientMatchesPerson(m, p))));
        }
    }

    private static bool PatientMatchesPerson(PersonReport person, UnidentifiedPatient patient)
    {
        if (!string.IsNullOrEmpty(patient.ZoneCode) && !string.IsNullOrEmpty(person.ZoneCode))
        {
            if (patient.ZoneCode != person.ZoneCode)
                return false;
        }

        if (!string.IsNullOrWhiteSpace(patient.PhysicalDescription) &&
            !string.IsNullOrWhiteSpace(person.Description) &&
            person.Description.Contains(patient.PhysicalDescription, StringComparison.OrdinalIgnoreCase))
            return true;

        if (!string.IsNullOrWhiteSpace(patient.Clothing) &&
            !string.IsNullOrWhiteSpace(person.PhysicalCharacteristics) &&
            person.PhysicalCharacteristics.Contains(patient.Clothing, StringComparison.OrdinalIgnoreCase))
            return true;

        if (!string.IsNullOrWhiteSpace(patient.EstimatedAge) &&
            person.Age.HasValue &&
            int.TryParse(patient.EstimatedAge, out var estAge) &&
            Math.Abs(person.Age.Value - estAge) <= 5)
            return true;

        return false;
    }

    private async Task DeactivateExpiredAlertsAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var repo = uow.Repository<Alert>();
        var expired = await repo.FindAsync(a =>
            a.IsActive && a.ExpiresAt != null && a.ExpiresAt <= DateTime.UtcNow);

        if (expired.Count == 0)
        {
            _logger.LogInformation("No expired alerts found");
            return;
        }

        foreach (var alert in expired)
        {
            alert.IsActive = false;
            await repo.UpdateAsync(alert);
        }

        await uow.SaveChangesAsync();
        _logger.LogInformation("Deactivated {Count} expired alerts", expired.Count);
    }

    private async Task MatchHelpOffersAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var offerRepo = uow.Repository<HelpOffer>();
        var requestRepo = uow.Repository<HelpRequest>();

        var offers = await offerRepo.FindAsync(o => o.IsAvailable);
        var pendingRequests = await requestRepo.FindAsync(r => r.Status == HelpRequestStatus.Pending);

        var matchCount = 0;

        foreach (var offer in offers)
        {
            var matchingRequests = pendingRequests
                .Where(r => !string.IsNullOrEmpty(r.ZoneCode) && r.ZoneCode == offer.ZoneCode)
                .ToList();

            if (matchingRequests.Count == 0)
                continue;

            matchCount += matchingRequests.Count;
            foreach (var request in matchingRequests)
            {
                _logger.LogInformation(
                    "Potential match: Offer {OfferId} ({OfferType}, Zone: {Zone}) -> Request {RequestId} ({RequestType}, Zone: {Zone})",
                    offer.Id, offer.OfferType, offer.ZoneCode, request.Id, request.RequestType, request.ZoneCode);
            }
        }

        _logger.LogInformation("Help offer matching: {OfferCount} offers checked, {RequestCount} pending requests, {MatchCount} potential matches",
            offers.Count, pendingRequests.Count, matchCount);
    }
}
