using BridgeApi.Application.Abstractions.Repositories.StoredFile;
using BridgeApi.Persistence.Contexts;

namespace BridgeApi.Persistence.Repositories.StoredFile;

public class StoredFileReadRepository : ReadRepository<BridgeApi.Domain.Entities.StoredFile>, IStoredFileReadRepository
{
    public StoredFileReadRepository(ApplicationDbContext context) : base(context)
    {
    }
}
