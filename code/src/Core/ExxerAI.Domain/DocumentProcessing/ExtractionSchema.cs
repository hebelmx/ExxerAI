namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents a schema for extracting specific fields from documents
/// </summary>
public class ExtractionSchema
{
    /// <summary>
    /// Gets or sets the unique identifier for the extraction schema
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
    /// Gets or sets the list of field definitions to extract
    /// </summary>
    public List<FieldDefinition> Fields { get; set; } = new();

    /// <summary>
    /// Gets or sets when this schema was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when this schema was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the accuracy score for this schema (0.0 to 1.0)
    /// </summary>
    public float AccuracyScore { get; set; } = 1.0f;

    /// <summary>
    /// Gets or sets whether this schema is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets additional schema metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the minimum confidence threshold for successful extraction
    /// </summary>
    public float MinimumConfidenceThreshold { get; set; } = 0.7f;

    /// <summary>
    /// Gets the required fields from this schema
    /// </summary>
    public IEnumerable<FieldDefinition> RequiredFields => Fields.Where(f => f.IsRequired);

    /// <summary>
    /// Gets the optional fields from this schema
    /// </summary>
    public IEnumerable<FieldDefinition> OptionalFields => Fields.Where(f => !f.IsRequired);

    /// <summary>
    /// Gets the number of fields in this schema
    /// </summary>
    public int FieldCount => Fields.Count;

    /// <summary>
    /// Gets the number of required fields in this schema
    /// </summary>
    public int RequiredFieldCount => RequiredFields.Count();

    /// <summary>
    /// Initializes a new instance of the ExtractionSchema class
    /// </summary>
    public ExtractionSchema() { }

    /// <summary>
    /// Initializes a new instance of the ExtractionSchema class with basic properties
    /// </summary>
    /// <param name="name">The schema name</param>
    /// <param name="documentType">The document type</param>
    public ExtractionSchema(string name, DocumentType documentType)
    {
        Name = name;
        DocumentType = documentType;
    }

    /// <summary>
    /// Adds a field definition to the schema
    /// </summary>
    /// <param name="field">The field definition to add</param>
    public void AddField(FieldDefinition field)
    {
        Fields.Add(field);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes a field definition from the schema
    /// </summary>
    /// <param name="fieldName">The name of the field to remove</param>
    /// <returns>True if the field was removed</returns>
    public bool RemoveField(string fieldName)
    {
        var field = Fields.FirstOrDefault(f => f.Name == fieldName);
        if (field != null)
        {
            Fields.Remove(field);
            UpdatedAt = DateTime.UtcNow;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets a field definition by name
    /// </summary>
    /// <param name="fieldName">The field name</param>
    /// <returns>The field definition or null if not found</returns>
    public FieldDefinition? GetField(string fieldName)
    {
        return Fields.FirstOrDefault(f => f.Name == fieldName);
    }

    /// <summary>
    /// Checks if a field exists in this schema
    /// </summary>
    /// <param name="fieldName">The field name to check</param>
    /// <returns>True if the field exists</returns>
    public bool HasField(string fieldName)
    {
        return Fields.Any(f => f.Name == fieldName);
    }

    /// <summary>
    /// Updates the accuracy score based on extraction results
    /// </summary>
    /// <param name="newScore">The new accuracy score</param>
    public void UpdateAccuracyScore(float newScore)
    {
        AccuracyScore = Math.Max(0.0f, Math.Min(1.0f, newScore));
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Validates the schema configuration
    /// </summary>
    /// <returns>A list of validation errors, empty if valid</returns>
    public List<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
        {
            errors.Add("Schema name is required");
        }

        if (Fields.Count == 0)
        {
            errors.Add("Schema must have at least one field");
        }

        var duplicateNames = Fields.GroupBy(f => f.Name)
                                   .Where(g => g.Count() > 1)
                                   .Select(g => g.Key);

        foreach (var duplicateName in duplicateNames)
        {
            errors.Add($"Duplicate field name: {duplicateName}");
        }

        return errors;
    }

    /// <summary>
    /// Creates a copy of this schema
    /// </summary>
    /// <returns>A new ExtractionSchema instance with copied values</returns>
    public ExtractionSchema Clone()
    {
        return new ExtractionSchema
        {
            Name = $"{Name}_Copy",
            DocumentType = DocumentType,
            Version = Version + 1,
            Fields = Fields.Select(f => new FieldDefinition(f.Name, f.Type, f.IsRequired, f.PrimaryPattern)
            {
                Description = f.Description,
                AlternativePatterns = new List<ExtractionPattern>(f.AlternativePatterns),
                ValidationRules = new List<ValidationRule>(f.ValidationRules)
            }).ToList(),
            AccuracyScore = AccuracyScore,
            IsActive = IsActive,
            Metadata = new Dictionary<string, object>(Metadata),
            MinimumConfidenceThreshold = MinimumConfidenceThreshold
        };
    }
} 