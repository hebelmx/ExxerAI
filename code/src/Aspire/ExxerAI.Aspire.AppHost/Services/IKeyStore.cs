namespace ExxerAI.Aspire.AppHost.Services;

/// <summary>
/// Secure key store for managing API keys, secrets, and credentials
/// Supports multiple storage backends and encryption
/// </summary>
public interface IKeyStore
{
    Task<string?> GetKeyAsync(string keyName, string? scope = null);

    Task SetKeyAsync(string keyName, string value, string? scope = null, TimeSpan? expiration = null);

    Task<bool> DeleteKeyAsync(string keyName, string? scope = null);

    Task<IEnumerable<string>> ListKeysAsync(string? scope = null);

    Task<bool> KeyExistsAsync(string keyName, string? scope = null);

    Task RotateKeyAsync(string keyName, string newValue, string? scope = null);

    Task<string> GenerateApiKeyAsync(string keyName, string? scope = null, int length = 32);
}