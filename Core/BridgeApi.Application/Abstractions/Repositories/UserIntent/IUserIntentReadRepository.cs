using BridgeApi.Domain.Entities;
using UserIntentEntity = BridgeApi.Domain.Entities.UserIntent;

namespace BridgeApi.Application.Abstractions.Repositories.UserIntent;

public interface IUserIntentReadRepository : IReadRepository<UserIntentEntity>
{
}
