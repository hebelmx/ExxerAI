using ExxerAI.Domain;
using ExxerAI.Domain.DocumentProcessing;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace ExxerAI.Infrastructure.DocumentProcessing;

/// <summary>
/// Engine responsible for extracting specific fields from documents using schema definitions
/// </summary>
internal class FieldExtractionEngine
{
    private readonly ILogger<FieldExtractionEngine> _logger;

    /// <summary>
    /// Initializes a new instance of the FieldExtractionEngine class
    /// </summary>
    /// <param name="logger">Logger instance for structured logging</param>
    public FieldExtractionEngine(ILogger<FieldExtractionEngine> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Extracts fields from text using a schema definition
    /// </summary>
    /// <param name="text">The extracted text to analyze</param>
    /// <param name="schema">Schema definition with field extraction patterns</param>
    /// <param name="cancellationToken">Cancellation token for operation control</param>
    /// <returns>Extracted data with field values and confidence scores</returns>
    public Task<Result<ExtractedData>> ExtractFieldsUsingSchemaAsync(
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

            return Task.FromResult(Result<ExtractedData>.WithSuccess(extractedData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting fields using schema");
            return Task.FromResult(Result<ExtractedData>.WithFailure($"Field extraction error: {ex.Message}"));
        }
    }

    /// <summary>
    /// Extracts a specific field value from text using field definition patterns
    /// </summary>
    /// <param name="text">The text to search for field values</param>
    /// <param name="field">Field definition with extraction patterns</param>
    /// <returns>Extracted field value or null if not found</returns>
    public static string? ExtractFieldValue(string text, FieldDefinition field)
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
} 