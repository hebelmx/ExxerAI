using ExxerAI.Domain.Operations;
using ExxerAI.MCPServer.Application.Services;

namespace ExxerAI.MCPServer.Application.Interfaces;

/// <summary>
/// Interface for resolving Google Drive credentials from multiple sources
/// Supports various credential formats and prioritizes environment variables over configuration files
/// </summary>
public interface IGoogleDriveCredentialResolver
{
    /// <summary>
    /// Resolves Google Drive credentials from available sources
    /// Priority: Environment Variables > User Secrets > appsettings.json > JSON credential file
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Resolved credentials or failure result</returns>
    Task<Result<GoogleDriveCredentials>> ResolveCredentialsAsync(CancellationToken cancellationToken = default);
}