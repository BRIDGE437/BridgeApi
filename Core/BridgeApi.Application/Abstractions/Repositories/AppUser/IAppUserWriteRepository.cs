using AppUserEntity = BridgeApi.Domain.Entities.AppUser;

namespace BridgeApi.Application.Abstractions.Repositories.AppUser;

public interface IAppUserWriteRepository
{
    Task<bool> UpdateAsync(AppUserEntity user);
    Task<int> SaveAsync();
}
