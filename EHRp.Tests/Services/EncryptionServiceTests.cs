using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using EHRp.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EHRp.Tests.Services
{
    public class EncryptionServiceTests
    {
        private readonly Mock<IEncryptionKeyManager> _keyManagerMock;
        private readonly Mock<ILogger<EncryptionService>> _loggerMock;
        private readonly byte[] _testKey;
        private readonly IEncryptionService _encryptionService;

        public EncryptionServiceTests()
        {
            // Set up test key
            _testKey = new byte[32]; // 256-bit key
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(_testKey);
            }

            // Set up mocks
            _keyManagerMock = new Mock<IEncryptionKeyManager>();
            _loggerMock = new Mock<ILogger<EncryptionService>>();

            // Configure key manager mock
            _keyManagerMock.Setup(m => m.GetCurrentKey()).Returns(_testKey);
            _keyManagerMock.Setup(m => m.GetKey(It.IsAny<int>())).Returns(_testKey);

            // Create encryption service
            _encryptionService = new EncryptionService(_keyManagerMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void Encrypt_WithValidData_ReturnsEncryptedData()
        {
            // Arrange
            var testData = Encoding.UTF8.GetBytes("This is a test message");

            // Act
            var encryptedData = _encryptionService.Encrypt(testData);

            // Assert
            Assert.NotNull(encryptedData);
            Assert.NotNull(encryptedData.Content);
            Assert.NotNull(encryptedData.IV);
            Assert.NotNull(encryptedData.Salt);
            Assert.NotEqual(0, encryptedData.KeyVersion);
            
            // Verify that the encrypted content is different from the original
            Assert.NotEqual(testData, encryptedData.Content);
            
            // Verify that the key manager was called
            _keyManagerMock.Verify(m => m.GetCurrentKey(), Times.Once);
        }

        [Fact]
        public void Decrypt_WithValidEncryptedData_ReturnsOriginalData()
        {
            // Arrange
            var testData = Encoding.UTF8.GetBytes("This is a test message");
            var encryptedData = _encryptionService.Encrypt(testData);

            // Act
            var decryptedData = _encryptionService.Decrypt(encryptedData);

            // Assert
            Assert.NotNull(decryptedData);
            Assert.Equal(testData, decryptedData);
            
            // Verify that the key manager was called
            _keyManagerMock.Verify(m => m.GetKey(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public void ReEncrypt_WithValidEncryptedData_ReturnsReEncryptedData()
        {
            // Arrange
            var testData = Encoding.UTF8.GetBytes("This is a test message");
            var encryptedData = _encryptionService.Encrypt(testData);

            // Act
            var reEncryptedData = _encryptionService.ReEncrypt(encryptedData);

            // Assert
            Assert.NotNull(reEncryptedData);
            Assert.NotNull(reEncryptedData.Content);
            Assert.NotNull(reEncryptedData.IV);
            Assert.NotNull(reEncryptedData.Salt);
            Assert.NotEqual(0, reEncryptedData.KeyVersion);
            
            // Verify that the re-encrypted content is different from the original encrypted content
            Assert.NotEqual(encryptedData.Content, reEncryptedData.Content);
            
            // Verify that the IVs are different
            Assert.NotEqual(encryptedData.IV, reEncryptedData.IV);
            
            // Verify that the key manager was called
            _keyManagerMock.Verify(m => m.GetKey(It.IsAny<int>()), Times.Once);
            _keyManagerMock.Verify(m => m.GetCurrentKey(), Times.AtLeast(1));
        }

        [Fact]
        public async Task EncryptFileAsync_WithValidFile_CreatesEncryptedFile()
        {
            // Arrange
            var tempSourcePath = Path.GetTempFileName();
            var tempDestPath = Path.GetTempFileName();
            
            try
            {
                // Create a test file
                var testData = Encoding.UTF8.GetBytes("This is a test file content");
                await File.WriteAllBytesAsync(tempSourcePath, testData);

                // Act
                await _encryptionService.EncryptFileAsync(tempSourcePath, tempDestPath);

                // Assert
                Assert.True(File.Exists(tempDestPath));
                
                // Verify that the encrypted file is different from the original
                var encryptedData = await File.ReadAllBytesAsync(tempDestPath);
                Assert.NotEqual(testData, encryptedData);
                
                // Verify that the key manager was called
                _keyManagerMock.Verify(m => m.GetCurrentKey(), Times.AtLeast(1));
            }
            finally
            {
                // Clean up
                if (File.Exists(tempSourcePath))
                    File.Delete(tempSourcePath);
                
                if (File.Exists(tempDestPath))
                    File.Delete(tempDestPath);
            }
        }

        [Fact]
        public async Task DecryptFileAsync_WithValidEncryptedFile_CreatesDecryptedFile()
        {
            // Arrange
            var tempSourcePath = Path.GetTempFileName();
            var tempEncryptedPath = Path.GetTempFileName();
            var tempDecryptedPath = Path.GetTempFileName();
            
            try
            {
                // Create a test file
                var testData = Encoding.UTF8.GetBytes("This is a test file content");
                await File.WriteAllBytesAsync(tempSourcePath, testData);
                
                // Encrypt the file
                await _encryptionService.EncryptFileAsync(tempSourcePath, tempEncryptedPath);

                // Act
                await _encryptionService.DecryptFileAsync(tempEncryptedPath, tempDecryptedPath);

                // Assert
                Assert.True(File.Exists(tempDecryptedPath));
                
                // Verify that the decrypted file matches the original
                var decryptedData = await File.ReadAllBytesAsync(tempDecryptedPath);
                Assert.Equal(testData, decryptedData);
                
                // Verify that the key manager was called
                _keyManagerMock.Verify(m => m.GetKey(It.IsAny<int>()), Times.AtLeast(1));
            }
            finally
            {
                // Clean up
                if (File.Exists(tempSourcePath))
                    File.Delete(tempSourcePath);
                
                if (File.Exists(tempEncryptedPath))
                    File.Delete(tempEncryptedPath);
                
                if (File.Exists(tempDecryptedPath))
                    File.Delete(tempDecryptedPath);
            }
        }

        [Fact]
        public void Encrypt_WithNullData_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _encryptionService.Encrypt(null));
            Assert.Contains("Data cannot be null or empty", exception.Message);
        }

        [Fact]
        public void Encrypt_WithEmptyData_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _encryptionService.Encrypt(Array.Empty<byte>()));
            Assert.Contains("Data cannot be null or empty", exception.Message);
        }

        [Fact]
        public void Decrypt_WithNullEncryptedData_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _encryptionService.Decrypt(null));
        }
    }
}