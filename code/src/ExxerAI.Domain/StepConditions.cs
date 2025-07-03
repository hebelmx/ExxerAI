namespace ExxerAI.Domain;

/// <summary>
/// Represents conditions for workflow step execution
/// </summary>
public class StepConditions
{
    /// <summary>
    /// Gets or sets whether the step should be executed
    /// </summary>
    public string? ExecutionCondition { get; set; }

    /// <summary>
    /// Gets or sets the timeout for step execution in seconds
    /// </summary>
    public int? TimeoutSeconds { get; set; }

    /// <summary>
    /// Gets or sets the maximum retry attempts
    /// </summary>
    public int MaxRetries { get; set; } = 0;

    /// <summary>
    /// Gets or sets whether to continue on failure
    /// </summary>
    public bool ContinueOnFailure { get; set; } = false;
}