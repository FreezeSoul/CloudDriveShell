

using System.ComponentModel;
using Prism.Mvvm;
using System.ComponentModel.Composition;
using System.Windows;
using CloudDriveShell.CloudDriveContent.ViewModels;
using CloudDriveShell.Infrastructure;
using CloudDriveShell.Infrastructure.Interfaces;
using CloudDriveShell.Infrastructure.Models;
using CloudDriveShell.Infrastructure.Views;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;
using Prism.Events;
using Transitionals;
using Transitionals.Transitions;

namespace CloudDriveShell
{
    [Export]
    public class ShellViewModel : BindableBase
    {

        private readonly IEventAggregator _eventAggregator;

        private bool _isSwitchUser;

        [ImportingConstructor]
        public ShellViewModel(IEventAggregator eventAggregator)
        {
            this._eventAggregator = eventAggregator;
        }

        public void RefreshResourceAction()
        {
            this._eventAggregator.GetEvent<RefreshEvent>().Publish(null);
        }

        public void PasteFileAction(string[] files)
        {
            this._eventAggregator.GetEvent<PasteFileEvent>().Publish(files);
        }

        public void PasteResourceAction()
        {
            this._eventAggregator.GetEvent<PasteResourceEvent>().Publish();
        }

        public bool SwitchUserAction()
        {
            var dialogView = ServiceLocator.Current.GetInstance<IDialogView>(RegionNames.DialogWindowView);
            if (!dialogView.ShowDialog(DialogEnum.OkOrCanel, "请确认", "是否切换当前用户？")) return false;
            this._eventAggregator.GetEvent<SwitchuserEvent>().Publish();
            this._isSwitchUser = true;
            return true;
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            if (!this._isSwitchUser)
            {
                var dialogView = ServiceLocator.Current.GetInstance<IDialogView>(RegionNames.DialogWindowView);
                if (!dialogView.ShowDialog(DialogEnum.OkOrCanel, "请确认", "是否退出云盘管理界面？"))
                {
                    e.Cancel = true;
                    return;
                }
            }
            this._eventAggregator.GetEvent<WindowClosingEvent>().Publish(e);
        }

    }

}
