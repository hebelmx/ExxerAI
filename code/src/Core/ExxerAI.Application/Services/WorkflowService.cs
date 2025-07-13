using Microsoft.Extensions.Logging;
using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.Entities;
using ExxerAI.Domain.ValueObjects;
using ExxerAI.Domain.Configurations;
using ExxerAI.Domain.Operations;

namespace ExxerAI.Application.Services;

/// <summary>
/// Service for managing workflow operations including creation, execution, and lifecycle management
/// </summary>
public class WorkflowService : IWorkflowService
{
	private readonly IWorkflowRepository _workflowRepository;
	private readonly IWorkflowExecutionRepository _executionRepository;
	private readonly IWorkflowExecutionEngine _executionEngine;
	private readonly ILogger<WorkflowService> _logger;

	/// <summary>
	/// Initializes a new instance of the WorkflowService
	/// </summary>
	/// <param name="workflowRepository">Repository for workflow operations</param>
	/// <param name="executionRepository">Repository for workflow execution operations</param>
	/// <param name="executionEngine">Engine for workflow execution</param>
	/// <param name="logger">Logger for recording service operations</param>
	/// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
	public WorkflowService(
		IWorkflowRepository workflowRepository,
		IWorkflowExecutionRepository executionRepository,
		IWorkflowExecutionEngine executionEngine,
		ILogger<WorkflowService> logger)
	{
		_workflowRepository = workflowRepository ?? throw new ArgumentNullException(nameof(workflowRepository));
		_executionRepository = executionRepository ?? throw new ArgumentNullException(nameof(executionRepository));
		_executionEngine = executionEngine ?? throw new ArgumentNullException(nameof(executionEngine));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Creates a new workflow
	/// </summary>
	/// <param name="name">The workflow name</param>
	/// <param name="description">The workflow description</param>
	/// <param name="steps">The workflow steps</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result of the operation containing the created workflow</returns>
	public async Task<Result<Workflow>> CreateWorkflowAsync(
		string name, 
		string description, 
		IEnumerable<WorkflowStep> steps, 
		CancellationToken cancellationToken = default)
	{
		// Early cancellation check
		if (cancellationToken.IsCancellationRequested)
			return ResultExtensions.Cancelled<Workflow>();

		try
		{
			if (string.IsNullOrWhiteSpace(name))
				return Result<Workflow>.WithFailure("Workflow name cannot be null or empty");

			var workflow = new Workflow
			{
				Name = name,
				Description = description ?? string.Empty,
				Definition = new WorkflowDefinition
				{
					Steps = steps?.ToList() ?? []
				},
				Status = WorkflowStatus.Draft,
				CreatedAt = DateTime.UtcNow
			};

			var result = await _workflowRepository.AddAsync(workflow, cancellationToken).ConfigureAwait(false);
			return result.IsFailure ? Result<Workflow>.WithFailure(result.Error ?? "Failed to add workflow") : Result<Workflow>.WithSuccess(workflow);
		}
		catch (OperationCanceledException)
		{
			return ResultExtensions.Cancelled<Workflow>();
		}
		catch (Exception ex)
		{
			return Result<Workflow>.WithFailure($"Error creating workflow: {ex.Message}");
		}
	}

	/// <summary>
	/// Gets a workflow by its identifier
	/// </summary>
	/// <param name="workflowId">The workflow identifier</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result of the operation containing the workflow if found</returns>
	public async Task<Result<Workflow>> GetWorkflowAsync(Guid workflowId, CancellationToken cancellationToken = default)
	{
		// Early cancellation check
		if (cancellationToken.IsCancellationRequested)
			return ResultExtensions.Cancelled<Workflow>();

		try
		{
			return await _workflowRepository.GetByIdAsync(workflowId, cancellationToken).ConfigureAwait(false);
		}
		catch (OperationCanceledException)
		{
			return ResultExtensions.Cancelled<Workflow>();
		}
		catch (Exception ex)
		{
			return Result<Workflow>.WithFailure($"Error retrieving workflow: {ex.Message}");
		}
	}

	/// <summary>
	/// Gets all active workflows
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result of the operation containing the list of active workflows</returns>
	public async Task<Result<IEnumerable<Workflow>>> GetActiveWorkflowsAsync(CancellationToken cancellationToken = default)
	{
		// Early cancellation check
		if (cancellationToken.IsCancellationRequested)
			return ResultExtensions.Cancelled<IEnumerable<Workflow>>();

		try
		{
			return await _workflowRepository.GetByStatusAsync(WorkflowStatus.Active, cancellationToken).ConfigureAwait(false);
		}
		catch (OperationCanceledException)
		{
			return ResultExtensions.Cancelled<IEnumerable<Workflow>>();
		}
		catch (Exception ex)
		{
			return Result<IEnumerable<Workflow>>.WithFailure($"Error retrieving active workflows: {ex.Message}");
		}
	}

	/// <summary>
	/// Updates a workflow's configuration
	/// </summary>
	/// <param name="workflowId">The workflow identifier</param>
	/// <param name="configuration">The new configuration</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result of the operation</returns>
	public async Task<Result<bool>> UpdateWorkflowConfigurationAsync(
		Guid workflowId, 
		WorkflowConfiguration configuration, 
		CancellationToken cancellationToken = default)
	{
		// Early cancellation check
		if (cancellationToken.IsCancellationRequested)
			return ResultExtensions.Cancelled<bool>();

		try
		{
			var workflowResult = await _workflowRepository.GetByIdAsync(workflowId, cancellationToken).ConfigureAwait(false);
			if (workflowResult.IsFailure) 
				return Result<bool>.WithFailure($"Workflow {workflowId} not found");

			var workflow = workflowResult.Value!;
			workflow.Definition.Configuration = configuration ?? new WorkflowConfiguration();

			var updateResult = await _workflowRepository.UpdateAsync(workflow, cancellationToken).ConfigureAwait(false);
			return updateResult.IsFailure ? Result<bool>.WithFailure(updateResult.Error ?? "Failed to update workflow") : Result<bool>.WithSuccess(true);
		}
		catch (OperationCanceledException)
		{
			return ResultExtensions.Cancelled<bool>();
		}
		catch (Exception ex)
		{
			return Result<bool>.WithFailure($"Error updating workflow configuration: {ex.Message}");
		}
	}

	/// <summary>
	/// Starts execution of a workflow
	/// </summary>
	/// <param name="workflowId">The workflow identifier</param>
	/// <param name="input">The input data for the workflow</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result of the operation containing the workflow execution</returns>
	public async Task<Result<WorkflowExecution>> ExecuteWorkflowAsync(
		Guid workflowId, 
		Dictionary<string, object> input, 
		CancellationToken cancellationToken = default)
	{
		// Early cancellation check
		if (cancellationToken.IsCancellationRequested)
			return ResultExtensions.Cancelled<WorkflowExecution>();

		try
		{
			var workflowResult = await _workflowRepository.GetByIdAsync(workflowId, cancellationToken).ConfigureAwait(false);
			if (workflowResult.IsFailure) 
				return Result<WorkflowExecution>.WithFailure($"Workflow {workflowId} not found");

			var execution = new WorkflowExecution
			{
				WorkflowId = workflowId,
				Input = input ?? [],
				Status = WorkflowExecutionStatus.Running,
				StartedAt = DateTime.UtcNow
			};

			return Result<WorkflowExecution>.WithSuccess(execution);
		}
		catch (OperationCanceledException)
		{
			return ResultExtensions.Cancelled<WorkflowExecution>();
		}
		catch (Exception ex)
		{
			return Result<WorkflowExecution>.WithFailure($"Error executing workflow: {ex.Message}");
		}
	}

	/// <summary>
	/// Gets a workflow execution by its identifier
	/// </summary>
	/// <param name="executionId">The execution identifier</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result of the operation containing the execution if found</returns>
	public async Task<Result<WorkflowExecution>> GetWorkflowExecutionAsync(
		Guid executionId, 
		CancellationToken cancellationToken = default)
	{
		// Early cancellation check
		if (cancellationToken.IsCancellationRequested)
			return ResultExtensions.Cancelled<WorkflowExecution>();

		try
		{
			var result = await _executionRepository.GetExecutionAsync(executionId, cancellationToken).ConfigureAwait(false);
			if (result.IsSuccess)
			{
				_logger.LogDebug("Retrieved workflow execution {ExecutionId} with status {Status}", 
					executionId, result.Value.Status);
			}
			else
			{
				_logger.LogWarning("Failed to retrieve workflow execution {ExecutionId}: {Error}", 
					executionId, string.Join(", ", result.Errors));
			}
			return result;
		}
		catch (OperationCanceledException)
		{
			return ResultExtensions.Cancelled<WorkflowExecution>();
		}
		catch (Exception ex)
		{
			return Result<WorkflowExecution>.WithFailure($"Error retrieving execution: {ex.Message}");
		}
	}

	/// <summary>
	/// Gets all executions for a specific workflow
	/// </summary>
	/// <param name="workflowId">The workflow identifier</param>
	/// <param name="status">Optional agentStatus filter</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result of the operation containing the list of executions</returns>
	public async Task<Result<IEnumerable<WorkflowExecution>>> GetWorkflowExecutionsAsync(
		Guid workflowId, 
		WorkflowExecutionStatus? status = null, 
		CancellationToken cancellationToken = default)
	{
		// Early cancellation check
		if (cancellationToken.IsCancellationRequested)
			return ResultExtensions.Cancelled<IEnumerable<WorkflowExecution>>();

		try
		{
			return await _workflowRepository.GetExecutionsAsync(workflowId, status, cancellationToken).ConfigureAwait(false);
		}
		catch (OperationCanceledException)
		{
			return ResultExtensions.Cancelled<IEnumerable<WorkflowExecution>>();
		}
		catch (Exception ex)
		{
			return Result<IEnumerable<WorkflowExecution>>.WithFailure($"Error retrieving workflow executions: {ex.Message}");
		}
	}

	/// <summary>
	/// Pauses a running workflow execution
	/// </summary>
	/// <param name="executionId">The execution identifier</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result of the operation</returns>
	public async Task<Result<bool>> PauseWorkflowExecutionAsync(Guid executionId, CancellationToken cancellationToken = default)
	{
		// Early cancellation check
		if (cancellationToken.IsCancellationRequested)
			return ResultExtensions.Cancelled<bool>();

		try
		{
			var executionResult = await _executionRepository.GetExecutionAsync(executionId, cancellationToken).ConfigureAwait(false);
			if (!executionResult.IsSuccess)
			{
				_logger.LogWarning("Cannot pause execution {ExecutionId}: execution not found", executionId);
				return Result<bool>.WithFailure($"Execution not found: {string.Join(", ", executionResult.Errors)}");
			}

			var execution = executionResult.Value;
			if (execution.Status != WorkflowExecutionStatus.Running)
			{
				_logger.LogWarning("Cannot pause execution {ExecutionId} in {Status} status", executionId, execution.Status);
				return Result<bool>.WithFailure($"Cannot pause execution in {execution.Status} status");
			}

			execution.Status = WorkflowExecutionStatus.Paused;
			execution.LastModified = DateTime.UtcNow;

			var updateResult = await _executionRepository.UpdateExecutionAsync(execution, cancellationToken).ConfigureAwait(false);
			if (!updateResult.IsSuccess)
			{
				_logger.LogError("Failed to update execution {ExecutionId} status to paused: {Error}", 
					executionId, string.Join(", ", updateResult.Errors));
				return Result<bool>.WithFailure($"Failed to update execution: {string.Join(", ", updateResult.Errors)}");
			}

			_logger.LogInformation("Execution {ExecutionId} paused successfully", executionId);
			return Result<bool>.WithSuccess(true);
		}
		catch (OperationCanceledException)
		{
			return ResultExtensions.Cancelled<bool>();
		}
		catch (Exception ex)
		{
			return Result<bool>.WithFailure($"Error pausing execution: {ex.Message}");
		}
	}

	/// <summary>
	/// Resumes a paused workflow execution
	/// </summary>
	/// <param name="executionId">The execution identifier</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result of the operation</returns>
	public async Task<Result<bool>> ResumeWorkflowExecutionAsync(Guid executionId, CancellationToken cancellationToken = default)
	{
		// Early cancellation check
		if (cancellationToken.IsCancellationRequested)
			return ResultExtensions.Cancelled<bool>();

		try
		{
			var executionResult = await _executionRepository.GetExecutionAsync(executionId, cancellationToken).ConfigureAwait(false);
			if (!executionResult.IsSuccess)
			{
				_logger.LogWarning("Cannot resume execution {ExecutionId}: execution not found", executionId);
				return Result<bool>.WithFailure($"Execution not found: {string.Join(", ", executionResult.Errors)}");
			}

			var execution = executionResult.Value;
			if (execution.Status != WorkflowExecutionStatus.Paused)
			{
				_logger.LogWarning("Cannot resume execution {ExecutionId} in {Status} status", executionId, execution.Status);
				return Result<bool>.WithFailure($"Cannot resume execution in {execution.Status} status");
			}

			// Use the execution engine to resume the workflow
			var resumeResult = await _executionEngine.ResumeExecutionAsync(executionId, cancellationToken).ConfigureAwait(false);
			if (!resumeResult.IsSuccess)
			{
				_logger.LogError("Failed to resume execution {ExecutionId}: {Error}", executionId, string.Join(", ", resumeResult.Errors));
				return Result<bool>.WithFailure($"Failed to resume execution: {string.Join(", ", resumeResult.Errors)}");
			}

			_logger.LogInformation("Execution {ExecutionId} resumed successfully", executionId);
			return Result<bool>.WithSuccess(true);
		}
		catch (OperationCanceledException)
		{
			return ResultExtensions.Cancelled<bool>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error resuming execution {ExecutionId}", executionId);
			return Result<bool>.WithFailure($"Error resuming execution: {ex.Message}");
		}
	}

	/// <summary>
	/// Cancels a workflow execution
	/// </summary>
	/// <param name="executionId">The execution identifier</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result of the operation</returns>
	public async Task<Result<bool>> CancelWorkflowExecutionAsync(Guid executionId, CancellationToken cancellationToken = default)
	{
		// Early cancellation check
		if (cancellationToken.IsCancellationRequested)
			return ResultExtensions.Cancelled<bool>();

		try
		{
			var executionResult = await _executionRepository.GetExecutionAsync(executionId, cancellationToken).ConfigureAwait(false);
			if (!executionResult.IsSuccess)
			{
				_logger.LogWarning("Cannot cancel execution {ExecutionId}: execution not found", executionId);
				return Result<bool>.WithFailure($"Execution not found: {string.Join(", ", executionResult.Errors)}");
			}

			var execution = executionResult.Value;
			if (execution.Status == WorkflowExecutionStatus.Completed || 
			    execution.Status == WorkflowExecutionStatus.Failed || 
			    execution.Status == WorkflowExecutionStatus.Cancelled)
			{
				_logger.LogWarning("Cannot cancel execution {ExecutionId} in {Status} status", executionId, execution.Status);
				return Result<bool>.WithFailure($"Cannot cancel execution in {execution.Status} status");
			}

			execution.Status = WorkflowExecutionStatus.Cancelled;
			execution.LastModified = DateTime.UtcNow;
			execution.CompletedAt = DateTime.UtcNow;

			var updateResult = await _executionRepository.UpdateExecutionAsync(execution, cancellationToken).ConfigureAwait(false);
			if (!updateResult.IsSuccess)
			{
				_logger.LogError("Failed to update execution {ExecutionId} status to cancelled: {Error}", 
					executionId, string.Join(", ", updateResult.Errors));
				return Result<bool>.WithFailure($"Failed to update execution: {string.Join(", ", updateResult.Errors)}");
			}

			_logger.LogInformation("Execution {ExecutionId} cancelled successfully", executionId);
			return Result<bool>.WithSuccess(true);
		}
		catch (OperationCanceledException)
		{
			return ResultExtensions.Cancelled<bool>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error cancelling execution {ExecutionId}", executionId);
			return Result<bool>.WithFailure($"Error cancelling execution: {ex.Message}");
		}
	}

	/// <summary>
	/// Deletes a workflow
	/// </summary>
	/// <param name="workflowId">The workflow identifier</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result of the operation</returns>
	public async Task<Result<bool>> DeleteWorkflowAsync(Guid workflowId, CancellationToken cancellationToken = default)
	{
		// Early cancellation check
		if (cancellationToken.IsCancellationRequested)
			return ResultExtensions.Cancelled<bool>();

		try
		{
			var workflowResult = await _workflowRepository.GetByIdAsync(workflowId, cancellationToken).ConfigureAwait(false);
			if (workflowResult.IsFailure) 
				return Result<bool>.WithFailure($"Workflow {workflowId} not found");

			var deleteResult = await _workflowRepository.DeleteAsync(workflowId, cancellationToken).ConfigureAwait(false);
			return deleteResult.IsFailure ? Result<bool>.WithFailure(deleteResult.Error ?? "Failed to delete workflow") : Result<bool>.WithSuccess(true);
		}
		catch (OperationCanceledException)
		{
			return ResultExtensions.Cancelled<bool>();
		}
		catch (Exception ex)
		{
			return Result<bool>.WithFailure($"Error deleting workflow: {ex.Message}");
		}
	}
}
