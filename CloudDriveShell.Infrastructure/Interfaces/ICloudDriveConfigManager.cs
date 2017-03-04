using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudDriveShell.Infrastructure.Interfaces
{
    public interface ICloudDriveConfigManager
    {
        void WriteConfig(string section, string name, string value);

        string ReadConfig(string section, string name);
    }
}
