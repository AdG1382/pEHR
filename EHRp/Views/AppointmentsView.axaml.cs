using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EHRp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace EHRp.Views
{
    public partial class AppointmentsView : UserControl
    {
        private readonly ILogger<AppointmentsView>? _logger;

        public AppointmentsView()
        {
            InitializeComponent();
            
            try
            {
                // Try to get the logger from the service provider
                var serviceProvider = Program.ServiceProvider;
                if (serviceProvider != null)
                {
                    _logger = serviceProvider.GetService<ILogger<AppointmentsView>>();
                }
                
                // Set a fallback ViewModel if DataContext is null
                if (DataContext == null)
                {
                    _logger?.LogWarning("AppointmentsView created without DataContext, using fallback ViewModel");
                    
                    // Try to get the ViewModel from DI
                    if (serviceProvider != null)
                    {
                        try
                        {
                            DataContext = serviceProvider.GetService<AppointmentsViewModel>();
                            _logger?.LogInformation("Successfully set AppointmentsViewModel from DI");
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, "Failed to get AppointmentsViewModel from DI, creating new instance");
                            DataContext = new AppointmentsViewModel();
                        }
                    }
                    else
                    {
                        // Create a new instance as last resort
                        DataContext = new AppointmentsViewModel();
                        _logger?.LogWarning("Created new AppointmentsViewModel instance as fallback");
                    }
                }
            }
            catch (Exception ex)
            {
                // Last resort fallback if everything else fails
                DataContext = new AppointmentsViewModel();
                Console.WriteLine($"Critical error in AppointmentsView initialization: {ex.Message}");
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}