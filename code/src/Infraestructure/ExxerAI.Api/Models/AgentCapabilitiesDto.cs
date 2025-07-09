using System.ComponentModel.DataAnnotations;

namespace ExxerAI.Api.Models;

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
    public List<string> SupportedTaskTypes { get; set; } = [];
}