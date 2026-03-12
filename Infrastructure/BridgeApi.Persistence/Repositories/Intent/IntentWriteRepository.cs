using BridgeApi.Application.Abstractions.Repositories.Intent;
using BridgeApi.Persistence.Contexts;

namespace BridgeApi.Persistence.Repositories.Intent;

public class IntentWriteRepository : WriteRepository<BridgeApi.Domain.Entities.Intent>, IIntentWriteRepository
{
    public IntentWriteRepository(ApplicationDbContext context) : base(context)
    {
    }
}
