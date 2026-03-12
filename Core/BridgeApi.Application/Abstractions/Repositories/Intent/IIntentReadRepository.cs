using BridgeApi.Domain.Entities;
using IntentEntity = BridgeApi.Domain.Entities.Intent;

namespace BridgeApi.Application.Abstractions.Repositories.Intent;

public interface IIntentReadRepository : IReadRepository<IntentEntity>
{
}
