using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EHRp.Views
{
    public partial class PatientsView : UserControl
    {
        public PatientsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}