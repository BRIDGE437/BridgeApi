using BridgeApi.Domain.Entities;
using PostLikeEntity = BridgeApi.Domain.Entities.PostLike;

namespace BridgeApi.Application.Abstractions.Repositories.PostLike;

public interface IPostLikeWriteRepository : IWriteRepository<PostLikeEntity>
{
}
