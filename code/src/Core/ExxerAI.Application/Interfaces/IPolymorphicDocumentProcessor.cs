using ExxerAI.Domain.DocumentProcessing;
using ExxerAI.Domain.Operations;

// For Result<>

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Advanced polymorphic document processor that learns to adapt to any document type.
/// Implements multi-stage processing: Direct Read → OCR → LLM Verification → Grounding.
/// Based on proven KpiExxerpro patterns with 15+ years of financial document processing.
/// </summary>
public interface IPolymorphicDocumentProcessor
{
    /// <summary>
    /// Processes a document through the complete polymorphic pipeline.
    /// </summary>
    /// <param name="documentData">The raw document byte data.</param>
    /// <param name="metadata">Document metadata and processing options.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The complete processing result with extracted data and confidence scores.</returns>
    Task<Result<DocumentProcessingResult>> ProcessDocumentAsync(
        byte[] documentData,
        DocumentMetadata metadata,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts specific fields from a document using adaptive patterns.
    /// </summary>
    /// <param name="document">The document to extract fields from.</param>
    /// <param name="schema">The extraction schema defining expected fields.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The field extraction results with confidence scores.</returns>
    Task<Result<ExtractionResult>> ExtractFieldsAsync(
        Document document,
        ExtractionSchema schema,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates and grounds extracted data against known patterns and business rules.
    /// </summary>
    /// <param name="data">The extracted data to validate.</param>
    /// <param name="context">The ground truth context for validation.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The validation result with grounded data.</returns>
    Task<Result<ValidationResultDocument>> ValidateAndGroundDataAsync(
        ExtractedData data,
        GroundTruthContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adapts processing rules based on processing history and feedback.
    /// </summary>
    /// <param name="history">The processing history to learn from.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The learning result with updated processing rules.</returns>
    Task<Result<LearningResult>> AdaptProcessingRulesAsync(
        ProcessingHistory history,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Learns document schema from sample documents to improve future processing.
    /// </summary>
    /// <param name="samples">Sample documents to learn from.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The learned schema definition.</returns>
    Task<Result<SchemaDefinition>> LearnDocumentSchemaAsync(
        IEnumerable<Document> samples,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the confidence score for processing a specific document type
    /// </summary>
    /// <param name="documentType">The document type to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The confidence score for processing this document type</returns>
    Task<Result<float>> GetProcessingConfidenceAsync(
        DocumentType documentType,
        CancellationToken cancellationToken = default);
}