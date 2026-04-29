using BridgeApi.Application.Abstractions.Repositories.Notification;
using BridgeApi.Persistence.Contexts;
using NotificationEntity = BridgeApi.Domain.Entities.Notification;

namespace BridgeApi.Persistence.Repositories.Notification;

public class NotificationReadRepository : ReadRepository<NotificationEntity>, INotificationReadRepository
{
    public NotificationReadRepository(ApplicationDbContext context) : base(context)
    {
    }
}
