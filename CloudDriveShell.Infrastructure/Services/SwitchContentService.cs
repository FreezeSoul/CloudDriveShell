using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using CloudDriveShell.Infrastructure.Interfaces;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;
using Prism.Regions;

namespace CloudDriveShell.Infrastructure.Services
{

    [Export(typeof(ISwitchContentService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SwitchContentService : ISwitchContentService
    {
        private readonly IRegionManager _regionManager;

        [ImportingConstructor]
        public SwitchContentService(IRegionManager regionManager)
        {
            this._regionManager = regionManager;
        }

        public void SwitchContentView(string viewKey)
        {
            this.SwitchContentView(RegionNames.ActionContentRegion, viewKey);
        }

        public void SwitchContentView(string regionName, string viewKey)
        {
            if (this._regionManager.Regions.ContainsRegionWithName(regionName))
            {
                IRegion region = this._regionManager.Regions[regionName];
                var view = ServiceLocator.Current.GetInstance<IContentView>(viewKey);
                region.RemoveAll();
                region.Add(view);
                region.Activate(view);
            }
        }
    }
}
