using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ganss.Xss;
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

    private static async Task EnsurePasswordExpirationTokenAsync(UserManager<ApplicationUser> userManager, ApplicationUser user)
    {
        var token = await userManager.GetAuthenticationTokenAsync(user, "PasswordExpiration", "LastChanged");
        if (string.IsNullOrEmpty(token))
        {
            await userManager.SetAuthenticationTokenAsync(user, "PasswordExpiration", "LastChanged", DateTime.UtcNow.ToString("O"));
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        var adminEmail = "admin@afterquake.com";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
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
        await EnsurePasswordExpirationTokenAsync(userManager, admin);

        var orgEmail = "cruzroja@afterquake.com";
        var org = await userManager.FindByEmailAsync(orgEmail);
        if (org == null)
        {
            org = new ApplicationUser
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
        await EnsurePasswordExpirationTokenAsync(userManager, org);
    }

    private static async Task SeedEmergencyContactsAsync(ApplicationDbContext context)
    {
        if (!await context.ContactDirectories.AnyAsync())
        {
            context.ContactDirectories.AddRange(
                new ContactDirectory { OrganizationName = "Sistema Nacional de Atención a Emergencias", OrganizationType = "Emergencia", PhoneNumber = "911", IsEmergencyNumber = true, DisplayOrder = 1, IsAvailable24Hours = true, Services = "Policía, ambulancia, bomberos, defensa civil" },
                new ContactDirectory { OrganizationName = "Centro de Operaciones de Emergencias (COE)", OrganizationType = "Emergencia", PhoneNumber = "*462", IsEmergencyNumber = true, DisplayOrder = 2, IsAvailable24Hours = true, Services = "Coordinación nacional de emergencias" },
                new ContactDirectory { OrganizationName = "Defensa Civil", OrganizationType = "Emergencia", PhoneNumber = "809-200-3500", IsEmergencyNumber = true, DisplayOrder = 3, IsAvailable24Hours = true, Services = "Rescate, evacuación, gestión de emergencias" },
                new ContactDirectory { OrganizationName = "Cruz Roja Dominicana", OrganizationType = "Salud", PhoneNumber = "809-567-4357", DisplayOrder = 4, OperatingHours = "24/7", IsAvailable24Hours = true, Services = "Atención prehospitalaria, albergues, donaciones" },
                new ContactDirectory { OrganizationName = "Ministerio de Salud Pública", OrganizationType = "Gobierno", PhoneNumber = "809-541-3121", DisplayOrder = 5, Services = "Coordinación sanitaria nacional" },
                new ContactDirectory { OrganizationName = "Hospital General Plaza de la Salud", OrganizationType = "Salud", PhoneNumber = "809-532-6000", DisplayOrder = 6, OperatingHours = "24/7", IsAvailable24Hours = true, Services = "Urgencias, traumatología, cirugía" },
                new ContactDirectory { OrganizationName = "Policía Nacional", OrganizationType = "Seguridad", PhoneNumber = "809-682-2151", IsEmergencyNumber = true, DisplayOrder = 7, IsAvailable24Hours = true }
            );
        }
    }

    private static async Task SeedGuideContentAsync(ApplicationDbContext context)
    {
        if (!await context.GuideContents.AnyAsync())
        {
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedTags.Clear();
            foreach (var tag in new[] { "p", "br", "strong", "em", "ul", "ol", "li", "h2", "h3", "a" })
                sanitizer.AllowedTags.Add(tag);
            sanitizer.AllowedAttributes.Clear();
            sanitizer.AllowedAttributes.Add("href");

            var guide1Content = sanitizer.Sanitize(@"## Antes del sismo
1. **Mochila de emergencia**: Prepara una mochila con agua, alimentos no perecibles, linterna, radio, botiquín, documentos importantes.
2. **Plan familiar**: Define puntos de encuentro y roles para cada miembro.
3. **Estructura segura**: Identifica zonas seguras en tu hogar (estructura firme, bajo mesas robustas).
4. **Kit de herramientas**: Ten a mano llaves, cortacorrientes, extintor.
5. **Comunicación**: Guarda números de emergencia en tu teléfono.");
            var guide2Content = sanitizer.Sanitize(@"## Durante el sismo
1. **Mantén la calma** y actúa con rapidez.
2. **Agáchate, cúbrete, sujétate**: Colócate bajo una mesa firme o marco de puerta.
3. **Aléjate de ventanas**, espejos, objetos que puedan caer.
4. **No uses ascensores**.
5. **Si estás en la calle**, aléjate de edificios, postes y cables eléctricos.
6. **Si conduces**, detén el vehículo en un lugar seguro, fuera de puentes o túneles.");
            var guide3Content = sanitizer.Sanitize(@"## Después del sismo
1. **Verifica tu estado** y el de quienes te rodean. Reporta heridos.
2. **Corta el suministro** de gas, agua y electricidad si hay fugas o daños.
3. **No entres a edificios dañados**.
4. **Mantente informado** por radio o medios oficiales.
5. **Ayuda a vecinos** si es seguro hacerlo.
6. **Reporta tu estado** en AfterQuake para que tu familia sepa que estás bien.");

            context.GuideContents.AddRange(
                new GuideContent
                {
                    Title = "Antes del sismo: Prepárate",
                    Summary = "Recomendaciones para preparar tu hogar y familia antes de un terremoto",
                    Content = guide1Content,
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
                    Content = guide2Content,
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
                    Content = guide3Content,
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
                new DisasterZone { ZoneCode = "ZONA-DN", Name = "Distrito Nacional", Region = "Santo Domingo", IsActive = true, EstimatedPopulation = 1000000, CurrentAlertLevel = AlertLevel.Green },
                new DisasterZone { ZoneCode = "ZONA-SD", Name = "Santiago", Region = "Santiago de los Caballeros", IsActive = true, EstimatedPopulation = 600000, CurrentAlertLevel = AlertLevel.Green },
                new DisasterZone { ZoneCode = "ZONA-PP", Name = "Puerto Plata", Region = "Puerto Plata", IsActive = true, EstimatedPopulation = 150000, CurrentAlertLevel = AlertLevel.Green },
                new DisasterZone { ZoneCode = "ZONA-LR", Name = "La Romana", Region = "La Romana", IsActive = true, EstimatedPopulation = 200000, CurrentAlertLevel = AlertLevel.Green },
                new DisasterZone { ZoneCode = "ZONA-SPM", Name = "San Pedro de Macorís", Region = "San Pedro de Macorís", IsActive = true, EstimatedPopulation = 180000, CurrentAlertLevel = AlertLevel.Green }
            );
        }
    }
}
