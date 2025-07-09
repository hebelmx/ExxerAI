namespace ExxerAI.Domain.DocumentProcessing;

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