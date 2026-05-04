using BridgeApi.Domain.Entities;
using NotificationEntity = BridgeApi.Domain.Entities.Notification;

namespace BridgeApi.Application.Abstractions.Repositories.Notification;

public interface INotificationWriteRepository : IWriteRepository<NotificationEntity>
{
}
