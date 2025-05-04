using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EHRp.Services
{
    /// <summary>
    /// Interface for managing encryption keys.
    /// </summary>
    public interface IEncryptionKeyManager
    {
        /// <summary>
        /// Gets the current encryption key.
        /// </summary>
        /// <returns>The current encryption key.</returns>
        byte[] GetCurrentKey();
        
        /// <summary>
        /// Gets an encryption key by version.
        /// </summary>
        /// <param name="version">The key version.</param>
        /// <returns>The encryption key for the specified version.</returns>
        byte[] GetKey(int version);
        
        /// <summary>
        /// Creates a new encryption key and sets it as the current key.
        /// </summary>
        /// <returns>The version of the newly created key.</returns>
        int RotateKey();
    }

    /// <summary>
    /// Implementation of the encryption key manager.
    /// </summary>
    public class EncryptionKeyManager : IEncryptionKeyManager
    {
        private readonly ILogger<EncryptionKeyManager> _logger;
        private readonly string _keyDirectory;
        private readonly ConcurrentDictionary<int, byte[]> _keyCache = new();
        private int _currentKeyVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionKeyManager"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="logger">The logger instance.</param>
        public EncryptionKeyManager(IConfiguration configuration, ILogger<EncryptionKeyManager> logger)
        {
            _logger = logger;
            
            // Get key directory from configuration
            var appSettings = configuration.GetSection("AppSettings");
            _keyDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EHRp",
                appSettings["EncryptionKeyDirectory"] ?? "Keys");
            
            // Ensure key directory exists
            if (!Directory.Exists(_keyDirectory))
            {
                Directory.CreateDirectory(_keyDirectory);
            }
            
            // Get current key version from configuration
            if (int.TryParse(appSettings["CurrentKeyVersion"], out int configKeyVersion))
            {
                _currentKeyVersion = configKeyVersion;
            }
            else
            {
                _currentKeyVersion = 1;
            }
            
            // Ensure current key exists
            if (!File.Exists(GetKeyPath(_currentKeyVersion)))
            {
                _logger.LogInformation("Current encryption key (version {Version}) not found. Creating new key.", _currentKeyVersion);
                CreateKey(_currentKeyVersion);
            }
            
            _logger.LogInformation("Encryption key manager initialized with current key version {Version}", _currentKeyVersion);
        }

        /// <inheritdoc/>
        public byte[] GetCurrentKey()
        {
            return GetKey(_currentKeyVersion);
        }

        /// <inheritdoc/>
        public byte[] GetKey(int version)
        {
            if (_keyCache.TryGetValue(version, out byte[]? key) && key != null)
            {
                return key;
            }
            
            string keyPath = GetKeyPath(version);
            if (!File.Exists(keyPath))
            {
                throw new FileNotFoundException($"Encryption key version {version} not found.", keyPath);
            }
            
            try
            {
                string keyJson = File.ReadAllText(keyPath);
                var keyData = JsonSerializer.Deserialize<KeyData>(keyJson);
                
                if (keyData == null)
                {
                    throw new InvalidOperationException($"Failed to deserialize key data for version {version}.");
                }
                
                byte[] keyBytes = Convert.FromBase64String(keyData.Key);
                _keyCache[version] = keyBytes;
                
                return keyBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading encryption key version {Version}", version);
                throw new InvalidOperationException($"Failed to load encryption key version {version}.", ex);
            }
        }

        /// <inheritdoc/>
        public int RotateKey()
        {
            int newVersion = _currentKeyVersion + 1;
            CreateKey(newVersion);
            _currentKeyVersion = newVersion;
            
            // Update configuration file (in a real app, this would be persisted)
            _logger.LogInformation("Rotated encryption key to version {Version}", newVersion);
            
            return newVersion;
        }

        private string GetKeyPath(int version)
        {
            return Path.Combine(_keyDirectory, $"key-v{version}.json");
        }

        private void CreateKey(int version)
        {
            try
            {
                // Generate a new random key
                using var aes = Aes.Create();
                aes.GenerateKey();
                
                var keyData = new KeyData
                {
                    Version = version,
                    Key = Convert.ToBase64String(aes.Key),
                    CreatedAt = DateTime.UtcNow
                };
                
                string keyJson = JsonSerializer.Serialize(keyData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(GetKeyPath(version), keyJson);
                
                // Add to cache
                _keyCache[version] = aes.Key;
                
                _logger.LogInformation("Created new encryption key version {Version}", version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating encryption key version {Version}", version);
                throw new InvalidOperationException($"Failed to create encryption key version {version}.", ex);
            }
        }

        private class KeyData
        {
            public int Version { get; set; }
            public string Key { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
        }
    }
}