using Microsoft.AspNetCore.Mvc;
using ExxerAI.Application.Interfaces;
using ExxerAI.Api.Models;
using ExxerAI.Domain;

namespace ExxerAI.Api.Controllers;

/// <summary>
/// Controller for managing AI agents
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AgentsController : ControllerBase
{
	private readonly IAgentService _agentService;
	private readonly ILogger<AgentsController> _logger;

	/// <summary>
	/// Initializes a new instance of the AgentsController
	/// </summary>
	/// <param name="agentService">The agent service</param>
	/// <param name="logger">The logger</param>
	public AgentsController(IAgentService agentService, ILogger<AgentsController> logger)
	{
		_agentService = agentService ?? throw new ArgumentNullException(nameof(agentService));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Creates a new agent
	/// </summary>
	/// <param name="request">The agent creation request</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The created agent</returns>
	/// <response code="201">Agent created successfully</response>
	/// <response code="400">Invalid request data</response>
	/// <response code="500">Internal server error</response>
	[HttpPost]
	[ProducesResponseType(typeof(ApiResponse<AgentResponse>), StatusCodes.Status201Created)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<ApiResponse<AgentResponse>>> CreateAgent(
		[FromBody] CreateAgentRequest request,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation("Creating new agent: {AgentName}", request.Name);

			if (!ModelState.IsValid)
			{
				var errors = ModelState
					.SelectMany(x => x.Value!.Errors)
					.Select(x => x.ErrorMessage)
					.ToList();

				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Invalid request data",
					Errors = errors
				});
			}

			var result = await _agentService.CreateAgentAsync(
				request.Name,
				request.Description,
				request.Capabilities.ToDomain(),
				cancellationToken);

			if (result.IsFailure)
			{
				_logger.LogWarning("Failed to create agent: {Error}", result.Error);
				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to create agent",
					Errors = [result.Error]
				});
			}

			var response = new ApiResponse<AgentResponse>
			{
				Success = true,
				Message = "Agent created successfully",
				Data = result.Data!.ToResponse()
			};

			_logger.LogInformation("Successfully created agent with ID: {AgentId}", result.Data!.Id);
			return CreatedAtAction(nameof(GetAgent), new { id = result.Data.Id }, response);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating agent");
			return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
			{
				Success = false,
				Message = "An internal error occurred",
				Errors = new List<string> { ex.Message }
			});
		}
	}

	/// <summary>
	/// Gets an agent by ID
	/// </summary>
	/// <param name="id">The agent identifier</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The agent</returns>
	/// <response code="200">Agent found</response>
	/// <response code="404">Agent not found</response>
	/// <response code="500">Internal server error</response>
	[HttpGet("{id:guid}")]
	[ProducesResponseType(typeof(ApiResponse<AgentResponse>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<ApiResponse<AgentResponse>>> GetAgent(
		Guid id,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation("Retrieving agent with ID: {AgentId}", id);

			var result = await _agentService.GetAgentAsync(id, cancellationToken);

			if (result.IsFailure)
			{
				_logger.LogWarning("Agent not found with ID: {AgentId}", id);
				return NotFound(new ApiResponse<object>
				{
					Success = false,
					Message = "Agent not found",
					Errors = new List<string> { result.Error }
				});
			}

			var response = new ApiResponse<AgentResponse>
			{
				Success = true,
				Message = "Agent retrieved successfully",
				Data = result.Data!.ToResponse()
			};

			return Ok(response);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving agent with ID: {AgentId}", id);
			return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
			{
				Success = false,
				Message = "An internal error occurred",
				Errors = new List<string> { ex.Message }
			});
		}
	}

	/// <summary>
	/// Gets all active agents
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The list of active agents</returns>
	/// <response code="200">Agents retrieved successfully</response>
	/// <response code="500">Internal server error</response>
	[HttpGet]
	[ProducesResponseType(typeof(ApiResponse<IEnumerable<AgentResponse>>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<ApiResponse<IEnumerable<AgentResponse>>>> GetActiveAgents(
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation("Retrieving all active agents");

			var result = await _agentService.GetActiveAgentsAsync(cancellationToken);

			if (result.IsFailure)
			{
				_logger.LogError("Failed to retrieve active agents: {Error}", result.Error);
				return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to retrieve agents",
					Errors = new List<string> { result.Error }
				});
			}

			var response = new ApiResponse<IEnumerable<AgentResponse>>
			{
				Success = true,
				Message = $"Retrieved {result.Data!.Count()} active agents",
				Data = result.Data!.Select(a => a.ToResponse())
			};

			return Ok(response);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving active agents");
			return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
			{
				Success = false,
				Message = "An internal error occurred",
				Errors = new List<string> { ex.Message }
			});
		}
	}

	/// <summary>
	/// Updates an agent's configuration
	/// </summary>
	/// <param name="id">The agent identifier</param>
	/// <param name="request">The configuration update request</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The update result</returns>
	/// <response code="204">Configuration updated successfully</response>
	/// <response code="400">Invalid request data</response>
	/// <response code="404">Agent not found</response>
	/// <response code="500">Internal server error</response>
	[HttpPut("{id:guid}/configuration")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult> UpdateAgentConfiguration(
		Guid id,
		[FromBody] UpdateAgentConfigurationRequest request,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation("Updating configuration for agent: {AgentId}", id);

			if (!ModelState.IsValid)
			{
				var errors = ModelState
					.SelectMany(x => x.Value!.Errors)
					.Select(x => x.ErrorMessage)
					.ToList();

				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Invalid request data",
					Errors = errors
				});
			}

			var result = await _agentService.UpdateAgentConfigurationAsync(
				id,
				request.ToDomain(),
				cancellationToken);

			if (result.IsFailure)
			{
				if (result.Error.Contains("not found"))
				{
					return NotFound(new ApiResponse<object>
					{
						Success = false,
						Message = "Agent not found",
						Errors = new List<string> { result.Error }
					});
				}

				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to update agent configuration",
					Errors = new List<string> { result.Error }
				});
			}

			_logger.LogInformation("Successfully updated configuration for agent: {AgentId}", id);
			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating configuration for agent: {AgentId}", id);
			return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
			{
				Success = false,
				Message = "An internal error occurred",
				Errors = new List<string> { ex.Message }
			});
		}
	}

	/// <summary>
	/// Updates an agent's status
	/// </summary>
	/// <param name="id">The agent identifier</param>
	/// <param name="request">The status update request</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The update result</returns>
	/// <response code="204">Status updated successfully</response>
	/// <response code="400">Invalid request data</response>
	/// <response code="404">Agent not found</response>
	/// <response code="500">Internal server error</response>
	[HttpPut("{id:guid}/status")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult> UpdateAgentStatus(
		Guid id,
		[FromBody] UpdateAgentStatusRequest request,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation("Updating status for agent: {AgentId} to {Status}", id, request.Status);

			if (!ModelState.IsValid)
			{
				var errors = ModelState
					.SelectMany(x => x.Value!.Errors)
					.Select(x => x.ErrorMessage)
					.ToList();

				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Invalid request data",
					Errors = errors
				});
			}

			var result = await _agentService.UpdateAgentStatusAsync(id, request.Status, cancellationToken);

			if (result.IsFailure)
			{
				if (result.Error.Contains("not found"))
				{
					return NotFound(new ApiResponse<object>
					{
						Success = false,
						Message = "Agent not found",
						Errors = new List<string> { result.Error }
					});
				}

				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to update agent status",
					Errors = new List<string> { result.Error }
				});
			}

			_logger.LogInformation("Successfully updated status for agent: {AgentId} to {Status}", id, request.Status);
			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating status for agent: {AgentId}", id);
			return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
			{
				Success = false,
				Message = "An internal error occurred",
				Errors = new List<string> { ex.Message }
			});
		}
	}

	/// <summary>
	/// Assigns a task to an agent
	/// </summary>
	/// <param name="id">The agent identifier</param>
	/// <param name="request">The task assignment request</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The assignment result</returns>
	/// <response code="204">Task assigned successfully</response>
	/// <response code="400">Invalid request data or assignment failed</response>
	/// <response code="404">Agent not found</response>
	/// <response code="500">Internal server error</response>
	[HttpPost("{id:guid}/tasks")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult> AssignTask(
		Guid id,
		[FromBody] AssignTaskRequest request,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation("Assigning task {TaskId} to agent {AgentId}", request.TaskId, id);

			if (!ModelState.IsValid)
			{
				var errors = ModelState
					.SelectMany(x => x.Value!.Errors)
					.Select(x => x.ErrorMessage)
					.ToList();

				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Invalid request data",
					Errors = errors
				});
			}

			var result = await _agentService.AssignTaskAsync(id, request.TaskId, cancellationToken);

			if (result.IsFailure)
			{
				if (result.Error.Contains("not found"))
				{
					return NotFound(new ApiResponse<object>
					{
						Success = false,
						Message = "Agent or task not found",
						Errors = new List<string> { result.Error }
					});
				}

				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to assign task",
					Errors = new List<string> { result.Error }
				});
			}

			_logger.LogInformation("Successfully assigned task {TaskId} to agent {AgentId}", request.TaskId, id);
			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error assigning task {TaskId} to agent {AgentId}", request.TaskId, id);
			return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
			{
				Success = false,
				Message = "An internal error occurred",
				Errors = new List<string> { ex.Message }
			});
		}
	}

	/// <summary>
	/// Finds the best available agent for a specific task type
	/// </summary>
	/// <param name="taskType">The task type</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The best agent for the task type</returns>
	/// <response code="200">Best agent found</response>
	/// <response code="404">No suitable agent found</response>
	/// <response code="500">Internal server error</response>
	[HttpGet("best-for-task/{taskType}")]
	[ProducesResponseType(typeof(ApiResponse<AgentResponse>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<ApiResponse<AgentResponse>>> FindBestAgentForTask(
		string taskType,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation("Finding best agent for task type: {TaskType}", taskType);

			var result = await _agentService.FindBestAgentForTaskAsync(taskType, cancellationToken);

			if (result.IsFailure)
			{
				_logger.LogWarning("No suitable agent found for task type: {TaskType}", taskType);
				return NotFound(new ApiResponse<object>
				{
					Success = false,
					Message = "No suitable agent found",
					Errors = new List<string> { result.Error }
				});
			}

			var response = new ApiResponse<AgentResponse>
			{
				Success = true,
				Message = "Best agent found",
				Data = result.Data!.ToResponse()
			};

			return Ok(response);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error finding best agent for task type: {TaskType}", taskType);
			return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
			{
				Success = false,
				Message = "An internal error occurred",
				Errors = new List<string> { ex.Message }
			});
		}
	}

	/// <summary>
	/// Deletes an agent
	/// </summary>
	/// <param name="id">The agent identifier</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The deletion result</returns>
	/// <response code="204">Agent deleted successfully</response>
	/// <response code="400">Cannot delete agent with active tasks</response>
	/// <response code="404">Agent not found</response>
	/// <response code="500">Internal server error</response>
	[HttpDelete("{id:guid}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult> DeleteAgent(
		Guid id,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation("Deleting agent: {AgentId}", id);

			var result = await _agentService.DeleteAgentAsync(id, cancellationToken);

			if (result.IsFailure)
			{
				if (result.Error.Contains("not found"))
				{
					return NotFound(new ApiResponse<object>
					{
						Success = false,
						Message = "Agent not found",
						Errors = new List<string> { result.Error }
					});
				}

				return BadRequest(new ApiResponse<object>
				{
					Success = false,
					Message = "Failed to delete agent",
					Errors = new List<string> { result.Error }
				});
			}

			_logger.LogInformation("Successfully deleted agent: {AgentId}", id);
			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting agent: {AgentId}", id);
			return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
			{
				Success = false,
				Message = "An internal error occurred",
				Errors = new List<string> { ex.Message }
			});
		}
	}
} 