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

namespace CloudDriveShell.CloudDriveContent.ViewModels
{
    [Export]
    class CloudLogsContentViewModel : BindableBase
    {
    }
}
