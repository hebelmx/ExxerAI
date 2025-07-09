namespace ExxerAI.Domain.DocumentProcessing;

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