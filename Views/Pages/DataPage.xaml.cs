using Simulator.ViewModels;
using Simulator.ViewModels.Pages;
using System.Windows.Controls;

namespace Simulator.Views.Pages
{
    public partial class DataPage : Page
    {
        public DataPage()
        {
            InitializeComponent();
            DataContext = new DrawViewModels();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
