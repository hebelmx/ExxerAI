namespace ExxerAI.Orchestration.Interfaces;

/// <summary>
/// Secure key store for managing API keys, secrets, and credentials
/// Supports multiple storage backends and encryption
/// </summary>
public interface IKeyStore
{
    Task<string?> GetKeyAsync(string keyName, string? scope = null, CancellationToken cancellationToken = default);

    Task SetKeyAsync(string keyName, string value, string? scope = null, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    Task<bool> DeleteKeyAsync(string keyName, string? scope = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> ListKeysAsync(string? scope = null, CancellationToken cancellationToken = default);

    Task<bool> KeyExistsAsync(string keyName, string? scope = null, CancellationToken cancellationToken = default);

    Task RotateKeyAsync(string keyName, string newValue, string? scope = null, CancellationToken cancellationToken = default);

    Task<string> GenerateApiKeyAsync(string keyName, string? scope = null, int length = 32, CancellationToken cancellationToken = default);
}