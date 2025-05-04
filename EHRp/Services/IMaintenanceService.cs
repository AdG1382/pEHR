using System.Threading.Tasks;

namespace EHRp.Services
{
    public interface IMaintenanceService
    {
        /// <summary>
        /// Creates a backup of the database and files
        /// </summary>
        /// <param name="userId">ID of the user creating the backup</param>
        /// <returns>Path to the created backup file</returns>
        Task<string> CreateBackupAsync(int userId);
        
        /// <summary>
        /// Restores the database and files from a backup
        /// </summary>
        /// <param name="userId">ID of the user performing the restore</param>
        /// <param name="backupFilePath">Path to the backup file</param>
        /// <returns>True if restore was successful, false otherwise</returns>
        Task<bool> RestoreBackupAsync(int userId, string backupFilePath);
        
        /// <summary>
        /// Checks the integrity of the database
        /// </summary>
        /// <returns>True if database integrity check passed, false otherwise</returns>
        Task<bool> CheckDatabaseIntegrityAsync();
        
        /// <summary>
        /// Compacts the database to reduce its size
        /// </summary>
        /// <returns>True if compaction was successful, false otherwise</returns>
        Task<bool> CompactDatabaseAsync();
    }
}