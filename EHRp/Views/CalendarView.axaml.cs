using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EHRp.Views
{
    public partial class CalendarView : UserControl
    {
        public CalendarView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}