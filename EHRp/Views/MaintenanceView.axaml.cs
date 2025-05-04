using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EHRp.Views
{
    public partial class MaintenanceView : UserControl
    {
        public MaintenanceView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}