using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using EHRp.Models;
using EHRp.ViewModels.Patients;
using System;
using Microsoft.Extensions.Logging;

namespace EHRp.Views.Patients
{
    public partial class PatientDetailView : UserControl
    {
        public PatientDetailView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        private void ViewLabReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is FileMetadata report && DataContext is PatientDetailViewModel viewModel)
                {
                    viewModel.ViewLabReportCommand.Execute(report);
                }
            }
            catch (Exception ex)
            {
                // Log the error through the view model's logger
                if (DataContext is PatientDetailViewModel viewModel)
                {
                    // The view model will handle the logging
                    viewModel.ErrorMessage = $"Error viewing lab report: {ex.Message}";
                }
                
                // Just catch the exception to prevent crashes
            }
        }
        
        /// <summary>
        /// Handles tab button clicks with error handling
        /// </summary>
        private void TabButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && DataContext is PatientDetailViewModel viewModel)
                {
                    // Get the tab index from the button's Tag
                    if (button.Tag is string tagStr && int.TryParse(tagStr, out int tabIndex))
                    {
                        // Set the selected tab index
                        viewModel.SelectedTabIndex = tabIndex;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error through the view model's logger
                if (DataContext is PatientDetailViewModel viewModel)
                {
                    // The view model will handle the logging
                    viewModel.ErrorMessage = $"Error switching tabs: {ex.Message}";
                }
                
                // Just catch the exception to prevent crashes
            }
        }
    }
}