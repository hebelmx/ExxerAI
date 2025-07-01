using System.ComponentModel.DataAnnotations;

namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents an extraction schema that defines how to extract specific fields from documents.
/// This is an alias for SchemaDefinition to maintain API compatibility.
/// </summary>
public class ExtractionSchema
{
    /// <summary>
    /// Gets or sets the unique identifier for the schema.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the schema name.
    /// </summary>
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document type this schema applies to.
    /// </summary>
    public DocumentType DocumentType { get; set; } = DocumentType.Unknown;

    /// <summary>
    /// Gets or sets the schema version.
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Gets or sets the list of field definitions for extraction.
    /// </summary>
    public List<FieldDefinition> Fields { get; set; } = new();

    /// <summary>
    /// Gets or sets when this schema was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when this schema was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the accuracy score for this schema.
    /// </summary>
    public float AccuracyScore { get; set; } = 1.0f;

    /// <summary>
    /// Gets or sets whether this schema is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets additional schema metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Gets the required fields from this schema.
    /// </summary>
    public IEnumerable<FieldDefinition> RequiredFields => Fields.Where(f => f.IsRequired);

    /// <summary>
    /// Gets the optional fields from this schema.
    /// </summary>
    public IEnumerable<FieldDefinition> OptionalFields => Fields.Where(f => !f.IsRequired);

    /// <summary>
    /// Gets the total number of fields in this schema.
    /// </summary>
    public int FieldCount => Fields.Count;

    /// <summary>
    /// Initializes a new instance of the ExtractionSchema class.
    /// </summary>
    public ExtractionSchema() { }

    /// <summary>
    /// Initializes a new instance of the ExtractionSchema class with basic properties.
    /// </summary>
    /// <param name="name">The schema name.</param>
    /// <param name="documentType">The document type this schema applies to.</param>
    public ExtractionSchema(string name, DocumentType documentType)
    {
        Name = name;
        DocumentType = documentType;
    }

    /// <summary>
    /// Adds a field definition to this schema.
    /// </summary>
    /// <param name="field">The field definition to add.</param>
    public void AddField(FieldDefinition field)
    {
        if (field != null && !Fields.Any(f => f.Name.Equals(field.Name, StringComparison.OrdinalIgnoreCase)))
        {
            Fields.Add(field);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Removes a field definition from this schema.
    /// </summary>
    /// <param name="fieldName">The name of the field to remove.</param>
    /// <returns>True if the field was removed, false if not found.</returns>
    public bool RemoveField(string fieldName)
    {
        var field = Fields.FirstOrDefault(f => f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
        if (field != null)
        {
            Fields.Remove(field);
            UpdatedAt = DateTime.UtcNow;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets a field definition by name.
    /// </summary>
    /// <param name="fieldName">The name of the field to find.</param>
    /// <returns>The field definition or null if not found.</returns>
    public FieldDefinition? GetField(string fieldName)
    {
        return Fields.FirstOrDefault(f => f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Validates that this schema is properly configured.
    /// </summary>
    /// <returns>A list of validation errors, empty if valid.</returns>
    public List<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
        {
            errors.Add("Schema name is required");
        }

        if (!Fields.Any())
        {
            errors.Add("Schema must have at least one field definition");
        }

        // Check for duplicate field names
        var duplicateFields = Fields.GroupBy(f => f.Name.ToLowerInvariant())
                                   .Where(g => g.Count() > 1)
                                   .Select(g => g.Key);

        foreach (var duplicate in duplicateFields)
        {
            errors.Add($"Duplicate field name: {duplicate}");
        }

        // Validate each field
        foreach (var field in Fields)
        {
            if (string.IsNullOrWhiteSpace(field.Name))
            {
                errors.Add("Field name is required for all fields");
            }

            if (string.IsNullOrWhiteSpace(field.PrimaryPattern) && !field.AlternativePatterns.Any())
            {
                errors.Add($"Field '{field.Name}' must have at least one extraction pattern");
            }
        }

        return errors;
    }

    /// <summary>
    /// Creates a copy of this schema with a new version number.
    /// </summary>
    /// <returns>A new schema instance with incremented version.</returns>
    public ExtractionSchema CreateNewVersion()
    {
        var newSchema = new ExtractionSchema(Name, DocumentType)
        {
            Version = Version + 1,
            Fields = Fields.Select(f => new FieldDefinition(f.Name, f.Type, f.IsRequired, f.PrimaryPattern)).ToList(),
            Metadata = new Dictionary<string, object>(Metadata)
        };

        return newSchema;
    }

    /// <summary>
    /// Converts this extraction schema to a schema definition.
    /// </summary>
    /// <returns>A SchemaDefinition equivalent of this extraction schema.</returns>
    public SchemaDefinition ToSchemaDefinition()
    {
        return new SchemaDefinition
        {
            Id = Id,
            Name = Name,
            DocumentType = DocumentType,
            Version = Version,
            Fields = Fields.ToList(),
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt,
            AccuracyScore = AccuracyScore,
            IsActive = IsActive,
            Metadata = new Dictionary<string, object>(Metadata)
        };
    }

    /// <summary>
    /// Creates an extraction schema from a schema definition.
    /// </summary>
    /// <param name="schemaDefinition">The schema definition to convert.</param>
    /// <returns>An ExtractionSchema equivalent.</returns>
    public static ExtractionSchema FromSchemaDefinition(SchemaDefinition schemaDefinition)
    {
        return new ExtractionSchema
        {
            Id = schemaDefinition.Id,
            Name = schemaDefinition.Name,
            DocumentType = schemaDefinition.DocumentType,
            Version = schemaDefinition.Version,
            Fields = schemaDefinition.Fields.ToList(),
            CreatedAt = schemaDefinition.CreatedAt,
            UpdatedAt = schemaDefinition.UpdatedAt,
            AccuracyScore = schemaDefinition.AccuracyScore,
            IsActive = schemaDefinition.IsActive,
            Metadata = new Dictionary<string, object>(schemaDefinition.Metadata)
        };
    }

    /// <summary>
    /// Returns a string representation of this schema.
    /// </summary>
    /// <returns>A formatted string with schema information.</returns>
    public override string ToString()
    {
        return $"ExtractionSchema[{Name} v{Version}, {FieldCount} fields, {DocumentType}]";
    }
} 