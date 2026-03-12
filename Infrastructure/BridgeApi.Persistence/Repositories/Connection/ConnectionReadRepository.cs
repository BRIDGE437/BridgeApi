using BridgeApi.Application.Abstractions.Repositories.Connection;
using BridgeApi.Persistence.Contexts;

namespace BridgeApi.Persistence.Repositories.Connection;

public class ConnectionReadRepository : ReadRepository<BridgeApi.Domain.Entities.Connection>, IConnectionReadRepository
{
    public ConnectionReadRepository(ApplicationDbContext context) : base(context)
    {
    }
}
