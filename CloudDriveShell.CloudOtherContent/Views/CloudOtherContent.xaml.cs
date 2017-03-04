using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CloudDriveShell.Infrastructure;
using CloudDriveShell.Infrastructure.Behaviors;
using CloudDriveShell.Infrastructure.Interfaces;

namespace CloudDriveShell.CloudOtherContent.Views
{
    /// <summary>
    /// Interaction logic for CloudDriveContent.xaml
    /// </summary>
    [Export(RegionNames.CloudOtherContentView, typeof(IContentView))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class CloudOtherContent : UserControl, IContentView
    {
        public CloudOtherContent()
        {
            InitializeComponent();
        }
    }
}
