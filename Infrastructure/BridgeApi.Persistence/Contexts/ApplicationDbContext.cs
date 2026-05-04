using BridgeApi.Domain.Entities;
using BridgeApi.Shared.Entities;
using BridgeApi.Application.Abstractions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace BridgeApi.Persistence.Contexts;

public class ApplicationDbContext : IdentityDbContext<AppUser, AppRole, string>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<InvestorProfile> InvestorProfiles => Set<InvestorProfile>();
    public DbSet<StartupProfile> StartupProfiles => Set<StartupProfile>();
    public DbSet<Intent> Intents => Set<Intent>();
    public DbSet<UserIntent> UserIntents => Set<UserIntent>();
    public DbSet<Connection> Connections => Set<Connection>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostLike> PostLikes => Set<PostLike>();
    public DbSet<PostComment> PostComments => Set<PostComment>();
    public DbSet<Follow> Follows => Set<Follow>();
    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Enable pgvector extension
        modelBuilder.HasPostgresExtension("vector");

        // 1-to-1 Relationship: AppUser <-> InvestorProfile
        modelBuilder.Entity<InvestorProfile>()
            .HasOne<AppUser>()
            .WithOne(u => u.InvestorProfile)
            .HasForeignKey<InvestorProfile>(ip => ip.UserId);

        // 1-to-1 Relationship: AppUser <-> StartupProfile
        modelBuilder.Entity<StartupProfile>(entity =>
        {
            entity.HasOne<AppUser>()
                .WithOne(u => u.StartupProfile)
                .HasForeignKey<StartupProfile>(sp => sp.UserId);

            entity.HasIndex(s => s.ExternalFingerprint);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            _ = entry.State switch
            {
                EntityState.Added => entry.Entity.CreatedAt = DateTime.UtcNow,
                EntityState.Modified => entry.Entity.UpdatedAt = DateTime.UtcNow,
                _ => default(DateTime)
            };
        }

        foreach (var entry in ChangeTracker.Entries<AppUser>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = DateTime.UtcNow;
            else if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
