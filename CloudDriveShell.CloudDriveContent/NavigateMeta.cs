using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudDriveShell.Infrastructure;
using CloudDriveShell.Infrastructure.Interfaces;
using CloudDriveShell.Infrastructure.Models;

namespace CloudDriveShell.CloudDriveContent
{
    [Export(typeof(INavigateMeta))]
    public class NavigateMeta : INavigateMeta
    {
        public NavigateMenu LoadNavigateMenu()
        {
            return new NavigateMenu()
            {
                MenuName = "云盘",
                MenuIcon = "Resources/cloud.png",
                RegionViewName = RegionNames.CloudDriveContentView,
                AssemblyName = GetType().Assembly.FullName,
                SortIndex = 1
            };
        }
    }
}
