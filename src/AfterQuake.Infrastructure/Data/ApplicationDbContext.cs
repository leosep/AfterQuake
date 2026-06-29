using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AfterQuake.Domain.Entities;
using AfterQuake.Domain.Common;

namespace AfterQuake.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public void ApplyGlobalFilter<T>(ModelBuilder builder) where T : BaseEntity
    {
        builder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
    }

    public DbSet<EmergencyReport> EmergencyReports => Set<EmergencyReport>();
    public DbSet<PersonReport> PersonReports => Set<PersonReport>();
    public DbSet<HelpRequest> HelpRequests => Set<HelpRequest>();
    public DbSet<HelpOffer> HelpOffers => Set<HelpOffer>();
    public DbSet<Shelter> Shelters => Set<Shelter>();
    public DbSet<DonationPoint> DonationPoints => Set<DonationPoint>();
    public DbSet<Donation> Donations => Set<Donation>();
    public DbSet<HealthCenter> HealthCenters => Set<HealthCenter>();
    public DbSet<Volunteer> Volunteers => Set<Volunteer>();
    public DbSet<VolunteerTask> VolunteerTasks => Set<VolunteerTask>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<ServiceStatus> ServiceStatuses => Set<ServiceStatus>();
    public DbSet<ContactDirectory> ContactDirectories => Set<ContactDirectory>();
    public DbSet<GuideContent> GuideContents => Set<GuideContent>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<DisasterZone> DisasterZones => Set<DisasterZone>();
    public DbSet<UnidentifiedPatient> UnidentifiedPatients => Set<UnidentifiedPatient>();
    public DbSet<OfficialCommunication> OfficialCommunications => Set<OfficialCommunication>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ApplicationDbContext).GetMethods()
                    .FirstOrDefault(m => m.Name == "ApplyGlobalFilter" && m.IsGenericMethod);
                if (method != null)
                {
                    var generic = method.MakeGenericMethod(entityType.ClrType);
                    generic.Invoke(this, new object[] { builder });
                }
            }
        }

        builder.Entity<EmergencyReport>(entity =>
        {
            entity.ToTable("EmergencyReports");
            entity.HasIndex(e => e.ZoneCode);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.ReportedAt);
            entity.HasOne(e => e.User).WithMany(u => u.EmergencyReports).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.AssignedToVolunteer).WithMany(v => v.AssignedEmergencies).HasForeignKey(e => e.AssignedToVolunteerId).OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<PersonReport>(entity =>
        {
            entity.ToTable("PersonReports");
            entity.HasIndex(e => e.MissingPersonName);
            entity.HasIndex(e => e.ZoneCode);
            entity.HasIndex(e => e.Status);
            entity.HasOne(e => e.User).WithMany(u => u.PersonReports).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.MatchedToReport).WithMany(e => e.PotentialMatches).HasForeignKey(e => e.MatchedToReportId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<HelpRequest>(entity =>
        {
            entity.ToTable("HelpRequests");
            entity.HasIndex(e => e.ZoneCode);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Priority);
            entity.HasOne(e => e.User).WithMany(u => u.HelpRequests).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.AssignedToVolunteer).WithMany(v => v.AssignedHelpRequests).HasForeignKey(e => e.AssignedToVolunteerId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.AssignedToShelter).WithMany(s => s.AssignedRequests).HasForeignKey(e => e.AssignedToShelterId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<HelpOffer>(entity =>
        {
            entity.ToTable("HelpOffers");
            entity.HasIndex(e => e.ZoneCode);
            entity.HasIndex(e => e.IsAvailable);
            entity.HasOne(e => e.User).WithMany(u => u.HelpOffers).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Shelter>(entity =>
        {
            entity.ToTable("Shelters");
            entity.HasIndex(e => e.ZoneCode);
            entity.HasIndex(e => e.Status);
            entity.Ignore(e => e.AvailableCapacity);
        });

        builder.Entity<DonationPoint>(entity =>
        {
            entity.ToTable("DonationPoints");
            entity.HasIndex(e => e.ZoneCode);
        });

        builder.Entity<Donation>(entity =>
        {
            entity.ToTable("Donations");
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.MonetaryAmount).HasPrecision(18, 2);
            entity.HasOne(e => e.User).WithMany(u => u.Donations).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.DonationPoint).WithMany(d => d.Donations).HasForeignKey(e => e.DonationPointId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<HealthCenter>(entity =>
        {
            entity.ToTable("HealthCenters");
            entity.HasIndex(e => e.ZoneCode);
            entity.HasIndex(e => e.IsOperational);
        });

        builder.Entity<Volunteer>(entity =>
        {
            entity.ToTable("Volunteers");
            entity.HasIndex(e => e.ZoneCode);
            entity.HasIndex(e => e.IsAvailable);
            entity.HasOne(e => e.User).WithOne(u => u.Volunteer).HasForeignKey<Volunteer>(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<VolunteerTask>(entity =>
        {
            entity.ToTable("VolunteerTasks");
            entity.HasOne(e => e.Volunteer).WithMany(v => v.AssignedTasks).HasForeignKey(e => e.VolunteerId).OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<Alert>(entity =>
        {
            entity.ToTable("Alerts");
            entity.HasIndex(e => e.ZoneCode);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.PublishedAt);
        });

        builder.Entity<ServiceStatus>(entity =>
        {
            entity.ToTable("ServiceStatuses");
            entity.HasIndex(e => e.ZoneCode);
            entity.HasIndex(e => e.ServiceType);
            entity.HasOne(e => e.UpdatedBy).WithMany().HasForeignKey(e => e.UpdatedById).OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<ContactDirectory>(entity =>
        {
            entity.ToTable("ContactDirectories");
            entity.HasIndex(e => e.ZoneCode);
            entity.HasIndex(e => e.OrganizationType);
            entity.HasIndex(e => e.DisplayOrder);
            entity.HasOne(e => e.UpdatedBy).WithMany().HasForeignKey(e => e.UpdatedById).OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<GuideContent>(entity =>
        {
            entity.ToTable("GuideContents");
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Language);
            entity.HasIndex(e => e.DisplayOrder);
        });

        builder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notifications");
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.IsRead);
            entity.HasIndex(e => e.SentAt);
            entity.HasOne(e => e.User).WithMany(u => u.Notifications).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.Action);
            entity.HasIndex(e => e.EntityName);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<DisasterZone>(entity =>
        {
            entity.ToTable("DisasterZones");
            entity.HasIndex(e => e.ZoneCode).IsUnique();
            entity.HasIndex(e => e.IsActive);
        });

        builder.Entity<UnidentifiedPatient>(entity =>
        {
            entity.ToTable("UnidentifiedPatients");
            entity.HasIndex(e => e.HospitalName);
            entity.HasIndex(e => e.IsIdentified);
            entity.HasIndex(e => e.ZoneCode);
            entity.HasOne(e => e.IdentifiedAsReport).WithMany().HasForeignKey(e => e.IdentifiedAsReportId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<OfficialCommunication>(entity =>
        {
            entity.ToTable("OfficialCommunications");
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.PublishedAt);
            entity.HasOne(e => e.PublishedBy).WithMany().HasForeignKey(e => e.PublishedById).OnDelete(DeleteBehavior.SetNull);
        });
    }
}
