using System.Text;
using AfterQuake.Domain.Entities;
using AfterQuake.Domain.Interfaces;
using AfterQuake.Application.DTOs;

namespace AfterQuake.Web.Services;

public class ExportService
{
    private readonly IUnitOfWork _uow;

    public ExportService(IUnitOfWork uow) => _uow = uow;

    public async Task<byte[]> ExportEmergenciesToCsvAsync()
    {
        var repo = _uow.Repository<EmergencyReport>();
        var items = await repo.FindAsync(e => !e.IsDeleted);
        var sb = new StringBuilder();
        sb.AppendLine("Id,Tipo,Severidad,Estado,Zona,Fecha,Descripción");
        foreach (var e in items.OrderByDescending(e => e.ReportedAt))
            sb.AppendLine($"{e.Id},{e.EmergencyType},{e.Severity},{e.Status},{e.ZoneCode},{e.ReportedAt:g},\"{e.Description}\"");
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    public async Task<byte[]> ExportPersonsToCsvAsync()
    {
        var repo = _uow.Repository<PersonReport>();
        var items = await repo.FindAsync(p => !p.IsDeleted);
        var sb = new StringBuilder();
        sb.AppendLine("Id,Tipo,Nombre,Edad,Género,Zona,Estado,Fecha");
        foreach (var p in items.OrderByDescending(p => p.ReportedAt))
            sb.AppendLine($"{p.Id},{p.ReportType},{p.MissingPersonName},{p.Age},{p.Gender},{p.ZoneCode},{p.Status},{p.ReportedAt:g}");
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    public async Task<byte[]> ExportDonationsToCsvAsync()
    {
        var repo = _uow.Repository<Donation>();
        var items = await repo.FindAsync(d => !d.IsDeleted);
        var sb = new StringBuilder();
        sb.AppendLine("Id,Tipo,Monto/Artículo,Cantidad,Donante,Estado,Fecha");
        foreach (var d in items.OrderByDescending(d => d.DonatedAt))
            sb.AppendLine($"{d.Id},{d.DonationType},{d.MonetaryAmount ?? 0},{d.ItemQuantity},{d.DonorName},{d.Status},{d.DonatedAt:g}");
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    public Task<byte[]> ExportDashboardToPdfAsync(DashboardDto dashboard)
    {
        var html = BuildDashboardHtml(dashboard);
        return Task.FromResult(Encoding.UTF8.GetBytes(html));
    }

    private static string BuildDashboardHtml(DashboardDto d)
    {
        return $@"<!DOCTYPE html><html><head><meta charset='utf-8'><style>
            body {{ font-family: Arial; padding: 20px; }}
            h1 {{ color: #dc2626; font-size: 24px; }}
            .card {{ border: 1px solid #ddd; padding: 15px; margin: 10px 0; border-radius: 8px; }}
            .stat {{ font-size: 18px; font-weight: bold; }}
        </style></head><body>
        <h1>AfterQuake - Reporte de Dashboard</h1>
        <p>Generado: {DateTime.Now:g}</p>
        <div class='card'>
            <p><span class='stat'>{d.ActiveEmergencies}</span> Emergencias activas</p>
            <p><span class='stat'>{d.ActiveAlerts}</span> Alertas activas</p>
            <p><span class='stat'>{d.MissingPersons}</span> Personas desaparecidas</p>
            <p><span class='stat'>{d.FoundPersons}</span> Personas encontradas</p>
            <p><span class='stat'>{d.PendingHelpRequests}</span> Solicitudes de ayuda pendientes</p>
            <p><span class='stat'>{d.ActiveShelters}</span> Albergues activos</p>
            <p><span class='stat'>{d.AvailableVolunteers}</span> Voluntarios disponibles</p>
            <p><span class='stat'>{d.TotalDonations}</span> Donaciones totales</p>
            <p><span class='stat'>{d.TotalDonationAmount:C}</span> Monto total donado</p>
        </div></body></html>";
    }
}
