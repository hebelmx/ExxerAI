namespace ExxerAi.MCPServer.Application.Services;

/// <summary>
/// Google Drive file metadata
/// </summary>
public class GoogleDriveFileMetadata
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime ModifiedTime { get; set; }
    public string? WebViewLink { get; set; }
    public string? DownloadUrl { get; set; }
}