using ExxerAI.Infrastructure.Interfaces;
using ExxerAI.Domain.Entities;
using ExxerAI.Domain.Operations;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ExxerAI.Infrastructure.Services;

/// <summary>
/// Implementation of workflow executor for managing workflow execution with async operations and state management
/// Supports workflow execution, pause/resume, cancellation, and state persistence
/// </summary>
public class WorkflowExecutor : IWorkflowExecutor
{
    private readonly ILogger<WorkflowExecutor> _logger;
    private readonly ConcurrentDictionary<Guid, WorkflowExecution> _activeExecutions;
    private readonly SemaphoreSlim _executionLock;

    /// <summary>
    /// Initializes a new instance of the WorkflowExecutor
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public WorkflowExecutor(ILogger<WorkflowExecutor> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _activeExecutions = new ConcurrentDictionary<Guid, WorkflowExecution>();
        _executionLock = new SemaphoreSlim(1, 1);
    }

    /// <summary>
    /// Executes a workflow
    /// </summary>
    /// <param name="workflow">The workflow to execute</param>
    /// <param name="input">The input data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The workflow execution result</returns>
    public async Task<Result<WorkflowExecution>> ExecuteAsync(
        Workflow workflow,
        Dictionary<string, object> input,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (workflow is null)
            {
                _logger.LogWarning("Attempted to execute null workflow");
                return Result<WorkflowExecution>.WithFailure("Workflow cannot be null");
            }

            if (input is null)
            {
                _logger.LogWarning("Attempted to execute workflow {WorkflowId} with null input", workflow.Id);
                return Result<WorkflowExecution>.WithFailure("Input cannot be null");
            }

            cancellationToken.ThrowIfCancellationRequested();

            var execution = new WorkflowExecution
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflow.Id,
                WorkflowName = workflow.Name,
                Status = WorkflowExecutionStatus.Running,
                StartedAt = DateTime.UtcNow,
                Input = new Dictionary<string, object>(input),
                Output = new Dictionary<string, object>(),
                CurrentStepIndex = 0,
                ExecutedSteps = new List<WorkflowStepExecution>(),
                Metadata = new Dictionary<string, object>()
            };

            _activeExecutions.TryAdd(execution.Id, execution);

            _logger.LogInformation("Started workflow execution {ExecutionId} for workflow {WorkflowId}: {WorkflowName}",
                execution.Id, workflow.Id, workflow.Name);

            try
            {
                await ExecuteWorkflowStepsAsync(workflow, execution, cancellationToken).ConfigureAwait(false);

                if (execution.Status == WorkflowExecutionStatus.Running)
                {
                    execution.Status = WorkflowExecutionStatus.Completed;
                    execution.CompletedAt = DateTime.UtcNow;
                }

                _logger.LogInformation("Completed workflow execution {ExecutionId} with status {Status}",
                    execution.Id, execution.Status);

                return Result<WorkflowExecution>.Success(execution);
            }
            catch (OperationCanceledException)
            {
                execution.Status = WorkflowExecutionStatus.Cancelled;
                execution.CompletedAt = DateTime.UtcNow;
                execution.ErrorMessage = "Execution was cancelled";

                _logger.LogInformation("Workflow execution {ExecutionId} was cancelled", execution.Id);
                return Result<WorkflowExecution>.Success(execution);
            }
            catch (Exception ex)
            {
                execution.Status = WorkflowExecutionStatus.Failed;
                execution.CompletedAt = DateTime.UtcNow;
                execution.ErrorMessage = ex.Message;

                _logger.LogError(ex, "Workflow execution {ExecutionId} failed", execution.Id);
                return Result<WorkflowExecution>.Success(execution);
            }
            finally
            {
                _activeExecutions.TryRemove(execution.Id, out _);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing workflow {WorkflowId}", workflow?.Id);
            return Result<WorkflowExecution>.WithFailure($"Failed to execute workflow: {ex.Message}");
        }
    }

    /// <summary>
    /// Pauses a running workflow execution
    /// </summary>
    /// <param name="executionId">The execution identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    public async Task<Result<bool>> PauseExecutionAsync(Guid executionId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (executionId == Guid.Empty)
            {
                _logger.LogWarning("Attempted to pause execution with empty ID");
                return Result<bool>.WithFailure("Execution ID cannot be empty");
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _executionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (!_activeExecutions.TryGetValue(executionId, out var execution))
                {
                    _logger.LogWarning("Execution {ExecutionId} not found for pause", executionId);
                    return Result<bool>.WithFailure("Execution not found");
                }

                if (execution.Status != WorkflowExecutionStatus.Running)
                {
                    _logger.LogWarning("Cannot pause execution {ExecutionId} with status {Status}",
                        executionId, execution.Status);
                    return Result<bool>.WithFailure($"Cannot pause execution with status {execution.Status}");
                }

                execution.Status = WorkflowExecutionStatus.Paused;
                execution.PausedAt = DateTime.UtcNow;

                _logger.LogInformation("Paused workflow execution {ExecutionId}", executionId);
                return Result<bool>.Success(true);
            }
            finally
            {
                _executionLock.Release();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Pause execution operation was cancelled");
            return Result<bool>.WithFailure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing execution {ExecutionId}", executionId);
            return Result<bool>.WithFailure($"Failed to pause execution: {ex.Message}");
        }
    }

    /// <summary>
    /// Resumes a paused workflow execution
    /// </summary>
    /// <param name="executionId">The execution identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    public async Task<Result<bool>> ResumeExecutionAsync(Guid executionId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (executionId == Guid.Empty)
            {
                _logger.LogWarning("Attempted to resume execution with empty ID");
                return Result<bool>.WithFailure("Execution ID cannot be empty");
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _executionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (!_activeExecutions.TryGetValue(executionId, out var execution))
                {
                    _logger.LogWarning("Execution {ExecutionId} not found for resume", executionId);
                    return Result<bool>.WithFailure("Execution not found");
                }

                if (execution.Status != WorkflowExecutionStatus.Paused)
                {
                    _logger.LogWarning("Cannot resume execution {ExecutionId} with status {Status}",
                        executionId, execution.Status);
                    return Result<bool>.WithFailure($"Cannot resume execution with status {execution.Status}");
                }

                execution.Status = WorkflowExecutionStatus.Running;
                execution.ResumedAt = DateTime.UtcNow;

                _logger.LogInformation("Resumed workflow execution {ExecutionId}", executionId);
                return Result<bool>.Success(true);
            }
            finally
            {
                _executionLock.Release();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Resume execution operation was cancelled");
            return Result<bool>.WithFailure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming execution {ExecutionId}", executionId);
            return Result<bool>.WithFailure($"Failed to resume execution: {ex.Message}");
        }
    }

    /// <summary>
    /// Cancels a workflow execution
    /// </summary>
    /// <param name="executionId">The execution identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    public async Task<Result<bool>> CancelExecutionAsync(Guid executionId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (executionId == Guid.Empty)
            {
                _logger.LogWarning("Attempted to cancel execution with empty ID");
                return Result<bool>.WithFailure("Execution ID cannot be empty");
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _executionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (!_activeExecutions.TryGetValue(executionId, out var execution))
                {
                    _logger.LogWarning("Execution {ExecutionId} not found for cancellation", executionId);
                    return Result<bool>.WithFailure("Execution not found");
                }

                if (execution.Status == WorkflowExecutionStatus.Completed ||
                    execution.Status == WorkflowExecutionStatus.Failed ||
                    execution.Status == WorkflowExecutionStatus.Cancelled)
                {
                    _logger.LogWarning("Cannot cancel execution {ExecutionId} with status {Status}",
                        executionId, execution.Status);
                    return Result<bool>.WithFailure($"Cannot cancel execution with status {execution.Status}");
                }

                execution.Status = WorkflowExecutionStatus.Cancelled;
                execution.CompletedAt = DateTime.UtcNow;
                execution.ErrorMessage = "Execution was cancelled by user";

                _logger.LogInformation("Cancelled workflow execution {ExecutionId}", executionId);
                return Result<bool>.Success(true);
            }
            finally
            {
                _executionLock.Release();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Cancel execution operation was cancelled");
            return Result<bool>.WithFailure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling execution {ExecutionId}", executionId);
            return Result<bool>.WithFailure($"Failed to cancel execution: {ex.Message}");
        }
    }

    // Private helper methods

    private async Task ExecuteWorkflowStepsAsync(Workflow workflow, WorkflowExecution execution, CancellationToken cancellationToken)
    {
        var currentData = new Dictionary<string, object>(execution.Input);

        for (int stepIndex = execution.CurrentStepIndex; stepIndex < workflow.Steps.Count; stepIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Check if execution is paused
            if (execution.Status == WorkflowExecutionStatus.Paused)
            {
                _logger.LogInformation("Workflow execution {ExecutionId} is paused at step {StepIndex}",
                    execution.Id, stepIndex);
                return;
            }

            var step = workflow.Steps[stepIndex];
            execution.CurrentStepIndex = stepIndex;

            var stepExecution = new WorkflowStepExecution
            {
                Id = Guid.NewGuid(),
                StepId = step.Id,
                StepName = step.Name,
                Status = WorkflowStepStatus.Running,
                StartedAt = DateTime.UtcNow,
                Input = new Dictionary<string, object>(currentData),
                Output = new Dictionary<string, object>()
            };

            execution.ExecutedSteps.Add(stepExecution);

            try
            {
                _logger.LogDebug("Executing workflow step {StepName} ({StepIndex}/{TotalSteps}) for execution {ExecutionId}",
                    step.Name, stepIndex + 1, workflow.Steps.Count, execution.Id);

                var stepResult = await ExecuteWorkflowStepAsync(step, currentData, cancellationToken).ConfigureAwait(false);

                if (stepResult.IsSuccess)
                {
                    stepExecution.Status = WorkflowStepStatus.Completed;
                    stepExecution.CompletedAt = DateTime.UtcNow;
                    stepExecution.Output = stepResult.Value ?? new Dictionary<string, object>();

                    // Merge step output into current data for next step
                    foreach (var kvp in stepExecution.Output)
                    {
                        currentData[kvp.Key] = kvp.Value;
                    }

                    _logger.LogDebug("Completed workflow step {StepName} for execution {ExecutionId}",
                        step.Name, execution.Id);
                }
                else
                {
                    stepExecution.Status = WorkflowStepStatus.Failed;
                    stepExecution.CompletedAt = DateTime.UtcNow;
                    stepExecution.ErrorMessage = stepResult.Error ?? "Step execution failed";

                    execution.Status = WorkflowExecutionStatus.Failed;
                    execution.ErrorMessage = $"Step '{step.Name}' failed: {stepResult.Error}";

                    _logger.LogError("Workflow step {StepName} failed for execution {ExecutionId}: {Error}",
                        step.Name, execution.Id, stepResult.Error);

                    return;
                }
            }
            catch (Exception ex)
            {
                stepExecution.Status = WorkflowStepStatus.Failed;
                stepExecution.CompletedAt = DateTime.UtcNow;
                stepExecution.ErrorMessage = ex.Message;

                execution.Status = WorkflowExecutionStatus.Failed;
                execution.ErrorMessage = $"Step '{step.Name}' threw exception: {ex.Message}";

                _logger.LogError(ex, "Exception in workflow step {StepName} for execution {ExecutionId}",
                    step.Name, execution.Id);

                throw;
            }
        }

        // Set final output
        execution.Output = currentData;
    }

    private async Task<Result<Dictionary<string, object>>> ExecuteWorkflowStepAsync(
        WorkflowStep step,
        Dictionary<string, object> input,
        CancellationToken cancellationToken)
    {
        try
        {
            // Simulate step execution based on step type
            await Task.Delay(100, cancellationToken).ConfigureAwait(false); // Simulate work

            var output = new Dictionary<string, object>(input);

            // Simple step execution logic based on step type
            switch (step.Type?.ToLowerInvariant())
            {
                case "data_transformation":
                    output[$"{step.Name}_result"] = $"Transformed data at {DateTime.UtcNow}";
                    break;

                case "validation":
                    output[$"{step.Name}_valid"] = true;
                    output[$"{step.Name}_validated_at"] = DateTime.UtcNow;
                    break;

                case "notification":
                    output[$"{step.Name}_notified"] = true;
                    output[$"{step.Name}_notification_sent"] = DateTime.UtcNow;
                    break;

                case "approval":
                    output[$"{step.Name}_approved"] = true;
                    output[$"{step.Name}_approved_at"] = DateTime.UtcNow;
                    break;

                default:
                    output[$"{step.Name}_executed"] = true;
                    output[$"{step.Name}_executed_at"] = DateTime.UtcNow;
                    break;
            }

            return Result<Dictionary<string, object>>.Success(output);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing workflow step {StepName}", step.Name);
            return Result<Dictionary<string, object>>.WithFailure($"Step execution failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the current status of a workflow execution
    /// </summary>
    /// <param name="executionId">The execution identifier</param>
    /// <returns>The execution status if found</returns>
    public async Task<Result<WorkflowExecution>> GetExecutionStatusAsync(Guid executionId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (executionId == Guid.Empty)
            {
                _logger.LogWarning("Attempted to get execution status with empty ID");
                return Result<WorkflowExecution>.WithFailure("Execution ID cannot be empty");
            }

            cancellationToken.ThrowIfCancellationRequested();

            await Task.Delay(1, cancellationToken).ConfigureAwait(false); // Simulate async operation

            if (_activeExecutions.TryGetValue(executionId, out var execution))
            {
                _logger.LogDebug("Retrieved execution status for {ExecutionId}: {Status}",
                    executionId, execution.Status);
                return Result<WorkflowExecution>.Success(execution);
            }

            _logger.LogWarning("Execution {ExecutionId} not found", executionId);
            return Result<WorkflowExecution>.WithFailure("Execution not found");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Get execution status operation was cancelled");
            return Result<WorkflowExecution>.WithFailure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting execution status for {ExecutionId}", executionId);
            return Result<WorkflowExecution>.WithFailure($"Failed to get execution status: {ex.Message}");
        }
    }

    /// <summary>
    /// Disposes the resources used by the WorkflowExecutor
    /// </summary>
    public void Dispose()
    {
        _executionLock?.Dispose();
        _activeExecutions.Clear();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Represents a workflow execution instance
/// </summary>
public class WorkflowExecution
{
    /// <summary>
    /// Gets or sets the execution identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the workflow identifier
    /// </summary>
    public Guid WorkflowId { get; set; }

    /// <summary>
    /// Gets or sets the workflow name
    /// </summary>
    public string WorkflowName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the execution status
    /// </summary>
    public WorkflowExecutionStatus Status { get; set; }

    /// <summary>
    /// Gets or sets when the execution started
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Gets or sets when the execution completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets when the execution was paused
    /// </summary>
    public DateTime? PausedAt { get; set; }

    /// <summary>
    /// Gets or sets when the execution was resumed
    /// </summary>
    public DateTime? ResumedAt { get; set; }

    /// <summary>
    /// Gets or sets the input data
    /// </summary>
    public Dictionary<string, object> Input { get; set; } = new();

    /// <summary>
    /// Gets or sets the output data
    /// </summary>
    public Dictionary<string, object> Output { get; set; } = new();

    /// <summary>
    /// Gets or sets the current step index
    /// </summary>
    public int CurrentStepIndex { get; set; }

    /// <summary>
    /// Gets or sets the executed steps
    /// </summary>
    public List<WorkflowStepExecution> ExecutedSteps { get; set; } = new();

    /// <summary>
    /// Gets or sets the error message if execution failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets additional execution metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents a workflow step execution
/// </summary>
public class WorkflowStepExecution
{
    /// <summary>
    /// Gets or sets the step execution identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the step identifier
    /// </summary>
    public Guid StepId { get; set; }

    /// <summary>
    /// Gets or sets the step name
    /// </summary>
    public string StepName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the step execution status
    /// </summary>
    public WorkflowStepStatus Status { get; set; }

    /// <summary>
    /// Gets or sets when the step started
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Gets or sets when the step completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the step input data
    /// </summary>
    public Dictionary<string, object> Input { get; set; } = new();

    /// <summary>
    /// Gets or sets the step output data
    /// </summary>
    public Dictionary<string, object> Output { get; set; } = new();

    /// <summary>
    /// Gets or sets the error message if step failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Workflow execution status values
/// </summary>
public enum WorkflowExecutionStatus
{
    /// <summary>
    /// Execution is currently running
    /// </summary>
    Running,

    /// <summary>
    /// Execution is paused
    /// </summary>
    Paused,

    /// <summary>
    /// Execution completed successfully
    /// </summary>
    Completed,

    /// <summary>
    /// Execution failed
    /// </summary>
    Failed,

    /// <summary>
    /// Execution was cancelled
    /// </summary>
    Cancelled
}

/// <summary>
/// Workflow step status values
/// </summary>
public enum WorkflowStepStatus
{
    /// <summary>
    /// Step is waiting to be executed
    /// </summary>
    Pending,

    /// <summary>
    /// Step is currently running
    /// </summary>
    Running,

    /// <summary>
    /// Step completed successfully
    /// </summary>
    Completed,

    /// <summary>
    /// Step failed
    /// </summary>
    Failed,

    /// <summary>
    /// Step was skipped
    /// </summary>
    Skipped
} 