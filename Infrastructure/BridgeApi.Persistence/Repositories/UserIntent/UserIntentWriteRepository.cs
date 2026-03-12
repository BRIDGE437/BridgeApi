using BridgeApi.Application.Abstractions.Repositories.UserIntent;
using BridgeApi.Persistence.Contexts;

namespace BridgeApi.Persistence.Repositories.UserIntent;

public class UserIntentWriteRepository : WriteRepository<BridgeApi.Domain.Entities.UserIntent>, IUserIntentWriteRepository
{
    public UserIntentWriteRepository(ApplicationDbContext context) : base(context)
    {
    }
}
