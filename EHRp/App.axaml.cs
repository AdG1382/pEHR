using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System;
using System.Linq;
using Avalonia.Markup.Xaml;
using EHRp.Data;
using EHRp.Services;
using EHRp.ViewModels;
using EHRp.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace EHRp
{
    /// <summary>
    /// Main application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the application.
        /// </summary>
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// Called when the framework initialization is completed.
        /// </summary>
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                try
                {
                    // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                    // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                    DisableAvaloniaDataAnnotationValidation();
                    
                    // Initialize the database
                    using (var scope = Program.ServiceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        
                        // Ensure database is deleted and recreated for development
                        dbContext.Database.EnsureDeleted();
                        dbContext.Database.EnsureCreated();
                        
                        // Initialize the database with seed data
                        DbInitializer.Initialize(dbContext);
                        
                        // Log database initialization
                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
                        logger.LogInformation("Database initialized with default user: doctor/password");
                    }
                    
                    // Initialize the theme manager
                    var themeManager = Program.ServiceProvider.GetRequiredService<ThemeManager>();
                    
                    // Apply the theme after a short delay to ensure the application is fully loaded
                    var appLogger = Program.ServiceProvider.GetRequiredService<ILogger<App>>();
                    appLogger.LogInformation("Initializing theme manager");
                    
                    // Apply theme immediately
                    themeManager.ApplyTheme();
                    
                    // And also schedule another application after UI is fully loaded
                    desktop.Startup += (_, _) => 
                    {
                        appLogger.LogInformation("Application startup completed, applying theme again");
                        themeManager.ApplyTheme();
                        
                        // Also refresh all windows after a short delay to ensure everything is loaded
                        System.Threading.Tasks.Task.Delay(500).ContinueWith(_ => 
                        {
                            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => 
                            {
                                appLogger.LogInformation("Refreshing theme on all windows after delay");
                                themeManager.RefreshThemeOnAllWindows();
                            });
                        });
                    };
                    
                    // Create the main window
                    var mainWindowViewModel = Program.ServiceProvider.GetRequiredService<MainWindowViewModel>();
                    var mainWindow = new MainWindow
                    {
                        DataContext = mainWindowViewModel,
                    };
                    
                    // Add handlers for window events
                    mainWindow.Opened += (_, _) =>
                    {
                        appLogger.LogInformation("Main window opened, refreshing theme");
                        themeManager.RefreshThemeOnAllWindows();
                    };
                    
                    mainWindow.Activated += (_, _) =>
                    {
                        appLogger.LogInformation("Main window activated, refreshing theme");
                        themeManager.RefreshThemeOnAllWindows();
                    };
                    
                    // Set as the main window
                    desktop.MainWindow = mainWindow;
                    
                    // Handle application exit
                    desktop.Exit += (sender, args) =>
                    {
                        Log.CloseAndFlush();
                    };
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "Application failed to start");
                    throw;
                }
            }

            base.OnFrameworkInitializationCompleted();
        }

        /// <summary>
        /// Disables Avalonia data annotation validation to avoid conflicts with CommunityToolkit.
        /// </summary>
        private void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}