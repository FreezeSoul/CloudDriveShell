using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CloudDriveShell.Infrastructure;
using CloudDriveShell.Infrastructure.Interfaces;
using CloudDriveShell.Infrastructure.Models;
using CloudDriveShell.Infrastructure.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace CloudDriveShell.LoginView.ViewModels
{
    [Export]
    class LoginOptionContentViewModel : BindableBase
    {
        private string _connectionInfo;

        public string ConnectionInfo
        {
            get { return this._connectionInfo; }
            set
            {
                SetProperty(ref this._connectionInfo, value);
            }
        }

        private string _dataServerAddress;

        public string DataServerAddress
        {
            get { return _dataServerAddress; }
            set { SetProperty(ref this._dataServerAddress, value); }
        }

        private string _dataServerPort;

        public string DataServerPort
        {
            get { return _dataServerPort; }
            set { SetProperty(ref this._dataServerPort, value); }
        }


        private readonly IRegionManager _regionManager;

        private readonly IEventAggregator _eventAggregator;

        private readonly ISwitchContentService _switchContentService;

        private readonly ICloudDriveConfigManager _cloudDriveConfigManager;

        private readonly IWebDavClientService _webDavClientService;

        public DelegateCommand TestConnectionCommand { get; private set; }
        public DelegateCommand AcceptConnectionCommand { get; private set; }


        [ImportingConstructor]
        public LoginOptionContentViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, ISwitchContentService switchContentService, ICloudDriveConfigManager cloudDriveConfigManager, IWebDavClientService webDavClientService)
        {
            this._regionManager = regionManager;
            this._eventAggregator = eventAggregator;
            this._switchContentService = switchContentService;
            this._webDavClientService = webDavClientService;
            this._cloudDriveConfigManager = cloudDriveConfigManager;
            this.TestConnectionCommand = new DelegateCommand(this.TestConnectionAction, () => true);
            this.AcceptConnectionCommand = new DelegateCommand(this.AcceptConnectionAction, () => true);

            this.DataServerAddress = this._cloudDriveConfigManager.ReadConfig(CloudDriveConfigManager.DataServerSectionConstant, CloudDriveConfigManager.DataServerAddressConstant);
            this.DataServerPort = this._cloudDriveConfigManager.ReadConfig(CloudDriveConfigManager.DataServerSectionConstant, CloudDriveConfigManager.DataServerPortConstant);
        }

        private void AcceptConnectionAction()
        {
            this._cloudDriveConfigManager.WriteConfig(CloudDriveConfigManager.DataServerSectionConstant, CloudDriveConfigManager.DataServerAddressConstant, this.DataServerAddress);
            this._cloudDriveConfigManager.WriteConfig(CloudDriveConfigManager.DataServerSectionConstant, CloudDriveConfigManager.DataServerPortConstant, this.DataServerPort);

            this._eventAggregator.GetEvent<ConfigChangeEvent>().Publish();

            this._switchContentService.SwitchContentView(RegionNames.LoginContentRegion, RegionNames.LoginViewContentView);
        }

        private async void TestConnectionAction()
        {
            try
            {
                this.ConnectionInfo = "开始测试链接...";
                //可手工配置，不提供配置界面，供外部测试用
                var webDevPath = this._cloudDriveConfigManager.ReadConfig(CloudDriveConfigManager.DataServerSectionConstant, CloudDriveConfigManager.DataServerPathConstant);
                var flag = await this._webDavClientService.Test(this.DataServerAddress, this.DataServerPort, string.IsNullOrEmpty(webDevPath) ? WebDavConstant.RootPath : webDevPath, this.DataServerPort.Equals("443"));
                this.ConnectionInfo = flag ? "测试链接成功" : "测试链接失败";
            }
            catch (Exception)
            {
                this.ConnectionInfo = "测试链接失败";
            }
        }
    }
}
