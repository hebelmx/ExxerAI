using ExxerAI.Application.DTOs;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Extracts specific fields from document regions using OCR
/// </summary>
public interface IRegionSpecificOCR
{
    /// <summary>
    /// Extracts key fields from specific document regions
    /// </summary>
    /// <param name="documentData">The document byte data</param>
    /// <param name="fieldNames">List of field names to extract</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Region-specific extraction results</returns>
    Task<List<RegionExtractionResult>> ExtractKeyFieldsByRegionAsync(byte[] documentData, List<string> fieldNames, CancellationToken cancellationToken = default);
} 