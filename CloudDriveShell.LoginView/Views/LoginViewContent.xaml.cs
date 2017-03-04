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
    /// Interaction logic for LoginViewContent.xaml
    /// </summary>
    [Export(RegionNames.LoginViewContentView, typeof(IContentView))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class LoginViewContent : UserControl, IContentView
    {
        public LoginViewContent()
        {
            InitializeComponent();
        }

        [Import]
        private LoginViewContentViewModel ViewModel
        {
            set
            {
                this.DataContext = value;
                value.SetConfigPassword = s => this.TxtPassword.Password = s;
                this.Loaded += (sender, args) =>
                {
                    value.ViewLoadedAction();
                };
            }
        }

        private void txtUserName_GotFocus(object sender, RoutedEventArgs e)
        {
            var txtUserName = (TextBox)sender;
            txtUserName.Background = new SolidColorBrush(SystemColors.WindowColor);
        }

        private void txtUserName_LostFocus(object sender, RoutedEventArgs e)
        {
            var txtUserName = (TextBox)sender;
            if (string.IsNullOrEmpty(txtUserName.Text))
            {
                txtUserName.Background = this.Resources["UserNameBrush"] as Brush;
            }
        }

        private void txtPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            var txtPassword = (PasswordBox)sender;
            txtPassword.Background = new SolidColorBrush(SystemColors.WindowColor);
        }

        private void txtPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            var txtPassword = (PasswordBox)sender;
            if (string.IsNullOrEmpty(txtPassword.Password))
            {
                txtPassword.Background = this.Resources["PasswordBrush"] as Brush;
            }
        }

        private void txtPassword_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            ((LoginViewContentViewModel)this.DataContext).LoginCommand.RaiseCanExecuteChanged();

            var txtPassword = (PasswordBox)sender;
            if (string.IsNullOrEmpty(txtPassword.Password))
            {
                txtPassword.Background = this.Resources[txtPassword.Tag.ToString()] as Brush;
            }
            else
            {
                txtPassword.Background = new SolidColorBrush(SystemColors.WindowColor);
            }
        }

        private void txtUserName_OnTextChanged(object sender, TextChangedEventArgs e)
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
