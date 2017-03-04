using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CloudDriveShell.Infrastructure.Models;
using Prism.Mvvm;
using System.Globalization;
using CloudDriveShell.Infrastructure;
using CloudDriveShell.Infrastructure.Interfaces;
using Prism.Events;
using Prism.Regions;
using Microsoft.Practices.ServiceLocation;

namespace CloudDriveShell.CloudSettingContent.ViewModels
{
    [Export]
    class CloudSettingContentViewModel : BindableBase
    {
        public List<LeftMenu> LeftMenus { set; get; }

        private LeftMenu _selectedMenu;
        public LeftMenu SelectedMenu
        {
            get { return this._selectedMenu; }
            set
            {
                SetProperty(ref this._selectedMenu, value);
            }
        }


        private readonly IRegionManager _regionManager;

        private readonly IEventAggregator _eventAggregator;

        private readonly ISwitchContentService _switchContentService;

        public Action ViewLoadedAction;

        [ImportingConstructor]
        public CloudSettingContentViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, ISwitchContentService switchContentService)
        {
            this._regionManager = regionManager;
            this._eventAggregator = eventAggregator;
            this._switchContentService = switchContentService;
            this.PropertyChanged += CloudDriveContentViewModel_PropertyChanged;
            this.ViewLoadedAction = () =>
            {
                this._switchContentService.SwitchContentView(RegionNames.ActionCloudSettingRightRegion, this.SelectedMenu.ViewName);


            };
            this.LeftMenus = new List<LeftMenu>()
            {
                new LeftMenu("设置", string.Format("/{0};component/Resources/{1}", GetType().Assembly.FullName, "set.png"),RegionNames.CloudSettingCommon),
            };
            this.SelectedMenu = LeftMenus.First();
        }

        void CloudDriveContentViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedMenu")
            {

                this._switchContentService.SwitchContentView(RegionNames.ActionCloudSettingRightRegion, this.SelectedMenu.ViewName);
                
            }
        }


    }
}
