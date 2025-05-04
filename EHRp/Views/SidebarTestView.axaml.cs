using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;

namespace EHRp.Views
{
    public partial class SidebarTestView : UserControl
    {
        private TextBlock _statusText;
        
        public SidebarTestView()
        {
            InitializeComponent();
            
            // Get references to controls
            _statusText = this.FindControl<TextBlock>("StatusText");
            
            // Log when the view is attached to the visual tree
            this.AttachedToVisualTree += (s, e) => {
                Debug.WriteLine("SidebarTestView attached to visual tree");
                Debug.WriteLine($"DataContext is: {DataContext?.GetType().Name ?? "null"}");
            };
            
            // Log when the DataContext changes
            this.DataContextChanged += (s, e) => {
                Debug.WriteLine($"SidebarTestView DataContext changed to: {DataContext?.GetType().Name ?? "null"}");
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        private void OnCodeBehindButtonClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Code-Behind button was clicked in SidebarTestView!");
            UpdateStatus("Code-Behind button was clicked!");
            
            // Show a popup to confirm the button works
            ShowClickFeedback(sender as Control, "Code-Behind button clicked!");
        }
        
        private void OnDirectEventButtonClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Direct Event button was clicked in SidebarTestView!");
            UpdateStatus("Direct Event button was clicked!");
            
            // Show a popup to confirm the button works
            ShowClickFeedback(sender as Control, "Direct Event button clicked!");
        }
        
        private void UpdateStatus(string message)
        {
            if (_statusText != null)
            {
                _statusText.Text = $"{message} (Time: {DateTime.Now.ToLongTimeString()})";
            }
        }
        
        private void ShowClickFeedback(Control control, string message)
        {
            if (control == null) return;
            
            var flyout = new Flyout
            {
                Content = new TextBlock { Text = message },
                Placement = PlacementMode.Bottom
            };
            
            FlyoutBase.SetAttachedFlyout(control, flyout);
            FlyoutBase.ShowAttachedFlyout(control);
        }
    }
}