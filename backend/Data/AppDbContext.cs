using Backend.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectBoundary> ProjectBoundaries => Set<ProjectBoundary>();
    public DbSet<TestType> TestTypes => Set<TestType>();
    public DbSet<TestResult> TestResults => Set<TestResult>();
    public DbSet<Observation> Observations => Set<Observation>();
    public DbSet<Sensor> Sensors => Set<Sensor>();
    public DbSet<SensorReading> SensorReadings => Set<SensorReading>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ProjectMembership> ProjectMemberships => Set<ProjectMembership>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<IngestBatch> IngestBatches => Set<IngestBatch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Project>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(200);
            entity.Property(x => x.Client).HasMaxLength(200);
        });

        modelBuilder.Entity<ProjectBoundary>(entity =>
        {
            entity.Property(x => x.Polygon).HasColumnType("geography");
            entity.HasIndex(x => x.ProjectId);
        });

        modelBuilder.Entity<TestType>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(100);
            entity.Property(x => x.Unit).HasMaxLength(50);
        });

        modelBuilder.Entity<TestResult>(entity =>
        {
            entity.Property(x => x.Location).HasColumnType("geography");
            entity.Property(x => x.Source).HasMaxLength(100);
            entity.Property(x => x.Technician).HasMaxLength(200);
            entity.HasIndex(x => new { x.ProjectId, x.TestTypeId, x.Timestamp });
        });

        modelBuilder.Entity<Observation>(entity =>
        {
            entity.Property(x => x.Location).HasColumnType("geography");
            entity.Property(x => x.Note).HasMaxLength(2000);
            entity.HasIndex(x => new { x.ProjectId, x.Timestamp });
        });

        modelBuilder.Entity<Sensor>(entity =>
        {
            entity.Property(x => x.Type).HasMaxLength(100);
            entity.Property(x => x.Location).HasColumnType("geography");
            entity.HasIndex(x => x.ProjectId);
        });

        modelBuilder.Entity<SensorReading>(entity =>
        {
            entity.HasIndex(x => new { x.SensorId, x.Timestamp });
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.Property(x => x.BlobUri).HasMaxLength(2000);
            entity.Property(x => x.ContentType).HasMaxLength(200);
            entity.Property(x => x.UploadedBy).HasMaxLength(200);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(x => x.Email).HasMaxLength(320);
            entity.Property(x => x.DisplayName).HasMaxLength(200);
        });

        modelBuilder.Entity<ProjectMembership>(entity =>
        {
            entity.Property(x => x.Role).HasMaxLength(100);
            entity.HasIndex(x => new { x.ProjectId, x.UserId }).IsUnique();
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.Property(x => x.Actor).HasMaxLength(200);
            entity.Property(x => x.Action).HasMaxLength(200);
            entity.Property(x => x.EntityType).HasMaxLength(200);
            entity.HasIndex(x => x.Timestamp);
        });

        modelBuilder.Entity<IngestBatch>(entity =>
        {
            entity.Property(x => x.IdempotencyKey).HasMaxLength(100);
            entity.HasIndex(x => new { x.ProjectId, x.IdempotencyKey }).IsUnique();
        });
    }
}
