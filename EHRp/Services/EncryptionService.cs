using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace EHRp.Services
{
    /// <summary>
    /// Interface for encryption and decryption services.
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Encrypts data using the current encryption key.
        /// </summary>
        /// <param name="data">The data to encrypt.</param>
        /// <returns>The encrypted data with metadata.</returns>
        EncryptedData Encrypt(byte[] data);
        
        /// <summary>
        /// Decrypts data using the appropriate encryption key.
        /// </summary>
        /// <param name="encryptedData">The encrypted data with metadata.</param>
        /// <returns>The decrypted data.</returns>
        byte[] Decrypt(EncryptedData encryptedData);
        
        /// <summary>
        /// Encrypts a file asynchronously.
        /// </summary>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="destinationFilePath">The destination file path.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task EncryptFileAsync(string sourceFilePath, string destinationFilePath);
        
        /// <summary>
        /// Decrypts a file asynchronously.
        /// </summary>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="destinationFilePath">The destination file path.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DecryptFileAsync(string sourceFilePath, string destinationFilePath);
        
        /// <summary>
        /// Re-encrypts data with the current encryption key.
        /// </summary>
        /// <param name="encryptedData">The encrypted data to re-encrypt.</param>
        /// <returns>The re-encrypted data with updated metadata.</returns>
        EncryptedData ReEncrypt(EncryptedData encryptedData);
    }

    /// <summary>
    /// Represents encrypted data with metadata.
    /// </summary>
    public class EncryptedData
    {
        /// <summary>
        /// Gets or sets the encrypted content.
        /// </summary>
        public byte[] Content { get; set; } = Array.Empty<byte>();
        
        /// <summary>
        /// Gets or sets the initialization vector.
        /// </summary>
        public byte[] IV { get; set; } = Array.Empty<byte>();
        
        /// <summary>
        /// Gets or sets the key version used for encryption.
        /// </summary>
        public int KeyVersion { get; set; }
        
        /// <summary>
        /// Gets or sets the salt used for encryption.
        /// </summary>
        public byte[] Salt { get; set; } = Array.Empty<byte>();
        
        /// <summary>
        /// Serializes the encrypted data to a byte array.
        /// </summary>
        /// <returns>The serialized encrypted data.</returns>
        public byte[] ToByteArray()
        {
            using var memoryStream = new MemoryStream();
            using var writer = new BinaryWriter(memoryStream);
            
            // Write format version
            writer.Write((byte)1);
            
            // Write key version
            writer.Write(KeyVersion);
            
            // Write salt length and salt
            writer.Write(Salt.Length);
            writer.Write(Salt);
            
            // Write IV length and IV
            writer.Write(IV.Length);
            writer.Write(IV);
            
            // Write content length and content
            writer.Write(Content.Length);
            writer.Write(Content);
            
            return memoryStream.ToArray();
        }
        
        /// <summary>
        /// Deserializes encrypted data from a byte array.
        /// </summary>
        /// <param name="data">The serialized encrypted data.</param>
        /// <returns>The deserialized encrypted data.</returns>
        public static EncryptedData FromByteArray(byte[] data)
        {
            using var memoryStream = new MemoryStream(data);
            using var reader = new BinaryReader(memoryStream);
            
            // Read format version
            byte formatVersion = reader.ReadByte();
            if (formatVersion != 1)
            {
                throw new InvalidOperationException($"Unsupported encrypted data format version: {formatVersion}");
            }
            
            // Read key version
            int keyVersion = reader.ReadInt32();
            
            // Read salt
            int saltLength = reader.ReadInt32();
            byte[] salt = reader.ReadBytes(saltLength);
            
            // Read IV
            int ivLength = reader.ReadInt32();
            byte[] iv = reader.ReadBytes(ivLength);
            
            // Read content
            int contentLength = reader.ReadInt32();
            byte[] content = reader.ReadBytes(contentLength);
            
            return new EncryptedData
            {
                KeyVersion = keyVersion,
                Salt = salt,
                IV = iv,
                Content = content
            };
        }
    }

    /// <summary>
    /// Implementation of the encryption service.
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        private readonly IEncryptionKeyManager _keyManager;
        private readonly ILogger<EncryptionService> _logger;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionService"/> class.
        /// </summary>
        /// <param name="keyManager">The encryption key manager.</param>
        /// <param name="logger">The logger instance.</param>
        public EncryptionService(IEncryptionKeyManager keyManager, ILogger<EncryptionService> logger)
        {
            _keyManager = keyManager;
            _logger = logger;
        }
        
        /// <inheritdoc/>
        public EncryptedData Encrypt(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                throw new ArgumentException("Data cannot be null or empty", nameof(data));
            }
            
            try
            {
                // Get current key and key version
                byte[] key = _keyManager.GetCurrentKey();
                int keyVersion = 1; // Default to version 1 if not available
                
                // Generate random salt and IV
                byte[] salt = GenerateRandomBytes(16);
                byte[] iv = GenerateRandomBytes(16);
                
                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;
                
                using var memoryStream = new MemoryStream();
                using var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
                
                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();
                
                return new EncryptedData
                {
                    Content = memoryStream.ToArray(),
                    IV = iv,
                    KeyVersion = keyVersion,
                    Salt = salt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting data");
                throw new InvalidOperationException("Failed to encrypt data.", ex);
            }
        }
        
        /// <inheritdoc/>
        public byte[] Decrypt(EncryptedData encryptedData)
        {
            if (encryptedData == null)
            {
                throw new ArgumentNullException(nameof(encryptedData));
            }
            
            if (encryptedData.Content == null || encryptedData.Content.Length == 0)
            {
                throw new ArgumentException("Encrypted content cannot be null or empty", nameof(encryptedData));
            }
            
            try
            {
                // Get key for the specified version
                byte[] key = _keyManager.GetKey(encryptedData.KeyVersion);
                
                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = encryptedData.IV;
                
                using var memoryStream = new MemoryStream();
                using var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write);
                
                cryptoStream.Write(encryptedData.Content, 0, encryptedData.Content.Length);
                cryptoStream.FlushFinalBlock();
                
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting data with key version {KeyVersion}", encryptedData.KeyVersion);
                throw new InvalidOperationException($"Failed to decrypt data with key version {encryptedData.KeyVersion}.", ex);
            }
        }
        
        /// <inheritdoc/>
        public EncryptedData ReEncrypt(EncryptedData encryptedData)
        {
            byte[] decryptedData = Decrypt(encryptedData);
            return Encrypt(decryptedData);
        }
        
        /// <inheritdoc/>
        public async Task EncryptFileAsync(string sourceFilePath, string destinationFilePath)
        {
            if (!File.Exists(sourceFilePath))
            {
                throw new FileNotFoundException("Source file not found.", sourceFilePath);
            }
            
            try
            {
                // Read file in chunks to avoid loading large files into memory
                const int bufferSize = 4096;
                byte[] buffer = new byte[bufferSize];
                
                using var sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);
                
                // Get current key and key version
                byte[] key = _keyManager.GetCurrentKey();
                int keyVersion = 1; // Default to version 1 if not available
                
                // Generate random salt and IV
                byte[] salt = GenerateRandomBytes(16);
                byte[] iv = GenerateRandomBytes(16);
                
                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;
                
                // Create metadata
                var metadata = new EncryptedData
                {
                    KeyVersion = keyVersion,
                    Salt = salt,
                    IV = iv,
                    Content = Array.Empty<byte>() // Placeholder
                };
                
                // Write metadata first - use a MemoryStream to build the metadata bytes
                using var metadataStream = new MemoryStream();
                using var metadataWriter = new BinaryWriter(metadataStream);
                
                metadataWriter.Write(keyVersion);
                metadataWriter.Write(salt.Length);
                metadataWriter.Write(salt);
                metadataWriter.Write(iv.Length);
                metadataWriter.Write(iv);
                
                byte[] metadataBytes = metadataStream.ToArray();
                
                using var destinationStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true);
                await destinationStream.WriteAsync(metadataBytes, 0, metadataBytes.Length);
                
                using var cryptoStream = new CryptoStream(destinationStream, aes.CreateEncryptor(), CryptoStreamMode.Write, true);
                
                int bytesRead;
                while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await cryptoStream.WriteAsync(buffer, 0, bytesRead);
                }
                
                await cryptoStream.FlushFinalBlockAsync();
                
                _logger.LogInformation("File encrypted successfully: {SourceFile} -> {DestinationFile}", 
                    Path.GetFileName(sourceFilePath), Path.GetFileName(destinationFilePath));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting file: {SourceFile}", Path.GetFileName(sourceFilePath));
                throw new InvalidOperationException($"Failed to encrypt file: {Path.GetFileName(sourceFilePath)}", ex);
            }
        }
        
        /// <inheritdoc/>
        public async Task DecryptFileAsync(string sourceFilePath, string destinationFilePath)
        {
            if (!File.Exists(sourceFilePath))
            {
                throw new FileNotFoundException("Source file not found.", sourceFilePath);
            }
            
            try
            {
                const int bufferSize = 4096;
                
                using var sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);
                
                // Read metadata
                byte[] keyVersionBytes = new byte[4];
                await sourceStream.ReadAsync(keyVersionBytes, 0, 4);
                int keyVersion = BitConverter.ToInt32(keyVersionBytes, 0);
                
                byte[] saltLengthBytes = new byte[4];
                await sourceStream.ReadAsync(saltLengthBytes, 0, 4);
                int saltLength = BitConverter.ToInt32(saltLengthBytes, 0);
                
                byte[] salt = new byte[saltLength];
                await sourceStream.ReadAsync(salt, 0, saltLength);
                
                byte[] ivLengthBytes = new byte[4];
                await sourceStream.ReadAsync(ivLengthBytes, 0, 4);
                int ivLength = BitConverter.ToInt32(ivLengthBytes, 0);
                
                byte[] iv = new byte[ivLength];
                await sourceStream.ReadAsync(iv, 0, ivLength);
                
                // Get key for the specified version
                byte[] key = _keyManager.GetKey(keyVersion);
                
                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;
                
                using var destinationStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true);
                using var cryptoStream = new CryptoStream(sourceStream, aes.CreateDecryptor(), CryptoStreamMode.Read, true);
                
                byte[] buffer = new byte[bufferSize];
                int bytesRead;
                while ((bytesRead = await cryptoStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await destinationStream.WriteAsync(buffer, 0, bytesRead);
                }
                
                _logger.LogInformation("File decrypted successfully: {SourceFile} -> {DestinationFile}", 
                    Path.GetFileName(sourceFilePath), Path.GetFileName(destinationFilePath));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting file: {SourceFile}", Path.GetFileName(sourceFilePath));
                throw new InvalidOperationException($"Failed to decrypt file: {Path.GetFileName(sourceFilePath)}", ex);
            }
        }
        
        private byte[] GenerateRandomBytes(int length)
        {
            byte[] bytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return bytes;
        }
    }
}