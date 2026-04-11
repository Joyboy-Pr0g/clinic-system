using HomeNursingSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HomeNursingSystem.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<NurseProfile> NurseProfiles => Set<NurseProfile>();
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<MedicalService> Services => Set<MedicalService>();
    public DbSet<NurseServiceLink> NurseServices => Set<NurseServiceLink>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AppointmentMessage> AppointmentMessages => Set<AppointmentMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<MedicalService>(e =>
        {
            e.ToTable("Services");
            e.HasKey(x => x.ServiceId);
            e.Property(x => x.BasePrice).HasPrecision(10, 2);
        });

        builder.Entity<NurseProfile>(e =>
        {
            e.HasKey(x => x.NurseProfileId);
            e.Property(x => x.AverageRating).HasPrecision(3, 2);
            e.HasOne(x => x.User)
                .WithOne(x => x.NurseProfile)
                .HasForeignKey<NurseProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Clinic>(e =>
        {
            e.HasKey(x => x.ClinicId);
            e.Property(x => x.AverageRating).HasPrecision(3, 2);
            e.HasOne(x => x.Owner)
                .WithMany(x => x.OwnedClinics)
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<NurseServiceLink>(e =>
        {
            e.HasKey(x => x.NurseServiceId);
            e.Property(x => x.CustomPrice).HasPrecision(10, 2);
            e.HasIndex(x => new { x.NurseProfileId, x.ServiceId }).IsUnique();
        });

        builder.Entity<Appointment>(e =>
        {
            e.HasKey(x => x.AppointmentId);
            e.Property(x => x.TotalPrice).HasPrecision(10, 2);
            e.HasOne(x => x.Patient)
                .WithMany(x => x.PatientAppointments)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.NurseProfile)
                .WithMany(x => x.Appointments)
                .HasForeignKey(x => x.NurseProfileId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Clinic)
                .WithMany(x => x.Appointments)
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Rating>(e =>
        {
            e.HasKey(x => x.RatingId);
            e.HasIndex(x => x.AppointmentId).IsUnique();
            e.HasOne(x => x.Appointment)
                .WithOne(x => x.Rating)
                .HasForeignKey<Rating>(x => x.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Patient)
                .WithMany(x => x.RatingsGiven)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Article>(e =>
        {
            e.HasKey(x => x.ArticleId);
            e.HasIndex(x => x.Slug).IsUnique();
            e.HasOne(x => x.Author)
                .WithMany(x => x.Articles)
                .HasForeignKey(x => x.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ContactMessage>(e =>
        {
            e.HasKey(x => x.ContactMessageId);
        });

        builder.Entity<Notification>(e =>
        {
            e.HasKey(x => x.NotificationId);
            e.HasOne(x => x.User)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AppointmentMessage>(e =>
        {
            e.HasKey(x => x.AppointmentMessageId);
            e.HasIndex(x => new { x.AppointmentId, x.CreatedAt });
            e.HasOne(x => x.Appointment)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Sender)
                .WithMany()
                .HasForeignKey(x => x.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
