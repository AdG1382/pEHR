using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using EHRp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;

namespace EHRp.Views
{
    public partial class TestButtonView : UserControl
    {
        private TextBlock _statusText;
        private Button _inlineButton;
        private Button _mvvmButton;
        
        public TestButtonView()
        {
            InitializeComponent();
            
            // Get references to controls
            _statusText = this.FindControl<TextBlock>("StatusText");
            _inlineButton = this.FindControl<Button>("InlineButton");
            _mvvmButton = this.FindControl<Button>("MvvmButton");
            
            // Attach event handler to inline button
            if (_inlineButton != null)
            {
                _inlineButton.Click += OnInlineButtonClick;
            }
            
            // Log when the view is attached to the visual tree
            this.AttachedToVisualTree += (s, e) => {
                Debug.WriteLine("TestButtonView attached to visual tree");
                Debug.WriteLine($"DataContext is: {DataContext?.GetType().Name ?? "null"}");
                
                // If DataContext is null, try to set it manually
                if (DataContext == null)
                {
                    Debug.WriteLine("DataContext is null, attempting to set it manually");
                    try
                    {
                        var viewModel = Program.ServiceProvider.GetRequiredService<TestButtonViewModel>();
                        DataContext = viewModel;
                        Debug.WriteLine($"DataContext manually set to: {DataContext?.GetType().Name ?? "null"}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error setting DataContext: {ex.Message}");
                    }
                }
                
                // Check if the MVVM button has a command
                if (_mvvmButton != null)
                {
                    Debug.WriteLine($"MVVM Button Command: {_mvvmButton.Command?.GetType().Name ?? "null"}");
                }
            };
            
            // Log when the DataContext changes
            this.DataContextChanged += (s, e) => {
                Debug.WriteLine($"TestButtonView DataContext changed to: {DataContext?.GetType().Name ?? "null"}");
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        private void OnTestButtonClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Code-Behind button was clicked!");
            UpdateStatus("Code-Behind button was clicked!");
            
            // Show a popup to confirm the button works
            ShowClickFeedback(sender as Control, "Code-Behind button clicked!");
        }
        
        private void OnDirectEventButtonClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Direct Event button was clicked!");
            UpdateStatus("Direct Event button was clicked!");
            
            // Show a popup to confirm the button works
            ShowClickFeedback(sender as Control, "Direct Event button clicked!");
        }
        
        private void OnInlineButtonClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Inline Event button was clicked!");
            UpdateStatus("Inline Event button was clicked!");
            
            // Show a popup to confirm the button works
            ShowClickFeedback(sender as Control, "Inline Event button clicked!");
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