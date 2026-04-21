using Microsoft.EntityFrameworkCore;
using MatchingApi.Models;

namespace MatchingApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Startup> Startups => Set<Startup>();
    public DbSet<Investor> Investors => Set<Investor>();
    public DbSet<MatchResult> MatchResults => Set<MatchResult>();
    public DbSet<StartupMatchResult> StartupMatchResults => Set<StartupMatchResult>();
    public DbSet<LlmScoreCache> LlmScoreCache => Set<LlmScoreCache>();
    public DbSet<MatchEvent> MatchEvents => Set<MatchEvent>();
    public DbSet<EventParticipation> EventParticipations => Set<EventParticipation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── pgvector extension ──
        modelBuilder.HasPostgresExtension("vector");

        // ── Startup ──
        modelBuilder.Entity<Startup>(entity =>
        {
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Tags);
            entity.HasIndex(e => e.HQ);
            entity.Property(e => e.Embedding).HasColumnType("vector(384)");
        });

        // ── Investor ──
        modelBuilder.Entity<Investor>(entity =>
        {
            entity.HasIndex(e => e.Active);
            entity.HasIndex(e => e.Type);
            entity.Property(e => e.Embedding).HasColumnType("vector(384)");
        });

        // ── MatchResult ──
        modelBuilder.Entity<MatchResult>(entity =>
        {
            entity.HasIndex(e => new { e.InvestorId, e.StartupId });
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(e => e.Startup)
                  .WithMany()
                  .HasForeignKey(e => e.StartupId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Investor)
                  .WithMany()
                  .HasForeignKey(e => e.InvestorId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Event)
                  .WithMany(e => e.MatchResults)
                  .HasForeignKey(e => e.EventId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ── StartupMatchResult ──
        modelBuilder.Entity<StartupMatchResult>(entity =>
        {
            entity.HasIndex(e => new { e.SourceStartupId, e.TargetStartupId });
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(e => e.SourceStartup)
                  .WithMany()
                  .HasForeignKey(e => e.SourceStartupId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TargetStartup)
                  .WithMany()
                  .HasForeignKey(e => e.TargetStartupId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Event)
                  .WithMany()
                  .HasForeignKey(e => e.EventId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ── MatchEvent ──
        modelBuilder.Entity<MatchEvent>(entity =>
        {
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ScheduledAt);
        });

        // ── EventParticipation ──
        modelBuilder.Entity<EventParticipation>(entity =>
        {
            entity.HasIndex(e => new { e.EventId, e.ParticipantId, e.ParticipantType }).IsUnique();
        });

        // ── LlmScoreCache ──
        modelBuilder.Entity<LlmScoreCache>(entity =>
        {
            entity.HasIndex(e => new { e.InvestorTextHash, e.StartupId, e.StartupTextHash })
                  .IsUnique();
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
