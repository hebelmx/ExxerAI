namespace ExxerAi.MCPServer.Application.Services;

/// <summary>
/// Document change information
/// </summary>
public class DocumentChange
{
    public string DocumentId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public long Size { get; set; }
}