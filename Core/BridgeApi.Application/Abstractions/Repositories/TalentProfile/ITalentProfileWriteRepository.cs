using TalentProfileEntity = BridgeApi.Domain.Entities.TalentProfile;

namespace BridgeApi.Application.Abstractions.Repositories.TalentProfile;

public interface ITalentProfileWriteRepository : IWriteRepository<TalentProfileEntity>
{
}
