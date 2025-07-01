using ExxerAI.Domain.DocumentProcessing;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Service for detecting and managing document versions
/// </summary>
public interface IVersionDetectionEngine
{
    /// <summary>
    /// Detects the version of a document based on its content and metadata
    /// </summary>
    /// <param name="content">The document content</param>
    /// <param name="metadata">The document metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The detected version information</returns>
    Task<DocumentVersionInfo> DetectVersionAsync(byte[] content, DocumentMetadata metadata, CancellationToken cancellationToken = default);

    /// <summary>
    /// Compares two document versions to determine their relationship
    /// </summary>
    /// <param name="version1">The first version</param>
    /// <param name="version2">The second version</param>
    /// <returns>Version comparison result</returns>
    VersionComparisonResult CompareVersions(DocumentVersionInfo version1, DocumentVersionInfo version2);

    /// <summary>
    /// Gets the latest version from a collection of document versions
    /// </summary>
    /// <param name="versions">Collection of document versions</param>
    /// <returns>The latest version</returns>
    DocumentVersionInfo GetLatestVersion(IEnumerable<DocumentVersionInfo> versions);

    /// <summary>
    /// Determines if a document has been modified based on version comparison
    /// </summary>
    /// <param name="currentVersion">The current version</param>
    /// <param name="previousVersion">The previous version</param>
    /// <returns>True if document has been modified, false otherwise</returns>
    bool IsModified(DocumentVersionInfo currentVersion, DocumentVersionInfo previousVersion);
}

/// <summary>
/// Document version information
/// </summary>
public record DocumentVersionInfo(
    string Version,
    DateTime CreatedDate,
    DateTime ModifiedDate,
    string Hash,
    long Size,
    Dictionary<string, object> Properties);

/// <summary>
/// Version comparison result
/// </summary>
public enum VersionComparisonResult
{
    /// <summary>
    /// Versions are identical
    /// </summary>
    Identical,
    
    /// <summary>
    /// First version is newer
    /// </summary>
    FirstNewer,
    
    /// <summary>
    /// Second version is newer
    /// </summary>
    SecondNewer,
    
    /// <summary>
    /// Versions are incomparable
    /// </summary>
    Incomparable
} 