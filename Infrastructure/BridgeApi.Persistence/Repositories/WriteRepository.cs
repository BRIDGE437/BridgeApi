using BridgeApi.Application.Abstractions.Repositories;
using BridgeApi.Domain.Entities;
using BridgeApi.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BridgeApi.Persistence.Repositories;

public class WriteRepository<T> : IWriteRepository<T> where T : BaseEntity
{
    private readonly ApplicationDbContext _context;

    public WriteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public DbSet<T> Table => _context.Set<T>();

    public async Task<bool> AddAsync(T model)
    {
        EntityEntry<T> entityEntry = await Table.AddAsync(model);
        return entityEntry.State == EntityState.Added;
    }

    public async Task<bool> AddRangeAsync(IEnumerable<T> models)
    {
        await Table.AddRangeAsync(models);
        return true;
    }

    public Task<bool> RemoveAsync(T model)
    {
        EntityEntry<T> entityEntry = Table.Remove(model);
        return Task.FromResult(entityEntry.State == EntityState.Deleted);
    }

    public async Task<bool> RemoveAsync(Guid id)
    {
        T? model = await Table.FirstOrDefaultAsync(data => data.Id == id);
        if (model == null)
            return false;
        return await RemoveAsync(model);
    }

    public Task<bool> RemoveRangeAsync(IEnumerable<T> models)
    {
        Table.RemoveRange(models);
        return Task.FromResult(true);
    }

    public Task<bool> UpdateAsync(T model)
    {
        EntityEntry<T> entityEntry = Table.Update(model);
        return Task.FromResult(entityEntry.State == EntityState.Modified);
    }

    public async Task<int> SaveAsync() => await _context.SaveChangesAsync();
}
