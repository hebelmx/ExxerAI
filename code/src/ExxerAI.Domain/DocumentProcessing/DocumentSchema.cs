using System.ComponentModel.DataAnnotations;

namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents metadata about a document including type, schema, and processing options
/// </summary>
public class DocumentMetadata
{
    /// <summary>
    /// Gets or sets the document identifier
    /// </summary>
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the filename
    /// </summary>
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document type
    /// </summary>
    public DocumentType DocumentType { get; set; } = DocumentType.Unknown;

    /// <summary>
    /// Gets or sets the expected schema for this document
    /// </summary>
    public SchemaDefinition? ExpectedSchema { get; set; }

    /// <summary>
    /// Gets or sets the source path of the document
    /// </summary>
    [StringLength(1000)]
    public string SourcePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the processing options for this document
    /// </summary>
    public ProcessingOptions ProcessingOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets the creation date of the document
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the modification date of the document
    /// </summary>
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the file size in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Gets or sets the MIME type
    /// </summary>
    [StringLength(100)]
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional metadata properties
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = new();
}

/// <summary>
/// Represents the types of documents that can be processed
/// </summary>
public enum DocumentType
{
    /// <summary>
    /// Unknown document type
    /// </summary>
    Unknown,
    
    /// <summary>
    /// IMSS payment receipt
    /// </summary>
    IMSSPayment,
    
    /// <summary>
    /// General invoice
    /// </summary>
    Invoice,
    
    /// <summary>
    /// Tax document
    /// </summary>
    TaxDocument,
    
    /// <summary>
    /// Insurance payment receipt
    /// </summary>
    InsurancePayment,
    
    /// <summary>
    /// Purchase order
    /// </summary>
    PurchaseOrder,
    
    /// <summary>
    /// Bank statement
    /// </summary>
    BankStatement,
    
    /// <summary>
    /// Contract document
    /// </summary>
    Contract,
    
    /// <summary>
    /// Financial report
    /// </summary>
    FinancialReport,
    
    /// <summary>
    /// Other business document
    /// </summary>
    Other
}

/// <summary>
/// Represents processing options for document processing
/// </summary>
public class ProcessingOptions
{
    /// <summary>
    /// Gets or sets whether to use OCR if direct text extraction fails
    /// </summary>
    public bool UseOCRFallback { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to use LLM for field extraction
    /// </summary>
    public bool UseLLMExtraction { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable schema learning
    /// </summary>
    public bool EnableSchemaLearning { get; set; } = true;

    /// <summary>
    /// Gets or sets the minimum confidence threshold for auto-processing
    /// </summary>
    public float MinimumConfidenceThreshold { get; set; } = 0.7f;

    /// <summary>
    /// Gets or sets whether to store in primary source of truth
    /// </summary>
    public bool StoreTruthRecord { get; set; } = true;

    /// <summary>
    /// Gets or sets the OCR language codes to use
    /// </summary>
    public List<string> OCRLanguages { get; init; } = new() { "spa", "eng" };

    /// <summary>
    /// Gets or sets custom processing parameters
    /// </summary>
    public Dictionary<string, object> CustomParameters { get; init; } = new();
}

/// <summary>
/// Represents a schema definition for a document type
/// </summary>
public class SchemaDefinition
{
    /// <summary>
    /// Gets or sets the unique identifier for the schema
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the schema name
    /// </summary>
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document type this schema applies to
    /// </summary>
    public DocumentType DocumentType { get; set; } = DocumentType.Unknown;

    /// <summary>
    /// Gets or sets the schema version
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Gets or sets the list of field definitions
    /// </summary>
    public List<FieldDefinition> Fields { get; init; } = new();

    /// <summary>
    /// Gets or sets when this schema was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when this schema was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the accuracy score for this schema
    /// </summary>
    public float AccuracyScore { get; set; } = 1.0f;

    /// <summary>
    /// Gets or sets whether this schema is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets additional schema metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// Gets the required fields from this schema
    /// </summary>
    public IEnumerable<FieldDefinition> RequiredFields => Fields.Where(f => f.IsRequired);

    /// <summary>
    /// Gets the optional fields from this schema
    /// </summary>
    public IEnumerable<FieldDefinition> OptionalFields => Fields.Where(f => !f.IsRequired);
}

/// <summary>
/// Represents a field definition within a document schema
/// </summary>
public class FieldDefinition
{
    /// <summary>
    /// Gets or sets the field name
    /// </summary>
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the field type
    /// </summary>
    public FieldType Type { get; set; } = FieldType.Text;

    /// <summary>
    /// Gets or sets whether this field is required
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// Gets or sets the primary extraction pattern for this field
    /// </summary>
    public string PrimaryPattern { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets alternative extraction patterns
    /// </summary>
    public List<ExtractionPattern> AlternativePatterns { get; init; } = new();

    /// <summary>
    /// Gets or sets the validation rules for this field
    /// </summary>
    public List<ValidationRule> ValidationRules { get; init; } = new();

    /// <summary>
    /// Gets or sets the field description
    /// </summary>
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets example values for this field
    /// </summary>
    public List<string> ExampleValues { get; init; } = new();

    /// <summary>
    /// Gets or sets the confidence threshold for this field
    /// </summary>
    public float ConfidenceThreshold { get; set; } = 0.8f;

    /// <summary>
    /// Initializes a new instance of the FieldDefinition class
    /// </summary>
    public FieldDefinition() { }

    /// <summary>
    /// Initializes a new instance of the FieldDefinition class with basic properties
    /// </summary>
    /// <param name="name">The field name</param>
    /// <param name="type">The field type</param>
    /// <param name="isRequired">Whether the field is required</param>
    /// <param name="primaryPattern">The primary extraction pattern</param>
    public FieldDefinition(string name, FieldType type, bool isRequired, string primaryPattern)
    {
        Name = name;
        Type = type;
        IsRequired = isRequired;
        PrimaryPattern = primaryPattern;
    }
}

/// <summary>
/// Represents the data types that can be extracted from fields
/// </summary>
public enum FieldType
{
    /// <summary>
    /// Plain text field
    /// </summary>
    Text,
    
    /// <summary>
    /// Alphanumeric field
    /// </summary>
    AlphaNumeric,
    
    /// <summary>
    /// Integer number
    /// </summary>
    Integer,
    
    /// <summary>
    /// Decimal number
    /// </summary>
    Decimal,
    
    /// <summary>
    /// Date in various formats
    /// </summary>
    Date,
    
    /// <summary>
    /// Date in MM-YYYY format
    /// </summary>
    Date_MMYYYY,
    
    /// <summary>
    /// Currency amount
    /// </summary>
    Currency,
    
    /// <summary>
    /// Email address
    /// </summary>
    Email,
    
    /// <summary>
    /// Phone number
    /// </summary>
    Phone,
    
    /// <summary>
    /// Boolean value
    /// </summary>
    Boolean,
    
    /// <summary>
    /// Custom type
    /// </summary>
    Custom
}

/// <summary>
/// Represents an extraction pattern for finding field values in documents
/// </summary>
public abstract class ExtractionPattern
{
    /// <summary>
    /// Gets or sets the pattern identifier
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the pattern name
    /// </summary>
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the confidence score for this pattern (0.0 - 1.0)
    /// </summary>
    public float Confidence { get; set; } = 1.0f;

    /// <summary>
    /// Gets or sets whether this pattern is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets additional pattern metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// Attempts to extract a value using this pattern
    /// </summary>
    /// <param name="text">The text to search in</param>
    /// <param name="context">Additional context for extraction</param>
    /// <returns>The extracted value or null if not found</returns>
    public abstract string? ExtractValue(string text, ExtractionContext context);
}

/// <summary>
/// Represents a regex-based extraction pattern
/// </summary>
public class RegexPattern : ExtractionPattern
{
    /// <summary>
    /// Gets or sets the regular expression pattern
    /// </summary>
    public string Pattern { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the regex options
    /// </summary>
    public System.Text.RegularExpressions.RegexOptions Options { get; set; } = 
        System.Text.RegularExpressions.RegexOptions.IgnoreCase;

    /// <summary>
    /// Initializes a new instance of the RegexPattern class
    /// </summary>
    public RegexPattern() { }

    /// <summary>
    /// Initializes a new instance of the RegexPattern class with pattern and confidence
    /// </summary>
    /// <param name="pattern">The regex pattern</param>
    /// <param name="confidence">The confidence score</param>
    public RegexPattern(string pattern, float confidence)
    {
        Pattern = pattern;
        Confidence = confidence;
    }

    /// <summary>
    /// Attempts to extract a value using the regex pattern
    /// </summary>
    /// <param name="text">The text to search in</param>
    /// <param name="context">Additional context for extraction</param>
    /// <returns>The extracted value or null if not found</returns>
    public override string? ExtractValue(string text, ExtractionContext context)
    {
        try
        {
            var regex = new System.Text.RegularExpressions.Regex(Pattern, Options);
            var match = regex.Match(text);
            return match.Success ? match.Groups[1].Value.Trim() : null;
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Represents a keyword-based extraction pattern with positional strategy
/// </summary>
public class KeywordPattern : ExtractionPattern
{
    /// <summary>
    /// Gets or sets the keyword to search for
    /// </summary>
    public string Keyword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the position strategy for value extraction
    /// </summary>
    public PositionStrategy Strategy { get; set; } = PositionStrategy.NextToken;

    /// <summary>
    /// Initializes a new instance of the KeywordPattern class
    /// </summary>
    public KeywordPattern() { }

    /// <summary>
    /// Initializes a new instance of the KeywordPattern class with keyword and strategy
    /// </summary>
    /// <param name="keyword">The keyword to search for</param>
    /// <param name="strategy">The position strategy</param>
    /// <param name="confidence">The confidence score</param>
    public KeywordPattern(string keyword, PositionStrategy strategy, float confidence)
    {
        Keyword = keyword;
        Strategy = strategy;
        Confidence = confidence;
    }

    /// <summary>
    /// Attempts to extract a value using the keyword pattern
    /// </summary>
    /// <param name="text">The text to search in</param>
    /// <param name="context">Additional context for extraction</param>
    /// <returns>The extracted value or null if not found</returns>
    public override string? ExtractValue(string text, ExtractionContext context)
    {
        var index = text.IndexOf(Keyword, StringComparison.OrdinalIgnoreCase);
        if (index == -1) return null;

        var lines = text.Split('\n');
        var keywordLine = lines.FirstOrDefault(line => line.Contains(Keyword, StringComparison.OrdinalIgnoreCase));
        if (keywordLine == null) return null;

        return Strategy switch
        {
            PositionStrategy.NextToken => ExtractNextToken(keywordLine, Keyword),
            PositionStrategy.SameLine => ExtractFromSameLine(keywordLine, Keyword),
            PositionStrategy.NextLine => ExtractFromNextLine(lines, keywordLine),
            _ => null
        };
    }

    private static string? ExtractNextToken(string line, string keyword)
    {
        var index = line.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
        if (index == -1) return null;

        var afterKeyword = line.Substring(index + keyword.Length).Trim();
        var tokens = afterKeyword.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return tokens.Length > 0 ? tokens[0] : null;
    }

    private static string? ExtractFromSameLine(string line, string keyword)
    {
        var index = line.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
        if (index == -1) return null;

        return line.Substring(index + keyword.Length).Trim();
    }

    private static string? ExtractFromNextLine(string[] lines, string keywordLine)
    {
        var keywordIndex = Array.IndexOf(lines, keywordLine);
        if (keywordIndex == -1 || keywordIndex + 1 >= lines.Length) return null;

        return lines[keywordIndex + 1].Trim();
    }
}

/// <summary>
/// Represents strategies for extracting values relative to keywords
/// </summary>
public enum PositionStrategy
{
    /// <summary>
    /// Extract the next token after the keyword
    /// </summary>
    NextToken,
    
    /// <summary>
    /// Extract from the same line as the keyword
    /// </summary>
    SameLine,
    
    /// <summary>
    /// Extract from the next line after the keyword
    /// </summary>
    NextLine,
    
    /// <summary>
    /// Extract from the same line or next if not found
    /// </summary>
    SameLineOrNext
}

/// <summary>
/// Represents context information for pattern extraction
/// </summary>
public class ExtractionContext
{
    /// <summary>
    /// Gets or sets the document type
    /// </summary>
    public DocumentType DocumentType { get; set; } = DocumentType.Unknown;

    /// <summary>
    /// Gets or sets the OCR regions if available
    /// </summary>
    public List<OCRRegion> OCRRegions { get; init; } = new();

    /// <summary>
    /// Gets or sets additional context properties
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = new();

    /// <summary>
    /// Gets or sets the confidence threshold for extraction
    /// </summary>
    public float ConfidenceThreshold { get; set; } = 0.8f;
} 