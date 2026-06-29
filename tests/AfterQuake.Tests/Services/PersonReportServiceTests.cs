using Microsoft.EntityFrameworkCore;
using AfterQuake.Application.DTOs;
using AfterQuake.Domain.Entities;
using AfterQuake.Domain.Enumerations;
using AfterQuake.Domain.Interfaces;
using AfterQuake.Infrastructure.Data;
using AfterQuake.Infrastructure.Services;

namespace AfterQuake.Tests.Services;

public class PersonReportServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _uow;
    private readonly PersonReportService _service;

    public PersonReportServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _uow = new UnitOfWork(_context);
        _service = new PersonReportService(_uow);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task CreateAsync_ShouldCreateMissingPersonReport()
    {
        var dto = new CreatePersonReportDto
        {
            ReportType = PersonReportType.Missing,
            MissingPersonName = "Juan Pérez",
            Age = 35,
            Gender = "Masculino",
            Description = "Camisa roja, jeans azules",
            LastKnownLatitude = -33.4489,
            LastKnownLongitude = -70.6693,
            LastKnownAddress = "Plaza de Armas",
            ZoneCode = "RM",
            ContactName = "María Pérez",
            ContactPhone = "+56912345678"
        };

        var result = await _service.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("Juan Pérez", result.MissingPersonName);
        Assert.Equal(PersonReportType.Missing, result.ReportType);
        Assert.Equal(PersonReportStatus.Active, result.Status);
    }

    [Fact]
    public async Task ReportFoundAsync_ShouldUpdateStatusToResolved()
    {
        var repo = _uow.Repository<PersonReport>();
        var person = new PersonReport
        {
            ReportType = PersonReportType.Missing,
            MissingPersonName = "Ana López",
            Status = PersonReportStatus.Active,
            ReportedAt = DateTime.UtcNow
        };
        await repo.AddAsync(person);
        await _uow.SaveChangesAsync();

        var foundDto = new ReportFoundDto
        {
            ReportId = person.Id,
            FoundByName = "Carlos López",
            FoundByPhone = "+56987654321",
            FoundLatitude = -33.4500,
            FoundLongitude = -70.6700,
            FoundNotes = "Encontrado en albergue municipal"
        };

        await _service.ReportFoundAsync(foundDto);

        var updated = await repo.GetByIdAsync(person.Id);
        Assert.NotNull(updated);
        Assert.Equal(PersonReportStatus.Resolved, updated.Status);
        Assert.Equal("Carlos López", updated.FoundByName);
        Assert.Equal("Encontrado en albergue municipal", updated.FoundNotes);
    }

    [Fact]
    public async Task SearchAsync_ShouldFilterByName()
    {
        var repo = _uow.Repository<PersonReport>();
        await repo.AddAsync(new PersonReport { MissingPersonName = "Pedro García", ReportType = PersonReportType.Missing, Status = PersonReportStatus.Active, ReportedAt = DateTime.UtcNow });
        await repo.AddAsync(new PersonReport { MissingPersonName = "Pedro González", ReportType = PersonReportType.Missing, Status = PersonReportStatus.Active, ReportedAt = DateTime.UtcNow });
        await repo.AddAsync(new PersonReport { MissingPersonName = "María Soto", ReportType = PersonReportType.Missing, Status = PersonReportStatus.Active, ReportedAt = DateTime.UtcNow });
        await _uow.SaveChangesAsync();

        var search = new PersonSearchDto { Query = "Pedro" };
        var result = await _service.SearchAsync(search);

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, r => Assert.Contains("Pedro", r.MissingPersonName));
    }

    [Fact]
    public async Task SearchAsync_ShouldFilterByZone()
    {
        var repo = _uow.Repository<PersonReport>();
        await repo.AddAsync(new PersonReport { MissingPersonName = "A", ZoneCode = "RM", ReportType = PersonReportType.Missing, Status = PersonReportStatus.Active, ReportedAt = DateTime.UtcNow });
        await repo.AddAsync(new PersonReport { MissingPersonName = "B", ZoneCode = "V", ReportType = PersonReportType.Missing, Status = PersonReportStatus.Active, ReportedAt = DateTime.UtcNow });
        await _uow.SaveChangesAsync();

        var search = new PersonSearchDto { ZoneCode = "RM" };
        var result = await _service.SearchAsync(search);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal("A", result.Items[0].MissingPersonName);
    }

    [Fact]
    public async Task GetPotentialMatchesAsync_ShouldFindByName()
    {
        var repo = _uow.Repository<PersonReport>();
        await repo.AddAsync(new PersonReport { MissingPersonName = "Luis Martínez", ReportType = PersonReportType.Missing, Status = PersonReportStatus.Active, ReportedAt = DateTime.UtcNow });
        await repo.AddAsync(new PersonReport { MissingPersonName = "Luis Fernández", ReportType = PersonReportType.Missing, Status = PersonReportStatus.Active, ReportedAt = DateTime.UtcNow });
        await repo.AddAsync(new PersonReport { MissingPersonName = "Andrés Soto", ReportType = PersonReportType.Missing, Status = PersonReportStatus.Active, ReportedAt = DateTime.UtcNow });
        await _uow.SaveChangesAsync();

        var matches = await _service.GetPotentialMatchesAsync("Luis");

        Assert.Equal(2, matches.Count);
        Assert.All(matches, m => Assert.Contains("Luis", m.MissingPersonName));
    }
}
