using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudDriveShell.Infrastructure.Models;

namespace CloudDriveShell.Infrastructure.Interfaces
{
    public interface INavigateMeta
    {
        NavigateMenu LoadNavigateMenu();
    }
}
