using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudDriveShell.Infrastructure.Interfaces
{
    public interface ISwitchContentService
    {
        void SwitchContentView(string viewKey);

        void SwitchContentView(string regionName, string viewKey);

    }
}
