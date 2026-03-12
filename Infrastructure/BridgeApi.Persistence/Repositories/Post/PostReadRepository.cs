using BridgeApi.Application.Abstractions.Repositories.Post;
using BridgeApi.Persistence.Contexts;

namespace BridgeApi.Persistence.Repositories.Post;

public class PostReadRepository : ReadRepository<BridgeApi.Domain.Entities.Post>, IPostReadRepository
{
    public PostReadRepository(ApplicationDbContext context) : base(context)
    {
    }
}
