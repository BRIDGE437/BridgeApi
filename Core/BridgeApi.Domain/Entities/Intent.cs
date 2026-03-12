namespace BridgeApi.Domain.Entities;

/// <summary>
/// Niyet / rozet tanımı (örn. Actively Hiring, Raising, Idea Stage).
/// Hem kullanıcı rozetleri (UserIntent) hem connection türü (Connection.IntentId) için kullanılır.
/// </summary>
public class Intent : BaseEntity
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<UserIntent> UserIntents { get; set; } = new List<UserIntent>();
    public ICollection<Connection> Connections { get; set; } = new List<Connection>();
}
