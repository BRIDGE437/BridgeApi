using BridgeApi.Domain.Entities;
using PostCommentEntity = BridgeApi.Domain.Entities.PostComment;

namespace BridgeApi.Application.Abstractions.Repositories.PostComment;

public interface IPostCommentWriteRepository : IWriteRepository<PostCommentEntity>
{
}
