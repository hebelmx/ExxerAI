namespace ExxerAI.Domain.Helpers;

/// <summary>
/// Constants used by the Result classes to ensure consistency and avoid magic strings.
/// This class serves as the single source of truth for all Result-related messages.
/// </summary>
public static class ResultConstants
{
    /// <summary>
    /// Default error message used when no specific error is provided.
    /// </summary>
    public const string DefaultErrorMessage = "Operation failed to execute successfully";
    
    /// <summary>
    /// Message used when combining errors but no errors are found.
    /// </summary>
    public const string NoErrorsFoundMessage = "No errors were found";
    
    /// <summary>
    /// Default warning message used when warnings collection is null or empty.
    /// </summary>
    public const string DefaultWarningMessage = "Warning: unexpected condition occurred";
    
    /// <summary>
    /// Prefix used in ToString() representation for successful results.
    /// </summary>
    public const string SuccessPrefix = "Success";
    
    /// <summary>
    /// Prefix used in ToString() representation for failed results.
    /// </summary>
    public const string FailurePrefix = "Failure";
    
    /// <summary>
    /// Default ToString() representation for failed results with no specific errors.
    /// </summary>
    public const string DefaultFailureString = "Failure";
    
    /// <summary>
    /// Error message template for RecoverWith type conversion failures.
    /// Use with string.Format(template, sourceType, targetType).
    /// </summary>
    public const string RecoverWithTypeConversionError = "Cannot convert value of type '{0}' to '{1}' in RecoverWith operation";
} 