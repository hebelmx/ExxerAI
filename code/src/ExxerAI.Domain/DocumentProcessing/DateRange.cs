namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents a date range for reporting periods
/// </summary>
public class DateRange
{
    /// <summary>
    /// Start date of the range (inclusive)
    /// </summary>
    public DateTime FromDate { get; set; }

    /// <summary>
    /// End date of the range (inclusive)
    /// </summary>
    public DateTime ToDate { get; set; }
}