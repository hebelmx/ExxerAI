namespace ExxerAI.Orchestration.Services;

/// <summary>
/// Represents a stored key with metadata
/// </summary>
public record StoredKey
{
    public string Name { get; init; } = string.Empty;
    public string EncryptedValue { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public string? Scope { get; init; }
}