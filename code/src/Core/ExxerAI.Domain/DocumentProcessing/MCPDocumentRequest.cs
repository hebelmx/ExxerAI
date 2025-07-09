namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents an MCP document processing request
/// </summary>
public class MCPDocumentRequest
{
    /// <summary>
    /// Gets or sets the document ID
    /// </summary>
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the processing options
    /// </summary>
    public ProcessingOptions ProcessingOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets the session ID
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional request parameters
    /// </summary>
    public Dictionary<string, object> Parameters { get; init; } = [];
}