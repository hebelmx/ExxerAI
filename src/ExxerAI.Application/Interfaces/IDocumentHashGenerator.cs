namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Service for generating and managing document hashes for change detection
/// </summary>
public interface IDocumentHashGenerator
{
    /// <summary>
    /// Generates a hash for the given document content
    /// </summary>
    /// <param name="content">The document content as byte array</param>
    /// <param name="algorithm">The hashing algorithm to use (default: SHA256)</param>
    /// <returns>The generated hash string</returns>
    Task<string> GenerateHashAsync(byte[] content, string algorithm = "SHA256");

    /// <summary>
    /// Generates a hash for a document file
    /// </summary>
    /// <param name="filePath">The path to the document file</param>
    /// <param name="algorithm">The hashing algorithm to use (default: SHA256)</param>
    /// <returns>The generated hash string</returns>
    Task<string> GenerateFileHashAsync(string filePath, string algorithm = "SHA256");

    /// <summary>
    /// Compares two hashes to determine if documents have changed
    /// </summary>
    /// <param name="hash1">The first hash</param>
    /// <param name="hash2">The second hash</param>
    /// <returns>True if hashes are different (document changed), false otherwise</returns>
    bool HasChanged(string hash1, string hash2);

    /// <summary>
    /// Validates that a hash is in the correct format for the specified algorithm
    /// </summary>
    /// <param name="hash">The hash to validate</param>
    /// <param name="algorithm">The expected algorithm</param>
    /// <returns>True if valid, false otherwise</returns>
    bool IsValidHash(string hash, string algorithm = "SHA256");
} 