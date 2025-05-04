using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;

namespace EHRp.ViewModels
{
    public partial class TestButtonViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _statusMessage = "No command executed yet.";
        
        public TestButtonViewModel()
        {
            Debug.WriteLine("TestButtonViewModel created");
        }
        
        [RelayCommand]
        private void TestCommand()
        {
            StatusMessage = $"Command executed successfully! (Time: {DateTime.Now.ToLongTimeString()})";
            Debug.WriteLine("TestCommand executed");
        }
    }
}