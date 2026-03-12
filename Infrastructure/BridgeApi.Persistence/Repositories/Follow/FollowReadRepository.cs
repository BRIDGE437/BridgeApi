using BridgeApi.Application.Abstractions.Repositories.Follow;
using BridgeApi.Persistence.Contexts;

namespace BridgeApi.Persistence.Repositories.Follow;

public class FollowReadRepository : ReadRepository<BridgeApi.Domain.Entities.Follow>, IFollowReadRepository
{
    public FollowReadRepository(ApplicationDbContext context) : base(context)
    {
    }
}
