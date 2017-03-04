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
using CloudDriveShell.Infrastructure.Models;

namespace CloudDriveShell
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    [Export]
    public partial class Shell : Window
    {
        public Shell()
        {
            InitializeComponent();

        }

        [Import]
        ShellViewModel ViewModel
        {
            set
            {
                this.DataContext = value;
                Closing -= value.OnWindowClosing;
                Closing += value.OnWindowClosing;
            }
        }


        #region TitleButton
        private void PART_TITLEBAR_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void PART_CLOSE_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PART_MAXIMIZE_RESTORE_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        private void PART_MINIMIZE_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void PPART_USERSWITCH_Click(object sender, RoutedEventArgs e)
        {
            if (!((ShellViewModel)this.DataContext).SwitchUserAction()) return;
            System.Windows.Forms.Application.Restart();
            this.Close();
        }

        #endregion

        #region ResizeWindows
        bool _resizeInProcess = false;
        private void Resize_Init(object sender, MouseButtonEventArgs e)
        {
            Rectangle senderRect = sender as Rectangle;
            if (senderRect != null)
            {
                _resizeInProcess = true;
                senderRect.CaptureMouse();
            }
        }

        private void Resize_End(object sender, MouseButtonEventArgs e)
        {
            Rectangle senderRect = sender as Rectangle;
            if (senderRect != null)
            {
                _resizeInProcess = false; ;
                senderRect.ReleaseMouseCapture();
            }
        }

        private void Resizeing_Form(object sender, MouseEventArgs e)
        {
            if (_resizeInProcess)
            {
                Rectangle senderRect = sender as Rectangle;
                if (senderRect != null)
                {
                    Window mainWindow = senderRect.Tag as Window;
                    if (mainWindow != null)
                    {
                        double width = e.GetPosition(mainWindow).X;
                        double height = e.GetPosition(mainWindow).Y;
                        senderRect.CaptureMouse();
                        if (senderRect.Name.ToLower().Contains("right"))
                        {
                            width += 5;
                            if (width > 0)
                                mainWindow.Width = width;
                        }
                        if (senderRect.Name.ToLower().Contains("left"))
                        {
                            width -= 5;
                            mainWindow.Left += width;
                            width = mainWindow.Width - width;
                            if (width > 0)
                            {
                                mainWindow.Width = width;
                            }
                        }
                        if (senderRect.Name.ToLower().Contains("bottom"))
                        {
                            height += 5;
                            if (height > 0)
                                mainWindow.Height = height;
                        }
                        if (senderRect.Name.ToLower().Contains("top"))
                        {
                            height -= 5;
                            mainWindow.Top += height;
                            height = mainWindow.Height - height;
                            if (height > 0)
                            {
                                mainWindow.Height = height;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region KeyDownEvent
        private void Shell_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                IDataObject dataObject = Clipboard.GetDataObject();
                if (dataObject != null && dataObject.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = dataObject.GetData(DataFormats.FileDrop) as string[];
                    if (files != null)
                    {
                        Clipboard.Clear();
                        ((ShellViewModel)this.DataContext).PasteFileAction(files.ToList().Where(System.IO.File.Exists).ToArray());
                    }
                }
                else
                {
                    if (Clipboard.ContainsData(CommonConstant.ClipboardCopyResourceItemsDataFormats))
                    {
                        Clipboard.Clear();
                        ((ShellViewModel)this.DataContext).PasteResourceAction();
                    }
                }

            }
            else if (e.Key == Key.F5)
            {
                ((ShellViewModel)this.DataContext).RefreshResourceAction();
            }
        }

        #endregion
    }
}
