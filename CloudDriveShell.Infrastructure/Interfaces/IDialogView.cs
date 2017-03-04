using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudDriveShell.Infrastructure.Views;

namespace CloudDriveShell.Infrastructure.Interfaces
{
    public interface IDialogView
    {
        bool ShowDialog(DialogEnum dialogEnum, string title, string message);
    }
}
