using BridgeApi.Application.Abstractions.Repositories.AppUser;
using BridgeApi.Persistence.Contexts;
using AppUserEntity = BridgeApi.Domain.Entities.AppUser;

namespace BridgeApi.Persistence.Repositories.AppUser;

public class AppUserWriteRepository : IAppUserWriteRepository
{
    private readonly ApplicationDbContext _context;

    public AppUserWriteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<bool> UpdateAsync(AppUserEntity user)
    {
        _context.Users.Update(user);
        return Task.FromResult(true);
    }

    public async Task<int> SaveAsync() => await _context.SaveChangesAsync();
}
