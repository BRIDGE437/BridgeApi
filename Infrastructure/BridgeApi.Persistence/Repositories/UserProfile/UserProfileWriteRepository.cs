using BridgeApi.Application.Abstractions.Repositories.UserProfile;
using BridgeApi.Persistence.Contexts;

namespace BridgeApi.Persistence.Repositories.UserProfile;

public class UserProfileWriteRepository : WriteRepository<BridgeApi.Domain.Entities.UserProfile>, IUserProfileWriteRepository
{
    public UserProfileWriteRepository(ApplicationDbContext context) : base(context)
    {
    }
}
