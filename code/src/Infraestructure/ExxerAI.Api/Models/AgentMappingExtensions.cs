using ExxerAI.Domain.Entities;
using ExxerAI.Domain.ValueObjects;
using ExxerAI.Domain.Configurations;

namespace ExxerAI.Api.Models;

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
            SupportedTaskTypes = capabilities.SupportedTaskTypes?.ToList() ?? []
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
                : []
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
            SupportedTaskTypes = dto.SupportedTaskTypes?.ToList() ?? []
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
                : []
        };
    }
}