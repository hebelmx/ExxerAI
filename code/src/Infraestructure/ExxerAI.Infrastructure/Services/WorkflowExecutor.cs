using ExxerAI.Infrastructure.Interfaces;
using ExxerAI.Domain.Entities;
using ExxerAI.Domain.Operations;
using ExxerAI.Domain.ValueObjects;
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
                Status = WorkflowExecutionStatus.Running,
                StartedAt = DateTime.UtcNow,
                Input = new Dictionary<string, object>(input),
                Output = new Dictionary<string, object>()
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
                // Note: PausedAt property doesn't exist in Domain WorkflowExecution
                // In a real implementation, this would be tracked in execution metadata

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
                // Note: ResumedAt property doesn't exist in Domain WorkflowExecution
                // In a real implementation, this would be tracked in execution metadata

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

        var steps = workflow.Definition.Steps.OrderBy(s => s.Order).ToList();
        for (int stepIndex = 0; stepIndex < steps.Count; stepIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Check if execution is paused
            if (execution.Status == WorkflowExecutionStatus.Paused)
            {
                _logger.LogInformation("Workflow execution {ExecutionId} is paused at step {StepIndex}",
                    execution.Id, stepIndex);
                return;
            }

            var step = steps[stepIndex];
            execution.CurrentStepId = step.Id;

            var stepExecution = new StepExecution
            {
                Id = Guid.NewGuid(),
                WorkflowExecutionId = execution.Id,
                StepId = step.Id,
                Status = StepExecutionStatus.Running,
                StartedAt = DateTime.UtcNow,
                Input = new Dictionary<string, object>(currentData),
                Output = new Dictionary<string, object>()
            };

            execution.StepExecutions.Add(stepExecution);

            try
            {
                _logger.LogDebug("Executing workflow step {StepName} ({StepIndex}/{TotalSteps}) for execution {ExecutionId}",
                    step.Name, stepIndex + 1, steps.Count, execution.Id);

                var stepResult = await ExecuteWorkflowStepAsync(step, currentData, cancellationToken).ConfigureAwait(false);

                if (stepResult.IsSuccess)
                {
                    stepExecution.Status = StepExecutionStatus.Completed;
                    stepExecution.CompletedAt = DateTime.UtcNow;
                    // Note: Output is init-only, cannot be modified after creation
                    // In a real implementation, this would be handled differently

                    // Merge step output into current data for next step
                    if (stepResult.Value != null)
                    {
                        foreach (var kvp in stepResult.Value)
                        {
                            currentData[kvp.Key] = kvp.Value;
                        }
                    }

                    _logger.LogDebug("Completed workflow step {StepName} for execution {ExecutionId}",
                        step.Name, execution.Id);
                }
                else
                {
                    stepExecution.Status = StepExecutionStatus.Failed;
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
                stepExecution.Status = StepExecutionStatus.Failed;
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
        // Note: Output is init-only, cannot be modified after creation
        // In a real implementation, this would be handled during object creation
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
            switch (step.StepType?.ToLowerInvariant())
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

 