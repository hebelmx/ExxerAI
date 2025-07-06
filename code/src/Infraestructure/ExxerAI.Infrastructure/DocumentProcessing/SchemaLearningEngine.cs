using ExxerAI.Domain.Entities;
using ExxerAI.Domain.ValueObjects;
using ExxerAI.Domain.Configurations;
using ExxerAI.Domain.Helpers;
using ExxerAI.Domain.DocumentProcessing;
using Microsoft.Extensions.Logging;

namespace ExxerAI.Infrastructure.DocumentProcessing;

/// <summary>
/// Engine responsible for learning document schemas and analyzing field patterns
/// </summary>
internal class SchemaLearningEngine
{
    private readonly ILogger<SchemaLearningEngine> _logger;

    /// <summary>
    /// Initializes a new instance of the SchemaLearningEngine class
    /// </summary>
    /// <param name="logger">Logger instance for structured logging</param>
    public SchemaLearningEngine(ILogger<SchemaLearningEngine> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initializes known schema definitions for common document types
    /// </summary>
    /// <returns>Dictionary of document types mapped to their schema definitions</returns>
    public Dictionary<DocumentType, SchemaDefinition> InitializeKnownSchemas()
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

    /// <summary>
    /// Gets an existing schema or creates a dynamic schema for the document type
    /// </summary>
    /// <param name="documentType">The document type to get schema for</param>
    /// <param name="extractedText">Sample extracted text for dynamic schema creation</param>
    /// <param name="knownSchemas">Dictionary of known schemas</param>
    /// <returns>Schema definition for the document type</returns>
    public SchemaDefinition GetOrCreateSchema(DocumentType documentType, string extractedText, Dictionary<DocumentType, SchemaDefinition> knownSchemas)
    {
        if (knownSchemas.TryGetValue(documentType, out var schema))
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

    /// <summary>
    /// Analyzes text patterns to identify common fields for a document type
    /// </summary>
    /// <param name="texts">Collection of text samples to analyze</param>
    /// <param name="documentType">The document type being analyzed</param>
    /// <returns>List of field definitions identified from the patterns</returns>
    public static List<FieldDefinition> AnalyzeFieldPatterns(IEnumerable<string> texts, DocumentType documentType)
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

    /// <summary>
    /// Extracts document type from document ID patterns
    /// </summary>
    /// <param name="documentId">The document identifier to analyze</param>
    /// <returns>Inferred document type based on ID patterns</returns>
    public static DocumentType ExtractDocumentTypeFromId(string documentId)
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

    /// <summary>
    /// Calculates confidence improvement based on historical processing results
    /// </summary>
    /// <param name="results">List of successful processing results to analyze</param>
    /// <returns>Calculated confidence improvement percentage</returns>
    public static float CalculateConfidenceImprovement(List<DocumentProcessingResult> results)
    {
        if (!results.Any()) return 0.0f;

        var averageConfidence = results.Average(r => r.OverallConfidence);
        return Math.Min(averageConfidence * 0.1f, 0.2f); // Cap improvement at 20%
    }
} 