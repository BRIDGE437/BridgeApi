using BridgeApi.Application.Abstractions.Repositories.FounderProfile;
using BridgeApi.Persistence.Contexts;

namespace BridgeApi.Persistence.Repositories.FounderProfile;

public class FounderProfileReadRepository : ReadRepository<BridgeApi.Domain.Entities.FounderProfile>, IFounderProfileReadRepository
{
    public FounderProfileReadRepository(ApplicationDbContext context) : base(context)
    {
    }
}
