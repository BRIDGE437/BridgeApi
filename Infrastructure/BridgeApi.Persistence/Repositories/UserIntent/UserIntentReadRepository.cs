using BridgeApi.Application.Abstractions.Repositories.UserIntent;
using BridgeApi.Persistence.Contexts;

namespace BridgeApi.Persistence.Repositories.UserIntent;

public class UserIntentReadRepository : ReadRepository<BridgeApi.Domain.Entities.UserIntent>, IUserIntentReadRepository
{
    public UserIntentReadRepository(ApplicationDbContext context) : base(context)
    {
    }
}
