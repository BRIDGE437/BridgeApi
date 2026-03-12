using BridgeApi.Application.Abstractions.Repositories.PostComment;
using BridgeApi.Persistence.Contexts;

namespace BridgeApi.Persistence.Repositories.PostComment;

public class PostCommentReadRepository : ReadRepository<BridgeApi.Domain.Entities.PostComment>, IPostCommentReadRepository
{
    public PostCommentReadRepository(ApplicationDbContext context) : base(context)
    {
    }
}
