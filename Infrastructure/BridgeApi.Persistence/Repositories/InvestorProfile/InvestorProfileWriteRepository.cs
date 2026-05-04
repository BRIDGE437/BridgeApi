using BridgeApi.Application.Abstractions.Repositories.InvestorProfile;
using BridgeApi.Persistence.Contexts;

namespace BridgeApi.Persistence.Repositories.InvestorProfile;

public class InvestorProfileWriteRepository : WriteRepository<BridgeApi.Domain.Entities.InvestorProfile>, IInvestorProfileWriteRepository
{
    public InvestorProfileWriteRepository(ApplicationDbContext context) : base(context)
    {
    }
}
