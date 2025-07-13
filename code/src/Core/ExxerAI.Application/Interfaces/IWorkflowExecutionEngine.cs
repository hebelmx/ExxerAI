using ExxerAI.Domain.Entities;
using ExxerAI.Domain.Operations;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Interface for the workflow execution engine responsible for executing workflow steps and managing execution lifecycle
/// </summary>
public interface IWorkflowExecutionEngine
{
    /// <summary>
    /// Executes a workflow asynchronously from start to completion
    /// </summary>
    /// <param name="workflowId">The workflow to execute</param>
    /// <param name="input">Input data for the workflow execution</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Result containing the completed workflow execution</returns>
    Task<Result<WorkflowExecution>> ExecuteWorkflowAsync(
        Guid workflowId, 
        Dictionary<string, object>? input = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes a paused workflow execution
    /// </summary>
    /// <param name="executionId">The execution to resume</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Result indicating success or failure of the resume operation</returns>
    Task<Result<WorkflowExecution>> ResumeExecutionAsync(
        Guid executionId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a single workflow step
    /// </summary>
    /// <param name="step">The workflow step to execute</param>
    /// <param name="execution">The current workflow execution context</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Result containing the step execution result</returns>
    Task<Result<StepExecution>> ExecuteStepAsync(
        WorkflowStep step, 
        WorkflowExecution execution, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles step failure with retry logic and error recovery
    /// </summary>
    /// <param name="stepExecution">The failed step execution</param>
    /// <param name="error">The error that occurred</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Result indicating if the step should be retried or the workflow should fail</returns>
    Task<Result<StepExecutionRecoveryAction>> HandleStepFailureAsync(
        StepExecution stepExecution, 
        Exception error, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates execution progress and notifies listeners
    /// </summary>
    /// <param name="execution">The workflow execution to update</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Result indicating success or failure of the progress update</returns>
    Task<Result<bool>> UpdateExecutionProgressAsync(
        WorkflowExecution execution, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if a workflow execution can be started or resumed
    /// </summary>
    /// <param name="execution">The workflow execution to validate</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Result containing validation result and any issues</returns>
    Task<Result<WorkflowExecutionValidationResult>> ValidateExecutionAsync(
        WorkflowExecution execution, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the action to take after a step failure
/// </summary>
public enum StepExecutionRecoveryAction
{
    /// <summary>
    /// Retry the step with the same inputs
    /// </summary>
    Retry,

    /// <summary>
    /// Skip the step and continue with the workflow
    /// </summary>
    Skip,

    /// <summary>
    /// Fail the entire workflow execution
    /// </summary>
    FailWorkflow,

    /// <summary>
    /// Pause the workflow for manual intervention
    /// </summary>
    PauseForIntervention
}

/// <summary>
/// Result of workflow execution validation
/// </summary>
public record WorkflowExecutionValidationResult
{
    /// <summary>
    /// Indicates if the execution is valid and can proceed
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// List of validation issues found
    /// </summary>
    public IEnumerable<string> Issues { get; init; } = [];

    /// <summary>
    /// Indicates if the issues are warnings that can be ignored
    /// </summary>
    public bool HasWarnings { get; init; }

    /// <summary>
    /// Creates a successful validation result
    /// </summary>
    public static WorkflowExecutionValidationResult Valid() => new() { IsValid = true };

    /// <summary>
    /// Creates a failed validation result with issues
    /// </summary>
    public static WorkflowExecutionValidationResult Invalid(IEnumerable<string> issues) => 
        new() { IsValid = false, Issues = issues };

    /// <summary>
    /// Creates a validation result with warnings
    /// </summary>
    public static WorkflowExecutionValidationResult WithWarnings(IEnumerable<string> warnings) => 
        new() { IsValid = true, HasWarnings = true, Issues = warnings };
}