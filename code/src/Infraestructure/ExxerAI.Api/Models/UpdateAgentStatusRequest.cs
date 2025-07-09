using System.ComponentModel.DataAnnotations;
using ExxerAI.Domain;
using ExxerAI.Domain.ValueObjects;

namespace ExxerAI.Api.Models;

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