using Microsoft.Extensions.Logging;
using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.Entities;
using ExxerAI.Domain.Operations;
using ExxerAI.Domain.ValueObjects;

namespace ExxerAI.Application.Services;

/// <summary>
/// Core engine responsible for executing workflow steps and managing execution lifecycle
/// Provides comprehensive workflow execution with error handling, retry logic, and progress tracking
/// </summary>
public class WorkflowExecutionEngine : IWorkflowExecutionEngine
{
    private readonly IWorkflowExecutionRepository _executionRepository;
    private readonly IWorkflowRepository _workflowRepository;
    private readonly IAgentService _agentService;
    private readonly ITaskService _taskService;
    private readonly ILogger<WorkflowExecutionEngine> _logger;

    /// <summary>
    /// Maximum number of retry attempts for failed steps
    /// </summary>
    private const int MaxRetryAttempts = 3;

    /// <summary>
    /// Delay between retry attempts
    /// </summary>
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Initializes a new instance of the WorkflowExecutionEngine
    /// </summary>
    public WorkflowExecutionEngine(
        IWorkflowExecutionRepository executionRepository,
        IWorkflowRepository workflowRepository,
        IAgentService agentService,
        ITaskService taskService,
        ILogger<WorkflowExecutionEngine> logger)
    {
        _executionRepository = executionRepository ?? throw new ArgumentNullException(nameof(executionRepository));
        _workflowRepository = workflowRepository ?? throw new ArgumentNullException(nameof(workflowRepository));
        _agentService = agentService ?? throw new ArgumentNullException(nameof(agentService));
        _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Result<WorkflowExecution>> ExecuteWorkflowAsync(
        Guid workflowId, 
        Dictionary<string, object>? input = null, 
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<WorkflowExecution>();

        try
        {
            _logger.LogInformation("Starting workflow execution for workflow {WorkflowId}", workflowId);

            // Load workflow definition
            var workflowResult = await _workflowRepository.GetByIdAsync(workflowId, cancellationToken).ConfigureAwait(false);
            if (!workflowResult.IsSuccess)
            {
                return Result<WorkflowExecution>.WithFailure($"Failed to load workflow: {string.Join(", ", workflowResult.Errors)}");
            }

            var workflow = workflowResult.Value;

            // Create new execution
            var execution = new WorkflowExecution
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflowId,
                Workflow = workflow,
                Status = WorkflowExecutionStatus.Starting,
                Input = input ?? new Dictionary<string, object>(),
                CreatedAt = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            // Validate execution
            var validationResult = await ValidateExecutionAsync(execution, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsSuccess || !validationResult.Value.IsValid)
            {
                var issues = validationResult.IsSuccess ? string.Join(", ", validationResult.Value.Issues) : validationResult.Errors?.FirstOrDefault() ?? "Unknown error";
                return Result<WorkflowExecution>.WithFailure($"Workflow execution validation failed: {issues}");
            }

            // Save initial execution
            var createResult = await _executionRepository.CreateExecutionAsync(execution, cancellationToken).ConfigureAwait(false);
            if (!createResult.IsSuccess)
            {
                return Result<WorkflowExecution>.WithFailure($"Failed to create execution: {createResult.Errors?.FirstOrDefault() ?? "Unknown error"}");
            }

            execution = createResult.Value;

            // Start execution
            execution.Status = WorkflowExecutionStatus.Running;
            execution.StartedAt = DateTime.UtcNow;

            await _executionRepository.UpdateExecutionAsync(execution, cancellationToken).ConfigureAwait(false);

            // Execute workflow steps
            var executionResult = await ExecuteWorkflowStepsAsync(execution, cancellationToken).ConfigureAwait(false);
            if (!executionResult.IsSuccess)
            {
                execution.Status = WorkflowExecutionStatus.Failed;
                execution.ErrorMessage = string.Join(", ", executionResult.Errors);
                execution.CompletedAt = DateTime.UtcNow;
                await _executionRepository.UpdateExecutionAsync(execution, cancellationToken).ConfigureAwait(false);
                return executionResult;
            }

            // Complete execution
            execution = executionResult.Value;
            execution.Status = WorkflowExecutionStatus.Completed;
            execution.CompletedAt = DateTime.UtcNow;

            var finalResult = await _executionRepository.UpdateExecutionAsync(execution, cancellationToken).ConfigureAwait(false);
            if (!finalResult.IsSuccess)
            {
                _logger.LogWarning("Failed to update completed execution {ExecutionId}: {Error}", 
                    execution.Id, finalResult.Errors?.FirstOrDefault() ?? "Unknown error");
            }

            _logger.LogInformation("Workflow execution {ExecutionId} completed successfully", execution.Id);
            return Result<WorkflowExecution>.WithSuccess(execution);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Workflow execution cancelled for workflow {WorkflowId}", workflowId);
            return ResultExtensions.Cancelled<WorkflowExecution>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing workflow {WorkflowId}", workflowId);
            return Result<WorkflowExecution>.WithFailure($"Workflow execution failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<WorkflowExecution>> ResumeExecutionAsync(Guid executionId, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<WorkflowExecution>();

        try
        {
            _logger.LogInformation("Resuming workflow execution {ExecutionId}", executionId);

            // Load execution
            var executionResult = await _executionRepository.GetExecutionAsync(executionId, cancellationToken).ConfigureAwait(false);
            if (!executionResult.IsSuccess)
            {
                return Result<WorkflowExecution>.WithFailure($"Failed to load execution: {executionResult.Errors?.FirstOrDefault() ?? "Unknown error"}");
            }

            var execution = executionResult.Value;

            // Validate can resume
            if (execution.Status != WorkflowExecutionStatus.Paused)
            {
                return Result<WorkflowExecution>.WithFailure($"Cannot resume execution in {execution.Status} status");
            }

            // Resume execution
            execution.Status = WorkflowExecutionStatus.Running;
            await _executionRepository.UpdateExecutionAsync(execution, cancellationToken).ConfigureAwait(false);

            // Continue execution from current step
            var continueResult = await ExecuteWorkflowStepsAsync(execution, cancellationToken).ConfigureAwait(false);
            if (!continueResult.IsSuccess)
            {
                execution.Status = WorkflowExecutionStatus.Failed;
                execution.ErrorMessage = string.Join(", ", continueResult.Errors);
                execution.CompletedAt = DateTime.UtcNow;
                await _executionRepository.UpdateExecutionAsync(execution, cancellationToken).ConfigureAwait(false);
                return continueResult;
            }

            // Complete execution
            execution = continueResult.Value;
            execution.Status = WorkflowExecutionStatus.Completed;
            execution.CompletedAt = DateTime.UtcNow;

            await _executionRepository.UpdateExecutionAsync(execution, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Workflow execution {ExecutionId} resumed and completed successfully", executionId);
            return Result<WorkflowExecution>.WithSuccess(execution);
        }
        catch (OperationCanceledException)
        {
            return ResultExtensions.Cancelled<WorkflowExecution>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming workflow execution {ExecutionId}", executionId);
            return Result<WorkflowExecution>.WithFailure($"Resume execution failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<StepExecution>> ExecuteStepAsync(
        WorkflowStep step, 
        WorkflowExecution execution, 
        CancellationToken cancellationToken = default)
    {
        if (step == null)
            return Result<StepExecution>.WithFailure("Step cannot be null");

        if (execution == null)
            return Result<StepExecution>.WithFailure("Execution cannot be null");

        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<StepExecution>();

        var stepExecution = new StepExecution
        {
            Id = Guid.NewGuid(),
            WorkflowExecutionId = execution.Id,
            StepId = step.Id,
            Status = StepExecutionStatus.Running,
            StartedAt = DateTime.UtcNow,
            Input = PrepareStepInput(step, execution)
        };

        try
        {
            _logger.LogDebug("Executing step {StepId} in workflow execution {ExecutionId}", step.Id, execution.Id);

            // Update current step
            execution.CurrentStepId = step.Id;
            await UpdateExecutionProgressAsync(execution, cancellationToken).ConfigureAwait(false);

            // Execute step based on type
            var result = await ExecuteStepByTypeAsync(step, stepExecution, cancellationToken).ConfigureAwait(false);
            
            if (result.IsSuccess)
            {
                stepExecution.Status = StepExecutionStatus.Completed;
                stepExecution.CompletedAt = DateTime.UtcNow;
                stepExecution.Output = result.Value;

                _logger.LogDebug("Step {StepId} completed successfully", step.Id);
            }
            else
            {
                stepExecution.Status = StepExecutionStatus.Failed;
                stepExecution.ErrorMessage = string.Join(", ", result.Errors);
                stepExecution.CompletedAt = DateTime.UtcNow;

                _logger.LogWarning("Step {StepId} failed: {Error}", step.Id, result.Errors?.FirstOrDefault() ?? "Unknown error");
            }

            // Add step execution to collection
            execution.StepExecutions.Add(stepExecution);

            return Result<StepExecution>.WithSuccess(stepExecution);
        }
        catch (OperationCanceledException)
        {
            stepExecution.Status = StepExecutionStatus.Cancelled;
            stepExecution.CompletedAt = DateTime.UtcNow;
            return ResultExtensions.Cancelled<StepExecution>();
        }
        catch (Exception ex)
        {
            stepExecution.Status = StepExecutionStatus.Failed;
            stepExecution.ErrorMessage = ex.Message;
            stepExecution.CompletedAt = DateTime.UtcNow;

            _logger.LogError(ex, "Error executing step {StepId}", step.Id);
            return Result<StepExecution>.WithFailure($"Step execution failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<StepExecutionRecoveryAction>> HandleStepFailureAsync(
        StepExecution stepExecution, 
        Exception error, 
        CancellationToken cancellationToken = default)
    {
        if (stepExecution == null)
            return Result<StepExecutionRecoveryAction>.WithFailure("Step execution cannot be null");

        if (error == null)
            return Result<StepExecutionRecoveryAction>.WithFailure("Error cannot be null");

        try
        {
            await Task.CompletedTask.ConfigureAwait(false);

            _logger.LogWarning("Handling step failure for step {StepId}: {Error}", stepExecution.StepId, error.Message);

            // Determine recovery action based on error type and retry count
            var action = DetermineRecoveryAction(stepExecution, error);

            _logger.LogInformation("Recovery action for step {StepId}: {Action}", stepExecution.StepId, action);

            return Result<StepExecutionRecoveryAction>.WithSuccess(action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling step failure for step {StepId}", stepExecution.StepId);
            return Result<StepExecutionRecoveryAction>.WithFailure($"Error handling step failure: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<bool>> UpdateExecutionProgressAsync(WorkflowExecution execution, CancellationToken cancellationToken = default)
    {
        if (execution == null)
            return Result<bool>.WithFailure("Execution cannot be null");

        try
        {
            var updateResult = await _executionRepository.UpdateExecutionAsync(execution, cancellationToken).ConfigureAwait(false);
            if (!updateResult.IsSuccess)
            {
                return Result<bool>.WithFailure($"Failed to update execution progress: {updateResult.Errors?.FirstOrDefault() ?? "Unknown error"}");
            }

            _logger.LogDebug("Updated execution progress for {ExecutionId}", execution.Id);
            return Result<bool>.WithSuccess(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating execution progress for {ExecutionId}", execution.Id);
            return Result<bool>.WithFailure($"Error updating progress: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<WorkflowExecutionValidationResult>> ValidateExecutionAsync(
        WorkflowExecution execution, 
        CancellationToken cancellationToken = default)
    {
        if (execution == null)
            return Result<WorkflowExecutionValidationResult>.WithFailure("Execution cannot be null");

        try
        {
            await Task.CompletedTask.ConfigureAwait(false);

            var issues = new List<string>();

            // Validate workflow exists
            if (execution.Workflow == null && execution.WorkflowId == Guid.Empty)
            {
                issues.Add("Workflow ID is required");
            }

            // Validate workflow has steps
            if (execution.Workflow?.Definition?.Steps?.Any() != true)
            {
                issues.Add("Workflow must have at least one step");
            }

            // Validate no circular dependencies
            if (execution.Workflow != null && HasCircularDependencies(execution.Workflow))
            {
                issues.Add("Workflow contains circular dependencies");
            }

            var result = issues.Any() 
                ? WorkflowExecutionValidationResult.Invalid(issues)
                : WorkflowExecutionValidationResult.Valid();

            return Result<WorkflowExecutionValidationResult>.WithSuccess(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating execution {ExecutionId}", execution.Id);
            return Result<WorkflowExecutionValidationResult>.WithFailure($"Validation error: {ex.Message}");
        }
    }

    #region Private Methods

    /// <summary>
    /// Executes all workflow steps in the correct order
    /// </summary>
    private async Task<Result<WorkflowExecution>> ExecuteWorkflowStepsAsync(
        WorkflowExecution execution, 
        CancellationToken cancellationToken)
    {
        if (execution.Workflow?.Definition?.Steps == null || !execution.Workflow.Definition.Steps.Any())
        {
            return Result<WorkflowExecution>.WithFailure("Workflow has no steps to execute");
        }

        var orderedSteps = execution.Workflow.Definition.Steps.OrderBy(s => s.Order).ToList();
        
        foreach (var step in orderedSteps)
        {
            if (cancellationToken.IsCancellationRequested)
                return ResultExtensions.Cancelled<WorkflowExecution>();

            // Skip if step already completed (for resume scenarios)
            if (execution.StepExecutions.Any(se => se.StepId == step.Id && se.Status == StepExecutionStatus.Completed))
            {
                continue;
            }

            var stepResult = await ExecuteStepWithRetryAsync(step, execution, cancellationToken).ConfigureAwait(false);
            if (!stepResult.IsSuccess)
            {
                return Result<WorkflowExecution>.WithFailure(stepResult.Errors?.FirstOrDefault() ?? "Unknown error");
            }

            // Update progress after each step
            await UpdateExecutionProgressAsync(execution, cancellationToken).ConfigureAwait(false);
        }

        return Result<WorkflowExecution>.WithSuccess(execution);
    }

    /// <summary>
    /// Executes a step with retry logic
    /// </summary>
    private async Task<Result<StepExecution>> ExecuteStepWithRetryAsync(
        WorkflowStep step, 
        WorkflowExecution execution, 
        CancellationToken cancellationToken)
    {
        var retryCount = 0;
        StepExecution? lastStepExecution = null;

        while (retryCount <= MaxRetryAttempts)
        {
            var stepResult = await ExecuteStepAsync(step, execution, cancellationToken).ConfigureAwait(false);
            
            if (stepResult.IsSuccess && stepResult.Value.Status == StepExecutionStatus.Completed)
            {
                return stepResult;
            }

            lastStepExecution = stepResult.Value;
            var error = new Exception(stepResult.Errors?.FirstOrDefault() ?? "Unknown error" ?? "Step execution failed");

            var recoveryResult = await HandleStepFailureAsync(lastStepExecution, error, cancellationToken).ConfigureAwait(false);
            if (!recoveryResult.IsSuccess)
            {
                return Result<StepExecution>.WithFailure($"Recovery handling failed: {recoveryResult.Errors?.FirstOrDefault() ?? "Unknown error"}");
            }

            switch (recoveryResult.Value)
            {
                case StepExecutionRecoveryAction.Retry:
                    retryCount++;
                    if (retryCount <= MaxRetryAttempts)
                    {
                        _logger.LogInformation("Retrying step {StepId}, attempt {Attempt}", step.Id, retryCount);
                        await Task.Delay(RetryDelay, cancellationToken).ConfigureAwait(false);
                        continue;
                    }
                    break;

                case StepExecutionRecoveryAction.Skip:
                    _logger.LogWarning("Skipping failed step {StepId}", step.Id);
                    lastStepExecution.Status = StepExecutionStatus.Skipped;
                    return Result<StepExecution>.WithSuccess(lastStepExecution);

                case StepExecutionRecoveryAction.FailWorkflow:
                    return Result<StepExecution>.WithFailure($"Step {step.Id} failed and cannot be recovered");

                case StepExecutionRecoveryAction.PauseForIntervention:
                    execution.Status = WorkflowExecutionStatus.Paused;
                    await _executionRepository.UpdateExecutionAsync(execution, cancellationToken).ConfigureAwait(false);
                    return Result<StepExecution>.WithFailure($"Step {step.Id} failed, workflow paused for intervention");
            }

            break;
        }

        return Result<StepExecution>.WithFailure($"Step {step.Id} failed after {MaxRetryAttempts} retry attempts");
    }

    /// <summary>
    /// Executes a step based on its type
    /// </summary>
    private async Task<Result<Dictionary<string, object>>> ExecuteStepByTypeAsync(
        WorkflowStep step, 
        StepExecution stepExecution, 
        CancellationToken cancellationToken)
    {
        // This is a simplified implementation. In a real system, you would have different step types
        // and corresponding executors (e.g., AgentStepExecutor, TaskStepExecutor, etc.)
        
        try
        {
            await Task.Delay(100, cancellationToken).ConfigureAwait(false); // Simulate work

            // For now, just return success with empty output
            var output = new Dictionary<string, object>
            {
                ["executedAt"] = DateTime.UtcNow,
                ["stepType"] = step.Name,
                ["success"] = true
            };

            return Result<Dictionary<string, object>>.WithSuccess(output);
        }
        catch (Exception ex)
        {
            return Result<Dictionary<string, object>>.WithFailure($"Step execution failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Prepares input data for a step execution
    /// </summary>
    private Dictionary<string, object> PrepareStepInput(WorkflowStep step, WorkflowExecution execution)
    {
        var input = new Dictionary<string, object>(execution.Input);

        // Add outputs from previous steps
        foreach (var stepExecution in execution.StepExecutions.Where(se => se.Status == StepExecutionStatus.Completed))
        {
            foreach (var output in stepExecution.Output)
            {
                input[$"step_{stepExecution.StepId}_{output.Key}"] = output.Value;
            }
        }

        return input;
    }

    /// <summary>
    /// Determines the recovery action for a failed step
    /// </summary>
    private StepExecutionRecoveryAction DetermineRecoveryAction(StepExecution stepExecution, Exception error)
    {
        // Simple logic - can be enhanced with more sophisticated error analysis
        if (stepExecution.RetryCount < MaxRetryAttempts)
        {
            return StepExecutionRecoveryAction.Retry;
        }

        // For certain error types, we might want to skip or pause
        if (error is ArgumentException or InvalidOperationException)
        {
            return StepExecutionRecoveryAction.Skip;
        }

        return StepExecutionRecoveryAction.FailWorkflow;
    }

    /// <summary>
    /// Checks if the workflow has circular dependencies
    /// </summary>
    private bool HasCircularDependencies(Workflow workflow)
    {
        // Simple check - can be enhanced with proper graph traversal
        var stepIds = workflow.Definition.Steps.Select(s => s.Id).ToHashSet();
        
        // For now, just check if all step IDs are unique
        return workflow.Definition.Steps.Count != stepIds.Count;
    }

    #endregion
}