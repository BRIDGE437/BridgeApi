using BridgeApi.Domain.Entities;
using ConnectionEntity = BridgeApi.Domain.Entities.Connection;

namespace BridgeApi.Application.Abstractions.Repositories.Connection;

public interface IConnectionReadRepository : IReadRepository<ConnectionEntity>
{
}
