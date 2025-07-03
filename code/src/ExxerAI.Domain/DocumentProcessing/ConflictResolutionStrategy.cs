namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents the strategies available for resolving data conflicts
/// </summary>
public enum ConflictResolutionStrategy
{
    /// <summary>
    /// Use the data with the highest confidence score
    /// </summary>
    MostConfident,

    /// <summary>
    /// Use the most recent data
    /// </summary>
    MostRecent,

    /// <summary>
    /// Use the data from the most trusted source
    /// </summary>
    MostTrusted,

    /// <summary>
    /// Merge data from multiple sources
    /// </summary>
    Merge,

    /// <summary>
    /// Require human intervention to resolve
    /// </summary>
    HumanReview,

    /// <summary>
    /// Use a custom algorithm to resolve
    /// </summary>
    Custom
}