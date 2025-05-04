using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using EHRp.Data;
using EHRp.Models;
using Microsoft.EntityFrameworkCore;

namespace EHRp.Services
{
    public class MaintenanceService : Services.IMaintenanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _appDataPath;
        private readonly string _backupPath;
        
        public MaintenanceService(ApplicationDbContext context)
        {
            _context = context;
            
            // Set up paths
            _appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EHRp");
                
            _backupPath = Path.Combine(_appDataPath, "Backups");
            
            // Create backup directory if it doesn't exist
            if (!Directory.Exists(_backupPath))
            {
                Directory.CreateDirectory(_backupPath);
            }
        }
        
        public async Task<string> CreateBackupAsync(int userId)
        {
            // Close the database connection
            await _context.Database.CloseConnectionAsync();
            
            // Get database file path
            string dbPath = Path.Combine(_appDataPath, "ehrp.db");
            
            // Generate backup filename with timestamp
            string backupFileName = $"EHRp_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
            string backupFilePath = Path.Combine(_backupPath, backupFileName);
            
            // Create zip file
            using (var zipArchive = ZipFile.Open(backupFilePath, ZipArchiveMode.Create))
            {
                // Add database file
                zipArchive.CreateEntryFromFile(dbPath, "ehrp.db");
                
                // Add files directory if it exists
                string filesPath = Path.Combine(_appDataPath, "Files");
                if (Directory.Exists(filesPath))
                {
                    foreach (var file in Directory.GetFiles(filesPath, "*", SearchOption.AllDirectories))
                    {
                        // Get relative path for zip entry
                        string relativePath = file.Substring(filesPath.Length + 1);
                        zipArchive.CreateEntryFromFile(file, Path.Combine("Files", relativePath));
                    }
                }
            }
            
            // Reopen the database connection
            await _context.Database.OpenConnectionAsync();
            
            // Update user settings with last backup date
            var userSettings = await _context.UserSettings
                .FirstOrDefaultAsync(us => us.UserId == userId);
                
            if (userSettings != null)
            {
                userSettings.LastBackupDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            
            // Log activity
            var activityLog = new ActivityLog
            {
                UserId = userId,
                ActivityType = "Backup",
                Description = $"Database backup created: {backupFileName}",
                EntityType = "System",
                Timestamp = DateTime.Now
            };
            
            _context.ActivityLogs.Add(activityLog);
            await _context.SaveChangesAsync();
            
            return backupFilePath;
        }
        
        public async Task<bool> RestoreBackupAsync(int userId, string backupFilePath)
        {
            if (!File.Exists(backupFilePath))
                throw new ArgumentException("Backup file does not exist", nameof(backupFilePath));
                
            // Close the database connection
            await _context.Database.CloseConnectionAsync();
            
            // Get database file path
            string dbPath = Path.Combine(_appDataPath, "ehrp.db");
            
            // Create temp directory for extraction
            string tempPath = Path.Combine(_appDataPath, "Temp");
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
            Directory.CreateDirectory(tempPath);
            
            try
            {
                // Extract backup
                ZipFile.ExtractToDirectory(backupFilePath, tempPath);
                
                // Replace database file
                string extractedDbPath = Path.Combine(tempPath, "ehrp.db");
                if (File.Exists(extractedDbPath))
                {
                    File.Copy(extractedDbPath, dbPath, true);
                }
                else
                {
                    throw new FileNotFoundException("Database file not found in backup");
                }
                
                // Replace files directory if it exists in the backup
                string extractedFilesPath = Path.Combine(tempPath, "Files");
                if (Directory.Exists(extractedFilesPath))
                {
                    string filesPath = Path.Combine(_appDataPath, "Files");
                    
                    // Delete existing files directory
                    if (Directory.Exists(filesPath))
                    {
                        Directory.Delete(filesPath, true);
                    }
                    
                    // Copy extracted files directory
                    DirectoryCopy(extractedFilesPath, filesPath, true);
                }
                
                // Reopen the database connection
                await _context.Database.OpenConnectionAsync();
                
                // Log activity
                var activityLog = new ActivityLog
                {
                    UserId = userId,
                    ActivityType = "Restore",
                    Description = $"Database restored from backup: {Path.GetFileName(backupFilePath)}",
                    EntityType = "System",
                    Timestamp = DateTime.Now
                };
                
                _context.ActivityLogs.Add(activityLog);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                // Log error
                var activityLog = new ActivityLog
                {
                    UserId = userId,
                    ActivityType = "Error",
                    Description = $"Backup restore failed: {ex.Message}",
                    EntityType = "System",
                    Timestamp = DateTime.Now
                };
                
                // Reopen the database connection
                await _context.Database.OpenConnectionAsync();
                
                _context.ActivityLogs.Add(activityLog);
                await _context.SaveChangesAsync();
                
                return false;
            }
            finally
            {
                // Clean up temp directory
                if (Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, true);
                }
            }
        }
        
        public async Task<bool> CheckDatabaseIntegrityAsync()
        {
            try
            {
                // Run SQLite PRAGMA integrity_check
                var result = await _context.Database
                    .ExecuteSqlRawAsync("PRAGMA integrity_check;");
                
                return result > 0;
            }
            catch
            {
                return false;
            }
        }
        
        public async Task<bool> CompactDatabaseAsync()
        {
            try
            {
                // Run SQLite VACUUM command to compact the database
                await _context.Database.ExecuteSqlRawAsync("VACUUM;");
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
    }
}