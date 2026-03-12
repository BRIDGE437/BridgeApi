using BridgeApi.Application.Abstractions.Repositories.Message;
using BridgeApi.Persistence.Contexts;

namespace BridgeApi.Persistence.Repositories.Message;

public class MessageWriteRepository : WriteRepository<BridgeApi.Domain.Entities.Message>, IMessageWriteRepository
{
    public MessageWriteRepository(ApplicationDbContext context) : base(context)
    {
    }
}
