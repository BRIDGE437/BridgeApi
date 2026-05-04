using BridgeApi.Domain.Entities;
using BridgeApi.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace BridgeApi.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<AppUser> Users { get; }
    DbSet<StartupProfile> StartupProfiles { get; }
    DbSet<InvestorProfile> InvestorProfiles { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
