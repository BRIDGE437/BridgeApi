using BridgeApi.Application.Abstractions.Repositories.StoredFile;
using BridgeApi.Persistence.Contexts;

namespace BridgeApi.Persistence.Repositories.StoredFile;

public class StoredFileWriteRepository : WriteRepository<BridgeApi.Domain.Entities.StoredFile>, IStoredFileWriteRepository
{
    public StoredFileWriteRepository(ApplicationDbContext context) : base(context)
    {
    }
}
