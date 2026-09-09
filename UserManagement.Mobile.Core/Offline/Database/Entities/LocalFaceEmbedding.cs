using SQLite;

namespace UserManagement.Mobile.Core.Offline.Database.Entities;

/// <summary>
/// Cached face embedding for offline face recognition verification.
/// Synced from server on login/app launch.
/// </summary>
[Table("face_embeddings")]
public sealed class LocalFaceEmbedding
{
    [PrimaryKey]
    public string UserId { get; set; } = string.Empty;

    /// <summary>512-dim float32 face embedding (2048 bytes).</summary>
    public byte[] Embedding { get; set; } = [];

    /// <summary>Small JPEG thumbnail for visual confirmation.</summary>
    public byte[]? Thumbnail { get; set; }

    public DateTimeOffset EnrolledAt { get; set; }
    public DateTimeOffset? LastVerifiedAt { get; set; }
}
