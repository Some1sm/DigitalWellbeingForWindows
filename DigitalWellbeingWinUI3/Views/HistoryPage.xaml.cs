using DigitalWellbeingWinUI3.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace DigitalWellbeingWinUI3.Views
{
    public sealed partial class HistoryPage : Page
    {
        public HistoryViewModel ViewModel { get; }

        public HistoryPage()
        {
            this.InitializeComponent();
            ViewModel = new HistoryViewModel();
            this.DataContext = ViewModel;
        }
    }
}
