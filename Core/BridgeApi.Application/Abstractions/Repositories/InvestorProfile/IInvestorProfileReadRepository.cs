using InvestorProfileEntity = BridgeApi.Domain.Entities.InvestorProfile;

namespace BridgeApi.Application.Abstractions.Repositories.InvestorProfile;

public interface IInvestorProfileReadRepository : IReadRepository<InvestorProfileEntity>
{
}
