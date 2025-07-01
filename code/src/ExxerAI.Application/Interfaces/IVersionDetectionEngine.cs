using ExxerAI.Domain.DocumentProcessing;
using ExxerAI.Domain;

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
    /// <returns>Result containing the detected version information</returns>
    Task<Result<DocumentVersionInfo>> DetectVersionAsync(byte[] content, DocumentMetadata metadata, CancellationToken cancellationToken = default);

    /// <summary>
    /// Compares two document versions to determine their relationship
    /// </summary>
    /// <param name="version1">The first version</param>
    /// <param name="version2">The second version</param>
    /// <returns>Result containing version comparison result</returns>
    Result<VersionComparisonResult> CompareVersions(DocumentVersionInfo version1, DocumentVersionInfo version2);

    /// <summary>
    /// Gets the latest version from a collection of document versions
    /// </summary>
    /// <param name="versions">Collection of document versions</param>
    /// <returns>Result containing the latest version</returns>
    Result<DocumentVersionInfo> GetLatestVersion(IEnumerable<DocumentVersionInfo> versions);

    /// <summary>
    /// Determines if a document has been modified based on version comparison
    /// </summary>
    /// <param name="currentVersion">The current version</param>
    /// <param name="previousVersion">The previous version</param>
    /// <returns>Result indicating if document has been modified</returns>
    Result<bool> IsModified(DocumentVersionInfo currentVersion, DocumentVersionInfo previousVersion);
    Task<Result<VersionStatus>> DetermineVersionStatusAsync(DocumentMetadata documentMetadata);
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

/// <summary>
/// Version status for document processing decisions
/// </summary>
public enum VersionStatus
{
    /// <summary>
    /// Document is new and should be processed
    /// </summary>
    New,
    
    /// <summary>
    /// Document has been updated and should be reprocessed
    /// </summary>
    Update,
    
    /// <summary>
    /// Document is unchanged and can be skipped
    /// </summary>
    Skip,
    
    /// <summary>
    /// Version determination failed or is uncertain
    /// </summary>
    Uncertain
} 