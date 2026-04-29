using BridgeApi.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace BridgeApi.Persistence.Contexts;

public class ApplicationDbContext : IdentityDbContext<AppUser, AppRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<FounderProfile> FounderProfiles => Set<FounderProfile>();
    public DbSet<InvestorProfile> InvestorProfiles => Set<InvestorProfile>();
    public DbSet<TalentProfile> TalentProfiles => Set<TalentProfile>();
    public DbSet<Intent> Intents => Set<Intent>();
    public DbSet<UserIntent> UserIntents => Set<UserIntent>();
    public DbSet<Connection> Connections => Set<Connection>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostLike> PostLikes => Set<PostLike>();
    public DbSet<PostComment> PostComments => Set<PostComment>();
    public DbSet<Follow> Follows => Set<Follow>();
    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
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
