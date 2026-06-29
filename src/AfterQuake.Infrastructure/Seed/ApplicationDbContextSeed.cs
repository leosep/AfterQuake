using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AfterQuake.Domain.Entities;
using AfterQuake.Domain.Enumerations;
using AfterQuake.Infrastructure.Data;

namespace AfterQuake.Infrastructure.Seed;

public static class ApplicationDbContextSeed
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        await SeedRolesAsync(roleManager);
        await SeedUsersAsync(userManager);
        await SeedEmergencyContactsAsync(context);
        await SeedGuideContentAsync(context);
        await SeedDisasterZonesAsync(context);
        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Citizen", "Volunteer", "ReliefOrganization", "Administrator", "SuperAdministrator" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        var adminEmail = "admin@afterquake.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Administrador AfterQuake",
                PreferredLanguage = "es",
                EmailConfirmed = true,
                IsActive = true
            };
            await userManager.CreateAsync(admin, "AfterQuake2024!");
            await userManager.AddToRoleAsync(admin, "SuperAdministrator");
        }

        var orgEmail = "cruzroja@afterquake.com";
        if (await userManager.FindByEmailAsync(orgEmail) == null)
        {
            var org = new ApplicationUser
            {
                UserName = orgEmail,
                Email = orgEmail,
                FullName = "Cruz Roja - Emergencias",
                PreferredLanguage = "es",
                EmailConfirmed = true,
                IsActive = true
            };
            await userManager.CreateAsync(org, "AfterQuake2024!");
            await userManager.AddToRoleAsync(org, "ReliefOrganization");
        }
    }

    private static async Task SeedEmergencyContactsAsync(ApplicationDbContext context)
    {
        if (!await context.ContactDirectories.AnyAsync())
        {
            context.ContactDirectories.AddRange(
                new ContactDirectory { OrganizationName = "Emergencias Nacional", OrganizationType = "Emergencia", PhoneNumber = "911", IsEmergencyNumber = true, DisplayOrder = 1, IsAvailable24Hours = true },
                new ContactDirectory { OrganizationName = "Bomberos", OrganizationType = "Emergencia", PhoneNumber = "132", IsEmergencyNumber = true, DisplayOrder = 2, IsAvailable24Hours = true },
                new ContactDirectory { OrganizationName = "Cruz Roja", OrganizationType = "Salud", PhoneNumber = "131", IsEmergencyNumber = true, DisplayOrder = 3, IsAvailable24Hours = true },
                new ContactDirectory { OrganizationName = "Defensa Civil", OrganizationType = "Emergencia", PhoneNumber = "133", IsEmergencyNumber = true, DisplayOrder = 4, IsAvailable24Hours = true, Services = "Rescate, evacuación, gestión de emergencias" },
                new ContactDirectory { OrganizationName = "Hospital Central", OrganizationType = "Salud", PhoneNumber = "+56 2 2123 4567", DisplayOrder = 5, OperatingHours = "24/7", IsAvailable24Hours = true, Services = "Urgencias, traumatología, cirugía" },
                new ContactDirectory { OrganizationName = "Carabineros de Chile", OrganizationType = "Seguridad", PhoneNumber = "133", IsEmergencyNumber = true, DisplayOrder = 6, IsAvailable24Hours = true },
                new ContactDirectory { OrganizationName = "Ministerio de Salud", OrganizationType = "Gobierno", PhoneNumber = "+56 2 2123 4500", DisplayOrder = 7, Services = "Coordinación sanitaria nacional" }
            );
        }
    }

    private static async Task SeedGuideContentAsync(ApplicationDbContext context)
    {
        if (!await context.GuideContents.AnyAsync())
        {
            context.GuideContents.AddRange(
                new GuideContent
                {
                    Title = "Antes del sismo: Prepárate",
                    Summary = "Recomendaciones para preparar tu hogar y familia antes de un terremoto",
                    Content = @"## Antes del sismo
1. **Mochila de emergencia**: Prepara una mochila con agua, alimentos no perecibles, linterna, radio, botiquín, documentos importantes.
2. **Plan familiar**: Define puntos de encuentro y roles para cada miembro.
3. **Estructura segura**: Identifica zonas seguras en tu hogar (estructura firme, bajo mesas robustas).
4. **Kit de herramientas**: Ten a mano llaves, cortacorrientes, extintor.
5. **Comunicación**: Guarda números de emergencia en tu teléfono.",
                    Category = "Prevencion",
                    Tags = "preparación, mochila, plan familiar, prevención",
                    IconClass = "bi-shield-check",
                    IsPdfAvailable = true,
                    DisplayOrder = 1
                },
                new GuideContent
                {
                    Title = "Durante el sismo: Protégete",
                    Summary = "Qué hacer durante un terremoto para mantenerte a salvo",
                    Content = @"## Durante el sismo
1. **Mantén la calma** y actúa con rapidez.
2. **Agáchate, cúbrete, sujétate**: Colócate bajo una mesa firme o marco de puerta.
3. **Aléjate de ventanas**, espejos, objetos que puedan caer.
4. **No uses ascensores**.
5. **Si estás en la calle**, aléjate de edificios, postes y cables eléctricos.
6. **Si conduces**, detén el vehículo en un lugar seguro, fuera de puentes o túneles.",
                    Category = "Durante",
                    Tags = "terremoto, protección, seguridad, sismo",
                    IconClass = "bi-exclamation-triangle",
                    IsPdfAvailable = true,
                    DisplayOrder = 2
                },
                new GuideContent
                {
                    Title = "Después del sismo: Actúa",
                    Summary = "Pasos a seguir después de un terremoto para mantenerte seguro",
                    Content = @"## Después del sismo
1. **Verifica tu estado** y el de quienes te rodean. Reporta heridos.
2. **Corta el suministro** de gas, agua y electricidad si hay fugas o daños.
3. **No entres a edificios dañados**.
4. **Mantente informado** por radio o medios oficiales.
5. **Ayuda a vecinos** si es seguro hacerlo.
6. **Reporta tu estado** en AfterQuake para que tu familia sepa que estás bien.",
                    Category = "Posteriores",
                    Tags = "después, réplicas, seguridad, evaluación",
                    IconClass = "bi-clipboard-check",
                    IsPdfAvailable = true,
                    DisplayOrder = 3
                }
            );
        }
    }

    private static async Task SeedDisasterZonesAsync(ApplicationDbContext context)
    {
        if (!await context.DisasterZones.AnyAsync())
        {
            context.DisasterZones.AddRange(
                new DisasterZone { ZoneCode = "ZONA-001", Name = "Zona Centro", Region = "Metropolitana", IsActive = true, EstimatedPopulation = 50000, CurrentAlertLevel = AlertLevel.Green },
                new DisasterZone { ZoneCode = "ZONA-002", Name = "Zona Norte", Region = "Valparaíso", IsActive = true, EstimatedPopulation = 30000, CurrentAlertLevel = AlertLevel.Green },
                new DisasterZone { ZoneCode = "ZONA-003", Name = "Zona Sur", Region = "Maule", IsActive = true, EstimatedPopulation = 25000, CurrentAlertLevel = AlertLevel.Green },
                new DisasterZone { ZoneCode = "ZONA-004", Name = "Zona Costera", Region = "Bio-Bío", IsActive = true, EstimatedPopulation = 40000, CurrentAlertLevel = AlertLevel.Green }
            );
        }
    }
}
