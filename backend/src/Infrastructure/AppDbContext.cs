using EnviroWatch.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnviroWatch.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<DataSnapshot> DataSnapshots => Set<DataSnapshot>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Alert> Alerts => Set<Alert>();

    public DbSet<AlertLog> AlertLogs => Set<AlertLog>();

    public DbSet<ManualObservation> ManualObservations => Set<ManualObservation>();

    public DbSet<Dashboard> Dashboards => Set<Dashboard>();

    public DbSet<Widget> Widgets => Set<Widget>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Dashboard>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasMaxLength(512);

            entity.HasIndex(e => new { e.UserId, e.UpdatedAt });

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Widget>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(e => e.Metric)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(e => e.Region)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(e => e.Source)
                .HasMaxLength(64);

            entity.Property(e => e.Type)
                .HasConversion<string>()
                .HasMaxLength(32);

            entity.HasIndex(e => new { e.DashboardId, e.SortOrder });

            entity.HasOne(e => e.Dashboard)
                .WithMany(d => d.Widgets)
                .HasForeignKey(e => e.DashboardId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ManualObservation>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Region)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(e => e.Metric)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(e => e.Unit)
                .HasMaxLength(16);

            entity.Property(e => e.Notes)
                .HasMaxLength(512);

            entity.HasIndex(e => new { e.UserId, e.ObservedAt });
            entity.HasIndex(e => new { e.Region, e.Metric, e.ObservedAt });

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Metric)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(e => e.Region)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(e => e.Condition)
                .HasConversion<string>()
                .HasMaxLength(16);

            entity.HasIndex(e => new { e.UserId, e.IsActive });
            entity.HasIndex(e => new { e.Region, e.Metric, e.IsActive });

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AlertLog>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => new { e.AlertId, e.TriggeredAt });

            entity.HasOne(e => e.Alert)
                .WithMany(a => a.Logs)
                .HasForeignKey(e => e.AlertId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(e => e.PasswordHash)
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(e => e.Role)
                .HasConversion<string>()
                .HasMaxLength(32);

            entity.HasIndex(e => e.Email)
                .IsUnique();
        });

        modelBuilder.Entity<DataSnapshot>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Source)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(e => e.Region)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(e => e.Metric)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(e => e.Unit)
                .HasMaxLength(16);

            entity.HasIndex(e => new { e.Region, e.Metric, e.Timestamp });
            entity.HasIndex(e => e.Timestamp);
        });
    }
}
