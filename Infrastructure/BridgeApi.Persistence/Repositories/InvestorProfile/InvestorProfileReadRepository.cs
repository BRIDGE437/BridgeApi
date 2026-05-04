using BridgeApi.Application.Abstractions.Repositories.InvestorProfile;
using BridgeApi.Persistence.Contexts;

namespace BridgeApi.Persistence.Repositories.InvestorProfile;

public class InvestorProfileReadRepository : ReadRepository<BridgeApi.Domain.Entities.InvestorProfile>, IInvestorProfileReadRepository
{
    public InvestorProfileReadRepository(ApplicationDbContext context) : base(context)
    {
    }
}
