using BridgeApi.Domain.Entities;
using PostEntity = BridgeApi.Domain.Entities.Post;

namespace BridgeApi.Application.Abstractions.Repositories.Post;

public interface IPostReadRepository : IReadRepository<PostEntity>
{
}
