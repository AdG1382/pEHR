using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;

namespace EHRp.Views
{
    public partial class ButtonTestView : UserControl
    {
        private TextBlock _statusText;
        
        public ButtonTestView()
        {
            InitializeComponent();
            
            // Get references to controls
            _statusText = this.FindControl<TextBlock>("StatusText");
            
            // Log when the view is attached to the visual tree
            this.AttachedToVisualTree += (s, e) => {
                Debug.WriteLine("ButtonTestView attached to visual tree");
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Button was clicked in ButtonTestView!");
            UpdateStatus("Button was clicked!");
            
            // Show a popup to confirm the button works
            ShowClickFeedback(sender as Control, "Button clicked!");
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