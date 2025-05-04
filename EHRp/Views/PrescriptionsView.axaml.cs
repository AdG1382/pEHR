using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EHRp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace EHRp.Views
{
    public partial class PrescriptionsView : UserControl
    {
        private readonly ILogger<PrescriptionsView>? _logger;

        public PrescriptionsView()
        {
            InitializeComponent();
            
            try
            {
                // Try to get the logger from the service provider
                var serviceProvider = Program.ServiceProvider;
                if (serviceProvider != null)
                {
                    _logger = serviceProvider.GetService<ILogger<PrescriptionsView>>();
                }
                
                // Set a fallback ViewModel if DataContext is null
                if (DataContext == null)
                {
                    _logger?.LogWarning("PrescriptionsView created without DataContext, using fallback ViewModel");
                    
                    // Try to get the ViewModel from DI
                    if (serviceProvider != null)
                    {
                        try
                        {
                            DataContext = serviceProvider.GetService<PrescriptionsViewModel>();
                            _logger?.LogInformation("Successfully set PrescriptionsViewModel from DI");
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, "Failed to get PrescriptionsViewModel from DI, creating new instance");
                            DataContext = new PrescriptionsViewModel();
                        }
                    }
                    else
                    {
                        // Create a new instance as last resort
                        DataContext = new PrescriptionsViewModel();
                        _logger?.LogWarning("Created new PrescriptionsViewModel instance as fallback");
                    }
                }
            }
            catch (Exception ex)
            {
                // Last resort fallback if everything else fails
                DataContext = new PrescriptionsViewModel();
                Console.WriteLine($"Critical error in PrescriptionsView initialization: {ex.Message}");
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}