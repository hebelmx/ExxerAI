using System.ComponentModel.DataAnnotations;

namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Provides context and reference data for validating and grounding extracted document data.
/// Contains business rules, validation patterns, and known good data for comparison.
/// </summary>
public class GroundTruthContext
{
    /// <summary>
    /// Gets or sets the unique identifier for this context.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the document type this context applies to.
    /// </summary>
    public DocumentType DocumentType { get; set; } = DocumentType.Unknown;

    /// <summary>
    /// Gets or sets the business rules for validation.
    /// </summary>
    public List<ValidationRule> BusinessRules { get; set; } = new();

    /// <summary>
    /// Gets or sets the reference data for field validation.
    /// </summary>
    public Dictionary<string, List<string>> ReferenceData { get; set; } = new();

    /// <summary>
    /// Gets or sets the known valid patterns for field values.
    /// </summary>
    public Dictionary<string, List<string>> ValidPatterns { get; set; } = new();

    /// <summary>
    /// Gets or sets the confidence thresholds for each field.
    /// </summary>
    public Dictionary<string, float> ConfidenceThresholds { get; set; } = new();

    /// <summary>
    /// Gets or sets the historical successful extractions for comparison.
    /// </summary>
    public List<ExtractedData> HistoricalData { get; set; } = new();

    /// <summary>
    /// Gets or sets the data quality requirements.
    /// </summary>
    public DataQualityRequirements QualityRequirements { get; set; } = new();

    /// <summary>
    /// Gets or sets the temporal context (date ranges, validity periods).
    /// </summary>
    public TemporalContext TemporalContext { get; set; } = new();

    /// <summary>
    /// Gets or sets additional context properties.
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = new();

    /// <summary>
    /// Gets or sets when this context was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when this context was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets whether strict validation is enabled.
    /// </summary>
    public bool StrictValidation { get; set; } = false;

    /// <summary>
    /// Gets or sets the minimum confidence threshold for acceptance.
    /// </summary>
    public float MinimumConfidenceThreshold { get; set; } = 0.7f;

    /// <summary>
    /// Gets or sets the tolerance level for data variations.
    /// </summary>
    public float ToleranceLevel { get; set; } = 0.1f;

    /// <summary>
    /// Initializes a new instance of the GroundTruthContext class.
    /// </summary>
    public GroundTruthContext() { }

    /// <summary>
    /// Initializes a new instance of the GroundTruthContext class for a specific document type.
    /// </summary>
    /// <param name="documentType">The document type this context applies to.</param>
    public GroundTruthContext(DocumentType documentType)
    {
        DocumentType = documentType;
        InitializeDefaultRules(documentType);
    }

    /// <summary>
    /// Adds a business rule for validation.
    /// </summary>
    /// <param name="rule">The validation rule to add.</param>
    public void AddBusinessRule(ValidationRule rule)
    {
        if (rule != null)
        {
            BusinessRules.Add(rule);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Adds reference data for a specific field.
    /// </summary>
    /// <param name="fieldName">The field name.</param>
    /// <param name="validValues">The list of valid values for this field.</param>
    public void AddReferenceData(string fieldName, IEnumerable<string> validValues)
    {
        if (!string.IsNullOrWhiteSpace(fieldName) && validValues != null)
        {
            ReferenceData[fieldName] = validValues.ToList();
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Adds valid patterns for a specific field.
    /// </summary>
    /// <param name="fieldName">The field name.</param>
    /// <param name="patterns">The list of valid regex patterns for this field.</param>
    public void AddValidPatterns(string fieldName, IEnumerable<string> patterns)
    {
        if (!string.IsNullOrWhiteSpace(fieldName) && patterns != null)
        {
            ValidPatterns[fieldName] = patterns.ToList();
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Sets the confidence threshold for a specific field.
    /// </summary>
    /// <param name="fieldName">The field name.</param>
    /// <param name="threshold">The minimum confidence threshold (0.0 to 1.0).</param>
    public void SetConfidenceThreshold(string fieldName, float threshold)
    {
        if (!string.IsNullOrWhiteSpace(fieldName) && threshold >= 0.0f && threshold <= 1.0f)
        {
            ConfidenceThresholds[fieldName] = threshold;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Adds historical extraction data for pattern analysis.
    /// </summary>
    /// <param name="historicalExtraction">The historical extraction data.</param>
    public void AddHistoricalData(ExtractedData historicalExtraction)
    {
        if (historicalExtraction != null)
        {
            HistoricalData.Add(historicalExtraction);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Validates an extracted field value against the ground truth context.
    /// </summary>
    /// <param name="fieldName">The field name.</param>
    /// <param name="value">The extracted value.</param>
    /// <param name="confidence">The extraction confidence.</param>
    /// <returns>The validation result for this field.</returns>
    public FieldValidationResult ValidateField(string fieldName, object value, float confidence)
    {
        var result = new FieldValidationResult
        {
            IsValid = true,
            Confidence = confidence
        };

        // Check confidence threshold
        if (ConfidenceThresholds.TryGetValue(fieldName, out var threshold) && confidence < threshold)
        {
            result.IsValid = false;
            result.ErrorMessage = $"Confidence {confidence:F2} below threshold {threshold:F2}";
            return result;
        }

        // Check against reference data
        if (ReferenceData.TryGetValue(fieldName, out var validValues))
        {
            var valueStr = value?.ToString() ?? string.Empty;
            if (!validValues.Any(v => v.Equals(valueStr, StringComparison.OrdinalIgnoreCase)))
            {
                result.IsValid = false;
                result.ErrorMessage = $"Value '{valueStr}' not found in reference data";
                result.SuggestedCorrection = FindClosestMatch(valueStr, validValues);
                return result;
            }
        }

        // Check against valid patterns
        if (ValidPatterns.TryGetValue(fieldName, out var patterns))
        {
            var valueStr = value?.ToString() ?? string.Empty;
            var matchesPattern = patterns.Any(pattern =>
            {
                try
                {
                    return System.Text.RegularExpressions.Regex.IsMatch(valueStr, pattern);
                }
                catch
                {
                    return false;
                }
            });

            if (!matchesPattern)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Value '{valueStr}' does not match any valid pattern";
                return result;
            }
        }

        // Check business rules
        foreach (var rule in BusinessRules.Where(r => r.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase)))
        {
            if (!rule.Validate(value))
            {
                result.IsValid = false;
                result.ErrorMessage = rule.ErrorMessage;
                return result;
            }
        }

        return result;
    }

    /// <summary>
    /// Finds the closest match in a list of valid values.
    /// </summary>
    /// <param name="value">The value to match.</param>
    /// <param name="validValues">The list of valid values.</param>
    /// <returns>The closest matching value or null if no good match found.</returns>
    private static string? FindClosestMatch(string value, List<string> validValues)
    {
        if (string.IsNullOrWhiteSpace(value) || !validValues.Any())
            return null;

        var bestMatch = validValues
            .Select(v => new { Value = v, Distance = CalculateLevenshteinDistance(value.ToLowerInvariant(), v.ToLowerInvariant()) })
            .OrderBy(x => x.Distance)
            .FirstOrDefault();

        // Only suggest if the distance is reasonable (less than half the length)
        return bestMatch?.Distance < value.Length / 2 ? bestMatch.Value : null;
    }

    /// <summary>
    /// Calculates the Levenshtein distance between two strings.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="target">The target string.</param>
    /// <returns>The edit distance between the strings.</returns>
    private static int CalculateLevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source)) return target?.Length ?? 0;
        if (string.IsNullOrEmpty(target)) return source.Length;

        var matrix = new int[source.Length + 1, target.Length + 1];

        for (var i = 0; i <= source.Length; i++) matrix[i, 0] = i;
        for (var j = 0; j <= target.Length; j++) matrix[0, j] = j;

        for (var i = 1; i <= source.Length; i++)
        {
            for (var j = 1; j <= target.Length; j++)
            {
                var cost = source[i - 1] == target[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1), matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[source.Length, target.Length];
    }

    /// <summary>
    /// Initializes default rules for a document type.
    /// </summary>
    /// <param name="documentType">The document type.</param>
    private void InitializeDefaultRules(DocumentType documentType)
    {
        switch (documentType)
        {
            case DocumentType.IMSSPayment:
                InitializeIMSSRules();
                break;
            case DocumentType.Invoice:
                InitializeInvoiceRules();
                break;
            // Add more document type specific rules as needed
        }
    }

    /// <summary>
    /// Initializes IMSS payment specific validation rules.
    /// </summary>
    private void InitializeIMSSRules()
    {
        // Add IMSS-specific validation patterns and reference data
        AddValidPatterns("registro_patronal", new[] { @"^[A-Z0-9]{10,15}$" });
        AddValidPatterns("periodo_imss", new[] { @"^\d{2}-\d{4}$", @"^\d{2}/\d{4}$" });
        SetConfidenceThreshold("registro_patronal", 0.9f);
        SetConfidenceThreshold("periodo_imss", 0.85f);
    }

    /// <summary>
    /// Initializes invoice specific validation rules.
    /// </summary>
    private void InitializeInvoiceRules()
    {
        // Add invoice-specific validation patterns
        AddValidPatterns("invoice_number", new[] { @"^INV-\d{4,}$", @"^\d{4,}$" });
        AddValidPatterns("total_amount", new[] { @"^\$?[\d,]+\.\d{2}$" });
        SetConfidenceThreshold("total_amount", 0.95f);
    }
}

/// <summary>
/// Represents data quality requirements for validation.
/// </summary>
public class DataQualityRequirements
{
    /// <summary>
    /// Gets or sets the minimum overall confidence required.
    /// </summary>
    public float MinimumConfidence { get; set; } = 0.8f;

    /// <summary>
    /// Gets or sets the completeness threshold (percentage of required fields).
    /// </summary>
    public float CompletenessThreshold { get; set; } = 0.9f;

    /// <summary>
    /// Gets or sets whether empty values are allowed.
    /// </summary>
    public bool AllowEmptyValues { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to require manual review for low confidence.
    /// </summary>
    public bool RequireManualReviewForLowConfidence { get; set; } = true;

    /// <summary>
    /// Gets or sets field-specific quality requirements.
    /// </summary>
    public Dictionary<string, FieldQualityRequirement> FieldRequirements { get; set; } = new();
}

/// <summary>
/// Represents quality requirements for a specific field.
/// </summary>
public class FieldQualityRequirement
{
    /// <summary>
    /// Gets or sets the minimum confidence for this field.
    /// </summary>
    public float MinimumConfidence { get; set; } = 0.8f;

    /// <summary>
    /// Gets or sets whether this field is required.
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum allowed length.
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets the minimum allowed length.
    /// </summary>
    public int? MinLength { get; set; }

    /// <summary>
    /// Gets or sets custom validation rules.
    /// </summary>
    public List<string> CustomValidationRules { get; set; } = new();
}

/// <summary>
/// Represents temporal context for data validation.
/// </summary>
public class TemporalContext
{
    /// <summary>
    /// Gets or sets the valid date range for the data.
    /// </summary>
    public DateRange ValidDateRange { get; set; } = new();

    /// <summary>
    /// Gets or sets the current business period.
    /// </summary>
    public string BusinessPeriod { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets temporal validation rules.
    /// </summary>
    public List<string> TemporalRules { get; set; } = new();

    /// <summary>
    /// Gets or sets timezone information.
    /// </summary>
    public string TimeZone { get; set; } = "UTC";
} 