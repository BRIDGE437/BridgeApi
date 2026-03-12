using AppUserEntity = BridgeApi.Domain.Entities.AppUser;

namespace BridgeApi.Application.Abstractions.Repositories.AppUser;

public interface IAppUserReadRepository
{
    Task<AppUserEntity?> GetByIdAsync(string id, bool tracking = true);
    Task<AppUserEntity?> GetByUsernameAsync(string username, bool tracking = true);
    Task<AppUserEntity?> GetByEmailAsync(string email, bool tracking = true);
    IQueryable<AppUserEntity> GetAll(bool tracking = true);
}
