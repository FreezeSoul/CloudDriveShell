using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudDriveShell.Infrastructure;
using CloudDriveShell.Infrastructure.Interfaces;
using CloudDriveShell.Infrastructure.Models;

namespace CloudDriveShell.CloudSettingContent
{
    [Export(typeof(INavigateMeta))]
    public class NavigateMeta : INavigateMeta
    {

        public NavigateMenu LoadNavigateMenu()
        {
            return new NavigateMenu()
            {
                MenuName = "设置",
                MenuIcon = "Resources/setting.png",
                RegionViewName = RegionNames.CloudSettingContentView,
                AssemblyName = GetType().Assembly.FullName,
                SortIndex = 2
            };
        }
    }
}
