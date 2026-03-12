using BridgeApi.Domain.Enums;

namespace BridgeApi.Domain.Entities;

public class StoredFile : BaseEntity
{
    public string OriginalFileName { get; set; } = null!;
    public string StoredFileName { get; set; } = null!;
    public string Path { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string Storage { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long Size { get; set; }
    public FileCategory Category { get; set; }
    public string UploadedByUserId { get; set; } = null!;

    // Navigation Property
    public AppUser UploadedByUser { get; set; } = null!;
}
