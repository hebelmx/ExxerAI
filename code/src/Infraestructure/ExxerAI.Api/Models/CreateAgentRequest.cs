using System.ComponentModel.DataAnnotations;

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