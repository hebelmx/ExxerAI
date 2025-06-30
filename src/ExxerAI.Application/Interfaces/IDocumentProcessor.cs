using ExxerAI.Domain.ValueObjects;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Interface for document processing and content extraction
/// Supports PDF, Word, Excel, and other document formats for business intelligence
/// </summary>
public interface IDocumentProcessor
{
    /// <summary>
    /// Processes uploaded document and extracts content and metadata
    /// </summary>
    /// <param name="documentContent">Raw document content</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="mimeType">Document MIME type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Document processing result with extracted content</returns>
    Task<DocumentProcessingResult> ProcessDocumentAsync(
        byte[] documentContent, 
        string fileName, 
        string mimeType, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts text content from document while preserving structure
    /// </summary>
    /// <param name="documentContent">Raw document content</param>
    /// <param name="documentType">Type of document</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Extracted text content with structure information</returns>
    Task<string> ExtractTextAsync(
        byte[] documentContent, 
        DocumentType documentType, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates document fingerprint for deduplication
    /// </summary>
    /// <param name="documentContent">Raw document content</param>
    /// <param name="metadata">Document metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Document fingerprint for comparison</returns>
    Task<DocumentFingerprint> GenerateFingerprintAsync(
        byte[] documentContent, 
        DocumentMetadata metadata, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Divides document into logical sections for better processing
    /// </summary>
    /// <param name="extractedText">Extracted document text</param>
    /// <param name="documentType">Type of document</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Document sections with categorized content</returns>
    Task<IEnumerable<DocumentSection>> SectionizeDocumentAsync(
        string extractedText, 
        DocumentType documentType, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates embeddings for document content using AI
    /// </summary>
    /// <param name="textContent">Document text content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Vector embeddings for semantic search</returns>
    Task<float[]> GenerateEmbeddingsAsync(
        string textContent, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Detects document language for localization
    /// </summary>
    /// <param name="textContent">Document text content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detected language code (ISO 639-1)</returns>
    Task<string> DetectLanguageAsync(
        string textContent, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates document format and checks for corruption
    /// </summary>
    /// <param name="documentContent">Raw document content</param>
    /// <param name="mimeType">Document MIME type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if document is valid and processable</returns>
    Task<bool> ValidateDocumentAsync(
        byte[] documentContent, 
        string mimeType, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets supported document formats and MIME types
    /// </summary>
    /// <returns>List of supported MIME types and formats</returns>
    IEnumerable<string> GetSupportedFormats();
} 