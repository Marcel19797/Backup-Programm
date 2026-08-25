using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Backup_Programm.Views;
using Backup_Programm.ViewModels;
using Backup_Programm.Models;
using Backup_Programm.Services;

namespace Backup_Programm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // ViewModel instanziieren und dem DataContext zuweisen
            var viewModel = new MainViewModel();
            DataContext = viewModel;

            // Auto-Scroll Logik: Abonniert Änderungen der ObservableCollection
            viewModel.LogMessages.CollectionChanged += (sender, e) =>
            {
                if (viewModel.LogMessages.Count > 0)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (viewModel.LogMessages.Count > 0)
                        {
                            LogListBox.ScrollIntoView(viewModel.LogMessages.Last());
                        }
                    }), System.Windows.Threading.DispatcherPriority.Background);
                }
            };
        }
    }
}