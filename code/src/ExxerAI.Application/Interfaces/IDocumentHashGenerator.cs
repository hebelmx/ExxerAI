using ExxerAI.Domain;
using ExxerAI.Domain.DocumentProcessing;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Service for generating and comparing document hashes for deduplication and integrity verification.
/// </summary>
public interface IDocumentHashGenerator
{
    /// <summary>
    /// Generates a comprehensive hash for a document including content and metadata.
    /// </summary>
    /// <param name="content">The raw document content.</param>
    /// <param name="metadata">Document metadata for fingerprinting.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The complete document hash with content and metadata fingerprints.</returns>
    Task<Result<DocumentHash>> GenerateHashAsync(
        byte[] content, 
        DocumentMetadata metadata, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Compares two document hashes for equality and similarity.
    /// </summary>
    /// <param name="hash1">The first hash to compare.</param>
    /// <param name="hash2">The second hash to compare.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>True if the hashes indicate the same or equivalent content.</returns>
    Task<Result<bool>> CompareHashesAsync(
        DocumentHash hash1, 
        DocumentHash hash2, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a document fingerprint for similarity detection.
    /// </summary>
    /// <param name="metadata">Document metadata to fingerprint.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The document fingerprint for similarity analysis.</returns>
    Task<Result<DocumentFingerprint>> CreateFingerprintAsync(
        DocumentMetadata metadata, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a SHA-256 content hash for the raw document data.
    /// </summary>
    /// <param name="content">The document content to hash.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The SHA-256 hash of the content.</returns>
    Task<Result<string>> GenerateContentHashAsync(
        byte[] content, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the integrity of a document by comparing its current hash with a stored hash.
    /// </summary>
    /// <param name="content">The current document content.</param>
    /// <param name="storedHash">The previously stored hash to compare against.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>True if the document integrity is verified.</returns>
    Task<Result<bool>> VerifyIntegrityAsync(
        byte[] content, 
        string storedHash, 
        CancellationToken cancellationToken = default);
}