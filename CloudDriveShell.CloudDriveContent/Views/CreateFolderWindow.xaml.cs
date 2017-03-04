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
using System.Windows.Shapes;
using CloudDriveShell.CloudDriveContent.ViewModels;
using CloudDriveShell.Infrastructure;
using CloudDriveShell.Infrastructure.Interfaces;

namespace CloudDriveShell.CloudDriveContent.Views
{
    /// <summary>
    /// Interaction logic for CreateFolderWindow.xaml
    /// </summary>
    [Export(RegionNames.CloudDriveCreateFolderView, typeof(IContentView))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class CreateFolderWindow : Window, IContentView
    {
        public CreateFolderWindow()
        {
            InitializeComponent();
        }


        [Import]
        private CreateFolderWindowViewModel ViewModel
        {
            set
            {
                this.DataContext = value;
                value.RequestClose += (sender, args) =>
                {
                    this.Close();
                };
            }
        }


        private void PART_TITLEBAR_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        } 
    }
}
