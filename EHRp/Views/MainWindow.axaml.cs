using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using EHRp.Messages;
using EHRp.ViewModels;
using System;
using System.Diagnostics;

namespace EHRp.Views;

public partial class MainWindow : Window, IRecipient<NavigationMessage>
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Log when the DataContext changes
        this.DataContextChanged += (s, e) => {
            Debug.WriteLine($"MainWindow DataContext changed to: {DataContext?.GetType().Name ?? "null"}");
        };
        
        // Register for navigation messages
        WeakReferenceMessenger.Default.Register<NavigationMessage>(this);
    }
    
    /// <summary>
    /// Handles navigation messages
    /// </summary>
    /// <param name="message">The navigation message</param>
    public void Receive(NavigationMessage message)
    {
        // Update the DataContext's CurrentViewModel property
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.CurrentViewModel = message.Value;
        }
    }
    
    protected override void OnClosed(EventArgs e)
    {
        // Unregister from messages when the window is closed
        WeakReferenceMessenger.Default.Unregister<NavigationMessage>(this);
        
        base.OnClosed(e);
    }
}