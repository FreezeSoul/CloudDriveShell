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
using CloudDriveShell.Infrastructure.Interfaces;

namespace CloudDriveShell.Infrastructure.Views
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    [Export(RegionNames.DialogWindowView, typeof(IDialogView))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class DialogWindow : Window, IDialogView
    {
        public DialogWindow()
        {
            InitializeComponent();
        }


        [Import]
        private DialogWindowViewModel ViewModel
        {
            set
            {
                this.DataContext = value;
                value.RequestClose += (sender, args) =>
                {
                    this.DialogResult = args;
                    this.Close();
                };
            }
        }


        private void PART_TITLEBAR_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        public bool ShowDialog(DialogEnum dialogEnum, string title, string message)
        {
            this.Owner = Application.Current.MainWindow;
            this.Icon = Application.Current.MainWindow.Icon;
            ((DialogWindowViewModel)this.DataContext).DialogTitle = title;
            ((DialogWindowViewModel)this.DataContext).DialogMessage = message;
            this.CannelBtn.Visibility = dialogEnum == DialogEnum.Ok ? Visibility.Collapsed : Visibility.Visible;
            return this.ShowDialog() ?? false;
        }
    }

    public enum DialogEnum
    {
        Ok,
        OkOrCanel
    }
}
