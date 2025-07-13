using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.Entities;
using ExxerAI.Domain.Operations;
using ExxerAI.Domain.ValueObjects;

namespace ExxerAI.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of workflow execution repository for development and testing
/// Provides thread-safe operations using ConcurrentDictionary for high-performance scenarios
/// </summary>
public class InMemoryWorkflowExecutionRepository : IWorkflowExecutionRepository
{
    private readonly ConcurrentDictionary<Guid, WorkflowExecution> _executions = new();
    private readonly ILogger<InMemoryWorkflowExecutionRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the InMemoryWorkflowExecutionRepository
    /// </summary>
    /// <param name="logger">Logger for recording repository operations</param>
    public InMemoryWorkflowExecutionRepository(ILogger<InMemoryWorkflowExecutionRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogInformation("InMemoryWorkflowExecutionRepository initialized");
    }

    /// <inheritdoc />
    public async Task<Result<WorkflowExecution>> GetExecutionAsync(Guid executionId, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<WorkflowExecution>();

        try
        {
            await Task.CompletedTask.ConfigureAwait(false);

            if (_executions.TryGetValue(executionId, out var execution))
            {
                _logger.LogDebug("Retrieved workflow execution {ExecutionId} with status {Status}", executionId, execution.Status);
                return Result<WorkflowExecution>.WithSuccess(execution);
            }

            _logger.LogDebug("Workflow execution {ExecutionId} not found", executionId);
            return Result<WorkflowExecution>.WithFailure($"Workflow execution with ID {executionId} not found");
        }
        catch (OperationCanceledException)
        {
            return ResultExtensions.Cancelled<WorkflowExecution>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow execution {ExecutionId}", executionId);
            return Result<WorkflowExecution>.WithFailure($"Error retrieving execution: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<WorkflowExecution>>> GetExecutionsForWorkflowAsync(
        Guid workflowId, 
        WorkflowExecutionStatus? status = null, 
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<IEnumerable<WorkflowExecution>>();

        try
        {
            await Task.CompletedTask.ConfigureAwait(false);

            var executions = _executions.Values
                .Where(e => e.WorkflowId == workflowId)
                .Where(e => status == null || e.Status == status)
                .OrderByDescending(e => e.CreatedAt)
                .ToList();

            _logger.LogDebug("Retrieved {Count} executions for workflow {WorkflowId} with status filter {Status}", 
                executions.Count, workflowId, status);

            return Result<IEnumerable<WorkflowExecution>>.WithSuccess(executions);
        }
        catch (OperationCanceledException)
        {
            return ResultExtensions.Cancelled<IEnumerable<WorkflowExecution>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving executions for workflow {WorkflowId}", workflowId);
            return Result<IEnumerable<WorkflowExecution>>.WithFailure($"Error retrieving executions: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<WorkflowExecution>> CreateExecutionAsync(WorkflowExecution execution, CancellationToken cancellationToken = default)
    {
        if (execution == null)
            return Result<WorkflowExecution>.WithFailure("Execution cannot be null");

        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<WorkflowExecution>();

        try
        {
            await Task.CompletedTask.ConfigureAwait(false);

            // Ensure the execution has a valid ID
            if (execution.Id == Guid.Empty)
            {
                execution.Id = Guid.NewGuid();
            }

            // Check for duplicate
            if (_executions.ContainsKey(execution.Id))
            {
                _logger.LogWarning("Attempted to create workflow execution with duplicate ID {ExecutionId}", execution.Id);
                return Result<WorkflowExecution>.WithFailure($"Workflow execution with ID {execution.Id} already exists");
            }

            // Set creation timestamp if not set
            if (execution.CreatedAt == default)
            {
                execution.CreatedAt = DateTime.UtcNow;
                execution.LastModified = DateTime.UtcNow;
            }

            if (_executions.TryAdd(execution.Id, execution))
            {
                _logger.LogInformation("Created workflow execution {ExecutionId} for workflow {WorkflowId}", 
                    execution.Id, execution.WorkflowId);
                return Result<WorkflowExecution>.WithSuccess(execution);
            }

            _logger.LogError("Failed to add workflow execution {ExecutionId} to collection", execution.Id);
            return Result<WorkflowExecution>.WithFailure("Failed to create workflow execution");
        }
        catch (OperationCanceledException)
        {
            return ResultExtensions.Cancelled<WorkflowExecution>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workflow execution {ExecutionId}", execution?.Id);
            return Result<WorkflowExecution>.WithFailure($"Error creating execution: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<WorkflowExecution>> UpdateExecutionAsync(WorkflowExecution execution, CancellationToken cancellationToken = default)
    {
        if (execution == null)
            return Result<WorkflowExecution>.WithFailure("Execution cannot be null");

        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<WorkflowExecution>();

        try
        {
            await Task.CompletedTask.ConfigureAwait(false);

            if (!_executions.ContainsKey(execution.Id))
            {
                _logger.LogWarning("Attempted to update non-existent workflow execution {ExecutionId}", execution.Id);
                return Result<WorkflowExecution>.WithFailure($"Workflow execution with ID {execution.Id} not found");
            }

            // Update last modified timestamp
            execution.LastModified = DateTime.UtcNow;

            _executions.TryUpdate(execution.Id, execution, _executions[execution.Id]);

            _logger.LogDebug("Updated workflow execution {ExecutionId} with status {Status}", 
                execution.Id, execution.Status);

            return Result<WorkflowExecution>.WithSuccess(execution);
        }
        catch (OperationCanceledException)
        {
            return ResultExtensions.Cancelled<WorkflowExecution>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating workflow execution {ExecutionId}", execution?.Id);
            return Result<WorkflowExecution>.WithFailure($"Error updating execution: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<bool>> DeleteExecutionAsync(Guid executionId, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<bool>();

        try
        {
            await Task.CompletedTask.ConfigureAwait(false);

            if (_executions.TryRemove(executionId, out var removedExecution))
            {
                _logger.LogInformation("Deleted workflow execution {ExecutionId} for workflow {WorkflowId}", 
                    executionId, removedExecution.WorkflowId);
                return Result<bool>.WithSuccess(true);
            }

            _logger.LogWarning("Attempted to delete non-existent workflow execution {ExecutionId}", executionId);
            return Result<bool>.WithFailure($"Workflow execution with ID {executionId} not found");
        }
        catch (OperationCanceledException)
        {
            return ResultExtensions.Cancelled<bool>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting workflow execution {ExecutionId}", executionId);
            return Result<bool>.WithFailure($"Error deleting execution: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<WorkflowExecution>>> GetActiveExecutionsAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<IEnumerable<WorkflowExecution>>();

        try
        {
            await Task.CompletedTask.ConfigureAwait(false);

            var activeStatuses = new[] { WorkflowExecutionStatus.Running, WorkflowExecutionStatus.Paused };
            var activeExecutions = _executions.Values
                .Where(e => activeStatuses.Contains(e.Status))
                .OrderBy(e => e.CreatedAt)
                .ToList();

            _logger.LogDebug("Retrieved {Count} active workflow executions", activeExecutions.Count);

            return Result<IEnumerable<WorkflowExecution>>.WithSuccess(activeExecutions);
        }
        catch (OperationCanceledException)
        {
            return ResultExtensions.Cancelled<IEnumerable<WorkflowExecution>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active workflow executions");
            return Result<IEnumerable<WorkflowExecution>>.WithFailure($"Error retrieving active executions: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<WorkflowExecution>>> GetExecutionsNeedingResumptionAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<IEnumerable<WorkflowExecution>>();

        try
        {
            await Task.CompletedTask.ConfigureAwait(false);

            var resumptionStatuses = new List<WorkflowExecutionStatus> { WorkflowExecutionStatus.Paused, WorkflowExecutionStatus.Scheduled };
            var executions = _executions.Values
                .Where(e => resumptionStatuses.Contains(e.Status))
                .OrderBy(e => e.CreatedAt)
                .ToList();

            _logger.LogDebug("Retrieved {Count} workflow executions needing resumption", executions.Count);

            return Result<IEnumerable<WorkflowExecution>>.WithSuccess(executions);
        }
        catch (OperationCanceledException)
        {
            return ResultExtensions.Cancelled<IEnumerable<WorkflowExecution>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving executions needing resumption");
            return Result<IEnumerable<WorkflowExecution>>.WithFailure($"Error retrieving executions for resumption: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<WorkflowExecutionStatistics>> GetExecutionStatisticsAsync(Guid? workflowId = null, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<WorkflowExecutionStatistics>();

        try
        {
            await Task.CompletedTask.ConfigureAwait(false);

            var executions = workflowId.HasValue 
                ? _executions.Values.Where(e => e.WorkflowId == workflowId.Value)
                : _executions.Values;

            var executionsList = executions.ToList();

            var completedExecutions = executionsList.Where(e => e.Status == WorkflowExecutionStatus.Completed).ToList();
            var averageDuration = completedExecutions.Any() && completedExecutions.All(e => e.CompletedAt.HasValue)
                ? TimeSpan.FromTicks((long)completedExecutions.Average(e => (e.CompletedAt!.Value - e.CreatedAt).Ticks))
                : (TimeSpan?)null;

            var statistics = new WorkflowExecutionStatistics
            {
                TotalExecutions = executionsList.Count,
                RunningExecutions = executionsList.Count(e => e.Status == WorkflowExecutionStatus.Running),
                PausedExecutions = executionsList.Count(e => e.Status == WorkflowExecutionStatus.Paused),
                CompletedExecutions = executionsList.Count(e => e.Status == WorkflowExecutionStatus.Completed),
                FailedExecutions = executionsList.Count(e => e.Status == WorkflowExecutionStatus.Failed),
                CancelledExecutions = executionsList.Count(e => e.Status == WorkflowExecutionStatus.Cancelled),
                AverageExecutionDuration = averageDuration
            };

            _logger.LogDebug("Generated statistics for {WorkflowFilter}: {Total} total, {Success}% success rate", 
                workflowId?.ToString() ?? "all workflows", 
                statistics.TotalExecutions, 
                Math.Round(statistics.SuccessRate * 100, 2));

            return Result<WorkflowExecutionStatistics>.WithSuccess(statistics);
        }
        catch (OperationCanceledException)
        {
            return ResultExtensions.Cancelled<WorkflowExecutionStatistics>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating execution statistics for workflow {WorkflowId}", workflowId);
            return Result<WorkflowExecutionStatistics>.WithFailure($"Error generating statistics: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the current count of executions in the repository
    /// Useful for monitoring and testing purposes
    /// </summary>
    /// <returns>Current number of executions stored</returns>
    public int GetExecutionCount() => _executions.Count;

    /// <summary>
    /// Clears all executions from the repository
    /// Primarily used for testing scenarios
    /// </summary>
    public void Clear()
    {
        var count = _executions.Count;
        _executions.Clear();
        _logger.LogInformation("Cleared {Count} workflow executions from repository", count);
    }
}