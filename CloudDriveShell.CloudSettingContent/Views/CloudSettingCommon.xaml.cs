﻿using System;
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
using CloudDriveShell.Infrastructure.Interfaces;
using CloudDriveShell.CloudSettingContent.ViewModels;


namespace CloudDriveShell.CloudSettingContent.Views
{
    /// <summary>
    /// Interaction logic for CloudSettingCommon.xaml
    /// </summary>
    [Export(RegionNames.CloudSettingCommon, typeof(IContentView))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class CloudSettingCommon : UserControl, IContentView
    {
        public CloudSettingCommon()
        {
            InitializeComponent();
        }

        [Import]
        private CloudSettingCommonViewModel ViewModel
        {
            set
            {
                this.DataContext = value;
                this.Loaded += (sender, args) =>
                {
                    value.ViewLoadedAction();
                };
            }
        }
        

    }
}
