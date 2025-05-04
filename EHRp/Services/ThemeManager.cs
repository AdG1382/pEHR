using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using EHRp.Messages;
using Microsoft.Extensions.Logging;

namespace EHRp.Services
{
    /// <summary>
    /// Manages application themes, including switching between themes and modes (light/dark)
    /// </summary>
    public class ThemeManager : ObservableObject
    {
        private readonly ILogger<ThemeManager> _logger;
        private string _configFilePath;

        // Available themes
        public static readonly List<string> AvailableThemes = new()
        {
            "Cool Blue",
            "Fiery Red",
            "Calming Green",
            "Elegant Purple",
            "Classic Gray"
        };

        // Available modes
        public static readonly List<string> AvailableModes = new()
        {
            "Light",
            "Dark"
        };

        private string _currentTheme = "Cool Blue";
        public string CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (SetProperty(ref _currentTheme, value))
                {
                    ApplyTheme();
                    SaveThemeSettings();
                }
            }
        }

        private string _currentMode = "Light";
        public string CurrentMode
        {
            get => _currentMode;
            set
            {
                if (SetProperty(ref _currentMode, value))
                {
                    ApplyTheme();
                    SaveThemeSettings();
                }
            }
        }

        public ThemeManager(ILogger<ThemeManager> logger)
        {
            _logger = logger;
            _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "theme_settings.json");
            
            // Load theme settings from config file
            LoadThemeSettings();
        }

        /// <summary>
        /// Applies the current theme and mode to the application
        /// </summary>
        public void ApplyTheme()
        {
            try
            {
                // Get the resource dictionary for the current theme and mode
                var resourcePath = $"avares://EHRp/Themes/{GetThemeResourceName()}.axaml";
                
                // Apply the theme to the application
                var app = Application.Current;
                if (app != null)
                {
                    _logger.LogInformation("Applying theme: {Theme} - {Mode} from {ResourcePath}", 
                        CurrentTheme, CurrentMode, resourcePath);
                    
                    // Clean up existing theme styles
                    // Find and remove any previously added theme styles
                    for (int i = app.Styles.Count - 1; i >= 0; i--)
                    {
                        var style = app.Styles[i];
                        if (style is Styles && style?.ToString()?.Contains("Themes/") == true)
                        {
                            _logger.LogDebug("Removing existing theme style: {Style}", style);
                            app.Styles.RemoveAt(i);
                        }
                    }
                    
                    // Remove existing theme resources from merged dictionaries
                    if (app.Resources.MergedDictionaries.Count > 1)
                    {
                        _logger.LogDebug("Cleaning up resource dictionaries. Current count: {Count}", 
                            app.Resources.MergedDictionaries.Count);
                            
                        // Keep only the first dictionary with converters
                        while (app.Resources.MergedDictionaries.Count > 1)
                        {
                            app.Resources.MergedDictionaries.RemoveAt(1);
                        }
                    }

                    try
                    {
                        // Load the new theme styles
                        var styles = AvaloniaXamlLoader.Load(new Uri(resourcePath)) as Styles;
                        if (styles != null)
                        {
                            // Add the styles to the application
                            app.Styles.Add(styles);
                            _logger.LogDebug("Added theme styles to application");
                            
                            // Extract and add resources if any
                            if (styles.Resources != null && styles.Resources.Count > 0)
                            {
                                try
                                {
                                    // Create a new resource dictionary for the theme
                                    var resourceDictionary = new ResourceDictionary();
                                    
                                    // Add the styles resources to the application directly
                                    _logger.LogDebug("Adding theme resources to application");
                                    
                                    // Get all keys as a separate list to avoid modification issues
                                    var keys = styles.Resources.Keys.ToList();
                                    
                                    foreach (var key in keys)
                                    {
                                        try
                                        {
                                            var value = styles.Resources[key];

                                            // Add to local theme dictionary (optional)
                                            resourceDictionary[key] = value;

                                            // Update application resources
                                            if (app.Resources.ContainsKey(key))
                                            {
                                                app.Resources[key] = value;
                                                _logger.LogDebug("Updated existing resource: {Key}", key);
                                            }
                                            else
                                            {
                                                app.Resources.Add(key, value);
                                                _logger.LogDebug("Added new resource: {Key}", key);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.LogError(ex, "Error processing resource key: {Key}", key);
                                        }
                                    }
                                    
                                    // Add the resource dictionary to the application
                                    app.Resources.MergedDictionaries.Add(resourceDictionary);
                                    _logger.LogDebug("Added resource dictionary to application");
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Error processing theme resources");
                                }
                            }
                            
                            // Force update of all top-level windows to apply the theme
                            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                            {
                                foreach (var window in desktop.Windows)
                                {
                                    try
                                    {
                                        // This will trigger a visual update
                                        window.InvalidateVisual();
                                        
                                        // Force background update on window content
                                        if (window.Content is Control windowContent)
                                        {
                                            // Recursively update all child controls
                                            UpdateControlRecursively(windowContent);
                                        }
                                        
                                        _logger.LogDebug("Updated window: {Window}", window.GetType().Name);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, "Error updating window: {Window}", window.GetType().Name);
                                    }
                                }
                            }
                            else
                            {
                                _logger.LogWarning("Could not find application lifetime to update windows");
                            }
                            
                            _logger.LogInformation("Theme styles applied successfully: {Theme} - {Mode}", CurrentTheme, CurrentMode);
                        }
                        else
                        {
                            _logger.LogError("Failed to load theme styles: {ResourcePath}", resourcePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error loading theme resource: {ResourcePath}", resourcePath);
                        
                        // Fallback to default theme
                        if (CurrentTheme != "Cool Blue" || CurrentMode != "Light")
                        {
                            _currentTheme = "Cool Blue";
                            _currentMode = "Light";
                            ApplyTheme();
                            return;
                        }
                    }

                    // Notify the application that the theme has changed
                    WeakReferenceMessenger.Default.Send(new ThemeChangedMessage(CurrentTheme, CurrentMode));
                    
                    _logger.LogInformation("Theme applied: {Theme} - {Mode}", CurrentTheme, CurrentMode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying theme: {Theme} - {Mode}", CurrentTheme, CurrentMode);
            }
        }
        
        /// <summary>
        /// Recursively updates all controls to apply the new theme
        /// </summary>
        private void UpdateControlRecursively(Control control)
        {
            try
            {
                // Force visual update on the control
                control.InvalidateVisual();
                
                // Update the Background property if it's bound to a DynamicResource
                UpdateDynamicResourceBindings(control);
                
                // Recursively update child controls
                if (control is Panel panel)
                {
                    foreach (var child in panel.Children)
                    {
                        if (child is Control childControl)
                        {
                            UpdateControlRecursively(childControl);
                        }
                    }
                }
                else if (control is ContentControl contentControl)
                {
                    if (contentControl.Content is Control content)
                    {
                        UpdateControlRecursively(content);
                    }
                }
                else if (control is ItemsControl itemsControl)
                {
                    // Update items presenter
                    if (itemsControl.Presenter is Control presenter)
                    {
                        UpdateControlRecursively(presenter);
                    }
                    
                    // Update visible items
                    foreach (var item in itemsControl.GetRealizedContainers())
                    {
                        if (item is Control itemControl)
                        {
                            UpdateControlRecursively(itemControl);
                        }
                    }
                }
                
                // Special handling for UserControl which might contain content presenters
                if (control is UserControl userControl)
                {
                    // Find presenters within the UserControl
                    foreach (var presenter in userControl.GetVisualDescendants()
                        .Where(x => x is Control && x.GetType().Name.Contains("Presenter")))
                    {
                        if (presenter is Control presenterControl)
                        {
                            UpdateControlRecursively(presenterControl);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating control: {Control}", control.GetType().Name);
            }
        }
        
        /// <summary>
        /// Updates dynamic resource bindings on a control
        /// </summary>
        private void UpdateDynamicResourceBindings(Control control)
        {
            try
            {
                // Force the control to re-evaluate all bindings
                control.InvalidateVisual();
                
                // For Border controls, handle specific properties
                if (control is Border border)
                {
                    // Force background and border brush to update
                    border.InvalidateVisual();
                }
                
                // For TextBlock controls, handle specific properties
                if (control is TextBlock textBlock)
                {
                    // Force foreground to update
                    textBlock.InvalidateVisual();
                }
                
                // For buttons, handle specific properties
                if (control is Button button)
                {
                    // Force background to update
                    button.InvalidateVisual();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dynamic resource bindings for control: {Control}", control.GetType().Name);
            }
        }

        /// <summary>
        /// Gets the resource name for the current theme and mode
        /// </summary>
        private string GetThemeResourceName()
        {
            // Convert theme name to resource name (e.g., "Cool Blue" -> "CoolBlue")
            string themeName = CurrentTheme.Replace(" ", "");
            
            // Append mode (e.g., "CoolBlue" + "Light" -> "CoolBlueLight")
            return $"{themeName}{CurrentMode}";
        }

        /// <summary>
        /// Loads theme settings from the config file
        /// </summary>
        private void LoadThemeSettings()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    var json = File.ReadAllText(_configFilePath);
                    var settings = JsonSerializer.Deserialize<ThemeSettings>(json);
                    
                    if (settings != null)
                    {
                        // Set properties without triggering ApplyTheme yet
                        _currentTheme = settings.Theme;
                        _currentMode = settings.Mode;
                        
                        // Now apply the theme
                        ApplyTheme();
                        
                        _logger.LogInformation("Theme settings loaded: {Theme} - {Mode}", CurrentTheme, CurrentMode);
                    }
                }
                else
                {
                    // Use defaults and save them
                    SaveThemeSettings();
                    _logger.LogInformation("Default theme settings applied: {Theme} - {Mode}", CurrentTheme, CurrentMode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading theme settings");
            }
        }

        /// <summary>
        /// Saves theme settings to the config file
        /// </summary>
        private void SaveThemeSettings()
        {
            try
            {
                var settings = new ThemeSettings
                {
                    Theme = CurrentTheme,
                    Mode = CurrentMode
                };
                
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configFilePath, json);
                
                _logger.LogInformation("Theme settings saved: {Theme} - {Mode}", CurrentTheme, CurrentMode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving theme settings");
            }
        }

        /// <summary>
        /// Class to store theme settings
        /// </summary>
        private class ThemeSettings
        {
            public string Theme { get; set; } = "Cool Blue";
            public string Mode { get; set; } = "Light";
        }
        
        /// <summary>
        /// Refreshes the theme on a specific control
        /// </summary>
        /// <param name="control">The control to refresh</param>
        public void RefreshThemeOnControl(Control control)
        {
            if (control == null)
            {
                _logger.LogWarning("Attempted to refresh theme on null control");
                return;
            }
            
            try
            {
                _logger.LogInformation("Refreshing theme on control: {ControlType}", control.GetType().Name);
                UpdateControlRecursively(control);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing theme on control: {ControlType}", control.GetType().Name);
            }
        }
        
        /// <summary>
        /// Refreshes the theme on all open windows
        /// </summary>
        public void RefreshThemeOnAllWindows()
        {
            try
            {
                _logger.LogInformation("Refreshing theme on all windows");
                
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    foreach (var window in desktop.Windows)
                    {
                        try
                        {
                            window.InvalidateVisual();
                            
                            if (window.Content is Control content)
                            {
                                UpdateControlRecursively(content);
                            }
                            
                            _logger.LogDebug("Refreshed theme on window: {WindowType}", window.GetType().Name);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error refreshing theme on window: {WindowType}", window.GetType().Name);
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Could not find application lifetime to refresh windows");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing theme on all windows");
            }
        }
    }
}