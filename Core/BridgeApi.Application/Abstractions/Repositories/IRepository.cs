using BridgeApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BridgeApi.Application.Abstractions.Repositories;

public interface IRepository<T> where T : BaseEntity
{
    DbSet<T> Table { get; }
}
