using BridgeApi.Application.Abstractions.Repositories.Post;
using BridgeApi.Persistence.Contexts;

namespace BridgeApi.Persistence.Repositories.Post;

public class PostWriteRepository : WriteRepository<BridgeApi.Domain.Entities.Post>, IPostWriteRepository
{
    public PostWriteRepository(ApplicationDbContext context) : base(context)
    {
    }
}
