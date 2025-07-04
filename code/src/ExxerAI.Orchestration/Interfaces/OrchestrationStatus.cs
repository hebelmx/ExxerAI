namespace ExxerAI.Orchestration.Interfaces;
using ExxerAI.Domain.Entities;
using ExxerAI.Domain.ValueObjects;
using ExxerAI.Domain.Configurations;
using ExxerAI.Domain.Helpers;

/// <summary>
/// Represents the agentStatus of the orchestration engine
/// </summary>
public enum OrchestrationStatus
{
    /// <summary>
    /// Engine is stopped
    /// </summary>
    Stopped,

    /// <summary>
    /// Engine is starting
    /// </summary>
    Starting,

    /// <summary>
    /// Engine is running
    /// </summary>
    Running,

    /// <summary>
    /// Engine is stopping
    /// </summary>
    Stopping,

    /// <summary>
    /// Engine encountered an error
    /// </summary>
    Error
}