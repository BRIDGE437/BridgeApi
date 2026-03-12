using BridgeApi.Application.Abstractions.Repositories.Message;
using BridgeApi.Persistence.Contexts;

namespace BridgeApi.Persistence.Repositories.Message;

public class MessageReadRepository : ReadRepository<BridgeApi.Domain.Entities.Message>, IMessageReadRepository
{
    public MessageReadRepository(ApplicationDbContext context) : base(context)
    {
    }
}
