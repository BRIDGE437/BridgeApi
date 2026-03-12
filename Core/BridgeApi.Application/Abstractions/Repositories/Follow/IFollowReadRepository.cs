using BridgeApi.Domain.Entities;
using FollowEntity = BridgeApi.Domain.Entities.Follow;

namespace BridgeApi.Application.Abstractions.Repositories.Follow;

public interface IFollowReadRepository : IReadRepository<FollowEntity>
{
}
