using ExxerAI.Application.DTOs;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Processes documents using OCR technology
/// </summary>
public interface IOCRProcessor
{
    /// <summary>
    /// Processes document data using OCR with specified language
    /// </summary>
    /// <param name="documentData">The document byte data</param>
    /// <param name="language">OCR language code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>OCR processing result</returns>
    Task<OCRResult> ProcessDocumentWithOCRAsync(byte[] documentData, string language, CancellationToken cancellationToken = default);
} 