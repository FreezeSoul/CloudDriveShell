using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Controls;
using CloudDriveShell.Infrastructure.Interfaces;
using CloudDriveShell.Infrastructure.Models;
using CloudDriveShell.Infrastructure.Services;
using CloudDriveShell.Infrastructure.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace CloudDriveShell.LoginView.ViewModels
{
    [Export]
    class LoginViewContentViewModel : BindableBase
    {
        private string _loginInfo;

        public string LoginInfo
        {
            get { return this._loginInfo; }
            set { SetProperty(ref this._loginInfo, value); }
        }

        private bool _remeberPassword;

        public bool RemeberPassword
        {
            get { return this._remeberPassword; }
            set { SetProperty(ref this._remeberPassword, value); }
        }

        private bool _autoLogin;

        public bool AutoLogin
        {
            get { return this._autoLogin; }
            set { SetProperty(ref this._autoLogin, value); }
        }

        private bool _isLoading;

        public bool IsLoading
        {
            get { return this._isLoading; }
            set { SetProperty(ref this._isLoading, value); }
        }

        private string _userName;

        public string UserName
        {
            get { return this._userName; }
            set
            {
                SetProperty(ref this._userName, value);
                LoginCommand.RaiseCanExecuteChanged();
            }
        }

        private readonly IRegionManager _regionManager;

        private readonly IEventAggregator _eventAggregator;

        private readonly ISwitchContentService _switchContentService;

        private readonly IWebDavClientService _webDavClientService;

        private readonly ICloudDriveConfigManager _cloudDriveConfigManager;

        public DelegateCommand<object> LoginCommand { get; private set; }


        public Action<string> SetConfigPassword;

        public Action ViewLoadedAction;

        [ImportingConstructor]
        public LoginViewContentViewModel(IRegionManager regionManager, IEventAggregator eventAggregator,
            ISwitchContentService switchContentService, IWebDavClientService webDavClientService,
            ICloudDriveConfigManager cloudDriveConfigManager)
        {
            this._regionManager = regionManager;
            this._eventAggregator = eventAggregator;
            this._switchContentService = switchContentService;
            this._webDavClientService = webDavClientService;
            this._cloudDriveConfigManager = cloudDriveConfigManager;
            this.LoginCommand = new DelegateCommand<object>(this.LoginAction, this.CanLoginAction);

            InitWebDavConfig();
            this.PropertyChanged += OnPropertyChanged;
            this.ViewLoadedAction = ReadUserInfoFromConfig;
            this._eventAggregator.GetEvent<ConfigChangeEvent>().Subscribe(InitWebDavConfig);
            this._eventAggregator.GetEvent<SwitchuserEvent>().Subscribe(SwitchuserFunction, ThreadOption.UIThread);

        }


        private void ReadUserInfoFromConfig()
        {
            this.AutoLogin = this._cloudDriveConfigManager.ReadConfig(CloudDriveConfigManager.LoginConfigSectionConstant, CloudDriveConfigManager.AutoLoginConstant) == "1";
            this.RemeberPassword = this._cloudDriveConfigManager.ReadConfig(CloudDriveConfigManager.LoginConfigSectionConstant, CloudDriveConfigManager.RemeberInfoConstant) == "1";
            this.UserName = this._cloudDriveConfigManager.ReadConfig(CloudDriveConfigManager.LoginConfigSectionConstant, CloudDriveConfigManager.UserNameConstant);
            var password = this._cloudDriveConfigManager.ReadConfig(CloudDriveConfigManager.LoginConfigSectionConstant, CloudDriveConfigManager.PasswordConstant);

            if (SetConfigPassword != null)
            {
                try
                {
                    password = SecurityHelper.DesDecrypt(password);
                }
                catch (Exception)
                {
                    password = string.Empty;
                }
                SetConfigPassword(password);
            }

            if (this.AutoLogin && !string.IsNullOrEmpty(this.UserName) && !string.IsNullOrEmpty(password))
            {
                LoginAction(password);
            }

        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "AutoLogin")
            {
                if (this.AutoLogin)
                {
                    this.RemeberPassword = true;
                }
            }

            if (propertyChangedEventArgs.PropertyName == "RemeberPassword")
            {
                if (!this.RemeberPassword)
                {
                    this.AutoLogin = false;
                }
            }
        }

        private void InitWebDavConfig()
        {
            var webDavAddres = this._cloudDriveConfigManager.ReadConfig(CloudDriveConfigManager.DataServerSectionConstant, CloudDriveConfigManager.DataServerAddressConstant);
            var webDavPort = this._cloudDriveConfigManager.ReadConfig(CloudDriveConfigManager.DataServerSectionConstant, CloudDriveConfigManager.DataServerPortConstant);
            //可手工配置，不提供配置界面，供外部测试用
            var webDevPath = this._cloudDriveConfigManager.ReadConfig(CloudDriveConfigManager.DataServerSectionConstant, CloudDriveConfigManager.DataServerPathConstant);
            if (!string.IsNullOrEmpty(webDevPath))
                WebDavConstant.RootPath = webDevPath;
            this._webDavClientService.Init(webDavAddres, webDavPort, WebDavConstant.RootPath, webDavPort.Equals("443"));
        }

        private void SwitchuserFunction()
        {
            this._cloudDriveConfigManager.WriteConfig(CloudDriveConfigManager.LoginConfigSectionConstant, CloudDriveConfigManager.AutoLoginConstant, "0");
        }

        private bool CanLoginAction(object parameter)
        {
            var passwordBox = (PasswordBox)parameter;
            return !string.IsNullOrEmpty(this.UserName) && !string.IsNullOrEmpty(passwordBox.Password);
        }

        private void LoginAction(object parameter)
        {
            var passwordBox = (PasswordBox)parameter;
            LoginAction(passwordBox.Password);
        }

        private async void LoginAction(string password)
        {
            try
            {
                this.IsLoading = true;
                this.LoginInfo = "正在登录...";
                SaveConfigInfo(password);
                var isLogin = await this._webDavClientService.Login(this.UserName, password);
                if (isLogin)
                {
                    this.LoginInfo = "登录成功";
                    CurrentAccountInfo.Instance.AccountName = this.UserName;
                    this._eventAggregator.GetEvent<LoginStatusEvent>().Publish(true);
                }
                else
                {
                    this.LoginInfo = "登录失败";
                }
            }
            catch (HttpException exception)
            {
                this.LoginInfo = (exception.GetHttpCode() == (int)HttpStatusCode.Unauthorized)
                    ? "用户名或密码错误"
                    : "存储服务连接失败";
            }
            catch (Exception)
            {
                this.LoginInfo = "登录失败";
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        private void SaveConfigInfo(string password)
        {
            this._cloudDriveConfigManager.WriteConfig(CloudDriveConfigManager.LoginConfigSectionConstant, CloudDriveConfigManager.AutoLoginConstant, this.AutoLogin ? "1" : "0");
            this._cloudDriveConfigManager.WriteConfig(CloudDriveConfigManager.LoginConfigSectionConstant, CloudDriveConfigManager.RemeberInfoConstant, this.RemeberPassword ? "1" : "0");
            this._cloudDriveConfigManager.WriteConfig(CloudDriveConfigManager.LoginConfigSectionConstant, CloudDriveConfigManager.UserNameConstant, (this.AutoLogin || this.RemeberPassword) ? this.UserName : string.Empty);
            this._cloudDriveConfigManager.WriteConfig(CloudDriveConfigManager.LoginConfigSectionConstant, CloudDriveConfigManager.PasswordConstant, this.RemeberPassword ? SecurityHelper.DesEncrypt(password) : string.Empty);
        }
    }


    public class ConfigChangeEvent : PubSubEvent
    {
    }
}
