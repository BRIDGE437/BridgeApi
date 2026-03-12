using BridgeApi.Domain.Entities;
using MessageEntity = BridgeApi.Domain.Entities.Message;

namespace BridgeApi.Application.Abstractions.Repositories.Message;

public interface IMessageReadRepository : IReadRepository<MessageEntity>
{
}
