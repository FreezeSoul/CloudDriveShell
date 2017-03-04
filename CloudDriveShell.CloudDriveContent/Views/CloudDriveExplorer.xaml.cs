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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CloudDriveShell.CloudDriveContent.ViewModels;
using CloudDriveShell.Infrastructure;
using CloudDriveShell.Infrastructure.Interfaces;
using CloudDriveShell.Infrastructure.Models;
using CloudDriveShell.Infrastructure.Utils;
using Clipboard = System.Windows.Clipboard;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using ListView = System.Windows.Controls.ListView;
using TextBox = System.Windows.Controls.TextBox;
using CheckBox = System.Windows.Controls.CheckBox;
using ListViewItem = System.Windows.Controls.ListBoxItem;
using UserControl = System.Windows.Controls.UserControl;

namespace CloudDriveShell.CloudDriveContent.Views
{
    /// <summary>
    /// Interaction logic for CloudDriveExplorer.xaml
    /// </summary>
    [Export(RegionNames.CloudDriveExplorerView, typeof(IContentView))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class CloudDriveExplorer : UserControl, IContentView
    {
        public CloudDriveExplorer()
        {
            InitializeComponent();
        }

        [Import]
        private CloudDriveExplorerViewModel ViewModel
        {
            set
            {
                this.DataContext = value;
            }
        }

        private bool CheckIsClickCheckBox(object obj)
        {
            if (XamlHelper.FindUpVisualTree<CheckBox>(obj as DependencyObject) != null) 
                return true;
            return false;
        }


        private void ListViewMouseDown_OnHandler(object sender, MouseButtonEventArgs e)
        {
            ((CloudDriveExplorerViewModel)this.DataContext).CurrentSelectResourceItem = null;
        }


        private void ListViewItemPreviewMouseDown_OnHandler(object sender, MouseButtonEventArgs e)
        {
            if (CheckIsClickCheckBox(e.OriginalSource)) return;
            var clickResourceItem = (ResourceItem)((ListViewItem)sender).DataContext;
            ((CloudDriveExplorerViewModel) this.DataContext).ResourceItemClickAction(Keyboard.IsKeyDown(Key.LeftCtrl),
                e.RightButton == MouseButtonState.Pressed, clickResourceItem);
        }

        private void ListViewItemMouseDoubleClick_OnHandler(object sender, MouseButtonEventArgs e)
        {
            ((CloudDriveExplorerViewModel)this.DataContext).ResouceOpenCommand.Execute();
        }

        private void ListBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((CloudDriveExplorerViewModel)this.DataContext).ResouceNavigateCommand.Execute(((ListBoxItem)sender).Content);
        }

        protected void Txtbox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            var resourceItem = textBox.DataContext as ResourceItem;
            if (resourceItem == null) return;
            ((CloudDriveExplorerViewModel)this.DataContext).RenameCallbackAction(resourceItem);
        }

        private void Txtbox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var textBox = (TextBox)sender;
                textBox.Visibility = Visibility.Collapsed;
            }
            else if (e.Key == Key.Escape)
            {
                ((CloudDriveExplorerViewModel)this.DataContext).RenameCallbackAction(null);
            }
        }

        private void ListView_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null)
                    ((CloudDriveExplorerViewModel)this.DataContext).DropUploadCallbackAction(files.ToList().Where(System.IO.File.Exists).ToArray());
            }
        }

        private void ListView_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                ((CloudDriveExplorerViewModel)this.DataContext).ResourceRenameCommand.Execute();
            }
            else if (e.Key == Key.C && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                Clipboard.SetData(CommonConstant.ClipboardCopyResourceItemsDataFormats, "复制");
                ((CloudDriveExplorerViewModel)this.DataContext).ResourceCopyCommand.Execute();
            }
            else if (e.Key == Key.X && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                Clipboard.SetData(CommonConstant.ClipboardCopyResourceItemsDataFormats, "移动");
                ((CloudDriveExplorerViewModel)this.DataContext).ResourceCutCommand.Execute();
            }

        }

        private void ListView_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                var resourceItem = ((ListView)sender).SelectedItem as ResourceItem;
                if (resourceItem == null) return;
                resourceItem.IsSelected = !resourceItem.IsSelected;
            }
            else if (e.Key == Key.Enter)
            {
                var resourceItem = ((ListView)sender).SelectedItem as ResourceItem;
                if (resourceItem == null) return;
                ((CloudDriveExplorerViewModel)this.DataContext).ResouceNavigateCommand.Execute(resourceItem);
            }
        }
    }
}
