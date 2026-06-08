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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
