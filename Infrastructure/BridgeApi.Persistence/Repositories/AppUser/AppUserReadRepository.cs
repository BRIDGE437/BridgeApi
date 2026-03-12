using BridgeApi.Application.Abstractions.Repositories.AppUser;
using BridgeApi.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using AppUserEntity = BridgeApi.Domain.Entities.AppUser;

namespace BridgeApi.Persistence.Repositories.AppUser;

public class AppUserReadRepository : IAppUserReadRepository
{
    private readonly ApplicationDbContext _context;

    public AppUserReadRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AppUserEntity?> GetByIdAsync(string id, bool tracking = true)
    {
        var query = _context.Users.AsQueryable();
        if (!tracking) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<AppUserEntity?> GetByUsernameAsync(string username, bool tracking = true)
    {
        var query = _context.Users.AsQueryable();
        if (!tracking) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(u => u.UserName == username);
    }

    public async Task<AppUserEntity?> GetByEmailAsync(string email, bool tracking = true)
    {
        var query = _context.Users.AsQueryable();
        if (!tracking) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(u => u.Email == email);
    }

    public IQueryable<AppUserEntity> GetAll(bool tracking = true)
    {
        var query = _context.Users.AsQueryable();
        if (!tracking) query = query.AsNoTracking();
        return query;
    }
}
