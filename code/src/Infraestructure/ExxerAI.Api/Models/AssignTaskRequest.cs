using System.ComponentModel.DataAnnotations;

namespace ExxerAI.Api.Models;

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