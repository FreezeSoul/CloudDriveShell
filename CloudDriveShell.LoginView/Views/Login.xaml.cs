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
using CloudDriveShell.LoginView.ViewModels;

namespace CloudDriveShell.LoginView.Views
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    [Export]
    public partial class Login : Window
    {

        public Login()
        {
            InitializeComponent();
        }

        [Import]
        private LoginViewModel ViewModel
        {
            set
            {
                this.DataContext = value;
                value.RequestClose += (sender, args) =>
                {
                    this.DialogResult = ((LoginStatusEventArgs)args).LoginStatus;
                    this.Close();
                };
                value.RequestMinimize += (sender, args) =>
                {
                    this.WindowState = WindowState.Minimized;
                };
                this.Loaded += (sender, args) =>
                {
                    value.ViewLoadedAction();
                };
            }
        }

        private void Login_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
