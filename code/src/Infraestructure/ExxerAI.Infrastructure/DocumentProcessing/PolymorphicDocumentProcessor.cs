using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Collections.Concurrent;
using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.Entities;
using ExxerAI.Domain.ValueObjects;
using ExxerAI.Domain.Configurations;
using ExxerAI.Domain.DocumentProcessing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
// PDF processing (temporarily commented out until PdfPig package is properly resolved)
// using PdfPig;
// using PdfPig.DocumentLayoutAnalysis.TextExtractor;

using System.Text;
using System.Text.RegularExpressions;
using ExxerAI.Domain.Operations;

namespace ExxerAI.Infrastructure.DocumentProcessing;

/// <summary>
/// Implementation of polymorphic document processor that adapts to any document type
/// Uses multi-stage processing: Direct Text → OCR → LLM Verification → Grounding
/// Implements KpiExxerpro patterns for financial document processing
/// </summary>
public class PolymorphicDocumentProcessor : IPolymorphicDocumentProcessor
{
    private readonly ILLMService _llmService;
    private readonly ILogger<PolymorphicDocumentProcessor> _logger;
    private readonly Dictionary<DocumentType, SchemaDefinition> _schemas;
    private readonly TextExtractionEngine _textExtractor;
    private readonly FieldExtractionEngine _fieldExtractor;
    private readonly ValidationEngine _validator;
    private readonly SchemaLearningEngine _learner;

    /// <summary>
    /// Initializes a new instance of the PolymorphicDocumentProcessor class
    /// </summary>
    /// <param name="llmService">The LLM service for advanced processing</param>
    /// <param name="logger">The logger</param>
    public PolymorphicDocumentProcessor(
        ILLMService llmService,
        ILogger<PolymorphicDocumentProcessor> logger)
    {
        _llmService = llmService;
        _logger = logger;
        
        // Initialize focused engine components
        var loggerFactory = Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance;
        _textExtractor = new TextExtractionEngine(loggerFactory.CreateLogger<TextExtractionEngine>());
        _fieldExtractor = new FieldExtractionEngine(loggerFactory.CreateLogger<FieldExtractionEngine>());
        _validator = new ValidationEngine(loggerFactory.CreateLogger<ValidationEngine>());
        _learner = new SchemaLearningEngine(loggerFactory.CreateLogger<SchemaLearningEngine>());
        
        _schemas = _learner.InitializeKnownSchemas();
    }

    /// <summary>
    /// Processes a document through the complete extraction pipeline
    /// </summary>
    /// <param name="documentData">The raw document data as byte array</param>
    /// <param name="metadata">Document metadata including type and schema information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the document processing operation</returns>
    public async Task<Result<DocumentProcessingResult>> ProcessDocumentAsync(
        byte[] documentData,
        DocumentMetadata metadata,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _logger.LogInformation("Starting document processing for {DocumentType}", metadata.DocumentType);

            // Create initial result object
            var result = new DocumentProcessingResult
            {
                DocumentId = metadata.FileName ?? "Unknown",
                ExtractionMethod = ExtractionMethod.DirectText,
                ExtractedFields = new Dictionary<string, object>(),
                ValidationResultDocument = new ValidationResultDocument { IsValid = true, Confidence = 1.0f }
            };

            // Stage 1: Direct Text Extraction
            var textExtractionResult = await _textExtractor.ExtractTextDirectlyAsync(documentData, metadata, cancellationToken);
            if (textExtractionResult.IsSuccess)
            {
                result.ExtractedText = textExtractionResult.Value!;
                result.Confidence = 0.9f; // High confidence for direct text
                _logger.LogDebug("Direct text extraction successful, {Length} characters extracted",
                    result.ExtractedText.Length);
            }
            else
            {
                // Stage 2: OCR Fallback
                _logger.LogInformation("Direct text extraction failed, falling back to OCR");
                result.ExtractionMethod = ExtractionMethod.OCR;

                var ocrResult = await _textExtractor.ExtractTextViaOCRAsync(documentData, metadata, cancellationToken);
                if (ocrResult.IsSuccess)
                {
                    result.ExtractedText = ocrResult.Value!;
                    result.Confidence = 0.7f; // Lower confidence for OCR
                }
                else
                {
                    return Result<DocumentProcessingResult>.WithFailure($"Both direct text and OCR extraction failed: {ocrResult.Error}");
                }
            }

            // Stage 3: Field Extraction using schema
            var schema = _learner.GetOrCreateSchema(metadata.DocumentType, result.ExtractedText, _schemas);
            var extractionResult = await _fieldExtractor.ExtractFieldsUsingSchemaAsync(result.ExtractedText, schema, cancellationToken);
            if (extractionResult.IsSuccess)
            {
                // Cannot assign to init-only ExtractedFields, but can assign to regular GroundedData property
                result.GroundedData = extractionResult.Value;
                // Note: ExtractedFields is init-only, would need to create new object to modify it
                // For now, data is accessible through GroundedData.Fields
            }

            // Stage 4: LLM Verification (if enabled and confidence is low)
            if (metadata.ProcessingOptions.UseLLMExtraction && result.Confidence < 0.8f)
            {
                var llmResult = await _validator.VerifyWithLLMAsync(result, schema, cancellationToken);
                if (llmResult.IsSuccess)
                {
                    result.LLMConfidence = llmResult.Value!;
                    result.ExtractionMethod = ExtractionMethod.Hybrid;
                }
            }
            else
            {
                result.LLMConfidence = result.Confidence; // Use base confidence
            }

            // Stage 5: Value Validation and Grounding
            var validationResult = await _validator.ValidateExtractedDataAsync(result.GroundedData, metadata, cancellationToken);
            if (validationResult.IsSuccess)
            {
                result.ValidationResultDocument = validationResult.Value!;
                result.GroundingConfidence = validationResult.Value.Confidence;
            }

            stopwatch.Stop();
            result.ProcessingTimeMs = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation("Document processing completed in {ProcessingTime}ms with overall confidence {Confidence:F2}",
                result.ProcessingTimeMs, result.OverallConfidence);

            return Result<DocumentProcessingResult>.WithSuccess(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during document processing");
            return Result<DocumentProcessingResult>.WithFailure($"Processing error: {ex.Message}");
        }
    }

    /// <summary>
    /// Extracts specific fields from a document using adaptive patterns.
    /// </summary>
    /// <param name="document">The document to extract fields from.</param>
    /// <param name="schema">The extraction schema defining expected fields.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The field extraction results with confidence scores.</returns>
    public async Task<Result<ExtractionResult>> ExtractFieldsAsync(
        Document document,
        ExtractionSchema schema,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Convert Document to bytes and process
            var documentData = document.Content;
            var metadata = new DocumentMetadata 
            { 
                DocumentType = document.Type,
                FileName = document.FileName
            };
            
            var textResult = await _textExtractor.ExtractTextDirectlyAsync(documentData, metadata, cancellationToken);
            if (!textResult.IsSuccess)
            {
                return Result<ExtractionResult>.WithFailure($"Text extraction failed: {textResult.Error}");
            }

            // Convert ExtractionSchema to SchemaDefinition if needed
            var schemaDefinition = new SchemaDefinition
            {
                Name = schema.Name,
                DocumentType = schema.DocumentType,
                Fields = schema.Fields.ToList() // ExtractionSchema.Fields is already List<FieldDefinition>
            };

            var extractedDataResult = await _fieldExtractor.ExtractFieldsUsingSchemaAsync(textResult.Value!, schemaDefinition, cancellationToken);
            if (!extractedDataResult.IsSuccess)
            {
                return Result<ExtractionResult>.WithFailure(extractedDataResult.Error);
            }

            // Convert ExtractedData to ExtractionResult
            var extractionResult = new ExtractionResult
            {
                Confidence = extractedDataResult.Value!.OverallConfidence,
                ExtractedAt = DateTime.UtcNow
            };

            // Add extracted fields to the result
            foreach (var field in extractedDataResult.Value.Fields)
            {
                extractionResult.AddField(field.Key, field.Value, extractedDataResult.Value.FieldConfidences.GetValueOrDefault(field.Key, 0.8f));
            }

            return Result<ExtractionResult>.WithSuccess(extractionResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting fields using schema {SchemaName}", schema.Name);
            return Result<ExtractionResult>.WithFailure($"Field extraction error: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates and grounds extracted data against known patterns and business rules.
    /// </summary>
    /// <param name="data">The extracted data to validate.</param>
    /// <param name="context">The ground truth context for validation.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The validation result with grounded data.</returns>
    public async Task<Result<ValidationResultDocument>> ValidateAndGroundDataAsync(
        ExtractedData data,
        GroundTruthContext context,
        CancellationToken cancellationToken = default)
    {
        return await _validator.ValidateExtractedDataAsync(data,
            new DocumentMetadata { Properties = context.Properties },
            cancellationToken);
    }

    /// <summary>
    /// Adapts processing rules based on processing history and feedback.
    /// </summary>
    /// <param name="history">The processing history to learn from.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The learning result with updated processing rules.</returns>
    public async Task<Result<LearningResult>> AdaptProcessingRulesAsync(
        ProcessingHistory history,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adapting processing rules from {Count} historical results",
            history.ProcessingResults.Count());

        try
        {
            await Task.Yield(); // Allow cooperative cancellation
            
            var patterns = new List<string>();
            var schemaUpdates = new List<string>();

            // Analyze successful extractions to identify patterns
            var successfulResults = history.ProcessingResults.Where(r => r.IsSuccessful).ToList();
            if (successfulResults.Any())
            {
                // Group by document type and analyze patterns
                var groupedByType = successfulResults.GroupBy(r => SchemaLearningEngine.ExtractDocumentTypeFromId(r.DocumentId));

                foreach (var group in groupedByType)
                {
                    var typePattern = $"Learned {group.Count()} successful patterns for {group.Key}";
                    patterns.Add(typePattern);
                    schemaUpdates.Add($"Enhanced schema for {group.Key}");
                }
            }

            // Create LearningResult with object initializer instead of post-construction assignment
            var learningResult = new LearningResult
            {
                PatternsLearned = successfulResults.Any(),
                NewPatterns = patterns,
                SchemaUpdates = schemaUpdates,
                ConfidenceImprovement = successfulResults.Any() ? SchemaLearningEngine.CalculateConfidenceImprovement(successfulResults) : 0.0f
            };

            _logger.LogInformation("Rule adaptation completed, learned {PatternCount} new patterns",
                patterns.Count);

            return Result<LearningResult>.WithSuccess(learningResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adapting processing rules");
            return Result<LearningResult>.WithFailure($"Rule adaptation error: {ex.Message}");
        }
    }

    /// <summary>
    /// Learns document schema from sample documents to improve future processing.
    /// </summary>
    /// <param name="samples">Sample documents to learn from.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The learned schema definition.</returns>
    public async Task<Result<SchemaDefinition>> LearnDocumentSchemaAsync(
        IEnumerable<Document> samples,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Learning schema from {SampleCount} samples", samples.Count());

        try
        {
            // Extract text from all samples and analyze common patterns
            var extractedTexts = new List<string>();
            var documentType = samples.FirstOrDefault()?.Type ?? DocumentType.Unknown;
            
            foreach (var sample in samples)
            {
                var metadata = new DocumentMetadata 
                { 
                    DocumentType = sample.Type,
                    FileName = sample.FileName
                };
                var textResult = await _textExtractor.ExtractTextDirectlyAsync(sample.Content, metadata, cancellationToken);
                if (textResult.IsSuccess)
                {
                    extractedTexts.Add(textResult.Value!);
                }
            }

            // Create schema with Fields in object initializer instead of post-construction assignment
            var schema = new SchemaDefinition
            {
                Name = $"Learned_{documentType}_{DateTime.UtcNow:yyyyMMdd}",
                DocumentType = documentType,
                CreatedAt = DateTime.UtcNow,
                Fields = SchemaLearningEngine.AnalyzeFieldPatterns(extractedTexts, documentType)
            };

            _logger.LogInformation("Schema learning completed, discovered {FieldCount} fields",
                schema.Fields.Count);

            return Result<SchemaDefinition>.WithSuccess(schema);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error learning document schema");
            return Result<SchemaDefinition>.WithFailure($"Schema learning error: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the confidence score for processing a specific document type
    /// </summary>
    /// <param name="documentType">The document type to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The confidence score for processing this document type</returns>
    public Task<Result<float>> GetProcessingConfidenceAsync(
        DocumentType documentType,
        CancellationToken cancellationToken = default)
    {
        var confidence = documentType switch
        {
            DocumentType.IMSSPayment => 0.95f, // High confidence based on KpiExxerpro experience
            DocumentType.Invoice => 0.90f,
            DocumentType.TaxDocument => 0.85f,
            DocumentType.Unknown => 0.60f,
            _ => 0.70f
        };

        return Task.FromResult(Result<float>.WithSuccess(confidence));
    }
}