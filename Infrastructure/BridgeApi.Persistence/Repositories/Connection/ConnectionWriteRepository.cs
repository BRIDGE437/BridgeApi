using BridgeApi.Application.Abstractions.Repositories.Connection;
using BridgeApi.Persistence.Contexts;

namespace BridgeApi.Persistence.Repositories.Connection;

public class ConnectionWriteRepository : WriteRepository<BridgeApi.Domain.Entities.Connection>, IConnectionWriteRepository
{
    public ConnectionWriteRepository(ApplicationDbContext context) : base(context)
    {
    }
}
