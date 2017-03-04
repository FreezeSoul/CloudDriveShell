using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudDriveShell.Infrastructure;
using CloudDriveShell.Infrastructure.Interfaces;
using CloudDriveShell.Infrastructure.Models;
using CloudDriveShell.LoginView.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace CloudDriveShell.LoginView.ViewModels
{
    [Export]
    class LoginViewModel : BindableBase
    {

        public event EventHandler RequestClose;
        public event EventHandler RequestMinimize;
        public DelegateCommand ExitShellCommand { get; private set; }
        public DelegateCommand MinimizeShellCommand { get; private set; }
        public DelegateCommand OptionSwitchShellCommand { get; private set; }

        public IRegionManager RegionManager
        {
            get { return _regionManager; }
        }

        private readonly IRegionManager _regionManager;

        private readonly IEventAggregator _eventAggregator;

        private readonly ISwitchContentService _switchContentService;

        public Action ViewLoadedAction;

        [ImportingConstructor]
        public LoginViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, ISwitchContentService switchContentService)
        {
            this._regionManager = regionManager;
            this._eventAggregator = eventAggregator;
            this._switchContentService = switchContentService;
            this.ExitShellCommand = new DelegateCommand(this.ExitShell, this.CanExitShell);
            this.MinimizeShellCommand = new DelegateCommand(this.MinimizeShell, this.CanMinimizeShell);
            this.OptionSwitchShellCommand = new DelegateCommand(this.OptionSwitchShell, this.CanOptionSwitchShell);
            this.ViewLoadedAction = () => { this._switchContentService.SwitchContentView(RegionNames.LoginContentRegion, RegionNames.LoginViewContentView); };
            this._eventAggregator.GetEvent<LoginStatusEvent>().Subscribe((status) =>
            {
                if (RequestClose != null)
                    RequestClose(this, new LoginStatusEventArgs(status));
            });
        }



        private void MinimizeShell()
        {
            if (RequestMinimize != null)
            {
                RequestMinimize(this, EventArgs.Empty);
            }
        }

        private bool CanMinimizeShell()
        {
            return true;
        }


        private void ExitShell()
        {
            if (RequestClose != null)
            {
                RequestClose(this, new LoginStatusEventArgs(false));
            }
        }

        private bool CanExitShell()
        {
            return true;
        }


        private void OptionSwitchShell()
        {
            this._switchContentService.SwitchContentView(RegionNames.LoginContentRegion, RegionNames.LoginOptionContentView);
        }

        private bool CanOptionSwitchShell()
        {
            return true;
        }
    }

    public class LoginStatusEvent : PubSubEvent<bool>
    {
    }

    public class LoginStatusEventArgs : EventArgs
    {

        public LoginStatusEventArgs(bool status)
        {
            LoginStatus = status;
        }


        public bool LoginStatus { set; get; }
    }

}
