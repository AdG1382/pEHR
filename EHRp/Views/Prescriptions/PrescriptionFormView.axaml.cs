using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EHRp.Views.Prescriptions
{
    public partial class PrescriptionFormView : UserControl
    {
        public PrescriptionFormView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}