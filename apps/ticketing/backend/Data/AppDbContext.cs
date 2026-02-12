using Ticketing.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ticketing.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<KnowledgeArticle> KnowledgeArticles => Set<KnowledgeArticle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("ticketing");

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(x => x.Email).HasMaxLength(320);
            entity.Property(x => x.DisplayName).HasMaxLength(200);
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(200);
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.ReportedBy).HasMaxLength(200);
            entity.Property(x => x.AssignedTo).HasMaxLength(200);
            entity.Property(x => x.Location).HasColumnType("geography");
            entity.Property(x => x.SourceApp).HasMaxLength(100);
            entity.Property(x => x.SourceEntityType).HasMaxLength(100);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => new { x.SourceApp, x.SourceEntityId });
        });

        modelBuilder.Entity<TicketComment>(entity =>
        {
            entity.Property(x => x.Author).HasMaxLength(200);
            entity.Property(x => x.Body).HasMaxLength(4000);
            entity.HasIndex(x => x.TicketId);
        });

        modelBuilder.Entity<TimeEntry>(entity =>
        {
            entity.Property(x => x.Technician).HasMaxLength(200);
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.Property(x => x.Hours).HasPrecision(6, 2);
            entity.Property(x => x.HourlyRate).HasPrecision(10, 2);
            entity.HasIndex(x => x.TicketId);
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(200);
            entity.Property(x => x.SerialNumber).HasMaxLength(100);
            entity.Property(x => x.Model).HasMaxLength(200);
            entity.HasIndex(x => x.SerialNumber).IsUnique();
        });

        modelBuilder.Entity<KnowledgeArticle>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(200);
            entity.Property(x => x.Content).HasMaxLength(10000);
            entity.Property(x => x.Tags).HasMaxLength(500);
        });
    }
}
