using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace LocalAI.Aspire.AppHost.Services;

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

/// <summary>
/// Implementation of secure key store with file-based storage and encryption
/// </summary>
public class SecureKeyStore : IKeyStore
{
    private readonly ILogger<SecureKeyStore> _logger;
    private readonly string _storePath;
    private readonly byte[] _encryptionKey;
    private readonly Dictionary<string, StoredKey> _cache = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public SecureKeyStore(ILogger<SecureKeyStore> logger, string? storePath = null, string? encryptionKey = null)
    {
        _logger = logger;
        _storePath = storePath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LocalAI", "keystore.json");
        
        // Initialize encryption key
        _encryptionKey = DeriveEncryptionKey(encryptionKey ?? Environment.MachineName);
        
        // Ensure store directory exists
        var directory = Path.GetDirectoryName(_storePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        // Load existing keys
        _ = LoadKeysAsync();
    }

    public async Task<string?> GetKeyAsync(string keyName, string? scope = null)
    {
        await _lock.WaitAsync();
        try
        {
            var fullKey = GetFullKeyName(keyName, scope);
            
            // Check environment variables first (highest priority)
            var envValue = Environment.GetEnvironmentVariable(ConvertToEnvVar(fullKey));
            if (!string.IsNullOrEmpty(envValue))
            {
                _logger.LogDebug("Retrieved key {KeyName} from environment variable", fullKey);
                return envValue;
            }
            
            // Check cache/storage
            if (_cache.TryGetValue(fullKey, out var storedKey))
            {
                // Check if key has expired
                if (storedKey.ExpiresAt.HasValue && storedKey.ExpiresAt < DateTime.UtcNow)
                {
                    _logger.LogWarning("Key {KeyName} has expired", fullKey);
                    await DeleteKeyAsync(keyName, scope);
                    return null;
                }
                
                var decryptedValue = DecryptValue(storedKey.EncryptedValue);
                _logger.LogDebug("Retrieved key {KeyName} from store", fullKey);
                return decryptedValue;
            }
            
            _logger.LogDebug("Key {KeyName} not found", fullKey);
            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SetKeyAsync(string keyName, string value, string? scope = null, TimeSpan? expiration = null)
    {
        await _lock.WaitAsync();
        try
        {
            var fullKey = GetFullKeyName(keyName, scope);
            var encryptedValue = EncryptValue(value);
            var expiresAt = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : null;
            
            var storedKey = new StoredKey
            {
                Name = fullKey,
                EncryptedValue = encryptedValue,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                Scope = scope
            };
            
            _cache[fullKey] = storedKey;
            await SaveKeysAsync();
            
            _logger.LogInformation("Stored key {KeyName} with scope {Scope}", keyName, scope ?? "global");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> DeleteKeyAsync(string keyName, string? scope = null)
    {
        await _lock.WaitAsync();
        try
        {
            var fullKey = GetFullKeyName(keyName, scope);
            var removed = _cache.Remove(fullKey);
            
            if (removed)
            {
                await SaveKeysAsync();
                _logger.LogInformation("Deleted key {KeyName}", fullKey);
            }
            
            return removed;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IEnumerable<string>> ListKeysAsync(string? scope = null)
    {
        await _lock.WaitAsync();
        try
        {
            return _cache.Values
                .Where(k => scope == null || k.Scope == scope)
                .Where(k => !k.ExpiresAt.HasValue || k.ExpiresAt > DateTime.UtcNow)
                .Select(k => k.Name)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> KeyExistsAsync(string keyName, string? scope = null)
    {
        var value = await GetKeyAsync(keyName, scope);
        return !string.IsNullOrEmpty(value);
    }

    public async Task RotateKeyAsync(string keyName, string newValue, string? scope = null)
    {
        await _lock.WaitAsync();
        try
        {
            var fullKey = GetFullKeyName(keyName, scope);
            
            // Backup old key with timestamp
            if (_cache.TryGetValue(fullKey, out var oldKey))
            {
                var backupKey = $"{fullKey}_backup_{DateTime.UtcNow:yyyyMMddHHmmss}";
                _cache[backupKey] = oldKey with { Name = backupKey };
            }
            
            // Set new key
            await SetKeyAsync(keyName, newValue, scope);
            
            _logger.LogInformation("Rotated key {KeyName}", fullKey);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<string> GenerateApiKeyAsync(string keyName, string? scope = null, int length = 32)
    {
        var apiKey = GenerateSecureRandomString(length);
        await SetKeyAsync(keyName, apiKey, scope);
        
        _logger.LogInformation("Generated new API key {KeyName} with length {Length}", keyName, length);
        return apiKey;
    }

    private static string GetFullKeyName(string keyName, string? scope)
    {
        return scope != null ? $"{scope}:{keyName}" : keyName;
    }

    private static string ConvertToEnvVar(string keyName)
    {
        return $"LOCALAI_{keyName.ToUpperInvariant().Replace(":", "_").Replace("-", "_")}";
    }

    private async Task LoadKeysAsync()
    {
        try
        {
            if (!File.Exists(_storePath))
            {
                _logger.LogInformation("Key store file not found, creating new store");
                return;
            }

            var jsonData = await File.ReadAllTextAsync(_storePath);
            var storedKeys = JsonSerializer.Deserialize<StoredKey[]>(jsonData) ?? Array.Empty<StoredKey>();
            
            _cache.Clear();
            foreach (var key in storedKeys)
            {
                _cache[key.Name] = key;
            }
            
            _logger.LogInformation("Loaded {Count} keys from store", storedKeys.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load keys from store");
        }
    }

    private async Task SaveKeysAsync()
    {
        try
        {
            var keysToStore = _cache.Values.ToArray();
            var jsonData = JsonSerializer.Serialize(keysToStore, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            
            await File.WriteAllTextAsync(_storePath, jsonData);
            _logger.LogDebug("Saved {Count} keys to store", keysToStore.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save keys to store");
        }
    }

    private string EncryptValue(string value)
    {
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.GenerateIV();
        
        using var encryptor = aes.CreateEncryptor();
        var valueBytes = Encoding.UTF8.GetBytes(value);
        var encryptedBytes = encryptor.TransformFinalBlock(valueBytes, 0, valueBytes.Length);
        
        // Combine IV and encrypted data
        var result = new byte[aes.IV.Length + encryptedBytes.Length];
        Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
        Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);
        
        return Convert.ToBase64String(result);
    }

    private string DecryptValue(string encryptedValue)
    {
        var data = Convert.FromBase64String(encryptedValue);
        
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        
        // Extract IV and encrypted data
        var iv = new byte[16]; // AES block size
        var encryptedBytes = new byte[data.Length - 16];
        Array.Copy(data, 0, iv, 0, 16);
        Array.Copy(data, 16, encryptedBytes, 0, encryptedBytes.Length);
        
        aes.IV = iv;
        using var decryptor = aes.CreateDecryptor();
        var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
        
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    private static byte[] DeriveEncryptionKey(string password)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes("LocalAI-Salt"), 10000, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(32); // 256-bit key
    }

    private static string GenerateSecureRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[length];
        rng.GetBytes(bytes);
        
        var result = new StringBuilder(length);
        foreach (var b in bytes)
        {
            result.Append(chars[b % chars.Length]);
        }
        
        return result.ToString();
    }
}

/// <summary>
/// Represents a stored key with metadata
/// </summary>
public record StoredKey
{
    public string Name { get; init; } = string.Empty;
    public string EncryptedValue { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public string? Scope { get; init; }
}
