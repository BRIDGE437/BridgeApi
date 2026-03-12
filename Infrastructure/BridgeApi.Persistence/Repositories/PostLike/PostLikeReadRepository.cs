using BridgeApi.Application.Abstractions.Repositories.PostLike;
using BridgeApi.Persistence.Contexts;

namespace BridgeApi.Persistence.Repositories.PostLike;

public class PostLikeReadRepository : ReadRepository<BridgeApi.Domain.Entities.PostLike>, IPostLikeReadRepository
{
    public PostLikeReadRepository(ApplicationDbContext context) : base(context)
    {
    }
}
