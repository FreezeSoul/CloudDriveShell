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
using CloudDriveShell.Infrastructure.Services;
using CloudDriveShell.Infrastructure.Utils;

namespace CloudDriveShell.CloudSettingContent.ViewModels
{
    [Export]
    class CloudSettingCommonViewModel : BindableBase
    {
        private bool _isAutoRun;
        public bool isAutoRun
        {
            get { return this._isAutoRun; }
            set
            {
                SetProperty(ref this._isAutoRun, value);

            }
        }


        private readonly IRegionManager _regionManager;

        private readonly IEventAggregator _eventAggregator;

        private readonly ISwitchContentService _switchContentService;

        private readonly ICloudDriveConfigManager _cloudDriveConfigManager;

        public Action ViewLoadedAction;

        [ImportingConstructor]
        public CloudSettingCommonViewModel(IRegionManager regionManager, IEventAggregator eventAggregator,
            ISwitchContentService switchContentService, ICloudDriveConfigManager cloudDriveConfigManager)
        {
            this._regionManager = regionManager;
            this._eventAggregator = eventAggregator;
            this._switchContentService = switchContentService;
            this._cloudDriveConfigManager = cloudDriveConfigManager;
            this.ViewLoadedAction = () =>
            {
            };
            this.PropertyChanged += CloudSettingCommonViewModel_PropertyChanged;
           
        }

        void CloudSettingCommonViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
           
        }
    }
}
