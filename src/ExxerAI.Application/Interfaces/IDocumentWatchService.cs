using ExxerAI.Domain.DocumentProcessing;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Service for watching and monitoring document changes
/// </summary>
public interface IDocumentWatchService
{
    /// <summary>
    /// Starts watching a folder for document changes
    /// </summary>
    /// <param name="folderId">The folder identifier to watch</param>
    /// <param name="includeSubdirectories">Whether to include subdirectories</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Watch session identifier</returns>
    Task<string> StartWatchingAsync(string folderId, bool includeSubdirectories = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops watching a folder
    /// </summary>
    /// <param name="watchId">The watch session identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success result</returns>
    Task<bool> StopWatchingAsync(string watchId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the list of active watch sessions
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active watch sessions</returns>
    Task<IEnumerable<string>> GetActiveWatchesAsync(CancellationToken cancellationToken = default);
} 