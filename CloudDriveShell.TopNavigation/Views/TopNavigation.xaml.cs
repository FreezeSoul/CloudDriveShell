using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CloudDriveShell.Infrastructure;
using CloudDriveShell.Infrastructure.Behaviors;
using CloudDriveShell.TopNavigation.ViewModels;

namespace CloudDriveShell.TopNavigation.Views
{
    /// <summary>
    /// Interaction logic for TopNavigation.xaml
    /// </summary>
    [ViewExport(RegionName = RegionNames.TopNavigationRegion)]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class TopNavigation : UserControl
    {
        public TopNavigation()
        {
            InitializeComponent();
        }

        [Import]
        TopNavigationViewModel ViewModel
        {
            set
            {
                this.DataContext = value;
            }
        }

        private void TopNavigation_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Application.Current.MainWindow.WindowState = Application.Current.MainWindow.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
    }
}
