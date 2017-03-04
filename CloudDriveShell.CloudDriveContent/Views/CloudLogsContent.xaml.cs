using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using CloudDriveShell.CloudDriveContent.ViewModels;
using CloudDriveShell.Infrastructure;
using CloudDriveShell.Infrastructure.Interfaces;


namespace CloudDriveShell.CloudDriveContent.Views
{
    /// <summary>
    /// Interaction logic for CloudSettingCommon.xaml
    /// </summary>
    [Export(RegionNames.CloudDriveLogsView, typeof(IContentView))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class CloudLogsContent : UserControl, IContentView
    {
        public CloudLogsContent()
        {
            InitializeComponent();
        }

        [Import]
        private CloudLogsContentViewModel ViewModel
        {
            set
            {
                this.DataContext = value;
            }
        }
        

    }
}
