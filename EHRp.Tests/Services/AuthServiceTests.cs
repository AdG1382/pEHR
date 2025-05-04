using System;
using System.Threading.Tasks;
using EHRp.Data;
using EHRp.Models;
using EHRp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EHRp.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        public AuthServiceTests()
        {
            // Set up in-memory database for testing
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"AuthServiceTestDb_{Guid.NewGuid()}")
                .Options;

            // Set up mocks
            _loggerMock = new Mock<ILogger<AuthService>>();

            // Seed the database with test data
            using var context = new ApplicationDbContext();
            context.Database.EnsureCreated();
            SeedDatabase(context);
        }

        private void SeedDatabase(ApplicationDbContext context)
        {
            // Add a test user
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                FullName = "Test User",
                Email = "test@example.com",
                CreatedAt = DateTime.Now,
                LastLogin = null
            };

            context.Users.Add(user);
            context.SaveChanges();
        }

        [Fact]
        public async Task AuthenticateAsync_WithValidCredentials_ReturnsUser()
        {
            // Arrange
            using var context = new ApplicationDbContext();
            var authService = new AuthService(context, _loggerMock.Object);

            // Act
            var result = await authService.AuthenticateAsync("testuser", "password123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testuser", result.Username);
            Assert.Equal("Test User", result.FullName);
            
            // Verify that LastLogin was updated
            Assert.NotNull(result.LastLogin);
            
            // Verify that an activity log was created
            var activityLog = await context.ActivityLogs.FirstOrDefaultAsync(a => 
                a.UserId == result.Id && a.ActivityType == Constants.AppConstants.ActivityTypes.Login);
            Assert.NotNull(activityLog);
        }

        [Fact]
        public async Task AuthenticateAsync_WithInvalidUsername_ReturnsNull()
        {
            // Arrange
            using var context = new ApplicationDbContext();
            var authService = new AuthService(context, _loggerMock.Object);

            // Act
            var result = await authService.AuthenticateAsync("nonexistentuser", "password123");

            // Assert
            Assert.Null(result);
            
            // Verify that no activity log was created
            var activityLog = await context.ActivityLogs.FirstOrDefaultAsync();
            Assert.Null(activityLog);
        }

        [Fact]
        public async Task AuthenticateAsync_WithInvalidPassword_ReturnsNull()
        {
            // Arrange
            using var context = new ApplicationDbContext();
            var authService = new AuthService(context, _loggerMock.Object);

            // Act
            var result = await authService.AuthenticateAsync("testuser", "wrongpassword");

            // Assert
            Assert.Null(result);
            
            // Verify that no activity log was created
            var activityLog = await context.ActivityLogs.FirstOrDefaultAsync();
            Assert.Null(activityLog);
        }

        [Fact]
        public async Task ChangePasswordAsync_WithValidCredentials_ReturnsTrue()
        {
            // Arrange
            using var context = new ApplicationDbContext();
            var authService = new AuthService(context, _loggerMock.Object);

            // Act
            var result = await authService.ChangePasswordAsync(1, "password123", "newpassword123");

            // Assert
            Assert.True(result);
            
            // Verify that the password was changed
            var user = await context.Users.FindAsync(1);
            Assert.True(BCrypt.Net.BCrypt.Verify("newpassword123", user.PasswordHash));
            
            // Verify that an activity log was created
            var activityLog = await context.ActivityLogs.FirstOrDefaultAsync(a => 
                a.UserId == user.Id && a.ActivityType == Constants.AppConstants.ActivityTypes.Update);
            Assert.NotNull(activityLog);
        }

        [Fact]
        public async Task ChangePasswordAsync_WithInvalidCurrentPassword_ReturnsFalse()
        {
            // Arrange
            using var context = new ApplicationDbContext();
            var authService = new AuthService(context, _loggerMock.Object);

            // Act
            var result = await authService.ChangePasswordAsync(1, "wrongpassword", "newpassword123");

            // Assert
            Assert.False(result);
            
            // Verify that the password was not changed
            var user = await context.Users.FindAsync(1);
            Assert.False(BCrypt.Net.BCrypt.Verify("newpassword123", user.PasswordHash));
            
            // Verify that no activity log was created
            var activityLog = await context.ActivityLogs.FirstOrDefaultAsync(a => 
                a.UserId == user.Id && a.ActivityType == Constants.AppConstants.ActivityTypes.Update);
            Assert.Null(activityLog);
        }

        [Fact]
        public async Task LogoutAsync_LogsUserLogoutActivity()
        {
            // Arrange
            using var context = new ApplicationDbContext();
            var authService = new AuthService(context, _loggerMock.Object);

            // Act
            await authService.LogoutAsync(1);

            // Assert
            // Verify that an activity log was created
            var activityLog = await context.ActivityLogs.FirstOrDefaultAsync(a => 
                a.UserId == 1 && a.ActivityType == Constants.AppConstants.ActivityTypes.Logout);
            Assert.NotNull(activityLog);
        }
    }
}