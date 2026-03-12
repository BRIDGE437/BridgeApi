using BridgeApi.Application.Abstractions.Repositories.UserProfile;
using BridgeApi.Persistence.Contexts;

namespace BridgeApi.Persistence.Repositories.UserProfile;

public class UserProfileReadRepository : ReadRepository<BridgeApi.Domain.Entities.UserProfile>, IUserProfileReadRepository
{
    public UserProfileReadRepository(ApplicationDbContext context) : base(context)
    {
    }
}
