using System;
using System.IO;
using System.Threading.Tasks;
using EHRp.Data;
using EHRp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EHRp.Services
{
    /// <summary>
    /// Interface for file management services.
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// Saves a file for a patient asynchronously.
        /// </summary>
        /// <param name="patientId">The patient ID.</param>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="description">The file description.</param>
        /// <param name="encrypt">Whether to encrypt the file.</param>
        /// <returns>The file metadata.</returns>
        Task<FileMetadata> SaveFileAsync(int patientId, string sourceFilePath, string description, bool encrypt);
        
        /// <summary>
        /// Gets the decrypted file path for a file asynchronously.
        /// </summary>
        /// <param name="fileId">The file ID.</param>
        /// <returns>The decrypted file path.</returns>
        Task<string> GetDecryptedFilePathAsync(int fileId);
        
        /// <summary>
        /// Deletes a file asynchronously.
        /// </summary>
        /// <param name="fileId">The file ID.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteFileAsync(int fileId);
        
        /// <summary>
        /// Cleans up temporary files.
        /// </summary>
        void CleanupTempFiles();
    }

    /// <summary>
    /// Implementation of the file management service.
    /// </summary>
    public class FileService : IFileService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<FileService> _logger;
        private readonly string _baseFilePath;
        private readonly string _encryptedPath;
        private readonly string _tempPath;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="FileService"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="encryptionService">The encryption service.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="logger">The logger instance.</param>
        public FileService(
            ApplicationDbContext context, 
            IEncryptionService encryptionService,
            IConfiguration configuration,
            ILogger<FileService> logger)
        {
            _context = context;
            _encryptionService = encryptionService;
            _logger = logger;
            
            // Get paths from configuration
            var appSettings = configuration.GetSection("AppSettings");
            string fileStoragePath = appSettings["FileStoragePath"] ?? "Files";
            string encryptedFilePath = appSettings["EncryptedFilePath"] ?? "Encrypted";
            string tempFilePath = appSettings["TempFilePath"] ?? "Temp";
            
            // Set up base file path in AppData
            _baseFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EHRp",
                fileStoragePath);
                
            // Create directory if it doesn't exist
            if (!Directory.Exists(_baseFilePath))
            {
                Directory.CreateDirectory(_baseFilePath);
                _logger.LogInformation("Created base file directory: {Path}", _baseFilePath);
            }
            
            // Create encrypted files directory
            _encryptedPath = Path.Combine(_baseFilePath, encryptedFilePath);
            if (!Directory.Exists(_encryptedPath))
            {
                Directory.CreateDirectory(_encryptedPath);
                _logger.LogInformation("Created encrypted files directory: {Path}", _encryptedPath);
            }
            
            // Create temp directory
            _tempPath = Path.Combine(_baseFilePath, tempFilePath);
            if (!Directory.Exists(_tempPath))
            {
                Directory.CreateDirectory(_tempPath);
                _logger.LogInformation("Created temp files directory: {Path}", _tempPath);
            }
        }
        
        /// <inheritdoc/>
        public async Task<FileMetadata> SaveFileAsync(int patientId, string sourceFilePath, string description, bool encrypt)
        {
            try
            {
                // Check if patient exists
                var patient = await _context.Patients
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == patientId);
                    
                if (patient == null)
                {
                    _logger.LogWarning("Patient not found: {PatientId}", patientId);
                    throw new ArgumentException("Patient not found", nameof(patientId));
                }
                
                // Get the tracked patient entity if it exists
                var trackedPatient = await _context.Patients.FindAsync(patientId);
                    
                // Get file info
                var fileInfo = new FileInfo(sourceFilePath);
                if (!fileInfo.Exists)
                {
                    _logger.LogWarning("Source file does not exist: {FilePath}", sourceFilePath);
                    throw new ArgumentException("Source file does not exist", nameof(sourceFilePath));
                }
                    
                // Create patient directory if it doesn't exist
                string patientDirectory = Path.Combine(_baseFilePath, patientId.ToString());
                if (!Directory.Exists(patientDirectory))
                {
                    Directory.CreateDirectory(patientDirectory);
                    _logger.LogDebug("Created patient directory: {Path}", patientDirectory);
                }
                
                // Generate unique filename
                string fileName = $"{DateTime.Now:yyyyMMddHHmmss}_{fileInfo.Name}";
                string destinationPath = Path.Combine(patientDirectory, fileName);
                
                // Handle encryption if needed
                if (encrypt)
                {
                    string encryptedFileName = $"{Guid.NewGuid()}.enc";
                    string encryptedPath = Path.Combine(_encryptedPath, encryptedFileName);
                    
                    // Encrypt and save the file asynchronously
                    await _encryptionService.EncryptFileAsync(sourceFilePath, encryptedPath);
                    
                    // Create file metadata
                    var fileMetadata = new FileMetadata
                    {
                        PatientId = patientId,
                        Patient = trackedPatient ?? patient,
                        FileName = fileInfo.Name,
                        FilePath = encryptedPath,
                        FileType = fileInfo.Extension.TrimStart('.').ToLower(),
                        Description = description,
                        IsEncrypted = true,
                        UploadDate = DateTime.Now,
                        FileSize = fileInfo.Length
                    };
                    
                    _context.FilesMetadata.Add(fileMetadata);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Saved encrypted file for patient {PatientId}: {FileName}", patientId, fileInfo.Name);
                    return fileMetadata;
                }
                else
                {
                    // Copy file without encryption using streams for better performance with large files
                    using (var sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                    using (var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                    {
                        await sourceStream.CopyToAsync(destinationStream);
                    }
                    
                    // Create file metadata
                    var fileMetadata = new FileMetadata
                    {
                        PatientId = patientId,
                        Patient = trackedPatient ?? patient,
                        FileName = fileInfo.Name,
                        FilePath = destinationPath,
                        FileType = fileInfo.Extension.TrimStart('.').ToLower(),
                        Description = description,
                        IsEncrypted = false,
                        UploadDate = DateTime.Now,
                        FileSize = fileInfo.Length
                    };
                    
                    _context.FilesMetadata.Add(fileMetadata);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Saved unencrypted file for patient {PatientId}: {FileName}", patientId, fileInfo.Name);
                    return fileMetadata;
                }
            }
            catch (Exception ex) when (ex is not ArgumentException)
            {
                _logger.LogError(ex, "Error saving file for patient {PatientId}: {FilePath}", patientId, sourceFilePath);
                throw new InvalidOperationException($"Failed to save file for patient {patientId}.", ex);
            }
        }
        
        /// <inheritdoc/>
        public async Task<string> GetDecryptedFilePathAsync(int fileId)
        {
            try
            {
                var fileMetadata = await _context.FilesMetadata
                    .AsNoTracking()
                    .FirstOrDefaultAsync(f => f.Id == fileId);
                    
                if (fileMetadata == null)
                {
                    _logger.LogWarning("File not found: {FileId}", fileId);
                    throw new ArgumentException("File not found", nameof(fileId));
                }
                    
                if (!fileMetadata.IsEncrypted)
                {
                    _logger.LogDebug("File is not encrypted, returning original path: {FilePath}", fileMetadata.FilePath);
                    return fileMetadata.FilePath;
                }
                    
                // Generate temp file path with a unique name to avoid conflicts
                string tempFilePath = Path.Combine(_tempPath, $"{Guid.NewGuid()}_{fileMetadata.FileName}");
                
                // Decrypt file asynchronously
                await _encryptionService.DecryptFileAsync(fileMetadata.FilePath, tempFilePath);
                
                _logger.LogInformation("Decrypted file {FileId} to temporary location", fileId);
                return tempFilePath;
            }
            catch (Exception ex) when (ex is not ArgumentException)
            {
                _logger.LogError(ex, "Error getting decrypted file path for file {FileId}", fileId);
                throw new InvalidOperationException($"Failed to get decrypted file path for file {fileId}.", ex);
            }
        }
        
        /// <inheritdoc/>
        public async Task DeleteFileAsync(int fileId)
        {
            try
            {
                var fileMetadata = await _context.FilesMetadata.FindAsync(fileId);
                if (fileMetadata == null)
                {
                    _logger.LogWarning("File not found for deletion: {FileId}", fileId);
                    throw new ArgumentException("File not found", nameof(fileId));
                }
                    
                // Delete physical file
                if (File.Exists(fileMetadata.FilePath))
                {
                    File.Delete(fileMetadata.FilePath);
                    _logger.LogDebug("Deleted physical file: {FilePath}", fileMetadata.FilePath);
                }
                else
                {
                    _logger.LogWarning("Physical file not found for deletion: {FilePath}", fileMetadata.FilePath);
                }
                
                // Remove from database
                _context.FilesMetadata.Remove(fileMetadata);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Deleted file metadata for file {FileId}", fileId);
            }
            catch (Exception ex) when (ex is not ArgumentException)
            {
                _logger.LogError(ex, "Error deleting file {FileId}", fileId);
                throw new InvalidOperationException($"Failed to delete file {fileId}.", ex);
            }
        }
        
        /// <inheritdoc/>
        public void CleanupTempFiles()
        {
            try
            {
                if (Directory.Exists(_tempPath))
                {
                    int deletedCount = 0;
                    int errorCount = 0;
                    
                    foreach (var file in Directory.GetFiles(_tempPath))
                    {
                        try
                        {
                            File.Delete(file);
                            deletedCount++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error deleting temp file: {FilePath}", file);
                            errorCount++;
                        }
                    }
                    
                    _logger.LogInformation("Cleaned up temp files. Deleted: {DeletedCount}, Errors: {ErrorCount}", 
                        deletedCount, errorCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up temp files");
            }
        }
    }
}