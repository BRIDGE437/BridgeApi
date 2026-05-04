using BridgeApi.Application.Abstractions.Repositories.Notification;
using BridgeApi.Persistence.Contexts;
using NotificationEntity = BridgeApi.Domain.Entities.Notification;

namespace BridgeApi.Persistence.Repositories.Notification;

public class NotificationWriteRepository : WriteRepository<NotificationEntity>, INotificationWriteRepository
{
    public NotificationWriteRepository(ApplicationDbContext context) : base(context)
    {
    }
}
