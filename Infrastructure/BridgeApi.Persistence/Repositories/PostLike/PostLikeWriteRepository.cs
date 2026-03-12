using BridgeApi.Application.Abstractions.Repositories.PostLike;
using BridgeApi.Persistence.Contexts;

namespace BridgeApi.Persistence.Repositories.PostLike;

public class PostLikeWriteRepository : WriteRepository<BridgeApi.Domain.Entities.PostLike>, IPostLikeWriteRepository
{
    public PostLikeWriteRepository(ApplicationDbContext context) : base(context)
    {
    }
}
