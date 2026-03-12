using BridgeApi.Application.Abstractions.Repositories.Follow;
using BridgeApi.Persistence.Contexts;

namespace BridgeApi.Persistence.Repositories.Follow;

public class FollowWriteRepository : WriteRepository<BridgeApi.Domain.Entities.Follow>, IFollowWriteRepository
{
    public FollowWriteRepository(ApplicationDbContext context) : base(context)
    {
    }
}
