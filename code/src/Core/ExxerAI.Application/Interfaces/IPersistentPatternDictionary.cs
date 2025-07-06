using ExxerAI.Application.DTOs;
using ExxerAI.Domain.DocumentProcessing;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Database-persisted pattern dictionary system based on KpiExxerpro field extraction patterns
/// Supports continuous learning and pattern evolution based on successful extractions
/// Maintains audit trail and confidence scoring for all pattern variations
/// </summary>
public interface IPersistentPatternDictionary
{
    /// <summary>
    /// Retrieves extraction patterns for a specific field and document type
    /// Returns patterns ordered by confidence score (highest first)
    /// </summary>
    /// <param name="fieldName">Name of the field to extract (e.g., "registro_patronal")</param>
    /// <param name="documentType">Type of document (e.g., "IMSSPayment")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of extraction patterns ordered by confidence</returns>
    Task<List<ExtractionPattern>> GetPatternsForFieldAsync(string fieldName, string documentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all extraction patterns for a specific document type
    /// Groups patterns by field name for efficient batch processing
    /// </summary>
    /// <param name="documentType">Type of document to get patterns for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary mapping field names to their extraction patterns</returns>
    Task<Dictionary<string, List<ExtractionPattern>>> GetPatternsForDocumentTypeAsync(string documentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates pattern success statistics based on successful field extraction
    /// Implements continuous learning to improve pattern confidence scores
    /// </summary>
    /// <param name="learningResult">Result of pattern learning analysis</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the update operation</returns>
    Task UpdatePatternFromSuccessfulExtractionAsync(PatternLearningResult learningResult, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the evolution history of patterns for a specific field
    /// Useful for analyzing pattern effectiveness over time
    /// </summary>
    /// <param name="fieldName">Field name to get history for</param>
    /// <param name="fromDate">Start date for history retrieval</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Pattern evolution history with confidence changes</returns>
    Task<PatternEvolutionHistory> GetPatternEvolutionHistoryAsync(string fieldName, DateTime fromDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current confidence score for a specific pattern
    /// Based on success rate and usage statistics
    /// </summary>
    /// <param name="patternId">Unique identifier of the pattern</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current confidence score (0.0 to 1.0)</returns>
    Task<float> GetPatternConfidenceScoreAsync(string patternId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new extraction pattern discovered through learning
    /// Validates pattern before adding to prevent duplicates and conflicts
    /// </summary>
    /// <param name="pattern">New extraction pattern to persist</param>
    /// <param name="initialConfidence">Initial confidence score for the pattern</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Unique identifier of the persisted pattern</returns>
    Task<string> PersistNewPatternAsync(ExtractionPattern pattern, float initialConfidence, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates patterns that consistently fail to extract correct data
    /// Implements pattern quality management to maintain system accuracy
    /// </summary>
    /// <param name="fieldName">Field name to analyze</param>
    /// <param name="documentType">Document type to analyze</param>
    /// <param name="minimumSuccessRate">Minimum success rate to keep pattern active</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of patterns deactivated</returns>
    Task<int> DeactivateUnderperformingPatternsAsync(string fieldName, string documentType, float minimumSuccessRate = 0.5f, CancellationToken cancellationToken = default);

    /// <summary>
    /// Seeds the pattern dictionary with proven KpiExxerpro patterns
    /// Used for initial system setup with battle-tested extraction patterns
    /// </summary>
    /// <param name="seedPatterns">Collection of proven patterns to seed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of patterns successfully seeded</returns>
    Task<int> SeedKpiExxerproPatternsAsync(IEnumerable<PatternDictionaryEntity> seedPatterns, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports pattern dictionary for backup or transfer to other systems
    /// Includes full pattern metadata and usage statistics
    /// </summary>
    /// <param name="documentType">Optional document type filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Serializable pattern dictionary export</returns>
    Task<PatternDictionaryExport> ExportPatternDictionaryAsync(string? documentType = null, CancellationToken cancellationToken = default);
}