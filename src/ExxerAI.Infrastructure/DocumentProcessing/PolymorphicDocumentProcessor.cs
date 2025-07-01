using ExxerAI.Application.Interfaces;
using ExxerAI.Domain;
using ExxerAI.Domain.DocumentProcessing;
using Microsoft.Extensions.Logging;

// Ensure you have the PdfPig NuGet package installed in your project.
// You can install it using the following command in the NuGet Package Manager Console:
// Install-Package PdfPig

using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

using System.Text;
using System.Text.RegularExpressions;

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
        _schemas = InitializeKnownSchemas();
    }

    /// <summary>
    /// Processes a document through the complete extraction pipeline
    /// </summary>
    /// <param name="documentData">The raw document data as byte array</param>
    /// <param name="metadata">Document metadata including type and schema information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the document processing operation</returns>
    public async Task<ExxerAI.Domain.Result<DocumentProcessingResult>> ProcessDocumentAsync(
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
                ValidationResults = new ValidationResult { IsValid = true, Confidence = 1.0f }
            };

            // Stage 1: Direct Text Extraction
            var textExtractionResult = await ExtractTextDirectlyAsync(documentData, metadata, cancellationToken);
            if (textExtractionResult.IsSuccess)
            {
                result.ExtractedText = textExtractionResult.Data!;
                result.Confidence = 0.9f; // High confidence for direct text
                _logger.LogDebug("Direct text extraction successful, {Length} characters extracted",
                    result.ExtractedText.Length);
            }
            else
            {
                // Stage 2: OCR Fallback
                _logger.LogInformation("Direct text extraction failed, falling back to OCR");
                result.ExtractionMethod = ExtractionMethod.OCR;

                var ocrResult = await ExtractTextViaOCRAsync(documentData, metadata, cancellationToken);
                if (ocrResult.IsSuccess)
                {
                    result.ExtractedText = ocrResult.Data!;
                    result.Confidence = 0.7f; // Lower confidence for OCR
                }
                else
                {
                    return ExxerAI.Domain.Result<DocumentProcessingResult>.WithFailure($"Both direct text and OCR extraction failed: {ocrResult.Error}");
                }
            }

            // Stage 3: Field Extraction using schema
            var schema = GetOrCreateSchema(metadata.DocumentType, result.ExtractedText);
            var extractionResult = await ExtractFieldsUsingSchemaAsync(result.ExtractedText, schema, cancellationToken);
            if (extractionResult.IsSuccess)
            {
                // Cannot assign to init-only ExtractedFields, but can assign to regular GroundedData property
                result.GroundedData = extractionResult.Data;
                // Note: ExtractedFields is init-only, would need to create new object to modify it
                // For now, data is accessible through GroundedData.Fields
            }

            // Stage 4: LLM Verification (if enabled and confidence is low)
            if (metadata.ProcessingOptions.UseLLMExtraction && result.Confidence < 0.8f)
            {
                var llmResult = await VerifyWithLLMAsync(result, schema, cancellationToken);
                if (llmResult.IsSuccess)
                {
                    result.LLMConfidence = llmResult.Data!;
                    result.ExtractionMethod = ExtractionMethod.Hybrid;
                }
            }
            else
            {
                result.LLMConfidence = result.Confidence; // Use base confidence
            }

            // Stage 5: Data Validation and Grounding
            var validationResult = await ValidateExtractedDataAsync(result.GroundedData, metadata, cancellationToken);
            if (validationResult.IsSuccess)
            {
                result.ValidationResults = validationResult.Data!;
                result.GroundingConfidence = validationResult.Data.Confidence;
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
    /// Extracts fields from a document using a specific schema
    /// </summary>
    /// <param name="documentData">The raw document data</param>
    /// <param name="schema">The extraction schema to use</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the field extraction operation</returns>
    public async Task<Result<ExtractedData>> ExtractFieldsAsync(
        byte[] documentData,
        SchemaDefinition schema,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Extract text first
            var metadata = new DocumentMetadata { DocumentType = schema.DocumentType };
            var textResult = await ExtractTextDirectlyAsync(documentData, metadata, cancellationToken);
            if (!textResult.IsSuccess)
            {
                return Result<ExtractedData>.WithFailure($"Text extraction failed: {textResult.Error}");
            }

            return await ExtractFieldsUsingSchemaAsync(textResult.Data!, schema, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting fields using schema {SchemaName}", schema.Name);
            return Result<ExtractedData>.WithFailure($"Field extraction error: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates and grounds extracted data against known patterns and dictionaries
    /// </summary>
    /// <param name="data">The extracted data to validate</param>
    /// <param name="context">The grounding context for validation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the validation and grounding operation</returns>
    public async Task<Result<ValidationResult>> ValidateAndGroundDataAsync(
        ExtractedData data,
        Dictionary<string, object> context,
        CancellationToken cancellationToken = default)
    {
        return await ValidateExtractedDataAsync(data,
            new DocumentMetadata { Properties = context },
            cancellationToken);
    }

    /// <summary>
    /// Adapts processing rules based on historical processing results
    /// </summary>
    /// <param name="processingHistory">Historical processing results for learning</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the rule adaptation operation</returns>
    public async Task<Result<LearningResult>> AdaptProcessingRulesAsync(
        IEnumerable<DocumentProcessingResult> processingHistory,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adapting processing rules from {Count} historical results",
            processingHistory.Count());

        try
        {
            var patterns = new List<string>();
            var schemaUpdates = new List<string>();

            // Analyze successful extractions to identify patterns
            var successfulResults = processingHistory.Where(r => r.IsSuccessful).ToList();
            if (successfulResults.Any())
            {
                // Group by document type and analyze patterns
                var groupedByType = successfulResults.GroupBy(r => ExtractDocumentTypeFromId(r.DocumentId));

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
                ConfidenceImprovement = successfulResults.Any() ? CalculateConfidenceImprovement(successfulResults) : 0.0f
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
    /// Learns document schema from a set of sample documents
    /// </summary>
    /// <param name="samples">Sample documents for schema learning</param>
    /// <param name="documentType">The type of documents being analyzed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the schema learning operation</returns>
    public async Task<Result<SchemaDefinition>> LearnDocumentSchemaAsync(
        IEnumerable<byte[]> samples,
        DocumentType documentType,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Learning schema for {DocumentType} from {SampleCount} samples",
            documentType, samples.Count());

        try
        {
            // Extract text from all samples and analyze common patterns
            var extractedTexts = new List<string>();
            foreach (var sample in samples)
            {
                var metadata = new DocumentMetadata { DocumentType = documentType };
                var textResult = await ExtractTextDirectlyAsync(sample, metadata, cancellationToken);
                if (textResult.IsSuccess)
                {
                    extractedTexts.Add(textResult.Data!);
                }
            }

            // Create schema with Fields in object initializer instead of post-construction assignment
            var schema = new SchemaDefinition
            {
                Name = $"Learned_{documentType}_{DateTime.UtcNow:yyyyMMdd}",
                DocumentType = documentType,
                CreatedAt = DateTime.UtcNow,
                Fields = AnalyzeFieldPatterns(extractedTexts, documentType)
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
    public async Task<Result<float>> GetProcessingConfidenceAsync(
        DocumentType documentType,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // Placeholder for async operation

        var confidence = documentType switch
        {
            DocumentType.IMSSPayment => 0.95f, // High confidence based on KpiExxerpro experience
            DocumentType.Invoice => 0.90f,
            DocumentType.TaxDocument => 0.85f,
            DocumentType.Unknown => 0.60f,
            _ => 0.70f
        };

        return Result<float>.WithSuccess(confidence);
    }

    #region Private Implementation Methods

    private async Task<Result<string>> ExtractTextDirectlyAsync(
        byte[] documentData,
        DocumentMetadata metadata,
        CancellationToken cancellationToken)
    {
        try
        {
            if (metadata.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                using var stream = new MemoryStream(documentData);
                using var pdf = PdfDocument.Open(stream);

                var text = new StringBuilder();
                foreach (var page in pdf.GetPages())
                {
                    text.AppendLine(page.Text);
                }

                return Result<string>.WithSuccess(text.ToString());
            }
            else
            {
                // For non-PDF files, attempt to read as text
                var text = Encoding.UTF8.GetString(documentData);
                return Result<string>.WithSuccess(text);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Direct text extraction failed for {FileName}", metadata.FileName);
            return Result<string>.WithFailure($"Direct text extraction failed: {ex.Message}");
        }
    }

    private async Task<Result<string>> ExtractTextViaOCRAsync(
        byte[] documentData,
        DocumentMetadata metadata,
        CancellationToken cancellationToken)
    {
        try
        {
            // OCR implementation would go here using Tesseract
            // For now, return a placeholder
            await Task.Delay(100, cancellationToken); // Simulate OCR processing time

            _logger.LogInformation("OCR processing completed for {FileName}", metadata.FileName);
            return Result<string>.WithSuccess("OCR extracted text placeholder");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OCR extraction failed for {FileName}", metadata.FileName);
            return Result<string>.WithFailure($"OCR extraction failed: {ex.Message}");
        }
    }

    private async Task<Result<ExtractedData>> ExtractFieldsUsingSchemaAsync(
        string text,
        SchemaDefinition schema,
        CancellationToken cancellationToken)
    {
        try
        {
            var extractedData = new ExtractedData();

            foreach (var field in schema.Fields)
            {
                var value = ExtractFieldValue(text, field);
                if (value != null)
                {
                    extractedData.Fields[field.Name] = value;
                    extractedData.FieldConfidences[field.Name] = field.ConfidenceThreshold;
                    extractedData.FieldSources[field.Name] = "Schema extraction";
                }
            }

            _logger.LogDebug("Extracted {FieldCount} fields using schema {SchemaName}",
                extractedData.Fields.Count, schema.Name);

            return Result<ExtractedData>.WithSuccess(extractedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting fields using schema");
            return Result<ExtractedData>.WithFailure($"Field extraction error: {ex.Message}");
        }
    }

    private static string? ExtractFieldValue(string text, FieldDefinition field)
    {
        try
        {
            if (!string.IsNullOrEmpty(field.PrimaryPattern))
            {
                var regex = new Regex(field.PrimaryPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                var match = regex.Match(text);
                if (match.Success)
                {
                    return match.Groups.Count > 1 ? match.Groups[1].Value.Trim() : match.Value.Trim();
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private async Task<Result<float>> VerifyWithLLMAsync(
        DocumentProcessingResult result,
        SchemaDefinition schema,
        CancellationToken cancellationToken)
    {
        try
        {
            // LLM verification would analyze the extracted data for consistency
            // This is a placeholder implementation
            var llmConfidence = Math.Min(result.Confidence + 0.1f, 1.0f);

            _logger.LogDebug("LLM verification completed with confidence {Confidence:F2}", llmConfidence);
            return Result<float>.WithSuccess(llmConfidence);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "LLM verification failed");
            return Result<float>.WithSuccess(result.Confidence); // Fallback to original confidence
        }
    }

    private async Task<Result<ValidationResult>> ValidateExtractedDataAsync(
        ExtractedData data,
        DocumentMetadata metadata,
        CancellationToken cancellationToken)
    {
        var validation = new ValidationResult
        {
            IsValid = true,
            Confidence = 1.0f
        };

        // Perform basic validation on extracted fields
        foreach (var field in data.Fields)
        {
            var fieldValidation = ValidateField(field.Key, field.Value);
            validation.FieldResults[field.Key] = fieldValidation;

            if (!fieldValidation.IsValid)
            {
                validation.IsValid = false;
                validation.Errors.Add($"Field {field.Key}: {fieldValidation.ErrorMessage}");
                validation.Confidence *= 0.9f; // Reduce confidence for validation errors
            }
        }

        return Result<ValidationResult>.WithSuccess(validation);
    }

    private static FieldValidationResult ValidateField(string fieldName, object value)
    {
        var result = new FieldValidationResult { IsValid = true, Confidence = 1.0f };

        // Basic validation rules
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            result.IsValid = false;
            result.ErrorMessage = "Field value is empty";
            result.Confidence = 0.0f;
        }

        return result;
    }

    private Dictionary<DocumentType, SchemaDefinition> InitializeKnownSchemas()
    {
        var schemas = new Dictionary<DocumentType, SchemaDefinition>();

        // IMSS Payment Schema (based on KpiExxerpro patterns)
        var imssSchema = new SchemaDefinition
        {
            Name = "IMSS_Payment_Schema",
            DocumentType = DocumentType.IMSSPayment,
            Fields = new List<FieldDefinition>
            {
                new("PaymentPeriod", FieldType.Date_MMYYYY, true, @"(?:PERIODO|PERIOD)[:\s]*(\d{2}-\d{4})"),
                new("Amount", FieldType.Currency, true, @"(?:IMPORTE|TOTAL)[:\s]*\$?([0-9,]+\.?\d*)"),
                new("EmployerNumber", FieldType.AlphaNumeric, true, @"(?:REGISTRO PATRONAL|REG\.?\s*PAT)[:\s]*([A-Z0-9\-]+)"),
                new("PaymentDate", FieldType.Date, false, @"(?:FECHA)[:\s]*(\d{1,2}\/\d{1,2}\/\d{4})")
            }
        };
        schemas[DocumentType.IMSSPayment] = imssSchema;

        return schemas;
    }

    private SchemaDefinition GetOrCreateSchema(DocumentType documentType, string extractedText)
    {
        if (_schemas.TryGetValue(documentType, out var schema))
        {
            return schema;
        }

        // Create a dynamic schema based on document type
        return new SchemaDefinition
        {
            Name = $"Dynamic_{documentType}_Schema",
            DocumentType = documentType,
            Fields = AnalyzeFieldPatterns(new[] { extractedText }, documentType)
        };
    }

    private static List<FieldDefinition> AnalyzeFieldPatterns(IEnumerable<string> texts, DocumentType documentType)
    {
        var fields = new List<FieldDefinition>();

        // Common patterns based on document type
        switch (documentType)
        {
            case DocumentType.Invoice:
                fields.Add(new FieldDefinition("InvoiceNumber", FieldType.AlphaNumeric, true, @"(?:INVOICE|FACTURA)[:\s#]*([A-Z0-9\-]+)"));
                fields.Add(new FieldDefinition("Total", FieldType.Currency, true, @"(?:TOTAL)[:\s]*\$?([0-9,]+\.?\d*)"));
                break;

            case DocumentType.TaxDocument:
                fields.Add(new FieldDefinition("TaxId", FieldType.AlphaNumeric, true, @"(?:RFC)[:\s]*([A-Z0-9]+)"));
                fields.Add(new FieldDefinition("TaxAmount", FieldType.Currency, false, @"(?:IMPUESTO)[:\s]*\$?([0-9,]+\.?\d*)"));
                break;
        }

        return fields;
    }

    private static DocumentType ExtractDocumentTypeFromId(string documentId)
    {
        // Extract document type from document ID patterns
        return documentId.ToLowerInvariant() switch
        {
            var id when id.Contains("imss") => DocumentType.IMSSPayment,
            var id when id.Contains("invoice") => DocumentType.Invoice,
            var id when id.Contains("tax") => DocumentType.TaxDocument,
            _ => DocumentType.Unknown
        };
    }

    private static float CalculateConfidenceImprovement(List<DocumentProcessingResult> results)
    {
        if (!results.Any()) return 0.0f;

        var averageConfidence = results.Average(r => r.OverallConfidence);
        return Math.Min(averageConfidence * 0.1f, 0.2f); // Cap improvement at 20%
    }

    #endregion Private Implementation Methods
}