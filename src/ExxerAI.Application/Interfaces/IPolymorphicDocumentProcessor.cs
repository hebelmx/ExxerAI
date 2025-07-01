using ExxerAI.Domain;
using ExxerAI.Domain.DocumentProcessing;
using FluentResults;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Interface for advanced polymorphic document processor that learns to adapt to any document type
/// Implements multi-stage processing: Direct Read → OCR → LLM Verification → Grounding
/// Based on proven KpiExxerpro patterns with 15+ years of financial document processing
/// </summary>
public interface IPolymorphicDocumentProcessor
{
    /// <summary>
    /// Processes a document through the complete extraction pipeline
    /// </summary>
    /// <param name="documentData">The raw document data as byte array</param>
    /// <param name="metadata">Document metadata including type and schema information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the document processing operation</returns>
    Task<Result<DocumentProcessingResult>> ProcessDocumentAsync(
        byte[] documentData, 
        DocumentMetadata metadata, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts fields from a document using a specific schema
    /// </summary>
    /// <param name="documentData">The raw document data</param>
    /// <param name="schema">The extraction schema to use</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the field extraction operation</returns>
    Task<Result<ExtractedData>> ExtractFieldsAsync(
        byte[] documentData, 
        SchemaDefinition schema, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates and grounds extracted data against known patterns and dictionaries
    /// </summary>
    /// <param name="data">The extracted data to validate</param>
    /// <param name="context">The grounding context for validation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the validation and grounding operation</returns>
    Task<Result<ValidationResult>> ValidateAndGroundDataAsync(
        ExtractedData data, 
        Dictionary<string, object> context, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adapts processing rules based on historical processing results
    /// </summary>
    /// <param name="processingHistory">Historical processing results for learning</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the rule adaptation operation</returns>
    Task<Result<LearningResult>> AdaptProcessingRulesAsync(
        IEnumerable<DocumentProcessingResult> processingHistory, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Learns document schema from a set of sample documents
    /// </summary>
    /// <param name="samples">Sample documents for schema learning</param>
    /// <param name="documentType">The type of documents being analyzed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the schema learning operation</returns>
    Task<Result<SchemaDefinition>> LearnDocumentSchemaAsync(
        IEnumerable<byte[]> samples, 
        DocumentType documentType, 
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