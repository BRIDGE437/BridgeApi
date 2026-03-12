using BridgeApi.Domain.Entities;
using UserProfileEntity = BridgeApi.Domain.Entities.UserProfile;

namespace BridgeApi.Application.Abstractions.Repositories.UserProfile;

public interface IUserProfileWriteRepository : IWriteRepository<UserProfileEntity>
{
}
