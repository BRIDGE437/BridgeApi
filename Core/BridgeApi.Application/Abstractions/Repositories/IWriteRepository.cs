using BridgeApi.Domain.Entities;

namespace BridgeApi.Application.Abstractions.Repositories;

public interface IWriteRepository<T> : IRepository<T> where T : BaseEntity
{
    Task<bool> AddAsync(T model);
    Task<bool> AddRangeAsync(IEnumerable<T> models);
    Task<bool> RemoveAsync(T model);
    Task<bool> RemoveAsync(Guid id);
    Task<bool> RemoveRangeAsync(IEnumerable<T> models);
    Task<bool> UpdateAsync(T model);
    Task<int> SaveAsync();
}
