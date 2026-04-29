using BridgeApi.Application.Abstractions.Repositories.FounderProfile;
using BridgeApi.Persistence.Contexts;

namespace BridgeApi.Persistence.Repositories.FounderProfile;

public class FounderProfileWriteRepository : WriteRepository<BridgeApi.Domain.Entities.FounderProfile>, IFounderProfileWriteRepository
{
    public FounderProfileWriteRepository(ApplicationDbContext context) : base(context)
    {
    }
}
