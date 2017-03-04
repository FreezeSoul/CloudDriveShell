using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
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
using CloudDriveShell.CloudDriveContent.ViewModels;
using CloudDriveShell.Infrastructure;
using CloudDriveShell.Infrastructure.Interfaces;
using CloudDriveShell.Infrastructure.Models;

namespace CloudDriveShell.CloudDriveContent.Views
{
    /// <summary>
    /// Interaction logic for CloudDriveTransfer.xaml
    /// </summary>
    [Export(RegionNames.CloudDriveTransferView, typeof(IContentView))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class CloudDriveTransfer : UserControl, IContentView
    {
        public CloudDriveTransfer()
        {
            InitializeComponent();      
        }


        [Import]
        private CloudDriveTransferViewModel ViewModel
        {
            set
            {
                this.DataContext = value;
            }
        }

        private void MouseDoubleClick_OnHandler(object sender, MouseButtonEventArgs e)
        {
            var textBox = (ListBoxItem)sender;
            var transferInfo = textBox.DataContext as TransferInfo;
            if (transferInfo == null) return;
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                var filePath = System.IO.Path.GetDirectoryName(transferInfo.FileLocalPath);
                if (System.IO.Directory.Exists(filePath))
                {
                    Process.Start("explorer.exe", filePath);
                }
            }
            else
            {
                ((CloudDriveTransferViewModel)this.DataContext).NavigateItemCommand.Execute(transferInfo);
            }
        }
    }
}
