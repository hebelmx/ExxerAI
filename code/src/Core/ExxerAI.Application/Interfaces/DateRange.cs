namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Represents a date range for reports and queries
/// </summary>
public class DateRange
{
    /// <summary>
    /// Gets or sets the start date
    /// </summary>
    public DateTime FromDate { get; set; }

    /// <summary>
    /// Gets or sets the end date
    /// </summary>
    public DateTime ToDate { get; set; }

    /// <summary>
    /// Gets the duration of the date range
    /// </summary>
    public TimeSpan Duration => ToDate - FromDate;
}