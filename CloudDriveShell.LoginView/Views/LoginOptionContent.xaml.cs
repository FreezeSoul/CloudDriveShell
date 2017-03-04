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
using CloudDriveShell.Infrastructure.Interfaces;
using CloudDriveShell.LoginView.ViewModels;

namespace CloudDriveShell.LoginView.Views
{
    /// <summary>
    /// Interaction logic for LoginOptionContent.xaml
    /// </summary>
    [Export(RegionNames.LoginOptionContentView, typeof(IContentView))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class LoginOptionContent : UserControl, IContentView
    {
        public LoginOptionContent()
        {
            InitializeComponent();
        }

        [Import]
        private LoginOptionContentViewModel ViewModel
        {
            set
            {
                this.DataContext = value;
            }
        }
        private void txtBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var txtBox = (TextBox)sender;
            txtBox.Background = new SolidColorBrush(SystemColors.WindowColor);
        }

        private void txtBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var txtBox = (TextBox)sender;
            if (string.IsNullOrEmpty(txtBox.Text))
            {
                txtBox.Background = this.Resources[txtBox.Tag.ToString()] as Brush;
            }
        }

        private void txtBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var txtBox = (TextBox)sender;
            if (string.IsNullOrEmpty(txtBox.Text))
            {
                txtBox.Background = this.Resources[txtBox.Tag.ToString()] as Brush;
            }
            else
            {
                txtBox.Background = new SolidColorBrush(SystemColors.WindowColor);
            }
        }
    }
}
