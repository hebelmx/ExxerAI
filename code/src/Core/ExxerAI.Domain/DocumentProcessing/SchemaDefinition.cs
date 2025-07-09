namespace ExxerAI.Domain.DocumentProcessing;

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
    public List<FieldDefinition> Fields { get; init; } = [];

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
    public Dictionary<string, object> Metadata { get; init; } = [];

    /// <summary>
    /// Gets the required fields from this schema
    /// </summary>
    public IEnumerable<FieldDefinition> RequiredFields => Fields.Where(f => f.IsRequired);

    /// <summary>
    /// Gets the optional fields from this schema
    /// </summary>
    public IEnumerable<FieldDefinition> OptionalFields => Fields.Where(f => !f.IsRequired);
}