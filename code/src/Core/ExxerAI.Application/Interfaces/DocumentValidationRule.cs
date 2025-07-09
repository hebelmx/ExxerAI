namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Individual validation rule for field validation
/// </summary>
public class DocumentValidationRule
{
    /// <summary>
    /// Gets or sets the rule type
    /// </summary>
    public string RuleType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rule expression or pattern
    /// </summary>
    public string Expression { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error message if validation fails
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}