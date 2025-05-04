using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Diagnostics;
using Avalonia.Controls.Primitives;

namespace EHRp.Views.Dashboard
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
            
            // Ensure DataContext is properly set
            this.AttachedToVisualTree += (s, e) => {
                Debug.WriteLine("DashboardView attached to visual tree");
                Debug.WriteLine($"DataContext is: {DataContext?.GetType().Name ?? "null"}");
            };
        }
        
        private void OnTestButtonClick(object sender, RoutedEventArgs e)
        {
            // This is a simple code-behind event handler to test button clicks
            Debug.WriteLine("Test button clicked from code-behind!");
            
            // Show a popup to confirm the button works
            var flyout = new Flyout
            {
                Content = new TextBlock { Text = "Button clicked successfully!" },
                Placement = PlacementMode.Bottom
            };
            
            if (sender is Control control)
            {
                FlyoutBase.SetAttachedFlyout(control, flyout);
                FlyoutBase.ShowAttachedFlyout(control);
            }
        }
    }
}