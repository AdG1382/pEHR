using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EHRp.Views.Visits
{
    public partial class VisitFormView : UserControl
    {
        public VisitFormView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}