using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using EHRp.Messages;
using EHRp.Services;
using Microsoft.Extensions.Logging;

namespace EHRp.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        private readonly ThemeManager _themeManager;
        private readonly ILogger<SettingsViewModel> _logger;
        
        [ObservableProperty]
        private string _selectedTheme;
        
        [ObservableProperty]
        private string _selectedMode;
        
        [ObservableProperty]
        private string _backupPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        
        [ObservableProperty]
        private bool _autoBackup = true;
        
        [ObservableProperty]
        private int _backupFrequencyDays = 7;
        
        [ObservableProperty]
        private string _fontSize = "Medium"; // Small, Medium, Large
        
        [ObservableProperty]
        private string _buttonStyle = "Rounded"; // Rounded or Flat
        
        [ObservableProperty]
        private string _defaultPrescriptionFormat = "Standard";
        
        [ObservableProperty]
        private string _statusMessage = "";
        
        [ObservableProperty]
        private bool _isStatusSuccess;
        
        public List<string> AvailableThemes => ThemeManager.AvailableThemes;
        
        public List<string> AvailableModes => ThemeManager.AvailableModes;
        
        public SettingsViewModel(ThemeManager themeManager, ILogger<SettingsViewModel> logger)
        {
            _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Initialize theme and mode from ThemeManager
            _selectedTheme = _themeManager.CurrentTheme;
            _selectedMode = _themeManager.CurrentMode;
            
            // Register for theme changed messages
            WeakReferenceMessenger.Default.Register<ThemeChangedMessage>(this, (r, m) => 
            {
                _selectedTheme = m.Value.Theme;
                _selectedMode = m.Value.Mode;
                OnPropertyChanged(nameof(SelectedTheme));
                OnPropertyChanged(nameof(SelectedMode));
            });
        }
        
        partial void OnSelectedThemeChanged(string value)
        {
            if (_themeManager != null)
            {
                _themeManager.CurrentTheme = value;
                _logger.LogInformation("Theme changed to: {Theme}", value);
            }
        }
        
        partial void OnSelectedModeChanged(string value)
        {
            if (_themeManager != null)
            {
                _themeManager.CurrentMode = value;
                _logger.LogInformation("Mode changed to: {Mode}", value);
            }
        }
        
        [RelayCommand]
        private void SaveSettings()
        {
            try
            {
                // This would save the settings to the database
                
                // Apply theme changes
                _themeManager.ApplyTheme();
                
                // Also refresh all windows to ensure complete update
                _themeManager.RefreshThemeOnAllWindows();
                
                StatusMessage = "Settings saved successfully";
                IsStatusSuccess = true;
                _logger.LogInformation("Theme applied after saving settings");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error saving settings: {ex.Message}";
                IsStatusSuccess = false;
                _logger.LogError(ex, "Error saving settings");
            }
        }
        
        [RelayCommand]
        private void ResetToDefaults()
        {
            // Reset settings to defaults
            SelectedTheme = "Cool Blue";
            SelectedMode = "Light";
            BackupPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            AutoBackup = true;
            BackupFrequencyDays = 7;
            FontSize = "Medium";
            ButtonStyle = "Rounded";
            DefaultPrescriptionFormat = "Standard";
            
            StatusMessage = "Settings reset to defaults";
            IsStatusSuccess = true;
        }
        
        [RelayCommand]
        private void BrowseBackupPath()
        {
            // This would open a folder browser dialog
            // For now, just set a dummy path
            BackupPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\EHRp Backups";
        }
        
        [RelayCommand]
        private void ChangeFontSize(string size)
        {
            FontSize = size;
            // This would apply the font size to the application
        }
        
        [RelayCommand]
        private void ChangeButtonStyle(string style)
        {
            ButtonStyle = style;
            // This would apply the button style to the application
        }
        
        [RelayCommand]
        private void ApplyThemeImmediately()
        {
            try
            {
                // Force immediate theme application
                _themeManager.ApplyTheme();
                
                // Also refresh all windows to ensure complete update
                _themeManager.RefreshThemeOnAllWindows();
                
                StatusMessage = $"Theme '{SelectedTheme}' in {SelectedMode} mode applied immediately";
                IsStatusSuccess = true;
                _logger.LogInformation("Theme applied immediately: {Theme} - {Mode}", SelectedTheme, SelectedMode);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error applying theme: {ex.Message}";
                IsStatusSuccess = false;
                _logger.LogError(ex, "Error applying theme immediately");
            }
        }
        
        public override void Cleanup()
        {
            // Unregister from messages
            WeakReferenceMessenger.Default.Unregister<ThemeChangedMessage>(this);
            
            base.Cleanup();
        }
    }
}