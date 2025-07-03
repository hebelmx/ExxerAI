using System.ComponentModel.DataAnnotations;
using ExxerAI.Domain;

namespace ExxerAI.Api.Models;

/// <summary>
/// Request model for creating a new agent
/// </summary>
public class CreateAgentRequest
{
	/// <summary>
	/// Gets or sets the agent name
	/// </summary>
	[Required]
	[StringLength(100, MinimumLength = 1)]
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the agent description
	/// </summary>
	[StringLength(500)]
	public string Description { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the agent capabilities
	/// </summary>
	[Required]
	public AgentCapabilitiesDto Capabilities { get; set; } = new();
}

/// <summary>
/// Request model for updating agent configuration
/// </summary>
public class UpdateAgentConfigurationRequest
{
	/// <summary>
	/// Gets or sets the timeout for task execution in seconds
	/// </summary>
	[Range(1, 3600)]
	public int TaskTimeoutSeconds { get; set; } = 300;

	/// <summary>
	/// Gets or sets the maximum retries for failed tasks
	/// </summary>
	[Range(0, 10)]
	public int MaxRetries { get; set; } = 3;

	/// <summary>
	/// Gets or sets the agent's priority level
	/// </summary>
	[Range(1, 10)]
	public int Priority { get; set; } = 1;

	/// <summary>
	/// Gets or sets custom configuration properties
	/// </summary>
	public Dictionary<string, object> CustomProperties { get; set; } = new();
}

/// <summary>
/// Request model for updating agent agentStatus
/// </summary>
public class UpdateAgentStatusRequest
{
	/// <summary>
	/// Gets or sets the new agentStatus for the agent
	/// </summary>
	[Required]
	public AgentStatus Status { get; set; }
}

/// <summary>
/// Request model for assigning a task to an agent
/// </summary>
public class AssignTaskRequest
{
	/// <summary>
	/// Gets or sets the task identifier to assign
	/// </summary>
	[Required]
	public Guid TaskId { get; set; }
}

/// <summary>
/// Response model for agent operations
/// </summary>
public class AgentResponse
{
	/// <summary>
	/// Gets or sets the agent identifier
	/// </summary>
	public Guid Id { get; set; }

	/// <summary>
	/// Gets or sets the agent name
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the agent description
	/// </summary>
	public string Description { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the agent agentStatus
	/// </summary>
	public AgentStatus Status { get; set; }

	/// <summary>
	/// Gets or sets the agent capabilities
	/// </summary>
	public AgentCapabilitiesDto Capabilities { get; set; } = new();

	/// <summary>
	/// Gets or sets the agent configuration
	/// </summary>
	public AgentConfigurationDto Configuration { get; set; } = new();

	/// <summary>
	/// Gets or sets when the agent was created
	/// </summary>
	public DateTime CreatedAt { get; set; }

	/// <summary>
	/// Gets or sets when the agent was last updated
	/// </summary>
	public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for agent capabilities
/// </summary>
public class AgentCapabilitiesDto
{
	/// <summary>
	/// Gets or sets whether the agent can process natural language
	/// </summary>
	public bool CanProcessNaturalLanguage { get; set; } = true;

	/// <summary>
	/// Gets or sets whether the agent can generate code
	/// </summary>
	public bool CanGenerateCode { get; set; } = false;

	/// <summary>
	/// Gets or sets whether the agent can analyze data
	/// </summary>
	public bool CanAnalyzeData { get; set; } = false;

	/// <summary>
	/// Gets or sets whether the agent can interact with external APIs
	/// </summary>
	public bool CanCallExternalAPIs { get; set; } = false;

	/// <summary>
	/// Gets or sets the maximum concurrent tasks the agent can handle
	/// </summary>
	[Range(1, 100)]
	public int MaxConcurrentTasks { get; set; } = 1;

	/// <summary>
	/// Gets or sets the supported task types for this agent
	/// </summary>
	public List<string> SupportedTaskTypes { get; set; } = new();
}

/// <summary>
/// DTO for agent configuration
/// </summary>
public class AgentConfigurationDto
{
	/// <summary>
	/// Gets or sets the timeout for task execution in seconds
	/// </summary>
	public int TaskTimeoutSeconds { get; set; } = 300;

	/// <summary>
	/// Gets or sets the maximum retries for failed tasks
	/// </summary>
	public int MaxRetries { get; set; } = 3;

	/// <summary>
	/// Gets or sets the agent's priority level
	/// </summary>
	public int Priority { get; set; } = 1;

	/// <summary>
	/// Gets or sets custom configuration properties
	/// </summary>
	public Dictionary<string, object> CustomProperties { get; set; } = new();
}

/// <summary>
/// Generic API response wrapper
/// </summary>
/// <typeparam name="T">The type of data in the response</typeparam>
public class ApiResponse<T>
{
	/// <summary>
	/// Gets or sets whether the operation was successful
	/// </summary>
	public bool Success { get; set; }

	/// <summary>
	/// Gets or sets the response data
	/// </summary>
	public T? Data { get; set; }

	/// <summary>
	/// Gets or sets error messages if the operation failed
	/// </summary>
	public List<string> Errors { get; set; } = new();

	/// <summary>
	/// Gets or sets a message describing the result
	/// </summary>
	public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Extension methods for converting between domain models and DTOs
/// </summary>
public static class AgentMappingExtensions
{
	/// <summary>
	/// Converts an Agent domain model to AgentResponse DTO
	/// </summary>
	/// <param name="agent">The agent domain model</param>
	/// <returns>The agent response DTO</returns>
	public static AgentResponse ToResponse(this Agent agent)
	{
		return new AgentResponse
		{
			Id = agent.Id,
			Name = agent.Name,
			Description = agent.Description,
			Status = agent.Status,
			Capabilities = agent.Capabilities.ToDto(),
			Configuration = agent.Configuration.ToDto(),
			CreatedAt = agent.CreatedAt,
			UpdatedAt = agent.UpdatedAt
		};
	}

	/// <summary>
	/// Converts AgentCapabilities domain model to DTO
	/// </summary>
	/// <param name="capabilities">The capabilities domain model</param>
	/// <returns>The capabilities DTO</returns>
	public static AgentCapabilitiesDto ToDto(this AgentCapabilities capabilities)
	{
		return new AgentCapabilitiesDto
		{
			CanProcessNaturalLanguage = capabilities.CanProcessNaturalLanguage,
			CanGenerateCode = capabilities.CanGenerateCode,
			CanAnalyzeData = capabilities.CanAnalyzeData,
			CanCallExternalAPIs = capabilities.CanCallExternalAPIs,
			MaxConcurrentTasks = capabilities.MaxConcurrentTasks,
			SupportedTaskTypes = capabilities.SupportedTaskTypes?.ToList() ?? new List<string>()
		};
	}

	/// <summary>
	/// Converts AgentConfiguration domain model to DTO
	/// </summary>
	/// <param name="configuration">The configuration domain model</param>
	/// <returns>The configuration DTO</returns>
	public static AgentConfigurationDto ToDto(this AgentConfiguration configuration)
	{
		return new AgentConfigurationDto
		{
			TaskTimeoutSeconds = configuration.TaskTimeoutSeconds,
			MaxRetries = configuration.MaxRetries,
			Priority = configuration.Priority,
			CustomProperties = configuration.CustomProperties != null 
				? new Dictionary<string, object>(configuration.CustomProperties)
				: new Dictionary<string, object>()
		};
	}

	/// <summary>
	/// Converts AgentCapabilitiesDto to domain model
	/// </summary>
	/// <param name="dto">The capabilities DTO</param>
	/// <returns>The capabilities domain model</returns>
	public static AgentCapabilities ToDomain(this AgentCapabilitiesDto dto)
	{
		return new AgentCapabilities
		{
			CanProcessNaturalLanguage = dto.CanProcessNaturalLanguage,
			CanGenerateCode = dto.CanGenerateCode,
			CanAnalyzeData = dto.CanAnalyzeData,
			CanCallExternalAPIs = dto.CanCallExternalAPIs,
			MaxConcurrentTasks = dto.MaxConcurrentTasks,
			SupportedTaskTypes = dto.SupportedTaskTypes?.ToList() ?? new List<string>()
		};
	}

	/// <summary>
	/// Converts UpdateAgentConfigurationRequest to domain model
	/// </summary>
	/// <param name="request">The configuration request</param>
	/// <returns>The configuration domain model</returns>
	public static AgentConfiguration ToDomain(this UpdateAgentConfigurationRequest request)
	{
		return new AgentConfiguration
		{
			TaskTimeoutSeconds = request.TaskTimeoutSeconds,
			MaxRetries = request.MaxRetries,
			Priority = request.Priority,
			CustomProperties = request.CustomProperties != null 
				? new Dictionary<string, object>(request.CustomProperties)
				: new Dictionary<string, object>()
		};
	}
} 