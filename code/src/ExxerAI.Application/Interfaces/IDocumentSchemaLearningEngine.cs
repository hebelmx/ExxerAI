using ExxerAI.Application.DTOs;
using ExxerAI.Domain.DocumentProcessing;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Engine for learning and evolving document schemas based on processing feedback
/// </summary>
public interface IDocumentSchemaLearningEngine
{
    /// <summary>
    /// Updates document schema based on learning feedback
    /// </summary>
    /// <param name="schema">The schema definition to update</param>
    /// <param name="feedback">Learning feedback from processing</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async task</returns>
    Task UpdateSchemaFromFeedbackAsync(SchemaDefinition schema, LearningFeedback feedback, CancellationToken cancellationToken = default);
} 