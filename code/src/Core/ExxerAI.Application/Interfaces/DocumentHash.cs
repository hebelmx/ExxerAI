namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Represents a complete document hash with content and metadata components.
/// </summary>
public class DocumentHash
{
    /// <summary>
    /// Gets or sets the SHA-256 hash of the document content.
    /// </summary>
    public string ContentHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hash of the metadata fingerprint.
    /// </summary>
    public string MetadataHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when this hash was generated.
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the algorithm used for hashing.
    /// </summary>
    public string Algorithm { get; set; } = "SHA-256";

    /// <summary>
    /// Gets or sets additional hash metadata.
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = new();

    /// <summary>
    /// Gets the combined hash representing both content and metadata.
    /// </summary>
    public string CombinedHash => $"{ContentHash}:{MetadataHash}";

    /// <summary>
    /// Initializes a new instance of the DocumentHash class.
    /// </summary>
    public DocumentHash() { }

    /// <summary>
    /// Initializes a new instance of the DocumentHash class with hash values.
    /// </summary>
    /// <param name="contentHash">The content hash.</param>
    /// <param name="metadataHash">The metadata hash.</param>
    public DocumentHash(string contentHash, string metadataHash)
    {
        ContentHash = contentHash;
        MetadataHash = metadataHash;
        GeneratedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if this hash is equivalent to another hash.
    /// </summary>
    /// <param name="other">The other hash to compare.</param>
    /// <returns>True if the hashes are equivalent.</returns>
    public bool IsEquivalentTo(DocumentHash other)
    {
        if (other == null) return false;
        return ContentHash.Equals(other.ContentHash, StringComparison.Ordinal) &&
               MetadataHash.Equals(other.MetadataHash, StringComparison.Ordinal);
    }

    /// <summary>
    /// Returns a string representation of the document hash.
    /// </summary>
    /// <returns>A formatted string with hash information.</returns>
    public override string ToString()
    {
        return $"DocumentHash[Content: {ContentHash[..8]}..., Metadata: {MetadataHash[..8]}..., Generated: {GeneratedAt:yyyy-MM-dd HH:mm:ss}]";
    }
}