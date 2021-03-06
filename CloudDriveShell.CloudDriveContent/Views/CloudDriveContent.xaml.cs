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
using CloudDriveShell.CloudDriveContent.ViewModels;
using CloudDriveShell.Infrastructure;
using CloudDriveShell.Infrastructure.Behaviors;
using CloudDriveShell.Infrastructure.Interfaces;
using CloudDriveShell.Infrastructure.Models;

namespace CloudDriveShell.CloudDriveContent.Views
{
    /// <summary>
    /// Interaction logic for CloudDriveContent.xaml
    /// </summary>
    [Export(RegionNames.CloudDriveContentView, typeof(IContentView))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class CloudDriveContent : UserControl, IContentView
    {
        public CloudDriveContent()
        {
            InitializeComponent();
        }

        [Import]
        private CloudDriveContentViewModel ViewModel
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
