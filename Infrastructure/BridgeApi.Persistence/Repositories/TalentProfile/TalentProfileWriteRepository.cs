using BridgeApi.Application.Abstractions.Repositories.TalentProfile;
using BridgeApi.Persistence.Contexts;

namespace BridgeApi.Persistence.Repositories.TalentProfile;

public class TalentProfileWriteRepository : WriteRepository<BridgeApi.Domain.Entities.TalentProfile>, ITalentProfileWriteRepository
{
    public TalentProfileWriteRepository(ApplicationDbContext context) : base(context)
    {
    }
}
