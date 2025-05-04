using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using EHRp.Data;
using EHRp.Models;
using EHRp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EHRp.Tests.Services
{
    public class FileServiceTests : IDisposable
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;
        private readonly Mock<IEncryptionService> _encryptionServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IConfigurationSection> _appSettingsSectionMock;
        private readonly Mock<ILogger<FileService>> _loggerMock;
        private readonly string _tempBasePath;

        public FileServiceTests()
        {
            // Set up in-memory database for testing
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"FileServiceTestDb_{Guid.NewGuid()}")
                .Options;

            // Set up mocks
            _encryptionServiceMock = new Mock<IEncryptionService>();
            _configurationMock = new Mock<IConfiguration>();
            _appSettingsSectionMock = new Mock<IConfigurationSection>();
            _loggerMock = new Mock<ILogger<FileService>>();

            // Configure configuration mock
            _configurationMock.Setup(c => c.GetSection("AppSettings")).Returns(_appSettingsSectionMock.Object);
            _appSettingsSectionMock.Setup(s => s["FileStoragePath"]).Returns("TestFiles");
            _appSettingsSectionMock.Setup(s => s["EncryptedFilePath"]).Returns("TestEncrypted");
            _appSettingsSectionMock.Setup(s => s["TempFilePath"]).Returns("TestTemp");

            // Set up temp directory for testing
            _tempBasePath = Path.Combine(Path.GetTempPath(), "EHRpTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempBasePath);
            Directory.CreateDirectory(Path.Combine(_tempBasePath, "TestFiles"));
            Directory.CreateDirectory(Path.Combine(_tempBasePath, "TestFiles", "TestEncrypted"));
            Directory.CreateDirectory(Path.Combine(_tempBasePath, "TestFiles", "TestTemp"));

            // Seed the database with test data
            using var context = new ApplicationDbContext(_dbContextOptions);
            context.Database.EnsureCreated();
            SeedDatabase(context);
        }

        private void SeedDatabase(ApplicationDbContext context)
        {
            // Add a test patient
            var patient = new Patient
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateTime(1980, 1, 1),
                Gender = "Male",
                Email = "john.doe@example.com",
                PhoneNumber = "123-456-7890",
                Address = "123 Main St, Anytown, USA",
                MedicalHistory = "None",
                Notes = "Regular checkup",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            context.Patients.Add(patient);
            context.SaveChanges();
        }

        [Fact]
        public async Task SaveFileAsync_WithEncryption_SavesEncryptedFile()
        {
            // Arrange
            var tempSourcePath = Path.Combine(_tempBasePath, "test.txt");
            File.WriteAllText(tempSourcePath, "This is a test file");

            // Create a new DbContext with a unique database name for this test
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"FileServiceTestDb_Encryption_{Guid.NewGuid()}")
                .Options;
                
            using var context = new ApplicationDbContext(options);
            
            // Seed the database with a test patient
            var patient = new Patient
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateTime(1980, 1, 1),
                Gender = "Male",
                Email = "john.doe@example.com",
                PhoneNumber = "123-456-7890",
                Address = "123 Main St, Anytown, USA",
                MedicalHistory = "None",
                Notes = "Regular checkup",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            context.Patients.Add(patient);
            await context.SaveChangesAsync();
            
            var fileService = new FileService(context, _encryptionServiceMock.Object, _configurationMock.Object, _loggerMock.Object);

            // Configure encryption service mock
            _encryptionServiceMock
                .Setup(e => e.EncryptFileAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await fileService.SaveFileAsync(1, tempSourcePath, "Test file description", true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.PatientId);
            Assert.Equal("test.txt", result.FileName);
            Assert.Equal("txt", result.FileType);
            Assert.Equal("Test file description", result.Description);
            Assert.True(result.IsEncrypted);
            
            // Verify that the encryption service was called
            _encryptionServiceMock.Verify(e => e.EncryptFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            
            // Verify that the file metadata was saved to the database
            var savedMetadata = await context.FilesMetadata.FirstOrDefaultAsync(f => f.PatientId == 1);
            Assert.NotNull(savedMetadata);
            Assert.Equal("test.txt", savedMetadata.FileName);
        }

        [Fact]
        public async Task SaveFileAsync_WithoutEncryption_SavesUnencryptedFile()
        {
            // Arrange
            var tempSourcePath = Path.Combine(_tempBasePath, "test.txt");
            File.WriteAllText(tempSourcePath, "This is a test file");

            // Create a new DbContext with a unique database name for this test
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"FileServiceTestDb_NoEncryption_{Guid.NewGuid()}")
                .Options;
                
            using var context = new ApplicationDbContext(options);
            
            // Seed the database with a test patient
            var patient = new Patient
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateTime(1980, 1, 1),
                Gender = "Male",
                Email = "john.doe@example.com",
                PhoneNumber = "123-456-7890",
                Address = "123 Main St, Anytown, USA",
                MedicalHistory = "None",
                Notes = "Regular checkup",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            context.Patients.Add(patient);
            await context.SaveChangesAsync();
            
            var fileService = new FileService(context, _encryptionServiceMock.Object, _configurationMock.Object, _loggerMock.Object);

            // Act
            var result = await fileService.SaveFileAsync(1, tempSourcePath, "Test file description", false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.PatientId);
            Assert.Equal("test.txt", result.FileName);
            Assert.Equal("txt", result.FileType);
            Assert.Equal("Test file description", result.Description);
            Assert.False(result.IsEncrypted);
            
            // Verify that the encryption service was not called
            _encryptionServiceMock.Verify(e => e.EncryptFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            
            // Verify that the file metadata was saved to the database
            var savedMetadata = await context.FilesMetadata.FirstOrDefaultAsync(f => f.PatientId == 1);
            Assert.NotNull(savedMetadata);
            Assert.Equal("test.txt", savedMetadata.FileName);
        }

        [Fact]
        public async Task GetDecryptedFilePathAsync_WithEncryptedFile_ReturnsDecryptedPath()
        {
            // Arrange
            // Create a new DbContext with a unique database name for this test
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"FileServiceTestDb_DecryptPath_{Guid.NewGuid()}")
                .Options;
                
            using var context = new ApplicationDbContext(options);
            
            // Add a test patient
            var patient = new Patient
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateTime(1980, 1, 1),
                Gender = "Male",
                Email = "john.doe@example.com",
                PhoneNumber = "123-456-7890",
                Address = "123 Main St, Anytown, USA",
                MedicalHistory = "None",
                Notes = "Regular checkup",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            context.Patients.Add(patient);
            await context.SaveChangesAsync();
            
            // Add a test file metadata
            var fileMetadata = new FileMetadata
            {
                Id = 1,
                PatientId = 1,
                Patient = patient,
                FileName = "test.txt",
                FilePath = Path.Combine(_tempBasePath, "TestFiles", "TestEncrypted", "test.enc"),
                FileType = "txt",
                Description = "Test file",
                IsEncrypted = true,
                UploadDate = DateTime.Now,
                FileSize = 100
            };
            
            context.FilesMetadata.Add(fileMetadata);
            await context.SaveChangesAsync();
            
            var fileService = new FileService(context, _encryptionServiceMock.Object, _configurationMock.Object, _loggerMock.Object);
            
            // Configure encryption service mock
            var expectedDecryptedPath = Path.Combine(_tempBasePath, "TestFiles", "TestTemp", "test.txt");
            _encryptionServiceMock
                .Setup(e => e.DecryptFileAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await fileService.GetDecryptedFilePathAsync(1);

            // Assert
            Assert.NotNull(result);
            
            // Verify that the encryption service was called
            _encryptionServiceMock.Verify(e => e.DecryptFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetDecryptedFilePathAsync_WithUnencryptedFile_ReturnsOriginalPath()
        {
            // Arrange
            // Create a new DbContext with a unique database name for this test
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"FileServiceTestDb_UnencryptedPath_{Guid.NewGuid()}")
                .Options;
                
            using var context = new ApplicationDbContext(options);
            
            // Add a test patient
            var patient = new Patient
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateTime(1980, 1, 1),
                Gender = "Male",
                Email = "john.doe@example.com",
                PhoneNumber = "123-456-7890",
                Address = "123 Main St, Anytown, USA",
                MedicalHistory = "None",
                Notes = "Regular checkup",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            context.Patients.Add(patient);
            await context.SaveChangesAsync();
            
            // Add a test file metadata
            var fileMetadata = new FileMetadata
            {
                Id = 2,
                PatientId = 1,
                Patient = patient,
                FileName = "test.txt",
                FilePath = Path.Combine(_tempBasePath, "TestFiles", "1", "test.txt"),
                FileType = "txt",
                Description = "Test file",
                IsEncrypted = false,
                UploadDate = DateTime.Now,
                FileSize = 100
            };
            
            context.FilesMetadata.Add(fileMetadata);
            await context.SaveChangesAsync();
            
            var fileService = new FileService(context, _encryptionServiceMock.Object, _configurationMock.Object, _loggerMock.Object);

            // Act
            var result = await fileService.GetDecryptedFilePathAsync(2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(fileMetadata.FilePath, result);
            
            // Verify that the encryption service was not called
            _encryptionServiceMock.Verify(e => e.DecryptFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task DeleteFileAsync_WithExistingFile_DeletesFileAndMetadata()
        {
            // Arrange
            var tempFilePath = Path.Combine(_tempBasePath, "TestFiles", "1", "test.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(tempFilePath));
            File.WriteAllText(tempFilePath, "This is a test file");

            // Create a new DbContext with a unique database name for this test
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"FileServiceTestDb_DeleteFile_{Guid.NewGuid()}")
                .Options;
                
            using var context = new ApplicationDbContext(options);
            
            // Add a test patient
            var patient = new Patient
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateTime(1980, 1, 1),
                Gender = "Male",
                Email = "john.doe@example.com",
                PhoneNumber = "123-456-7890",
                Address = "123 Main St, Anytown, USA",
                MedicalHistory = "None",
                Notes = "Regular checkup",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            context.Patients.Add(patient);
            await context.SaveChangesAsync();
            
            // Add a test file metadata
            var fileMetadata = new FileMetadata
            {
                Id = 3,
                PatientId = 1,
                Patient = patient,
                FileName = "test.txt",
                FilePath = tempFilePath,
                FileType = "txt",
                Description = "Test file",
                IsEncrypted = false,
                UploadDate = DateTime.Now,
                FileSize = 100
            };
            
            context.FilesMetadata.Add(fileMetadata);
            await context.SaveChangesAsync();
            
            var fileService = new FileService(context, _encryptionServiceMock.Object, _configurationMock.Object, _loggerMock.Object);

            // Act
            await fileService.DeleteFileAsync(3);

            // Assert
            // Verify that the file was deleted
            Assert.False(File.Exists(tempFilePath));
            
            // Verify that the file metadata was removed from the database
            var deletedMetadata = await context.FilesMetadata.FindAsync(3);
            Assert.Null(deletedMetadata);
        }

        [Fact]
        public void CleanupTempFiles_DeletesAllTempFiles()
        {
            // Arrange
            var tempDir = Path.Combine(_tempBasePath, "TestFiles", "TestTemp");
            var tempFile1 = Path.Combine(tempDir, "temp1.txt");
            var tempFile2 = Path.Combine(tempDir, "temp2.txt");
            
            File.WriteAllText(tempFile1, "Temp file 1");
            File.WriteAllText(tempFile2, "Temp file 2");
            
            // Make sure the files exist before the test
            Assert.True(File.Exists(tempFile1));
            Assert.True(File.Exists(tempFile2));
            
            // Configure the configuration mock to return our test temp path
            _appSettingsSectionMock.Setup(s => s["TempFilePath"]).Returns("TestTemp");
            
            using var context = new ApplicationDbContext(_dbContextOptions);
            var fileService = new FileService(context, _encryptionServiceMock.Object, _configurationMock.Object, _loggerMock.Object);

            // Override the private _tempPath field using reflection
            var tempPathField = typeof(FileService).GetField("_tempPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            tempPathField.SetValue(fileService, tempDir);

            // Act
            fileService.CleanupTempFiles();

            // Assert
            // Verify that the temp files were deleted
            Assert.False(File.Exists(tempFile1));
            Assert.False(File.Exists(tempFile2));
        }

        public void Dispose()
        {
            // Clean up temp directory
            if (Directory.Exists(_tempBasePath))
            {
                try
                {
                    Directory.Delete(_tempBasePath, true);
                }
                catch
                {
                    // Ignore errors during cleanup
                }
            }
        }
    }
}