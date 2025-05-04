using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using EHRp.Constants;
using EHRp.Data;
using EHRp.Messages;
using EHRp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EHRp.Services
{
    /// <summary>
    /// Interface for authentication services.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates a user asynchronously.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The authenticated user, or null if authentication failed.</returns>
        Task<User> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Changes a user's password asynchronously.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="currentPassword">The current password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the password was changed successfully, false otherwise.</returns>
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Logs out a user asynchronously.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LogoutAsync(int userId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Validates a password against the password policy.
        /// </summary>
        /// <param name="password">The password to validate.</param>
        /// <returns>True if the password meets the policy requirements, false otherwise.</returns>
        bool IsPasswordValid(string password);
    }

    /// <summary>
    /// Implementation of the authentication service.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthService> _logger;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthService"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="logger">The logger instance.</param>
        public AuthService(
            ApplicationDbContext context,
            ILogger<AuthService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <inheritdoc/>
        public async Task<User> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    _logger.LogWarning("Authentication attempt with empty username or password");
                    return null;
                }
                
                _logger.LogDebug("Authentication attempt for user: {Username}", username);
                
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
                    
                // Check if user exists
                if (user == null)
                {
                    _logger.LogWarning("Authentication failed: User not found: {Username}", username);
                    return null;
                }
                    
                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    _logger.LogWarning("Authentication failed: Invalid password for user: {Username}", username);
                    return null;
                }
                    
                // Update last login
                user.LastLogin = DateTime.Now;
                
                // Log activity
                var activityLog = new ActivityLog
                {
                    UserId = user.Id,
                    ActivityType = AppConstants.ActivityTypes.Login,
                    Description = "User logged in",
                    EntityType = AppConstants.EntityTypes.User,
                    EntityId = user.Id,
                    Timestamp = DateTime.Now
                };
                
                _context.ActivityLogs.Add(activityLog);
                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("User authenticated successfully: {Username} (ID: {UserId})", username, user.Id);
                
                // Send message that user logged in using the WeakReferenceMessenger
                WeakReferenceMessenger.Default.Send(new UserLoggedInMessage(user));
                
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating user: {Username}", username);
                throw new InvalidOperationException($"Failed to authenticate user: {username}", ex);
            }
        }
        
        /// <inheritdoc/>
        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
                
                if (user == null)
                {
                    _logger.LogWarning("Password change failed: User not found: {UserId}", userId);
                    return false;
                }
                    
                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                {
                    _logger.LogWarning("Password change failed: Invalid current password for user: {UserId}", userId);
                    return false;
                }
                
                // Validate new password
                if (!IsPasswordValid(newPassword))
                {
                    _logger.LogWarning("Password change failed: New password does not meet requirements for user: {UserId}", userId);
                    return false;
                }
                    
                // Update password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                
                // Log activity
                var activityLog = new ActivityLog
                {
                    UserId = user.Id,
                    ActivityType = AppConstants.ActivityTypes.Update,
                    Description = "Password changed",
                    EntityType = AppConstants.EntityTypes.User,
                    EntityId = user.Id,
                    Timestamp = DateTime.Now
                };
                
                _context.ActivityLogs.Add(activityLog);
                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
                throw new InvalidOperationException($"Failed to change password for user: {userId}", ex);
            }
        }
        
        /// <inheritdoc/>
        public async Task LogoutAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Log activity
                var activityLog = new ActivityLog
                {
                    UserId = userId,
                    ActivityType = AppConstants.ActivityTypes.Logout,
                    Description = "User logged out",
                    EntityType = AppConstants.EntityTypes.User,
                    EntityId = userId,
                    Timestamp = DateTime.Now
                };
                
                _context.ActivityLogs.Add(activityLog);
                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("User logged out: {UserId}", userId);
                
                // Send message that user logged out using the WeakReferenceMessenger
                WeakReferenceMessenger.Default.Send(new UserLoggedOutMessage(userId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging out user: {UserId}", userId);
                throw new InvalidOperationException($"Failed to log out user: {userId}", ex);
            }
        }
        
        /// <inheritdoc/>
        public bool IsPasswordValid(string password)
        {
            // Password policy:
            // - At least 8 characters
            // - At least one uppercase letter
            // - At least one lowercase letter
            // - At least one digit
            // - At least one special character
            
            if (string.IsNullOrEmpty(password) || password.Length < 8)
            {
                return false;
            }
            
            bool hasUppercase = password.Any(char.IsUpper);
            bool hasLowercase = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecialChar = password.Any(c => !char.IsLetterOrDigit(c));
            
            return hasUppercase && hasLowercase && hasDigit && hasSpecialChar;
        }
    }
}