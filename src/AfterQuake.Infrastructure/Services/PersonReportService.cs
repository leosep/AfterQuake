using Microsoft.EntityFrameworkCore;
using AfterQuake.Application.DTOs;
using AfterQuake.Application.Interfaces;
using AfterQuake.Domain.Entities;
using AfterQuake.Domain.Enumerations;
using AfterQuake.Domain.Interfaces;

namespace AfterQuake.Infrastructure.Services;

public class PersonReportService : IPersonReportService
{
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notificationService;

    public PersonReportService(IUnitOfWork uow, INotificationService notificationService)
    {
        _uow = uow;
        _notificationService = notificationService;
    }

    public async Task<PersonReportDto?> GetByIdAsync(Guid id)
    {
        var repo = _uow.Repository<PersonReport>();
        var entity = await repo.GetByIdAsync(id);
        return entity is null ? null : MapToDto(entity);
    }

    public async Task<PagedResult<PersonReportDto>> SearchAsync(PersonSearchDto search, int page = 1, int pageSize = 20)
    {
        var repo = _uow.Repository<PersonReport>();
        var query = repo.Query();

        if (!string.IsNullOrWhiteSpace(search.Query))
            query = query.Where(p => p.MissingPersonName != null && p.MissingPersonName.Contains(search.Query));

        if (!string.IsNullOrWhiteSpace(search.ZoneCode))
            query = query.Where(p => p.ZoneCode == search.ZoneCode);

        if (search.ReportType.HasValue)
            query = query.Where(p => p.ReportType == search.ReportType.Value);

        if (search.Status.HasValue)
            query = query.Where(p => p.Status == search.Status.Value);

        if (search.MinAge.HasValue)
            query = query.Where(p => p.Age >= search.MinAge.Value);

        if (search.MaxAge.HasValue)
            query = query.Where(p => p.Age <= search.MaxAge.Value);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(p => p.ReportedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<PersonReportDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IReadOnlyList<PersonReportDto>> GetPotentialMatchesAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return new List<PersonReportDto>();

        var repo = _uow.Repository<PersonReport>();
        var matches = await repo.FindAsync(p =>
            p.ReportType == PersonReportType.Missing &&
            p.Status == PersonReportStatus.Active &&
            p.MissingPersonName != null &&
            p.MissingPersonName.Contains(name));

        return matches.Select(MapToDto).ToList();
    }

    public async Task<PersonReportDto> CreateAsync(CreatePersonReportDto dto, string? userId = null)
    {
        var entity = new PersonReport
        {
            ReportType = dto.ReportType,
            MissingPersonName = dto.MissingPersonName,
            Age = dto.Age,
            Gender = dto.Gender,
            Description = dto.Description,
            PhysicalCharacteristics = dto.PhysicalCharacteristics,
            LastKnownClothing = dto.LastKnownClothing,
            LastKnownLatitude = dto.LastKnownLatitude,
            LastKnownLongitude = dto.LastKnownLongitude,
            LastKnownAddress = dto.LastKnownAddress,
            ZoneCode = dto.ZoneCode,
            LastSeenAt = dto.LastSeenAt,
            ContactName = dto.ContactName,
            ContactPhone = dto.ContactPhone,
            ContactEmail = dto.ContactEmail,
            UserId = userId
        };

        if (!string.IsNullOrWhiteSpace(dto.MissingPersonName))
        {
            var potentialMatches = await GetPotentialMatchesAsync(dto.MissingPersonName);
            if (potentialMatches.Count > 0)
            {
                if (potentialMatches.Count == 1 && string.Equals(potentialMatches[0].MissingPersonName, dto.MissingPersonName, StringComparison.OrdinalIgnoreCase))
                {
                    entity.MatchedToReportId = potentialMatches[0].Id;
                }

                var matchLinks = string.Join(", ", potentialMatches.Select(m => m.Id.ToString()));
                if (!string.IsNullOrEmpty(userId))
                {
                    await _notificationService.SendToUserAsync(userId, "Posibles coincidencias encontradas",
                        $"Se encontraron {potentialMatches.Count} reporte(s) con nombre similar a '{dto.MissingPersonName}'. IDs: {matchLinks}",
                        $"/Person/Details/{entity.Id}");
                }
                await _notificationService.SendToRoleAsync("Administrator", "Revisión de coincidencias requerida",
                    $"Nuevo reporte de '{dto.MissingPersonName}' puede coincidir con {potentialMatches.Count} reporte(s) existente(s). IDs: {matchLinks}",
                    $"/Person/Details/{entity.Id}");
                await _notificationService.SendToRoleAsync("Volunteer", "Revisión de coincidencias requerida",
                    $"Nuevo reporte de '{dto.MissingPersonName}' puede coincidir con {potentialMatches.Count} reporte(s) existente(s). IDs: {matchLinks}",
                    $"/Person/Details/{entity.Id}");
            }
        }

        var repo = _uow.Repository<PersonReport>();
        await repo.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task ReportFoundAsync(ReportFoundDto dto, string? userId = null)
    {
        var repo = _uow.Repository<PersonReport>();
        var entity = await repo.GetByIdAsync(dto.ReportId);
        if (entity is null) return;

        entity.Status = PersonReportStatus.Resolved;
        entity.FoundByName = dto.FoundByName;
        entity.FoundByPhone = dto.FoundByPhone;
        entity.FoundLatitude = dto.FoundLatitude;
        entity.FoundLongitude = dto.FoundLongitude;
        entity.FoundAt = DateTime.UtcNow;
        entity.FoundNotes = dto.FoundNotes;
        entity.UpdatedById = userId;

        await repo.UpdateAsync(entity);
        await _uow.SaveChangesAsync();
    }

    public async Task<int> GetActiveMissingCountAsync()
    {
        var repo = _uow.Repository<PersonReport>();
        return await repo.CountAsync(p => p.ReportType == PersonReportType.Missing && p.Status == PersonReportStatus.Active);
    }

    public async Task<int> GetFoundCountAsync()
    {
        var repo = _uow.Repository<PersonReport>();
        return await repo.CountAsync(p => p.Status == PersonReportStatus.Resolved &&
            (p.ReportType == PersonReportType.Found || p.ReportType == PersonReportType.Missing));
    }

    private static PersonReportDto MapToDto(PersonReport e) => new()
    {
        Id = e.Id,
        ReportType = e.ReportType,
        ReportTypeName = e.ReportType.ToString(),
        Status = e.Status,
        StatusName = e.Status.ToString(),
        MissingPersonName = e.MissingPersonName,
        Age = e.Age,
        Gender = e.Gender,
        Description = e.Description,
        PhotoUrl = e.PhotoUrl,
        LastKnownLatitude = e.LastKnownLatitude,
        LastKnownLongitude = e.LastKnownLongitude,
        LastKnownAddress = e.LastKnownAddress,
        ZoneCode = e.ZoneCode,
        LastSeenAt = e.LastSeenAt,
        ContactName = e.ContactName,
        ContactPhone = e.ContactPhone,
        ReportedAt = e.ReportedAt,
        MatchedToReportId = e.MatchedToReportId
    };
}
