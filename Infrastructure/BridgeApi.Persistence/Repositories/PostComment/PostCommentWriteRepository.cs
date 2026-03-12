using BridgeApi.Application.Abstractions.Repositories.PostComment;
using BridgeApi.Persistence.Contexts;

namespace BridgeApi.Persistence.Repositories.PostComment;

public class PostCommentWriteRepository : WriteRepository<BridgeApi.Domain.Entities.PostComment>, IPostCommentWriteRepository
{
    public PostCommentWriteRepository(ApplicationDbContext context) : base(context)
    {
    }
}
