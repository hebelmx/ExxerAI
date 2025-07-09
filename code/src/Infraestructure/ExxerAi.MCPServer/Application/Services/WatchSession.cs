namespace ExxerAi.MCPServer.Application.Services;

/// <summary>
/// Watch session data structure
/// </summary>
public class WatchSession
{
    public string WatchId { get; set; } = string.Empty;
    public string FolderId { get; set; } = string.Empty;
    public string FolderName { get; set; } = string.Empty;
    public bool IncludeSubdirectories { get; set; }
    public bool AutoProcess { get; set; }
    public TimeSpan PollingInterval { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime LastCheck { get; set; }
    public bool IsActive { get; set; }
    public List<DocumentChange> DetectedChanges { get; set; } = new();
}