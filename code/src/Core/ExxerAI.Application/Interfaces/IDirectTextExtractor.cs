using ExxerAI.Application.DTOs;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Extracts text directly from digital documents without OCR
/// </summary>
public interface IDirectTextExtractor
{
    /// <summary>
    /// Extracts text directly from digital document data
    /// </summary>
    /// <param name="documentData">The document byte data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Direct text extraction result</returns>
    Task<DirectTextResult> ExtractTextAsync(byte[] documentData, CancellationToken cancellationToken = default);
} 