using BridgeApi.Application.Abstractions.Repositories.TalentProfile;
using BridgeApi.Persistence.Contexts;

namespace BridgeApi.Persistence.Repositories.TalentProfile;

public class TalentProfileReadRepository : ReadRepository<BridgeApi.Domain.Entities.TalentProfile>, ITalentProfileReadRepository
{
    public TalentProfileReadRepository(ApplicationDbContext context) : base(context)
    {
    }
}
