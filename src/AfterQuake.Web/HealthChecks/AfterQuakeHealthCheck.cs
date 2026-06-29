using Microsoft.Extensions.Diagnostics.HealthChecks;
using AfterQuake.Domain.Interfaces;
using AfterQuake.Domain.Entities;

namespace AfterQuake.Web.HealthChecks;

public class AfterQuakeHealthCheck : IHealthCheck
{
    private readonly IUnitOfWork _uow;

    public AfterQuakeHealthCheck(IUnitOfWork uow) => _uow = uow;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>();

        try
        {
            var canConnect = await _uow.Repository<Shelter>().AnyAsync(_ => true, cancellationToken);
            data["Database:Connectivity"] = canConnect ? "Healthy" : "Unhealthy";

            var shelterCount = await _uow.Repository<Shelter>().CountAsync(_ => true, cancellationToken);
            var emergencyCount = await _uow.Repository<EmergencyReport>().CountAsync(_ => true, cancellationToken);

            data["SeedData:Shelters"] = shelterCount;
            data["SeedData:Emergencies"] = emergencyCount;

            var hasEssentialData = shelterCount > 0;
            data["SeedData:HasEssentialData"] = hasEssentialData;

            if (canConnect && hasEssentialData)
                return HealthCheckResult.Healthy("La aplicación funciona correctamente. Base de datos conectada y datos esenciales presentes.", data);

            if (!hasEssentialData)
                return HealthCheckResult.Degraded("La base de datos está conectada pero faltan datos semilla esenciales.", data: data);

            return HealthCheckResult.Unhealthy(description: "No se pudo conectar a la base de datos.", data: data);
        }
        catch (Exception ex)
        {
            data["Database:Error"] = ex.Message;
            return HealthCheckResult.Unhealthy("Error al verificar el estado de la aplicación.", ex, data);
        }
    }
}
