using BridgeApi.Application.Abstractions.Repositories.Intent;
using BridgeApi.Persistence.Contexts;

namespace BridgeApi.Persistence.Repositories.Intent;

public class IntentReadRepository : ReadRepository<BridgeApi.Domain.Entities.Intent>, IIntentReadRepository
{
    public IntentReadRepository(ApplicationDbContext context) : base(context)
    {
    }
}
